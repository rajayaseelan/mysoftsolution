using System.IO;
using System.Web;

/// <summary>
/// Html32TextWriter重写
/// </summary>
public class FormFixerHtml32TextWriter : System.Web.UI.Html32TextWriter
{
    public FormFixerHtml32TextWriter(TextWriter writer)
        : base(writer)
    {
    }

    public override void WriteAttribute(string name, string value, bool encode)
    {
        // 如果当前输出的属性为form标记的action属性，则将其值替换为重写后的虚假URL  
        if (string.Compare(name, "action", true) == 0)
        {
            value = HttpContext.Current.Request.RawUrl;
        }
        base.WriteAttribute(name, value, encode);
    }
}

/// <summary>
/// HtmlTextWriter重写
/// </summary>
public class FormFixerHtmlTextWriter : System.Web.UI.HtmlTextWriter
{
    public FormFixerHtmlTextWriter(TextWriter writer)
        : base(writer)
    {
    }

    public override void WriteAttribute(string name, string value, bool encode)
    {
        if (string.Compare(name, "action", true) == 0)
        {
            value = HttpContext.Current.Request.RawUrl;
        }

        base.WriteAttribute(name, value, encode);
    }
}


