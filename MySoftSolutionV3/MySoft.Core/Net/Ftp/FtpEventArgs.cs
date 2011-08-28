using System;
using System.Runtime.InteropServices;

namespace MySoft.Net.FTP
{

    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class FtpCommandEventArgs : EventArgs
    {
        string m_commandString;

        internal FtpCommandEventArgs(string command)
        {
            m_commandString = command;
        }
        public string CommandString
        {
            get { return m_commandString; }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class FtpResponseEventArgs : EventArgs
    {
        string m_responseString;

        internal FtpResponseEventArgs(string response)
        {
            m_responseString = response;
        }
        public string ResponseString
        {
            get { return m_responseString; }
        }
    }
}
