using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MySoft.Net.FTP
{
    public enum TransferMode
    {
        Ascii,
        Binary,
        Unknown
    }

    #region COM interface IControlChannel
    public interface IControlChannel
    {
        void FtpCommand(string command);
        FtpResponse LastResponse { get; }
    }
    #endregion

    [ClassInterface(ClassInterfaceType.None)]
    public class ControlChannel : IControlChannel
    {
        private FtpSession m_sessionHost;
        private SessionConnected m_session;
        private TcpClient m_connection;
        private string m_server;
        private int m_port;
        private TransferMode m_currentTransferMode;
        private FtpResponse m_lastResponse;

        #region Regular expression to handle ftp response
        private static Regex m_regularExpression = new Regex("(\\()(.*)(\\))");
        private static Regex m_pwdExpression = new Regex("(\")(.*)(\")");
        #endregion

        internal ControlChannel(FtpSession host)
        {
            m_connection = new TcpClient();
            m_server = "localhost";
            m_port = 21;
            m_sessionHost = host;
            m_currentTransferMode = TransferMode.Unknown;
        }

        internal string Server
        {
            get { return m_server; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Server", "Cannot set null to Server property.");
                m_server = value;
            }
        }
        internal int Port
        {
            get { return m_port; }
            set { m_port = value; }
        }

        public FtpResponse LastResponse
        {
            get { return m_lastResponse; }
        }
        internal void Connect()
        {
            m_connection.Connect(m_server, m_port);
            try
            {
                m_lastResponse = new FtpResponse(m_connection.GetStream());
                if (m_lastResponse.Code != FtpResponse.ServiceReady)
                    throw new FtpException("Ftp service unavailable.", m_lastResponse);
            }
            catch
            {
                Close();
                throw;
            }
        }
        internal void Close()
        {
            m_connection.Close();
            GC.SuppressFinalize(this);
        }

        ~ControlChannel()
        {
            m_connection.Close();
        }

        public void FtpCommand(string cmd)
        {
            m_sessionHost.RaiseCommandEvent(cmd);


            byte[] buff = System.Text.Encoding.Default.GetBytes(cmd + "\r\n");
            Stream stream = m_connection.GetStream();

            lock (this)
            {
                stream.Write(buff, 0, buff.Length);
                m_lastResponse = new FtpResponse(stream);
            }
            foreach (string s in m_lastResponse.Respones)
                m_sessionHost.RaiseResponseEvent(s);

        }

        public void RefreshResponse()
        {
            lock (this)
                m_lastResponse = new FtpResponse(m_connection.GetStream());
            foreach (string s in m_lastResponse.Respones)
                m_sessionHost.RaiseResponseEvent(s);

        }

        internal SessionConnected Session
        {
            get { return m_session; }
            set { m_session = value; }
        }

        internal void REST(long offset)
        {
            FtpCommand("REST " + offset);
            if (m_lastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpResumeNotSupportedException(m_lastResponse);
        }

        internal void STOR(string name)
        {
            Type(TransferMode.Binary);
            FtpCommand("STOR " + name);
            if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart
                && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpException("Failed to send file " + name, m_lastResponse);
        }

        internal void RETR(string name)
        {
            Type(TransferMode.Binary);
            FtpCommand("RETR " + name);
            if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart
                && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                throw new FtpException("Failed to retrive file " + name, m_lastResponse);
        }

        internal void DELE(string fileName)
        {
            FtpCommand("DELE " + fileName);
            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)// 250)
                throw new FtpException("Failed to delete file " + fileName, m_lastResponse);
        }

        internal void RMD(string dirName)
        {
            FtpCommand("RMD " + dirName);
            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)// 250)
                throw new FtpException("Failed to subdirectory " + dirName, m_lastResponse);
        }

        internal void MKD(string dirName)
        {
            FtpCommand("MKD " + dirName);
            if (m_lastResponse.Code != 257)// 257)
                throw new FtpException("Failed to subdirectory " + dirName, m_lastResponse);
        }

        internal string PWD()
        {
            FtpCommand("PWD");
            if (m_lastResponse.Code != 257)
                throw new FtpException("Cannot print(get) current directory.", m_lastResponse);
            Match m = m_pwdExpression.Match(m_lastResponse.Message);
            return m.Groups[2].Value;
        }
        internal void CDUp()
        {
            FtpCommand("CDUP");
            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpException("Cannot move to parent directory(CDUP).", m_lastResponse);
        }
        internal void CWD(string path)
        {
            FtpCommand("CWD " + path);
            if (m_lastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpException("Cannot change directory to " + path + ".", m_lastResponse);
        }
        internal void Quit()
        {
            FtpCommand("QUIT");
        }
        internal void Type(TransferMode mode)
        {
            if (mode == TransferMode.Unknown)
                return;
            if (mode == TransferMode.Ascii && m_currentTransferMode != TransferMode.Ascii)
                FtpCommand("TYPE A");
            else if (mode == TransferMode.Binary && m_currentTransferMode != TransferMode.Binary)
                FtpCommand("TYPE I");
            m_currentTransferMode = mode;
        }
        internal void Rename(string newName, string oldName)
        {
            FtpCommand("RNFR " + oldName);
            if (LastResponse.Code != FtpResponse.RequestFileActionPending)
                throw new FtpException("Failed to reanme file from " + oldName + " to " + newName);
            FtpCommand("RNTO " + newName);
            if (LastResponse.Code != FtpResponse.RequestFileActionComplete)
                throw new FtpException("Failed to reanme file from " + oldName + " to " + newName);

        }
        internal Queue List(bool passive)
        {
            const string errorMsgListing = "Error when listing server directory.";

            try
            {
                Type(TransferMode.Ascii);
                FtpDataStream dataStream = GetPassiveDataStream();

                Queue lineQueue = new Queue();
                FtpCommand("LIST");
                if (m_lastResponse.Code != FtpResponse.DataChannelOpenedTransferStart
                    && m_lastResponse.Code != FtpResponse.FileOkBeginOpenDataChannel)
                    throw new FtpException(errorMsgListing, m_lastResponse);

                StreamReader lineReader = new StreamReader(dataStream, System.Text.Encoding.Default);

                string line;
                while ((line = lineReader.ReadLine()) != null)
                    lineQueue.Enqueue(line);

                lineReader.Close();
                if (m_lastResponse.Code != FtpResponse.ClosingDataChannel)
                    throw new FtpException(errorMsgListing, m_lastResponse);

                return lineQueue;
            }
            catch (IOException ie)
            {
                throw new FtpException(errorMsgListing, ie);
            }
            catch (SocketException se)
            {
                throw new FtpException(errorMsgListing, se);
            }
        }

        internal FtpDataStream GetPassiveDataStream()
        {
            return GetPassiveDataStream(TransferDirection.Download);
        }

        internal FtpDataStream GetPassiveDataStream(TransferDirection direction)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(m_server, GetPassivePort());
                if (direction == TransferDirection.Download)
                    return new FtpInputDataStream(this, client);
                else
                    return new FtpOutputDataStream(this, client);
            }
            catch (IOException ie)
            {
                throw new FtpException("Failed to open data connecton.", ie);
            }
            catch (SocketException se)
            {
                throw new FtpException("Failed to open data connecton.", se);
            }
        }

        private int GetPassivePort()
        {
            FtpCommand("PASV");
            if (m_lastResponse.Code == FtpResponse.EnterPassiveMode)
            {
                string[] numbers = m_regularExpression.
                    Match(m_lastResponse.Message).
                    Groups[2].
                    Value.
                    Split(',');
                return int.Parse(numbers[4]) * 256 + int.Parse(numbers[5]);
            }
            else
                throw new FtpException("Failed to enter passive mode.", m_lastResponse);
        }
    }
}
