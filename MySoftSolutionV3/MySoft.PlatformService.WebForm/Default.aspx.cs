using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySoft.PlatformService.UserService;
using MySoft.IoC;
using System.Net;
using MySoft.IoC.Status;
using System.Text;

namespace MySoft.PlatformService.WebForm
{
    public partial class Server : MySoft.Web.UI.AjaxPage
    {
        protected IList<RemoteNode> nodelist = new List<RemoteNode>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                nodelist = CastleFactory.Create().GetRemoteNodes();
            }
        }

        [MySoft.Web.UI.AjaxMethod]
        public void ClearServerStatus()
        {
            try
            {
                nodelist = CastleFactory.Create().GetRemoteNodes();
                foreach (var node in nodelist)
                {
                    CastleFactory.Create().GetChannel<IStatusService>(node).ClearServerStatus();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override bool EnableAjaxCallback
        {
            get
            {
                return true;
            }
        }
    }
}