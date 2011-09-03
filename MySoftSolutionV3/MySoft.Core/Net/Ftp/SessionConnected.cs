using System;
using System.IO;

namespace MySoft.Net.FTP
{
    internal class SessionConnected : SessionState
    {
        private FtpSession m_host;
        private ControlChannel m_ctrlChannel;
        private FtpDirectory m_root;
        private FtpDirectory m_current;
        private FtpDataStream m_dataStream;

        internal SessionConnected(FtpSession h, ControlChannel ctrl)
        {
            m_host = h;
            m_ctrlChannel = ctrl;
            m_ctrlChannel.Session = this;
        }
        internal void InitRootDirectory()
        {
            m_root = new FtpDirectory(this);
            m_current = m_root;
        }
        override public int Port
        {
            get { return m_ctrlChannel.Port; }
        }
        override public FtpDirectory RootDirectory
        {
            get { return m_root; }
        }
        override public FtpDirectory CurrentDirectory
        {
            get { return m_current; }
            set
            {
                m_ctrlChannel.CWD(value.FullName);
                m_current = value;
                m_current.ClearItems();
            }
        }
        override public bool IsBusy
        {
            get { return m_dataStream != null; }
        }

        internal FtpSession Host
        {
            get { return m_host; }
        }

        public override ControlChannel ControlChannel
        {
            get { return m_ctrlChannel; }
        }

        internal void BeginDataTransfer(FtpDataStream stream)
        {
            lock (this)
            {
                if (m_dataStream != null)
                    throw new FtpDataTransferException();
                m_dataStream = stream;
            }
        }
        internal void EndDataTransfer()
        {
            lock (this)
            {
                if (m_dataStream == null)
                    throw new InvalidOperationException();
                m_dataStream = null;
            }
        }
        // You can only aborting file transfer started by
        // BeginPutFile and BeginGetFile
        override public void AbortTransfer()
        {
            // Save a copy of m_dataStream since it will be set 
            // to null when FtpDataStream call EndDataTransfer
            FtpDataStream tempDataStream = m_dataStream;
            if (tempDataStream != null)
            {
                tempDataStream.Abort();
                while (!tempDataStream.IsClosed)
                    System.Threading.Thread.Sleep(0);
            }
        }

        override public void Close()
        {
            m_host.State = new SessionDisconnected(m_host);
            m_host.Server = m_ctrlChannel.Server;
            m_host.Port = m_ctrlChannel.Port;
            try
            {
                m_ctrlChannel.Quit();
            }
            catch (IOException)
            {
                return;
            }
            catch (FtpException)
            {
                return;
            }
            finally
            {
                m_ctrlChannel.Close();
            }
        }
    }
}