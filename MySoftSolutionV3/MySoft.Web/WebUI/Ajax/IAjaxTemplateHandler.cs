using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MySoft.Web.UI
{
    /// <summary>
    /// 提供模板控件加载所需接口
    /// </summary>
    public interface IAjaxTemplateHandler
    {
        /// <summary>
        /// 模板读取事件
        /// </summary>
        /// <param name="callbackParams"></param>
        void OnAjaxTemplateRender(CallbackParams callbackParams);
    }

    /// <summary>
    /// 模板加载基类
    /// </summary>
    public abstract class AjaxTemplateControl : UserControl, IAjaxTemplateHandler
    {
        /// <summary>
        /// 模板数据内容
        /// </summary>
        public string TemplateContent { get; set; }

        #region IAjaxTemplateHandler 成员

        /// <summary>
        /// 模板读取事件
        /// </summary>
        /// <param name="callbackParams"></param>
        public void OnAjaxTemplateRender(CallbackParams callbackParams)
        {
            //读取模板及内容
            var content = GetAjaxTemplateContent(callbackParams);
            var data = GetAjaxTemplateData(callbackParams);

            //转换成json数据
            var jsonString = SerializationManager.SerializeJson(data);

            var obj = JObject.Parse(jsonString);
            obj["jst"] = content;

            //返回模板信息
            var templateContent = obj.ToString(Newtonsoft.Json.Formatting.Indented);
            this.TemplateContent = templateContent;
        }

        /// <summary>
        /// 读取控件内容
        /// </summary>
        /// <param name="controlPath"></param>
        /// <param name="callbackParams"></param>
        /// <returns></returns>
        protected string ReaderControl(string controlPath, CallbackParams callbackParams)
        {
            //控件路径
            controlPath = controlPath.ToLower().EndsWith(".ascx") ? controlPath : controlPath + ".ascx";

            //读取模板内容
            Control control = Page.LoadControl(controlPath);
            if (control != null)
            {
                if (control is IAjaxInitHandler)
                    (control as IAjaxInitHandler).OnAjaxInit(callbackParams);

                if (control is IAjaxProcessHandler)
                    (control as IAjaxProcessHandler).OnAjaxProcess(callbackParams);

                //处理模板信息
                StringBuilder sb = new StringBuilder();
                control.RenderControl(new HtmlTextWriter(new StringWriter(sb)));
                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <returns></returns>
        protected abstract string GetAjaxTemplateContent(CallbackParams callbackParams);

        /// <summary>
        /// 获取模板数据
        /// </summary>
        /// <param name="callbackParams"></param>
        /// <returns></returns>
        protected abstract object GetAjaxTemplateData(CallbackParams callbackParams);

        #endregion
    }
}
