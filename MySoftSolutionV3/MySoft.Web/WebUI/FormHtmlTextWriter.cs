using System.IO;
using System.Web;

/// <summary>
/// FormHtmlTextWriter 的摘要说明
/// </summary>
internal class FormFixerHtml32TextWriter : System.Web.UI.Html32TextWriter
{
    private string _url; // 假的URL

    internal FormFixerHtml32TextWriter(TextWriter writer)
        : base(writer)
    {
        _url = HttpContext.Current.Request.RawUrl;
    }

    public override void WriteAttribute(string name, string value, bool encode)
    {
        // 如果当前输出的属性为form标记的action属性，则将其值替换为重写后的虚假URL
        if (_url != null && string.Compare(name, "action", true) == 0)
        {
            value = _url;
        }
        base.WriteAttribute(name, value, encode);
    }
}

internal class FormFixerHtmlTextWriter : System.Web.UI.HtmlTextWriter
{
    private string _url;
    internal FormFixerHtmlTextWriter(TextWriter writer)
        : base(writer)
    {
        _url = HttpContext.Current.Request.RawUrl;
    }

    public override void WriteAttribute(string name, string value, bool encode)
    {
        if (_url != null && string.Compare(name, "action", true) == 0)
        {
            value = _url;
        }

        base.WriteAttribute(name, value, encode);
    }
}


