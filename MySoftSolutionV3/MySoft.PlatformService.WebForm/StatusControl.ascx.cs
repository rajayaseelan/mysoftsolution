using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySoft.IoC;
using MySoft.IoC.Status;

namespace MySoft.PlatformService.WebForm
{
    public partial class StatusControl : System.Web.UI.UserControl, MySoft.Web.UI.IAjaxProcessHandler
    {
        protected IList<RemoteNode> nodelist = new List<RemoteNode>();
        protected IDictionary<string, ServerStatus> statuslist = new Dictionary<string, ServerStatus>();
        protected IDictionary<string, IList<ClientInfo>> clientlist = new Dictionary<string, IList<ClientInfo>>();

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentIndex = 0;
            InitControl();
        }

        #region IAjaxProcessHandler 成员

        protected int CurrentIndex = 0;
        public void OnAjaxProcess(MySoft.Web.UI.CallbackParams callbackParams)
        {
            CurrentIndex = callbackParams["CurrentIndex"].To<int>();
            InitControl();
        }

        private void InitControl()
        {
            try
            {
                nodelist = CastleFactory.Create().GetRemoteNodes();
                foreach (var node in nodelist)
                {
                    var status = CastleFactory.Create().GetChannel<IStatusService>(node).GetServerStatus();
                    statuslist[node.Key] = status;

                    var clients = CastleFactory.Create().GetChannel<IStatusService>(node).GetClientList();
                    clientlist[node.Key] = clients;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}