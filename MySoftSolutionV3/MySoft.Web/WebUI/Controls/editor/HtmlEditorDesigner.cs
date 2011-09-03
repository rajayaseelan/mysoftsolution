using System.Globalization;
using System.Web.UI.Design;

namespace MySoft.Web.UI.Controls
{
    public class HtmlEditorDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            HtmlEditor component = (HtmlEditor)base.Component;
            return string.Format(CultureInfo.InvariantCulture, "<div><table width=\"{0}\" height=\"{1}\" bgcolor=\"#f5f5f5\" bordercolor=\"#c7c7c7\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\"><tr><td valign=\"middle\" align=\"center\">HtmlEditor V1.0 - <b>{2}</b></td></tr></table></div>", new object[] { component.Width, component.Height, component.ID });
        }
    }
}
