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
    public partial class _Default : System.Web.UI.Page
    {
        protected static IList<ClientInfo> clients;
        protected static ServerStatus status;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    status = CastleFactory.Create().GetChannel<IStatusService>().GetServerStatus();
                    clients = CastleFactory.Create().GetChannel<IStatusService>().GetClientList();
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            CastleFactory.Create().GetChannel<IStatusService>().ClearServerStatus();
        }
    }
}