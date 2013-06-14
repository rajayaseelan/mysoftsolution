
/* 
 * 项目名称:	CustomService (自定义服务)
 * 版权所有:	海天人
 * 官方网站:	http://www.seaskyer.net/
 * 技术支持:	http://bbs.seaskyer.net/
 * 功能简介:    
 *				通过 HTTP 的 POST 方式传输数据，主要应用于少量数据的加密传输，用户身份验证，基本资料的添加和修改
 *			加密是采用 RSA 和 DES 相结合的方式，充分利用两者的优点互补，实现大量数据的快速加密和安全解密
 * 提供方法:
 *	Send();			向指定 Uri 地址以 POST 方式发送数据
 *
 *	Receive();		获取以 POST 方式发送过来的流数据和 Header 中的信息
 *
 *	GetResponseStream(); 发送方获取接收方返回的信息
 *
 *	Encrypt();	使用 DES 和 RSA 相结合的加密函数
 *
 *	Decrypt();	使用 DES 和 RSA 相结合的解密函数
 *
 *
 * 作者姓名:	怒容.Net
 * Email   :	hktkmaster@163.com
 * QQ号 码 :	17251920			QQ群号码 :	711255
 * 备    注:
 * 
 *		欢迎大家和我们交流、讨论。
 *		我们的宗旨是：让更多的人都能够使用免费、开源、可拓展的系统。
 * 
 *														海天工作室(Seasky Studio.)
 */


using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MySoft.Common
{
    /// <summary>
    /// PostService : 自定义服务
    /// </summary>
    public class PostService
    {
        HttpResponse Response = HttpContext.Current.Response;
        HttpRequest Request = HttpContext.Current.Request;
        //		HttpServerUtility	Server		= HttpContext.Current.Server;

        #region 发送和接收流信息

        #region 发送数据


        /// <summary>
        /// 向指定 Uri 地址以 POST 方式发送文本数据
        /// </summary>
        /// <param name="PostUrl">目标Uri地址</param>
        /// <param name="Content">要发送的文本内容</param>
        /// <returns>WebResponse</returns>
        public WebResponse Send(string PostUrl, string Content)
        {
            WebResponse res = null;

            try
            {
                WebRequest req = WebRequest.Create(PostUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                byte[] bytes = Encoding.UTF8.GetBytes(Content);
                req.ContentLength = bytes.Length;

                Stream newStream = req.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                res = req.GetResponse();

            }
            catch (Exception exc)
            {
                Response.Write(exc.ToString());
            }

            return res;
        }

        /// <summary>
        /// 向指定 Uri 地址以 POST 方式发送 byte[] 数据
        /// </summary>
        /// <param name="PostUrl">目标Uri地址</param>
        /// <param name="Content">要发送的文本内容</param>
        /// <returns>WebResponse</returns>
        public WebResponse Send(string PostUrl, byte[] Content)
        {
            WebResponse res = null;

            try
            {
                WebRequest req = WebRequest.Create(PostUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                byte[] bytes = Content;
                req.ContentLength = bytes.Length;

                Stream newStream = req.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                res = req.GetResponse();

            }
            catch (Exception exc)
            {
                Response.Write(exc.ToString());
            }

            return res;
        }

        /// <summary>
        /// 向指定 Uri 地址以 POST 方式发送文本数据
        /// </summary>
        /// <param name="PostUrl">目标Uri地址</param>
        /// <param name="Content">要发送的文本内容</param>
        /// <param name="headerCollection">要通过 Header 传送的数据集合</param>
        /// <returns>WebResponse</returns>
        public WebResponse Send(string PostUrl, string Content, Hashtable headerCollection)
        {
            WebResponse res = null;

            try
            {
                WebRequest req = WebRequest.Create(PostUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                // 添加数据到Header中
                IDictionaryEnumerator myEnumerator = headerCollection.GetEnumerator();

                while (myEnumerator.MoveNext())
                {
                    req.Headers.Add(myEnumerator.Key.ToString(), myEnumerator.Value.ToString());
                }

                byte[] bytes = Encoding.UTF8.GetBytes(Content);
                req.ContentLength = bytes.Length;

                Stream newStream = req.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                res = req.GetResponse();

            }
            catch (Exception exc)
            {
                Response.Write(exc.ToString());
            }

            return res;
        }

        /// <summary>
        /// 向指定 Uri 地址以 POST 方式发送二进制数据
        /// </summary>
        /// <param name="PostUrl">目标Uri地址</param>
        /// <param name="Content">要发送的二进制内容</param>
        /// <param name="headerCollection">要通过 Header 传送的数据集合</param>
        /// <returns>WebResponse</returns>
        public WebResponse Send(string PostUrl, byte[] Content, Hashtable headerCollection)
        {
            WebResponse res = null;

            try
            {

                WebRequest req = WebRequest.Create(PostUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                // 添加数据到Header中
                IDictionaryEnumerator myEnumerator = headerCollection.GetEnumerator();

                while (myEnumerator.MoveNext())
                {
                    req.Headers.Add(myEnumerator.Key.ToString(), myEnumerator.Value.ToString());
                }

                byte[] bytes = Content;
                req.ContentLength = bytes.Length;

                Stream newStream = req.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                res = req.GetResponse();
            }
            catch (Exception exc)
            {
                Response.Write(exc.ToString());
            }

            return res;
        }
        #endregion

        #region 接收数据
        /// <summary>
        /// 获取以 POST 方式发送过来的流数据和 Header 中的信息
        /// </summary>
        /// <param name="RsaDESSTRING">用于DES加密解密的 Key 和 IV 的集合</param>
        /// <returns>string</returns>
        public string ReceiveToString(out string RsaDESSTRING)
        {
            if (Request.RequestType != "POST")
            {
                RsaDESSTRING = "";
                return "";
            }

            try
            {
                Stream stream = Request.InputStream;
                string strResult = "";

                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                char[] read = new char[256];
                int count = sr.Read(read, 0, 256);
                int i = 0;
                while (count > 0)
                {
                    i += Encoding.UTF8.GetByteCount(read, 0, 256);
                    string str = new String(read, 0, count);
                    strResult += str;
                    count = sr.Read(read, 0, 256);
                }

                RsaDESSTRING = FunctionHelper.CheckValiable(Request.Headers["CS_DESSTRING"]) ? Request.Headers["CS_DESSTRING"] : "";
                return strResult;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }


        /// <summary>
        /// 获取以 POST 方式发送过来的流数据和 Header 中的信息
        /// </summary>
        /// <param name="RsaDESSTRING">用于DES加密解密的 Key 和 IV 的集合</param>
        /// <returns>byte[]</returns>
        public byte[] ReceiveToBytes(out string RsaDESSTRING)
        {
            if (Request.RequestType != "POST")
            {
                RsaDESSTRING = "";
                return null;
            }

            try
            {
                Stream stream = Request.InputStream;
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    byte[] buffer = new byte[(int)stream.Length];
                    stream.Write(buffer, 0, buffer.Length);
                    RsaDESSTRING = FunctionHelper.CheckValiable(Request.Headers["CS_DESSTRING"]) ? Request.Headers["CS_DESSTRING"] : "";

                    return buffer;
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        #endregion

        #region 发送方获取接收方返回的信息
        /// <summary>
        /// 发送方获取接收方返回的信息
        /// </summary>
        /// <param name="res">返回给发送方的 Response 对象</param>
        /// <returns>string</returns>
        public string GetResponseStream(WebResponse res)
        {
            string strResult = "";

            Stream ResponseStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(ResponseStream, Encoding.UTF8);

            Char[] read = new Char[256];

            // Read 256 charcters at a time.    
            int count = sr.Read(read, 0, 256);

            while (count > 0)
            {
                // Dump the 256 characters on a string and display the string onto the console.
                string str = new String(read, 0, count);
                strResult += str;
                count = sr.Read(read, 0, 256);
            }

            // 释放资源
            sr.Close();
            res.Close();

            return strResult;
        }
        #endregion

        #endregion

        #region 数据传输的加密/解密


        #region 数据加密
        /// <summary>
        /// 加密文本并返回 string
        /// </summary>
        /// <param name="Content">加密内容</param>
        /// <param name="publicKey">公钥(XML格式)</param>
        /// <param name="desKey">DES密钥</param>
        /// <param name="desIV">DES向量</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <returns>string</returns>
        public string EncryptString(string Content, string publicKey, string desKey, string desIV, out string rsaDes)
        {
            string strResult = "";

            if (FunctionHelper.CheckValiable(publicKey))
            {
                // DES加密内容
                DESCrypto DC = new DESCrypto();
                strResult = DC.EncryptString(Content, desKey, desIV);

                // 加密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = desKey + "§" + desIV;
                rsaDes = RC.RSAEncrypt(publicKey, des);
            }
            else
            {
                rsaDes = "";
                strResult = Content;
            }


            return strResult;
        }


        /// <summary>
        /// 加密文本并返回 byte[]
        /// </summary>
        /// <param name="Content">加密内容</param>
        /// <param name="publicKey">公钥(XML格式)</param>
        /// <param name="desKey">DES密钥</param>
        /// <param name="desIV">DES向量</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <returns>byte[]</returns>
        public byte[] EncryptBytes(string Content, string publicKey, string desKey, string desIV, out string rsaDes)
        {
            byte[] byteResult = null;

            if (FunctionHelper.CheckValiable(publicKey))
            {
                // DES加密内容
                DESCrypto DC = new DESCrypto();
                byteResult = DC.EncryptBytes(Content, desKey, desIV);

                // 加密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = desKey + "§" + desIV;
                rsaDes = RC.RSAEncrypt(publicKey, des);

            }
            else
            {
                rsaDes = "";
                byteResult = Encoding.UTF8.GetBytes(Content);
            }


            return byteResult;
        }


        /// <summary>
        /// 加密 byte[] 并返回 string
        /// </summary>
        /// <param name="Content">加密内容</param>
        /// <param name="publicKey">公钥(XML格式)</param>
        /// <param name="desKey">DES密钥</param>
        /// <param name="desIV">DES向量</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <returns>string</returns>
        public string EncryptString(byte[] Content, string publicKey, string desKey, string desIV, out string rsaDes)
        {
            string strResult = "";

            if (FunctionHelper.CheckValiable(publicKey))
            {
                // DES加密内容
                DESCrypto DC = new DESCrypto();
                strResult = DC.EncryptString(Content, desKey, desIV);

                // 加密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = desKey + "§" + desIV;
                rsaDes = RC.RSAEncrypt(publicKey, des);
            }
            else
            {
                rsaDes = "";
                strResult = Encoding.UTF8.GetString(Content);
            }


            return strResult;
        }


        /// <summary>
        /// 加密 byte[] 并返回 byte[]
        /// </summary>
        /// <param name="Content">加密内容</param>
        /// <param name="publicKey">公钥(XML格式)</param>
        /// <param name="desKey">DES密钥</param>
        /// <param name="desIV">DES向量</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <returns>byte[]</returns>
        public byte[] EncryptBytes(byte[] Content, string publicKey, string desKey, string desIV, out string rsaDes)
        {
            byte[] byteResult = null;

            if (FunctionHelper.CheckValiable(publicKey))
            {
                // DES加密内容
                DESCrypto DC = new DESCrypto();
                byteResult = DC.EncryptBytes(Content, desKey, desIV);

                // 加密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = desKey + "§" + desIV;
                rsaDes = RC.RSAEncrypt(publicKey, des);
            }
            else
            {
                rsaDes = "";
                byteResult = Content;
            }


            return byteResult;
        }
        #endregion


        #region 数据解密
        /// <summary>
        /// 解密函数
        /// </summary>
        /// <param name="Content">解密内容</param>
        /// <param name="privateKey">私钥(XML格式)</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <param name="desKey">经RSA解密后的desKey</param>
        /// <param name="desIV">经RSA解密后的desKey</param>
        /// <returns>string</returns>
        public string DecryptString(string Content, string privateKey, string rsaDes, out string desKey, out string desIV)
        {
            string strResult = "";

            if (FunctionHelper.CheckValiable(rsaDes))
            {
                // 解密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = RC.RSADecrypt(privateKey, rsaDes);

                string[] desArray = FunctionHelper.SplitArray(des, '§');

                desKey = desArray[0];
                desIV = desArray[1];


                // DES解密内容
                DESCrypto DC = new DESCrypto();
                strResult = DC.DecryptString(Content, desKey, desIV);
            }
            else
            {
                desKey = "";
                desIV = "";
                strResult = Content;
            }


            return strResult;
        }



        /// <summary>
        /// 解密函数
        /// </summary>
        /// <param name="Content">解密内容</param>
        /// <param name="privateKey">私钥(XML格式)</param>
        /// <param name="rsaDes">经RSA加密后的desKey与desIV的集合</param>
        /// <param name="desKey">经RSA解密后的desKey</param>
        /// <param name="desIV">经RSA解密后的desKey</param>
        /// <returns>byte[]</returns>
        public byte[] DecryptBytes(string Content, string privateKey, string rsaDes, out string desKey, out string desIV)
        {
            byte[] byteResult = null;

            if (FunctionHelper.CheckValiable(rsaDes))
            {
                // 解密DES密钥和初始化向量
                RSACrypto RC = new RSACrypto();

                string des = RC.RSADecrypt(privateKey, rsaDes);

                string[] desArray = FunctionHelper.SplitArray(des, '§');

                desKey = desArray[0];
                desIV = desArray[1];


                // DES解密内容
                DESCrypto DC = new DESCrypto();
                byteResult = DC.DecryptBytes(Content, desKey, desIV);

            }
            else
            {
                desKey = "";
                desIV = "";

                byteResult = Convert.FromBase64String(Content);
            }


            return byteResult;
        }
        #endregion


        #endregion
    }
}
