using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySoft.PlatformService.UserService;
using MySoft.IoC;
using System.Net;

namespace MySoft.PlatformService.WebForm
{
    public partial class _Default : System.Web.UI.Page
    {
        protected ServerStatus status;
        protected IList<ConnectInfo> clients;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    status = CastleFactory.Create().GetService<IStatusService>("bbbb").GetServerStatus();
                    clients = CastleFactory.Create().GetService<IStatusService>("bbbb").GetConnectInfoList();
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            CastleFactory.Create().GetService<IStatusService>().ClearStatus();
        }
    }
}