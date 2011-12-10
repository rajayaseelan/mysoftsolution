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
        private static IStatusService service;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    if (service == null)
                    {
                        var listener = new StatusListener(ref status);
                        service = CastleFactory.Create().GetChannel<IStatusService>(listener);
                    }

                    clients = CastleFactory.Create().GetChannel<IStatusService>().GetClientInfoList();
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }
            }
        }

        public class StatusListener : IStatusListener
        {
            private ServerStatus status;
            public StatusListener(ref ServerStatus status)
            {
                this.status = status;
            }

            #region IStatusListener 成员

            public void Push(EndPoint endPoint, bool connected)
            {
                //throw new NotImplementedException();
            }

            public void Push(EndPoint endPoint, AppClient appClient)
            {
                //throw new NotImplementedException();
            }

            public void Push(CallError callError)
            {
                //throw new NotImplementedException();
            }

            public void Push(CallTimeout callTimeout)
            {
                //throw new NotImplementedException();
            }

            public void Push(ServerStatus serverStatus)
            {
                //throw new NotImplementedException();
                this.status = serverStatus;
            }

            #endregion
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            CastleFactory.Create().GetChannel<IStatusService>().ClearServerStatus();
        }
    }
}