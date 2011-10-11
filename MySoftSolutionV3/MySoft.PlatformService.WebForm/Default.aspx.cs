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
        protected ServerStatus status;
        protected IList<ConnectionInfo> clients;
        //protected string perfValue;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    //var p = CastleFactory.Create().GetService<IStatusService>("bbb").GetProcessInfos(2532);
                    //StringBuilder sb = new StringBuilder();
                    //foreach (var a in p)
                    //{
                    //    sb.AppendFormat("{0} - {1} - {2} - {3} - {4} - {5}", a.Name, a.Id, a.Title, a.Path, a.WorkingSet, a.CpuUsage);
                    //    sb.Append("<br/>");
                    //}

                    //perfValue = sb.ToString();

                    status = CastleFactory.Create().GetService<IStatusService>().GetServerStatus();
                    clients = CastleFactory.Create().GetService<IStatusService>().GetConnectInfoList();
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