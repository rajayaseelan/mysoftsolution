using System;
using System.Runtime.InteropServices;

namespace MySoft.Net.Ftp
{
    public delegate void FtpCommandEventHandler(object sender, FtpCommandEventArgs args);
    public delegate void FtpResponseEventHandler(object sender, FtpResponseEventArgs args);
    public delegate void FtpFileEventHandler(object sender, IFtpFileTransferArgs args);

    #region COM Interface IFtpSession
    public interface IFtpSession
    {
        string Server { get; set; }
        int Port { get; set; }
        FtpDirectory CurrentDirectory { get; set; }
        FtpDirectory RootDirectory { get; }
        bool IsBusy { get; }
        bool IsConnected { get; }
        void AbortTransfer();
        void Connect(string user, string pass);
        void Close();
    }
    #endregion
    #region COM Event Interface IFtpEvents
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IFtpEvents
    {
        void BeginPutFile(object sender, IFtpFileTransferArgs args);
        void EndPutFile(object sender, IFtpFileTransferArgs args);
        void BeginGetFile(object sender, IFtpFileTransferArgs args);
        void EndGetFile(object sender, IFtpFileTransferArgs args);
        void FileTransferProgress(object sender, IFtpFileTransferArgs args);
        void ResponseReceived(object sender, FtpResponseEventArgs args);
        void CommandSent(object sender, FtpCommandEventArgs args);
    }
    #endregion

    [
        Guid("0ED287BC-9F27-4c7e-931B-01AE0FC0BB02"),
        ComSourceInterfaces(typeof(IFtpEvents)),
        ClassInterface(ClassInterfaceType.None)
    ]
    public class FtpSession : IFtpSession
    {
        SessionState m_state;

        public FtpSession()
        {
            m_state = new SessionDisconnected(this);
        }

        public string Server
        {
            get { return m_state.Server; }
            set { m_state.Server = value; }
        }

        public int Port
        {
            set { m_state.Port = value; }
            get { return m_state.Port; }
        }

        public FtpDirectory CurrentDirectory
        {
            get { return m_state.CurrentDirectory; }
            set { m_state.CurrentDirectory = value; }
        }

        public FtpDirectory RootDirectory
        {
            get { return m_state.RootDirectory; }
        }

        public ControlChannel ControlChannel
        {
            get { return m_state.ControlChannel; }
        }

        public bool IsConnected
        {
            get { return m_state.GetType() == typeof(SessionConnected); }
        }
        public bool IsBusy
        {
            get { return m_state.IsBusy; }
        }

        public void AbortTransfer()
        {
            m_state.AbortTransfer();
        }

        public void Connect(string user, string pass)
        {
            m_state.Connect(user, pass);
        }

        public void Close()
        {
            m_state.Close();
        }

        internal SessionState State
        {
            set { m_state = value; }
            get { return m_state; }
        }

        public event FtpFileEventHandler BeginPutFile;
        public event FtpFileEventHandler EndPutFile;
        public event FtpFileEventHandler BeginGetFile;
        public event FtpFileEventHandler EndGetFile;
        public event FtpFileEventHandler FileTransferProgress;
        public event FtpResponseEventHandler ResponseReceived;
        public event FtpCommandEventHandler CommandSent;


        #region Itnernal raise event routines
        internal void RaiseResponseEvent(string response)
        {
            if (ResponseReceived != null)
                ResponseReceived(this, new FtpResponseEventArgs(response));
        }

        internal void RaiseCommandEvent(string command)
        {
            if (CommandSent != null)
                CommandSent(this, new FtpCommandEventArgs(command));
        }
        internal void RaiseBeginPutFileEvent(IFtpFileTransferArgs args)
        {
            if (BeginPutFile != null)
                BeginPutFile(this, args);
        }
        internal void RaiseEndPutFile(IFtpFileTransferArgs args)
        {
            if (EndPutFile != null)
                EndPutFile(this, args);
        }

        internal void RaiseBeginGetFileEvent(IFtpFileTransferArgs args)
        {
            if (BeginGetFile != null)
                BeginGetFile(this, args);
        }
        internal void RaiseEndGetFile(IFtpFileTransferArgs args)
        {
            if (EndGetFile != null)
                EndGetFile(this, args);
        }
        internal void RaiseFileTransferProgressEvent(IFtpFileTransferArgs args)
        {
            if (FileTransferProgress != null)
                FileTransferProgress(this, args);
        }
        #endregion
    }

    #region Ftp State base class
    internal class SessionState
    {
        public virtual IntPtr FtpHandle
        {
            get { throw new InvalidOperationException(); }
        }
        public virtual string Server
        {
            set { throw new InvalidOperationException(); }
            get { return ""; }
        }
        public virtual int Port
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }
        public virtual FtpDirectory CurrentDirectory
        {
            set { throw new InvalidOperationException(); }
            get { throw new InvalidOperationException(); }
        }
        public virtual FtpDirectory RootDirectory
        {
            get { throw new InvalidOperationException(); }
        }
        public virtual ControlChannel ControlChannel
        {
            get { throw new InvalidOperationException(); }
        }
        public virtual bool IsBusy
        {
            get { throw new InvalidOperationException(); }
        }
        public virtual void AbortTransfer()
        {
            throw new InvalidOperationException();
        }
        public virtual void Connect(string user, string pass)
        {
            throw new InvalidOperationException();
        }
        public virtual void Close()
        {
            //throw new InvalidOperationException();
        }
    }
    #endregion
}
