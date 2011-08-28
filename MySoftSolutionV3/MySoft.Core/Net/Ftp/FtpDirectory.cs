using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MySoft.Net.FTP
{
    internal struct ItemInfo
    {
        public string permission;
        public bool isDirectory;
        public long size;
        public string name;
        public string fullPath;
        public void Init()
        {
            permission = null;
            name = null;
            fullPath = null;
            isDirectory = false;
            size = 0;
        }
    }

    #region COM interface IFtpDirectory
    //[Guid("7141228F-F872-42a2-953B-5349AEE26AF1")]
    public interface IFtpDirectory : IFtpItem
    {
        FtpDirectory Parent { get; }
        VbEnumableCollection Files { get; }
        VbEnumableCollection SubDirectories { get; }
        FtpDirectory FindSubdirectory(string dirName);
        FtpDirectory FindSubdirectory(string dirName, bool ignoreCase);
        IFtpItem FindItem(string name);
        void PutFile(string localFile);
        void PutFile(string localFile, string remoteFile);
        void GetFile(string remoteFile);
        void GetFile(string localFile, string remoteFile);
        void BeginPutFile(string localFile);
        void BeginPutFile(string localFile, string remoteFile);
        void BeginGetFile(string remoteFile);
        void BeginGetFile(string localFile, string remoteFile);
    }
    #endregion

    [ClassInterface(ClassInterfaceType.None)]
    public class FtpDirectory : IFtpDirectory
    {
        private SessionConnected m_session;
        private string m_name;
        private string m_fullPath;
        private Hashtable m_subDirectories;
        private Hashtable m_files;

        #region Regular expression to parse list lines
        static Regex m_UnixListLineExpression = new Regex(@"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)");
        static Regex m_UnixListLineExpression1 = new Regex(@"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{2}:\d{2})\s+(?<name>.+)");
        static Regex m_DosListLineExpression = new Regex(@"(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)");
        #endregion

        internal FtpDirectory(SessionConnected s)
        {
            m_session = s;

            m_fullPath = s.ControlChannel.PWD();
            if (m_fullPath == "/")
            {
                m_name = m_fullPath;
                return;
            }

            string[] directories = m_fullPath.Split('/');
            m_name = directories[directories.Length - 1];
            m_fullPath += "/";
        }

        internal FtpDirectory(SessionConnected s, string parentPath, string name)
        {
            m_session = s;
            if (name != "")
            {
                m_name = name;
                m_fullPath = parentPath + m_name + "/";
            }
            else
                m_name = m_fullPath = "/";
        }

        public string Name
        {
            get { return m_name; }
            set
            {
                // Assume Parent is Current Directory. 
                // Will cause SERIOUS PROBLEM if Parent is not current directory.
                if ((m_session.CurrentDirectory.FullName + m_name + "/") != m_fullPath)
                    throw new FtpException("Cannot rename items not belongs to current directory.");
                m_session.CurrentDirectory.RenameSubitem(this, value);
                m_name = value;
                m_fullPath = m_session.CurrentDirectory.FullName + m_name + "/";
            }
        }

        public string FullName
        {
            get { return m_fullPath; }
        }
        public bool IsFile
        {
            get { return false; }
        }
        public bool IsDirectory
        {
            get { return true; }
        }
        public FtpDirectory Parent
        {
            get
            {
                CheckSessionCurrentDirectory();
                if (m_fullPath == m_session.RootDirectory.m_fullPath)
                    return null;
                StringBuilder parentPath = new StringBuilder();
                m_fullPath = m_session.ControlChannel.PWD();

                string[] paths = m_fullPath.Split('/');

                for (int i = 0; i < paths.Length - 2; i++)
                {
                    if (paths[i] == "")
                        parentPath.Append('/');
                    else
                    {
                        parentPath.Append(paths[i]);
                        parentPath.Append('/');
                    }
                }
                FtpDirectory parent = new FtpDirectory(m_session, parentPath.ToString(), paths[paths.Length - 2]);
                m_fullPath += "/";
                if (parent.m_fullPath == m_session.RootDirectory.m_fullPath)
                    return m_session.RootDirectory;
                return parent;
            }
        }

        public VbEnumableCollection SubDirectories
        {
            get
            {
                InitHashtable();
                return new VbEnumableCollection(m_subDirectories.Values);
            }
        }

        public VbEnumableCollection Files
        {
            get
            {
                InitHashtable();
                return new VbEnumableCollection(m_files.Values);
            }
        }

        public FtpFile FindFile(string fileName)
        {
            InitHashtable();
            return (FtpFile)m_files[fileName];
        }

        public FtpDirectory FindSubdirectory(string dirName)
        {
            return FindSubdirectory(dirName, false);
        }

        public FtpDirectory FindSubdirectory(string dirName, bool ignoreCase)
        {
            InitHashtable();
            FtpDirectory d = (FtpDirectory)m_subDirectories[dirName];
            if (d != null)
                return d;
            else if (ignoreCase)
            {
                string upperCase = dirName.ToUpper();
                foreach (string s in m_subDirectories.Keys)
                {
                    if (s.ToUpper() == upperCase)
                        return (FtpDirectory)m_subDirectories[s];
                }
            }
            return null;
        }

        public IFtpItem FindItem(string name)
        {
            IFtpItem item;
            InitHashtable();
            item = FindSubdirectory(name);
            if (item != null)
                return item;
            item = FindFile(name);
            return item;
        }
        //#region Routines provided for COM clients
        /*public DirectoryCollection GetSubDirectorieCollection()
        {
            InitHashtable();
            return new DirectoryCollection(m_subDirectories.Values);	
        }
        public FileCollection GetFileCollection()
        {
            InitHashtable();
            return new FileCollection(m_files.Values);
        }*/
        //#endregion

        public void PutFile(string localFile)
        {
            PutFile(localFile, null);
        }

        public void PutFile(string localFile, string remoteFile)
        {
            CheckSessionCurrentDirectory();
            FileInfo fi = new FileInfo(localFile);
            if (remoteFile == null)
                remoteFile = fi.Name;
            FtpFileTransferer transfer = new FtpFileTransferer(
                this,
                localFile,
                remoteFile,
                fi.Length,
                TransferDirection.Upload);
            transfer.StartTransfer();
        }

        public void GetFile(string remoteFile)
        {
            GetFile(remoteFile, remoteFile);
        }

        public void GetFile(string localFile, string remoteFile)
        {
            InitHashtable();
            FtpFile file = (FtpFile)m_files[remoteFile];
            if (file == null)
                throw new FtpException("Remote file (" + remoteFile + ") not found. Try refresh the directory.");
            FtpFileTransferer transfer = new FtpFileTransferer(
                this,
                localFile,
                remoteFile,
                file.Size,
                TransferDirection.Download);
            transfer.StartTransfer();
        }

        public void BeginPutFile(string localFile)
        {
            BeginPutFile(localFile, null, null);
        }

        #region BeginPutFile overloads
        public void BeginPutFile(string localFile, FtpFileEventHandler callback)
        {
            BeginPutFile(localFile, null, callback);
        }

        public void BeginPutFile(string localFile, string remoteFile)
        {
            BeginPutFile(localFile, remoteFile, null);
        }

        public void BeginPutFile(string localFile, string remoteFile, FtpFileEventHandler callback)
        {

            CheckSessionCurrentDirectory();
            FileInfo fi = new FileInfo(localFile);
            if (remoteFile == null)
                remoteFile = fi.Name;
            FtpFileTransferer transfer = new FtpFileTransferer(
                this,
                localFile,
                remoteFile,
                fi.Length,
                TransferDirection.Upload);
            transfer.StartAsyncTransfer(callback);
        }
        #endregion

        public void BeginGetFile(string remoteFile)
        {
            BeginGetFile(remoteFile, remoteFile, null);
        }

        #region BeginGetFile overloads
        public void BeginGetFile(string remoteFile, FtpFileEventHandler callback)
        {
            BeginGetFile(remoteFile, remoteFile, callback);
        }

        public void BeginGetFile(string localFile, string remoteFile)
        {
            BeginGetFile(localFile, remoteFile, null);
        }

        public void BeginGetFile(string localFile, string remoteFile, FtpFileEventHandler callback)
        {
            InitHashtable();
            FtpFile file = (FtpFile)m_files[remoteFile];
            if (file == null)
                throw new FtpException("Remote file (" + remoteFile + ") not found. Try refresh the directory.");
            FtpFileTransferer transfer = new FtpFileTransferer(
                this,
                localFile,
                remoteFile,
                file.Size,
                TransferDirection.Download);
            transfer.StartAsyncTransfer(callback);
        }
        #endregion

        public void RemoveFile(string fileName)
        {
            CheckSessionCurrentDirectory();
            m_session.ControlChannel.DELE(fileName);
            m_files.Remove(fileName);
        }

        public void RemoveSubdir(string dirName)
        {
            CheckSessionCurrentDirectory();
            m_session.ControlChannel.RMD(dirName);
            m_subDirectories.Remove(dirName);
        }

        public FtpDirectory CreateSubdir(string dirName)
        {
            CheckSessionCurrentDirectory();
            m_session.ControlChannel.MKD(dirName);

            var dir = new FtpDirectory(FtpSession, this.FullName, dirName);
            m_subDirectories.Add(dirName, dir);

            return dir;
        }

        public void RemoveItem(IFtpItem item)
        {
            if (FindItem(item.Name) != item)
                throw new ArgumentException("Invalid subitem (" + item.Name + ") for directory " + m_name, "item");
            if (item.IsDirectory)
                RemoveSubdir(item.Name);
            else
                RemoveFile(item.Name);
        }

        internal void RenameSubitem(IFtpItem item, string newName)
        {
            CheckSessionCurrentDirectory();
            if (FindItem(item.Name) != item)
                throw new ArgumentException("Invalid subitem (" + item.Name + ") for directory " + m_name, "item");
            m_session.ControlChannel.Rename(newName, item.Name);
            if (item.IsFile)
            {
                m_files.Remove(item.Name);
                m_files[newName] = item;
            }
            else
            {
                m_subDirectories.Remove(item.Name);
                m_subDirectories[newName] = item;
            }
        }

        public FtpFile CreateFile(string newFileName)
        {
            FtpDataStream stream = CreateFileStream(newFileName);
            stream.Close();
            return (FtpFile)m_files[newFileName];
        }

        public FtpOutputDataStream CreateFileStream(string newFileName)
        {
            InitHashtable();
            FtpDataStream stream = m_session.ControlChannel.GetPassiveDataStream(TransferDirection.Upload);
            try
            {
                m_session.ControlChannel.STOR(newFileName);
                FtpFile newFile = new FtpFile(this, newFileName);
                m_files[newFileName] = newFile;
                return (FtpOutputDataStream)stream;
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
        }

        public void Refresh()
        {
            ClearItems();
            InitHashtable();
        }

        internal void ClearItems()
        {
            m_subDirectories = null;
            m_files = null;
        }

        internal SessionConnected FtpSession
        {
            get { return m_session; }
        }

        internal void CheckSessionCurrentDirectory()
        {
            if (m_session.CurrentDirectory.m_fullPath != m_fullPath)
                throw new InvalidOperationException(m_fullPath + " is not current directory.");
        }

        #region support routines
        private void LoadDirecotryItems()
        {
            if (m_session.CurrentDirectory != this)
                throw new InvalidOperationException(m_name + " is not current active directory");

            Queue lineQueue = m_session.ControlChannel.List(false);
            ItemInfo info = new ItemInfo();
            foreach (string line in lineQueue)
            {
                if (ParseListLine(line, ref info))
                {
                    if (info.isDirectory)
                        m_subDirectories.Add(info.name, new FtpDirectory(m_session, m_fullPath, info.name));
                    else
                        m_files.Add(info.name, new FtpFile(this, ref info));
                }
            }
        }

        private void InitHashtable()
        {
            CheckSessionCurrentDirectory();
            if (m_subDirectories != null && m_files != null)
                return;

            if (m_subDirectories == null)
                m_subDirectories = new Hashtable();
            if (m_files == null)
                m_files = new Hashtable();
            LoadDirecotryItems();
        }
        private bool ParseListLine(string line, ref ItemInfo info)
        {
            Match m;
            if ((m = MatchingListLine(line)) == null)
                return false;

            info.Init();
            info.name = m.Groups["name"].Value;
            info.fullPath = m_fullPath + info.name;
            string dir = m.Groups["dir"].Value;

            if (dir != "" && dir != "-")
            {
                info.isDirectory = true;
                info.fullPath += "/";
            }
            else
                info.size = long.Parse(m.Groups["size"].Value);

            string permission = m.Groups["permission"].Value;
            if (permission != "")
                info.permission = permission;
            return true;
        }

        private Match MatchingListLine(string line)
        {
            Match m = m_UnixListLineExpression.Match(line);
            if (m.Success)
                return m;
            m = m_UnixListLineExpression1.Match(line);
            if (m.Success)
                return m;
            m = m_DosListLineExpression.Match(line);
            if (m.Success)
                return m;
            return null;
        }
        #endregion
    }
}