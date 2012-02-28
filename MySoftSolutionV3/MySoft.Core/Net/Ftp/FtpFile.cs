using System;
using System.Runtime.InteropServices;

namespace MySoft.Net.Ftp
{
    #region COM interface IFtpFile
    public interface IFtpFile : IFtpItem
    {
        long Size { get; }
    }
    #endregion

    [ClassInterface(ClassInterfaceType.None)]
    public class FtpFile : IFtpFile
    {
        FtpDirectory m_parent;
        string m_name;
        string m_permission;
        long m_size;

        internal FtpFile(FtpDirectory parent, ref ItemInfo info)
        {
            m_parent = parent;
            m_name = info.name;
            m_size = info.size;
            m_permission = info.permission;
        }
        internal FtpFile(FtpDirectory parent, string name)
        {
            m_parent = parent;
            m_name = name;
        }
        public bool IsFile
        {
            get { return true; }
        }
        public bool IsDirectory
        {
            get { return false; }
        }
        public string Name
        {
            get { return m_name; }
            set
            {
                // Assume Parent is Current Directory. 
                // Will cause SERIOUS PROBLEM if Parent is not current directory.
                m_parent.RenameSubitem(this, value);
                m_name = value;
            }
        }
        public string FullName
        {
            get { return m_parent.FullName + m_name; }
        }
        public long Size
        {
            get { return m_size; }
        }
        public FtpDirectory Parent
        {
            get
            {
                m_parent.CheckSessionCurrentDirectory();
                return m_parent;
            }
        }
        public string FullPath
        {
            get { return m_parent.FullName + m_name; }
        }
        public void Rename(string newName)
        {
        }
        public FtpInputDataStream GetInputStream()
        {
            return (FtpInputDataStream)GetStream(0, TransferDirection.Download);
        }
        public FtpOutputDataStream GetOutputStream()
        {
            return (FtpOutputDataStream)GetStream(0, TransferDirection.Upload);
        }
        public FtpInputDataStream GetInputStream(long offset)
        {
            return (FtpInputDataStream)GetStream(offset, TransferDirection.Download);
        }
        public FtpOutputDataStream GetOutputStream(long offset)
        {
            return (FtpOutputDataStream)GetStream(offset, TransferDirection.Upload);
        }
        private FtpDataStream GetStream(long offset, TransferDirection dir)
        {
            m_parent.CheckSessionCurrentDirectory();
            SessionConnected session = m_parent.FtpSession;
            if (offset != 0)
                session.ControlChannel.REST(offset);
            FtpDataStream stream = session.ControlChannel.GetPassiveDataStream(dir);
            try
            {
                if (dir == TransferDirection.Download)
                    session.ControlChannel.RETR(m_name);
                else
                    session.ControlChannel.STOR(m_name);
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
            return stream;
        }
    }
}