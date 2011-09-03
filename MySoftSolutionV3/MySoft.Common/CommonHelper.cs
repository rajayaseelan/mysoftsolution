using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace MySoft.Common
{
    /// <summary>
    /// WebUtility : 基于System.Web的拓展类。
    /// </summary>
    public abstract class CommonHelper
    {
        /// <summary>
        /// 检测指定的 Uri 是否有效
        /// </summary>
        /// <param name="url">指定的Url地址</param>
        /// <returns>bool</returns>
        public static bool ValidateUrl(string url)
        {
            Uri newUri = new Uri(url);

            try
            {
                WebRequest req = WebRequest.Create(newUri);
                //req.Timeout				= 10000;
                WebResponse res = req.GetResponse();
                HttpWebResponse httpRes = (HttpWebResponse)res;

                if (httpRes.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #region 文件下载
        // 输出硬盘文件，提供下载 支持大文件、续传、速度限制、资源占用小
        // 输入参数 _fileName: 下载文件名， _fullPath: 带文件名下载路径， _speed 每秒允许下载的字节数
        // 返回是否成功
        /// <summary>
        /// 输出硬盘文件，提供下载 支持大文件、续传、速度限制、资源占用小
        /// </summary>
        /// <param name="_fileName">下载文件名</param>
        /// <param name="_fullPath">带文件名下载路径</param>
        /// <param name="_speed">每秒允许下载的字节数</param>
        /// <returns>返回是否成功</returns>
        public static bool DownloadFile(string _fullPath, string _fileName, long _speed)
        {
            HttpRequest _Request = System.Web.HttpContext.Current.Request;
            HttpResponse _Response = System.Web.HttpContext.Current.Response;

            try
            {
                FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                try
                {
                    _Response.AddHeader("Accept-Ranges", "bytes");
                    _Response.Buffer = false;
                    long fileLength = myFile.Length;
                    long startBytes = 0;

                    int pack = 10240; //10K bytes
                    //int sleep = 200;   //每秒5次   即5*10K bytes每秒
                    double dblValue = 1000 * pack / _speed;
                    int sleep = (int)Math.Floor(dblValue) + 1;
                    if (_Request.Headers["Range"] != null)
                    {
                        _Response.StatusCode = 206;
                        string[] range = _Request.Headers["Range"].Split(new char[] { '=', '-' });
                        startBytes = Convert.ToInt64(range[1]);
                    }
                    _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                    if (startBytes != 0)
                    {
                        _Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }
                    _Response.AddHeader("Connection", "Keep-Alive");
                    _Response.ContentType = "application/octet-stream";
                    _Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8));


                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    dblValue = (fileLength - startBytes) / pack;
                    int maxCount = (int)Math.Floor(dblValue) + 1;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (_Response.IsClientConnected)
                        {
                            _Response.BinaryWrite(br.ReadBytes(pack));
                            Thread.Sleep(sleep);
                        }
                        else
                        {
                            i = maxCount;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 下载服务器上的文件（适用于非WEB路径下，且是大文件，该方法在IE中不会显示下载进度）
        /// </summary>
        /// <param name="path">下载文件的绝对路径</param>
        /// <param name="saveName">保存的文件名，包括后缀符</param>
        public static void Download(string path, string saveName)
        {
            HttpResponse Response = System.Web.HttpContext.Current.Response;

            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(saveName, System.Text.Encoding.UTF8));
            Response.TransmitFile(path);
            Response.End();
        }


        /// <summary>
        /// 下载服务器上的文件（适用于非WEB路径下，且是大文件，该方法在IE中会显示下载进度）
        /// </summary>
        /// <param name="path">下载文件的绝对路径</param>
        /// <param name="saveName">保存的文件名，包括后缀符</param>
        public static void DownloadFile(string path, string saveName)
        {
            Stream iStream = null;


            // Buffer to read 10K bytes in chunk:
            byte[] buffer = new Byte[10240];

            // Length of the file:
            int length;

            // Total bytes to read:
            long dataToRead;

            // Identify the file to download including its path.
            string filepath = path;

            // Identify the file name.
            string filename = Path.GetFileName(filepath);

            try
            {
                // Open the file.
                iStream = new System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Web.HttpContext.Current.Response.Clear();

                // Total bytes to read:
                dataToRead = iStream.Length;

                long p = 0;
                if (System.Web.HttpContext.Current.Request.Headers["Range"] != null)
                {
                    System.Web.HttpContext.Current.Response.StatusCode = 206;
                    p = long.Parse(System.Web.HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "").Replace("-", ""));
                }
                if (p != 0)
                {
                    System.Web.HttpContext.Current.Response.AddHeader("Content-Range", "bytes " + p.ToString() + "-" + ((long)(dataToRead - 1)).ToString() + "/" + dataToRead.ToString());
                }
                System.Web.HttpContext.Current.Response.AddHeader("Content-Length", ((long)(dataToRead - p)).ToString());
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
                System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(saveName, System.Text.Encoding.UTF8));

                iStream.Position = p;
                dataToRead = dataToRead - p;
                // Read the bytes.
                while (dataToRead > 0)
                {
                    // Verify that the client is connected.
                    if (System.Web.HttpContext.Current.Response.IsClientConnected)
                    {
                        // Read the data in buffer.
                        length = iStream.Read(buffer, 0, 10240);

                        // Write the data to the current output stream.
                        System.Web.HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);

                        // Flush the data to the HTML output.
                        System.Web.HttpContext.Current.Response.Flush();

                        buffer = new Byte[10240];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                // Trap the error, if any.
                System.Web.HttpContext.Current.Response.Write("Error : " + ex.Message);
            }
            finally
            {
                if (iStream != null)
                {
                    //Close the file.
                    iStream.Close();
                }

                System.Web.HttpContext.Current.Response.End();
            }
        }
        #endregion

        #region 获取指定页面的内容
        /// <summary>
        /// 从指定的URL下载页面内容(使用WebRequest)
        /// </summary>
        /// <param name="url">指定URL</param>
        /// <returns></returns>
        public static string LoadURLString(string url)
        {
            HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
            Stream stream = myWebResponse.GetResponseStream();

            string strResult = "";
            StreamReader sr = new StreamReader(stream, System.Text.Encoding.GetEncoding("gb2312"));
            Char[] read = new Char[256];
            int count = sr.Read(read, 0, 256);
            int i = 0;
            while (count > 0)
            {
                i += Encoding.UTF8.GetByteCount(read, 0, 256);
                String str = new String(read, 0, count);
                strResult += str;
                count = sr.Read(read, 0, 256);
            }

            return strResult;
        }


        /// <summary>
        /// 从指定的URL下载页面内容(使用WebClient)
        /// </summary>
        /// <param name="url">指定URL</param>
        /// <returns></returns>
        public static string LoadPageContent(string url)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultCredentials;
            byte[] pageData = wc.DownloadData(url);
            return (Encoding.GetEncoding("gb2312").GetString(pageData));
        }
        #endregion

        #region 远程服务器下载文件
        /// <summary>
        /// 远程提取文件保存至服务器上(使用WebRequest)
        /// </summary>
        /// <param name="url">一个URI上的文件</param>
        /// <param name="saveurl">文件保存在服务器上的全路径</param>
        public static void RemoteGetFile(string url, string saveurl)
        {
            HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
            Stream stream = myWebResponse.GetResponseStream();

            //获得请求的文件大小
            int fileSize = (int)myWebResponse.ContentLength;

            int bufferSize = 102400;
            byte[] buffer = new byte[bufferSize];
            FileHelper.WriteFile(saveurl, "temp");
            // 建立一个写入文件的流对象
            FileStream saveFile = File.Create(saveurl, bufferSize);
            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                saveFile.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            saveFile.Flush();
            saveFile.Close();
            stream.Flush();
            stream.Close();
        }

        /// <summary>
        /// 远程提取一个文件保存至服务器上(使用WebClient)
        /// </summary>
        /// <param name="url">一个URI上的文件</param>
        /// <param name="saveurl">保存文件</param>
        public static void WebClientGetFile(string url, string saveurl)
        {
            WebClient wc = new WebClient();

            try
            {
                FileHelper.WriteFile(saveurl, "temp");
                wc.DownloadFile(url, saveurl);
            }
            catch
            { }

            wc.Dispose();
        }

        /// <summary>
        /// 远程提取一组文件保存至服务器上(使用WebClient)
        /// </summary>
        /// <param name="urls">一组URI上的文件</param>
        /// <param name="saveurls">保存文件</param>
        public static void WebClientGetFile(string[] urls, string[] saveurls)
        {
            WebClient wc = new WebClient();
            for (int i = 0; i < urls.Length; i++)
            {
                try
                {
                    wc.DownloadFile(urls[i], saveurls[i]);
                }
                catch
                { }
            }
            wc.Dispose();
        }
        #endregion

        #region 文件上传
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="upfile">获取客户段上传的文件</param>
        /// <param name="limitType">允许上传的文件类型，null值为无限制</param>
        /// <param name="limitSize">上传文件的大小限制(0为无限制)</param>
        /// <param name="autoName">是否自动命名</param>
        /// <param name="savepath">上传文件的保存路径</param>
        /// <returns>string[]</returns>
        public static string[] UploadFile(HttpPostedFile upfile, string limitType, int limitSize, bool autoName, string savepath)
        {
            string[] strResult = new string[5];
            string[] extName = null;
            if (!Object.Equals(limitType, null) || Object.Equals(limitType, ""))
            {
                extName = FunctionHelper.SplitArray(limitType, ',');
            }

            int fileSize = upfile.ContentLength;								// 上传文件大小
            string fileClientName = upfile.FileName;							// 在客户端的文件全路径
            string fileFullName = Path.GetFileName(fileClientName);				// 上传文件名（包括后缀符）
            if (fileFullName == null || fileFullName == "")
            {
                strResult[0] = "无文件";
                strResult[1] = "";
                strResult[2] = "";
                strResult[3] = "";
                strResult[4] = "<font color=red>失败</font>";
                return strResult;
            }
            else
            {
                string fileType = upfile.ContentType;								// 上传文件的MIME类型
                string[] array = FunctionHelper.SplitArray(fileFullName, '.');
                string fileExt = array[array.Length - 1];							// 上传文件后缀符
                int fileNameLength = fileFullName.Length - fileExt.Length - 1;
                string fileName = "";												// 上传文件名（不包括后缀符）
                if (autoName)
                {
                    fileName = "_" + StringHelper.MakeName();
                }
                else
                {
                    fileName = fileFullName.Substring(0, fileNameLength);
                }


                string savename = fileName + "." + fileExt;
                strResult[0] = fileClientName;
                strResult[1] = savename;
                strResult[2] = fileType;
                strResult[3] = fileSize.ToString();
                bool EnableUpload = false;
                if (limitSize <= fileSize && limitSize != 0)
                {
                    strResult[4] = "<font color=red>失败</font>，上传文件过大";
                    EnableUpload = false;
                }
                else if (extName != null)
                {
                    for (int i = 0; i <= extName.Length - 1; i++)
                    {
                        if (string.Compare(fileExt, extName[i].ToString(), true) == 0)
                        {
                            EnableUpload = true;
                            break;
                        }
                        else
                        {
                            strResult[4] = "<font color=red>失败</font>，文件类型不允许上传";
                            EnableUpload = false;
                        }
                    }
                }
                else
                {
                    EnableUpload = true;
                }

                // 符合上传条件，开始执行上传文件操作。
                if (EnableUpload)
                {
                    try
                    {
                        string savefile = savepath + savename;
                        FileHelper.WriteFile(savefile, "临时文件");
                        upfile.SaveAs(savefile);
                        strResult[4] = "成功";
                        //strResult[4] = "成功<!--" + FunctionHelper.GetRealPath(savepath) + savename + "-->";
                    }
                    catch (Exception exc)
                    {
                        strResult[4] = "<font color=red>失败</font><!-- " + exc.Message + " -->";
                    }
                }

                return strResult;
            }
        }
        #endregion
    }
}
