using System;
using System.Web.UI;

namespace MySoft.Web.UI
{
    /// <summary>
    /// AjaxPage 的摘要说明
    /// </summary>
    [Serializable]
    public class AjaxPage : MySoft.Web.UI.Page
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

        protected override void OnPreInit(EventArgs e)
        {
            AjaxRequestPage info = new AjaxRequestPage(this);
            info.EnableAjaxCallback = EnableAjaxCallback;
            info.EnableAjaxTemplate = EnableAjaxTemplate;

            //只有启用Ajax才处理
            if (info.EnableAjaxCallback)
            {
                AjaxRequest request = new AjaxRequest(info);
                request.SendRequest();
            }

            base.OnPreInit(e);
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            //base.VerifyRenderingInServerForm(control);
        }
    }
}