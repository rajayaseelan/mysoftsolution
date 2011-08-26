using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;

namespace MySoft.Web.UI.Controls
{
    [ToolboxData(@"<{0}:HtmlEditor runat=server></{0}:HtmlEditor>")]
    [Designer("Itfort.WebControls.HtmlEditorDesigner")]
    [DefaultProperty("Value")]
    [ValidationProperty("Value")]
    public class HtmlEditor : Control, IPostBackDataHandler
    {
        private string _Width;

        [DefaultValue("100%")]
        [Category("Layout")]
        public string Width
        {
            get
            {
                if (string.IsNullOrEmpty(_Width))
                {
                    return "100%";
                }
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }

        private string _Height;

        [DefaultValue("320px")]
        [Category("Layout")]
        public string Height
        {
            get
            {
                if (string.IsNullOrEmpty(_Height))
                {
                    return "320px";
                }
                return _Height;
            }
            set
            {
                _Height = value;
            }
        }

        private string _Path;

        [DefaultValue("/htmleditor/")]
        [Category("Appearance")]
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(_Path))
                {
                    return "/htmleditor/";
                }
                return _Path;
            }
            set
            {
                _Path = value;
            }
        }

        private string _Value = string.Empty;

        [DefaultValue("")]
        [Category("Appearance")]
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_Value))
                {
                    return string.Empty;
                }
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = postCollection[postDataKey];
            str = str.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
            if (str != this.Value)
            {
                this.Value = str;
                return true;
            }
            return false;
        }

        public void RaisePostDataChangedEvent()
        { }

        /// <summary>
        /// 呈现在客户端时生成的代码 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div>");
            writer.Write(Environment.NewLine);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\t<input type=\"hidden\" id=\"{0}\" name=\"{0}\" value=\"{1}\" />", this.ClientID, HttpUtility.HtmlEncode(this.Value));
            sb.Append(Environment.NewLine);
            sb.AppendFormat("\t<iframe id=\"{0}___Frame\" name=\"{0}___Frame\" src=\"{1}editor.htm\" width=\"{2}\" height=\"{3}\" frameborder=\"no\" scrolling=\"no\"></iframe>", this.ClientID, this.Path, this.Width, this.Height);

            writer.Write(sb.ToString());

            writer.Write(Environment.NewLine);
            writer.Write("</div>");

            base.Render(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            string key = "editorjs" + this.ClientID;
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(key))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(key, string.Format("{0}editor.js", this.Path));
            }

            string skey = "editor" + this.ClientID;
            if (!this.Page.ClientScript.IsStartupScriptRegistered(skey))
            {
                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(_Width))
                    sb.AppendFormat("var {0} = new HTMLEditor('{0}','{1}');", this.ClientID, this.Height);
                else
                    sb.AppendFormat("var {0} = new HTMLEditor('{0}','{1}','{2}');", this.ClientID, this.Width, this.Height);
                sb.Append(Environment.NewLine);
                sb.AppendFormat("var source{0} = document.getElementById('{0}');", this.ClientID);
                sb.Append(Environment.NewLine);
                sb.AppendFormat("{0}.setHTML(source{0}.value);", this.ClientID);
                sb.Append(Environment.NewLine);

                this.Page.ClientScript.RegisterStartupScript(this.GetType(), skey, sb.ToString(), true);
            }

            string mkey = "submit" + this.ClientID;
            if (!this.Page.ClientScript.IsOnSubmitStatementRegistered(mkey))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("var source{0} = document.getElementById('{0}');", this.ClientID);
                sb.Append(Environment.NewLine);
                sb.AppendFormat("source{0}.value = {0}.getHTML();", this.ClientID);
                sb.Append(Environment.NewLine);

                this.Page.ClientScript.RegisterOnSubmitStatement(this.GetType(), mkey, sb.ToString());
            }

            base.OnPreRender(e);
        }
    }
}
