using System;

namespace MySoft.Web.UI
{
    /// <summary>
    /// The MasterPage base class.
    /// </summary>
    public class MasterAjaxPage : MasterPage
    {
        #region 页面方法重写

        /// <summary>
        /// 是否启用Ajax回调功能
        /// </summary>
        protected virtual bool EnableAjaxCallback
        {
            get { return false; }
        }

        /// <summary>
        /// 是否启用Ajax模板处理功能
        /// </summary>
        protected virtual bool EnableAjaxTemplate
        {
            get { return false; }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            AjaxRequestPage info = new AjaxRequestPage(this.Page);
            info.EnableAjaxCallback = EnableAjaxCallback;
            info.EnableAjaxTemplate = EnableAjaxTemplate;

            //只有启用Ajax才处理
            if (info.EnableAjaxCallback)
            {
                AjaxRequest request = new AjaxRequest(info);
                request.SendRequest();
            }

            base.OnInit(e);
        }
    }
}
