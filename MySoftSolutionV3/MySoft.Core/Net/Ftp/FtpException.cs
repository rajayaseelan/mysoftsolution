using System;
using System.Runtime.InteropServices;

namespace MySoft.Net.FTP
{
    [ComVisible(false)]
    public class FtpException : Exception
    {
        private FtpResponse m_ftpResponse = null;

        internal FtpException(string message)
            : base(message)
        {
        }

        internal FtpException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal FtpException(string message, FtpResponse ftpResponse)
            : base(message)
        {
            m_ftpResponse = ftpResponse;
        }
        public string ResponseMessage
        {
            get
            {
                if (m_ftpResponse != null)
                    return m_ftpResponse.Message;
                else
                    return "";
            }
        }
        public override string Message
        {
            get { return base.Message; }
        }
    }

    [ComVisible(false)]
    public class FtpServerDownException : FtpException
    {
        internal FtpServerDownException(FtpResponse ftpResponse)
            : base("FTP service was down.", ftpResponse)
        {
        }
    }

    [ComVisible(false)]
    public class FtpDataTransferException : FtpException
    {
        internal FtpDataTransferException()
            : base("Data transfer error: pervious transfer not finished.")
        {
        }
    }

    internal class FtpUserAbortException : FtpException
    {
        internal FtpUserAbortException()
            : base("File Transfer aborted by user.")
        {
        }
    }
    [ComVisible(false)]
    public class FtpResumeNotSupportedException : FtpException
    {
        internal FtpResumeNotSupportedException(FtpResponse ftpResponse)
            : base("Data transfer error: server don't support resuming", ftpResponse)
        {
        }
    }

    /*
    public class FtpServerNotFoundException : System.ComponentModel.Win32Exception
    {
        private string		m_serverName;

        internal FtpServerNotFoundException(string serverName, string errMsg, int errorCode) : base(errorCode, errMsg)
        {
            m_serverName = serverName;
        }
        public string Server
        {
            get {return m_serverName;}
        }
    }

    public class FtpFileChangedException : Exception
    {
    }
    */
}
