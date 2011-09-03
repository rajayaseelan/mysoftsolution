using System.Web.UI.WebControls;

namespace MySoft.Web.UI.Controls
{
    public class RepeaterItem<TDataItem> : System.Web.UI.WebControls.RepeaterItem
    {
        public RepeaterItem(int itemIndex, ListItemType itemType)
            : base(itemIndex, itemType)
        {
        }

        public new TDataItem DataItem
        {
            get { return (TDataItem)base.DataItem; }
            set { base.DataItem = value; }
        }
    }
}