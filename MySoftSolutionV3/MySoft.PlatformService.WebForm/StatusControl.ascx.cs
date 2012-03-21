using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySoft.IoC;
using MySoft.IoC.Messages;

namespace MySoft.PlatformService.WebForm
{
    public partial class StatusControl : System.Web.UI.UserControl, MySoft.Web.UI.IAjaxProcessHandler
    {
        protected IList<ServerNode> nodelist = new List<ServerNode>();
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
                nodelist = CastleFactory.Create().GetServerNodes();
                foreach (var node in nodelist)
                {
                    var service = CastleFactory.Create().GetChannel<IStatusService>(node);
                    statuslist[node.Key] = service.GetServerStatus();
                    clientlist[node.Key] = service.GetClientList();
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