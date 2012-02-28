
namespace MySoft.Net.Ftp
{
    internal class SessionDisconnected : SessionState
    {
        private FtpSession m_host;
        private string m_server;
        private int m_port;

        internal SessionDisconnected(FtpSession h)
        {
            m_port = 21;
            m_host = h;
        }

        override public string Server
        {
            set { m_server = value; }
            get { return m_server; }
        }

        override public int Port
        {
            set { m_port = value; }
            get { return m_port; }
        }

        override public void Connect(string user, string pass)
        {
            ControlChannel ctrl = new ControlChannel(m_host);
            ctrl.Server = m_server;
            ctrl.Port = m_port;
            ctrl.Connect();
            try
            {
                ctrl.FtpCommand("USER " + user);
                if (ctrl.LastResponse.Code == FtpResponse.UserAcceptedWaitingPass)
                    ctrl.FtpCommand("PASS " + pass);
                if (ctrl.LastResponse.Code != FtpResponse.UserLoggedIn)
                    throw new FtpException("Failed to login.", ctrl.LastResponse);
                m_host.State = new SessionConnected(m_host, ctrl);
                ((SessionConnected)m_host.State).InitRootDirectory();
            }
            catch
            {
                ctrl.Close();
                throw;
            }
        }
    }
}