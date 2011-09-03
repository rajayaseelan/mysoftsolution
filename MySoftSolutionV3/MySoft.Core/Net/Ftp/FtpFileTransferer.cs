using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MySoft.Net.FTP
{
    public enum TransferDirection
    {
        Upload,
        Download
    }

    public interface IFtpFileTransferArgs
    {
        string LocalFileName { get; }
        string RemoteFileName { get; }
        long TotalBytes { get; }
        long TotalBytesTransfered { get; }
        int TransferedPercentage { get; }
        TransferDirection TransferDirection { get; }
        IFtpAsyncResult TransferResult { get; }
    }
    internal class FtpFileTransferer : IFtpFileTransferArgs
    {
        #region Delegates used to bind routines
        private delegate void FileEventDelegate(IFtpFileTransferArgs args);
        private delegate void FtpDelegate(string remoteFileName);
        private delegate void StreamCopyDelegate(Stream remote, Stream local);
        #endregion
        #region dynamic binded routines for upload and download
        private FileEventDelegate m_beginEvent;
        private FileEventDelegate m_endEvent;
        private StreamCopyDelegate m_streamCopyRoutine;
        private FtpDelegate m_ftpFileCommandRoutine;
        #endregion
        private FtpFileEventHandler m_callback;
        private FtpDirectory m_transferStarter;
        private SessionConnected m_session;
        private string m_localFile;
        private string m_remoteFile;
        private long m_totalBytes;
        private long m_totalBytesTransfered;
        private int m_transferedPercentage;
        private TransferDirection m_transferDirection;
        private FileMode m_localFileOpenMode;
        private FtpAsyncResult m_transferResult;

        public string LocalFileName
        {
            get { return m_localFile; }
        }
        public string RemoteFileName
        {
            get { return m_remoteFile; }
        }
        public long TotalBytes
        {
            get { return m_totalBytes; }
        }
        public long TotalBytesTransfered
        {
            get { return m_totalBytesTransfered; }
        }
        public TransferDirection TransferDirection
        {
            get { return m_transferDirection; }
        }
        public IFtpAsyncResult TransferResult
        {
            get { return m_transferResult; }
        }
        public int TransferedPercentage
        {
            get { return m_transferedPercentage; }
        }

        internal FtpFileTransferer(FtpDirectory transferStarter, string localFile, string remoteFile, long totalBytes, TransferDirection dir)
        {
            m_transferStarter = transferStarter;
            m_transferDirection = dir;
            m_session = transferStarter.FtpSession;
            m_localFile = localFile;
            m_remoteFile = remoteFile;
            m_totalBytes = totalBytes;

            if (dir == TransferDirection.Upload)
            {
                m_beginEvent = new FileEventDelegate(m_session.Host.RaiseBeginPutFileEvent);
                m_endEvent = new FileEventDelegate(m_session.Host.RaiseEndPutFile);
                m_streamCopyRoutine = new StreamCopyDelegate(LocalToRemote);
                m_ftpFileCommandRoutine = new FtpDelegate(m_session.ControlChannel.STOR);
                m_localFileOpenMode = FileMode.Open;
            }
            else
            {
                m_beginEvent = new FileEventDelegate(m_session.Host.RaiseBeginGetFileEvent);
                m_endEvent = new FileEventDelegate(m_session.Host.RaiseEndGetFile);
                m_streamCopyRoutine = new StreamCopyDelegate(RemoteToLocal);
                m_ftpFileCommandRoutine = new FtpDelegate(m_session.ControlChannel.RETR);
                m_localFileOpenMode = FileMode.Create;
            }
        }

        private void TransferThreadProc()
        {
            try
            {
                StartTransfer();
                CallCallback("Success.", FtpAsyncResult.Complete);
            }
            catch (IOException ioe)
            {
                CallCallback("Transfer fail: " + ioe.Message, FtpAsyncResult.Fail);
            }
            catch (FtpUserAbortException fae)
            {
                CallCallback(fae.Message, FtpAsyncResult.Abort);
            }
            catch (FtpException fe)
            {
                CallCallback("Transfer fail: " + fe.Message, FtpAsyncResult.Fail);
            }
            catch (SocketException se)
            {
                CallCallback("Transfer fail: " + se.Message, FtpAsyncResult.Fail);
            }
            catch (Exception e)
            {
                CallCallback("Transfer fail: " + e.Message, FtpAsyncResult.Fail);
            }
        }
        internal void StartTransfer()
        {
            FileStream localStream = null;
            FtpDataStream remoteStream = null;
            try
            {
                m_beginEvent(this);
                localStream = new FileStream(m_localFile, m_localFileOpenMode);
                remoteStream = m_session.ControlChannel.GetPassiveDataStream(m_transferDirection);

                m_ftpFileCommandRoutine(m_remoteFile);
                m_streamCopyRoutine(remoteStream, localStream);

                remoteStream.Close();
                TestTransferResult();

                m_endEvent(this);
            }
            catch (FtpUserAbortException)
            {
                remoteStream.Close();
                throw;
            }
            catch (Exception)
            {
                m_endEvent(this);
                throw;
            }
            finally
            {
                if (remoteStream != null)
                    remoteStream.Dispose();
                if (localStream != null)
                    localStream.Close();
            }
        }

        internal void StartAsyncTransfer(FtpFileEventHandler callback)
        {
            m_callback = callback;
            Thread thread = new Thread(new ThreadStart(TransferThreadProc));
            thread.Name = "Transfer file thread: " + m_remoteFile;
            thread.Start();
        }
        private void CallCallback(string message, int transferResult)
        {
            m_transferResult = new FtpAsyncResult(message, transferResult);
            if (m_callback != null)
                m_callback(m_transferStarter, this);
        }
        private void TestTransferResult()
        {
            int responseCode = m_session.ControlChannel.LastResponse.Code;
            if (responseCode == FtpResponse.ClosingDataChannel)
                return;
            if (responseCode == FtpResponse.RequestFileActionComplete)
                return;
            throw new FtpException("Failed to transfer file.", m_session.ControlChannel.LastResponse);
        }

        private void RemoteToLocal(Stream remote, Stream local)
        {
            StreamCopy(local, remote);
        }
        private void LocalToRemote(Stream remote, Stream local)
        {
            StreamCopy(remote, local);
        }
        private void StreamCopy(Stream dest, Stream source)
        {
            int byteRead;
            long onePercentage, bytesReadFromLastProgressEvent;

            onePercentage = m_totalBytes / 100;
            bytesReadFromLastProgressEvent = 0;
            byte[] buffer = new byte[4 * 1024];
            while ((byteRead = source.Read(buffer, 0, 4 * 1024)) != 0)
            {
                m_totalBytesTransfered += byteRead;
                bytesReadFromLastProgressEvent += byteRead;
                if (bytesReadFromLastProgressEvent > onePercentage)
                {
                    m_transferedPercentage = (int)((float)m_totalBytesTransfered / (float)m_totalBytes * 100);
                    m_session.Host.RaiseFileTransferProgressEvent(this);
                    bytesReadFromLastProgressEvent = 0;
                }
                dest.Write(buffer, 0, byteRead);
            }
        }
    }
}
