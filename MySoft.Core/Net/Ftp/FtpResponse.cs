using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace MySoft.Net.Ftp
{
    #region COM interface IFtpResponse
    public interface IFtpResponse
    {
        string Message { get; }
        int Code { get; }
    }
    #endregion

    [ClassInterface(ClassInterfaceType.None)]
    public class FtpResponse : IFtpResponse
    {
        private readonly Queue m_responses;
        private readonly int m_code;

        public const int InvalidCode = -1;
        public const int DataChannelOpenedTransferStart = 125;
        public const int FileOkBeginOpenDataChannel = 150;
        public const int ServiceReady = 220;
        public const int ClosingDataChannel = 226;
        public const int EnterPassiveMode = 227;
        public const int RequestFileActionComplete = 250;
        public const int UserLoggedIn = 230;
        public const int UserAcceptedWaitingPass = 331;
        public const int RequestFileActionPending = 350;
        public const int ServiceUnavailable = 421;
        public const int TransferAborted = 426;


        internal FtpResponse(Stream m)
        {
            TextReader reader = new StreamReader(m);
            m_responses = new Queue();
            while (true)
            {
                string response = GetLine(reader);
                try
                {
                    m_code = InvalidCode;
                    m_code = int.Parse(response.Substring(0, 3));
                }
                catch
                {
                    throw new FtpException("Invalid response", this);
                }
                m_responses.Enqueue(response);
                if (response.Length >= 4 && response[3] == '-')
                    continue;
                break;
            }
            if (m_code == ServiceUnavailable)
                throw new FtpServerDownException(this);
        }

        public string Message
        {
            get { return (string)m_responses.Peek(); }
        }

        public Queue Respones
        {
            get { return m_responses; }
        }

        public int Code
        {
            get { return m_code; }
        }

        private char ReadAppendChar(TextReader r, System.Text.StringBuilder toAppend)
        {
            char c = (char)r.Read();
            toAppend.Append(c);
            return c;
        }

        private string GetLine(TextReader reader)
        {
            System.Text.StringBuilder buff = new System.Text.StringBuilder();

            while (true)
            {
                while (ReadAppendChar(reader, buff) != '\r') ;
                while (ReadAppendChar(reader, buff) == '\r') ;
                if (buff[buff.Length - 1] == '\n')
                    break;
            }
            return buff.ToString();
        }
    }
}
