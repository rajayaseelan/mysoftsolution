using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using MySoft.Web.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MySoft.Web.UI
{
    /// <summary>
    /// Ajax相关信息
    /// </summary>
    public class AjaxRequestPage
    {
        public System.Web.UI.Page CurrentPage { get; private set; }
        public bool EnableAjaxCallback { get; set; }
        public bool EnableAjaxTemplate { get; set; }

        public AjaxRequestPage(System.Web.UI.Page currentPage)
        {
            this.CurrentPage = currentPage;
        }
    }

    /// <summary>
    /// Ajax调用类
    /// </summary>
    public class AjaxRequest
    {
        private AjaxRequestPage info;
        public AjaxRequest(AjaxRequestPage info)
        {
            this.info = info;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        public void SendRequest()
        {
            try
            {
                bool AjaxProcess = WebHelper.GetRequestParam<bool>(info.CurrentPage.Request, "X-Ajax-Process", false);
                if (!AjaxProcess)
                {
                    WebHelper.RegisterPageCssFile(info.CurrentPage, info.CurrentPage.ClientScript.GetWebResourceUrl(typeof(AjaxPage), "MySoft.Web.Resources.pager.css"));

                    //需要启用模板加载
                    if (info.EnableAjaxTemplate)
                    {
                        WebHelper.RegisterPageJsFile(info.CurrentPage, info.CurrentPage.ClientScript.GetWebResourceUrl(typeof(AjaxPage), "MySoft.Web.Resources.template.js"));
                    }

                    WebHelper.RegisterPageForAjax(info.CurrentPage, info.CurrentPage.Request.Path);
                }
                else
                {
                    var callbackParams = GetCallbackParams();

                    //只有启用Ajax，才调用初始化方法
                    if (info.CurrentPage is IAjaxInitHandler)
                        (info.CurrentPage as IAjaxInitHandler).OnAjaxInit(callbackParams);

                    if (info.CurrentPage is IAjaxProcessHandler)
                        (info.CurrentPage as IAjaxProcessHandler).OnAjaxProcess(callbackParams);

                    bool AjaxRegister = WebHelper.GetRequestParam<bool>(info.CurrentPage.Request, "X-Ajax-Register", false);
                    bool AjaxRequest = WebHelper.GetRequestParam<bool>(info.CurrentPage.Request, "X-Ajax-Request", false);
                    bool AjaxLoad = WebHelper.GetRequestParam<bool>(info.CurrentPage.Request, "X-Ajax-Load", false);
                    string AjaxKey = WebHelper.GetRequestParam<string>(info.CurrentPage.Request, "X-Ajax-Key", Guid.NewGuid().ToString());

                    if (AjaxRegister)
                    {
                        //将value写入Response流
                        WriteAjaxMethods(info.CurrentPage.GetType());
                    }
                    else if (AjaxRequest)
                    {
                        string AjaxMethodName = WebHelper.GetRequestParam<string>(info.CurrentPage.Request, "X-Ajax-Method", null);

                        if (CheckHeader(AjaxKey))
                        {
                            AjaxCallbackParam value = InvokeMethod(info.CurrentPage, AjaxMethodName);

                            //将value写入Response流
                            WriteToBuffer(value);
                        }
                        else
                            throw new AjaxException("Method \"" + AjaxMethodName + "\" Is Invoke Error！");
                    }
                    else if (AjaxLoad)
                    {
                        string AjaxControlPath = WebHelper.GetRequestParam<string>(info.CurrentPage.Request, "X-Ajax-Path", null);
                        bool UseAjaxTemplate = WebHelper.GetRequestParam<bool>(info.CurrentPage.Request, "X-Ajax-Template", false);

                        if (CheckHeader(AjaxKey))
                        {
                            AjaxCallbackParam param = LoadAjaxControl(AjaxControlPath, UseAjaxTemplate);

                            //将param写入Response流
                            WriteToBuffer(param);
                        }
                        else
                            throw new AjaxException("Control \"" + AjaxControlPath + "\" Is Load Error！");
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (AjaxException ex)
            {
                WriteErrorMessage("AjaxError: " + ex.Message);
            }
            catch (BusinessException ex)
            {
                WriteErrorMessage(string.Format("[{0}]{1}", ex.Code, ex.Message));
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
            }
        }

        private void WriteErrorMessage(string message)
        {
            AjaxCallbackParam param = new AjaxCallbackParam(message);
            param.Success = false;

            WriteToBuffer(param);
        }

        #region Ajax类的处理

        private void WriteAjaxMethods(Type ajaxType)
        {
            Dictionary<string, AsyncMethodInfo> ajaxMethods = AjaxMethodHelper.GetAjaxMethods(ajaxType);
            List<AjaxMethodInfo> methodInfoList = new List<AjaxMethodInfo>();
            List<string> paramList = new List<string>();
            foreach (string key in ajaxMethods.Keys)
            {
                paramList.Clear();
                AjaxMethodInfo methodInfo = new AjaxMethodInfo();
                methodInfo.Name = key;
                foreach (ParameterInfo pi in ajaxMethods[key].Method.GetParameters())
                {
                    paramList.Add(pi.Name);
                }

                methodInfo.Async = ajaxMethods[key].Async;
                methodInfo.Paramters = paramList.ToArray();
                methodInfoList.Add(methodInfo);
            }

            WriteToBuffer(methodInfoList.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invokeObject"></param>
        /// <param name="MethodName"></param>
        /// <returns></returns>
        private AjaxCallbackParam InvokeMethod(object invokeObject, string MethodName)
        {
            var ajaxMethods = AjaxMethodHelper.GetAjaxMethods(invokeObject.GetType());
            if (ajaxMethods.ContainsKey(MethodName))
            {
                var parameters = ajaxMethods[MethodName].Method.GetParameters();
                var plist = new List<object>();
                foreach (var p in parameters)
                {
                    object obj = GetObject(p.ParameterType, p.Name);
                    plist.Add(obj);
                }

                var method = ajaxMethods[MethodName].Method;
                object value = method.FastInvoke(invokeObject, plist.ToArray());
                return new AjaxCallbackParam(value);
            }
            else
            {
                throw new AjaxException(string.Format("未找到服务器端方法{0}！", MethodName));
            }
        }

        /// <summary>
        /// 加载控件返回String
        /// </summary>
        /// <param name="controlPath"></param>
        /// <returns></returns>
        private AjaxCallbackParam LoadAjaxControl(string controlPath)
        {
            return LoadAjaxControl(controlPath, false);
        }

        /// <summary>
        /// 加载控件返回String
        /// </summary>
        /// <param name="controlPath"></param>
        /// <param name="useTemplate"></param>
        /// <returns></returns>
        private AjaxCallbackParam LoadAjaxControl(string controlPath, bool useTemplate)
        {
            string path = controlPath.ToLower().EndsWith(".ascx") ? controlPath : controlPath + ".ascx";

            //从缓存读取数据
            string html = GetCache(path, info.CurrentPage.Request.Form.ToString());
            if (html == null)
            {
                Control control = info.CurrentPage.LoadControl(path);
                if (control != null)
                {
                    var callbackParams = GetCallbackParams();

                    if (control is IAjaxInitHandler)
                        (control as IAjaxInitHandler).OnAjaxInit(callbackParams);

                    if (control is IAjaxProcessHandler)
                        (control as IAjaxProcessHandler).OnAjaxProcess(callbackParams);

                    //判断是否启用模板
                    if (info.EnableAjaxTemplate && useTemplate)
                    {
                        //模板控件必须继承自AjaxTemplateControl
                        if (control is IAjaxTemplateHandler)
                            (control as IAjaxTemplateHandler).OnAjaxTemplateRender(callbackParams);
                    }

                    StringBuilder sb = new StringBuilder();
                    control.RenderControl(new HtmlTextWriter(new StringWriter(sb)));
                    html = sb.ToString();

                    //将数据放入缓存
                    SetCache(path, info.CurrentPage.Request.Form.ToString(), html);
                }
            }

            return new AjaxCallbackParam(html);
        }

        private bool CheckHeader(string AjaxKey)
        {
            string ajaxKey = "AjaxProcess";
            bool ret = AjaxKey == WebHelper.MD5Encrypt(ajaxKey);

            return ret;
        }

        /// <summary>
        /// 获取控件的cache
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string GetCache(string path, string parameter)
        {
            string key = path + (parameter != string.Empty ? "?" + parameter : null);
            key = "Cache_Control_" + key.ToLower();

            var config = CacheControlConfiguration.GetConfig();

            //判断config配置信息
            if (config != null && config.Enabled)
            {
                return CacheHelper.Get<string>(key);
            }

            return null;
        }

        /// <summary>
        /// 设置控件的cache
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameter"></param>
        /// <param name="html"></param>
        private void SetCache(string path, string parameter, string html)
        {
            if (html == null) return;
            string key = path + (parameter != string.Empty ? "?" + parameter : null);
            key = "Cache_Control_" + key.ToLower();

            var config = CacheControlConfiguration.GetConfig();

            //判断config配置信息
            if (config != null && config.Enabled)
            {
                for (int index = 0; index < config.Rules.Count; index++)
                {
                    var rule = config.Rules[index];
                    if (rule.Path.ToLower().Contains(path.ToLower()))
                    {
                        CacheHelper.Insert(key, html, null, rule.Timeout);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取页面参数
        /// </summary>
        private CallbackParams GetCallbackParams()
        {
            CallbackParams callbackParams = new CallbackParams();
            NameValueCollection eventArgument = info.CurrentPage.Request.Form;
            if (eventArgument.Count > 0)
            {
                string[] keys = eventArgument.AllKeys;
                foreach (string key in keys)
                {
                    callbackParams[key] = new CallbackParam(eventArgument[key]);
                }
            }
            return callbackParams;
        }
        #endregion

        #region 将数据写入页面流中

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramsKey">传入key值获取对象</param>
        /// <returns></returns>
        private T GetObject<T>(string paramsKey)
        {
            return WebHelper.StrongTyped<T>(GetObject(typeof(T), paramsKey));
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        private object GetObject(Type type, string paramsKey)
        {
            string jsonString = WebHelper.GetRequestParam<string>(info.CurrentPage.Request, paramsKey, "");
            return SerializationManager.DeserializeJson(type, jsonString);
        }

        /// <summary>
        /// 将数据写入页面流
        /// </summary>
        /// <param name="param"></param>
        private void WriteToBuffer(AjaxCallbackParam param)
        {
            try
            {
                info.CurrentPage.Response.Clear();

                if (param != null)
                    info.CurrentPage.Response.Write(SerializationManager.SerializeJson(param));
                else
                    info.CurrentPage.Response.ContentType = "image/gif";

                info.CurrentPage.Response.Cache.SetNoStore();
                info.CurrentPage.Response.Flush();
                //info.CurrentPage.Response.End();

                //完成请求
                info.CurrentPage.Response.Close();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch
            {
                //不处理异常
            }
        }

        /// <summary>
        /// 将数据写入页面流
        /// </summary>
        /// <param name="methods"></param>
        private void WriteToBuffer(AjaxMethodInfo[] methods)
        {
            try
            {
                info.CurrentPage.Response.Clear();
                info.CurrentPage.Response.Write(SerializationManager.SerializeJson(methods));
                info.CurrentPage.Response.Cache.SetNoStore();
                info.CurrentPage.Response.Flush();
                //info.CurrentPage.Response.End();

                //完成请求
                info.CurrentPage.Response.Close();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch
            {
                //不处理异常
            }
        }

        #endregion
    }
}
