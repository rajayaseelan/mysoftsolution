using System.Collections;
using System.Runtime.InteropServices;

namespace MySoft.Net.FTP
{
    #region COM Interface IFtpAsyncResult
    public interface IFtpAsyncResult
    {
        bool IsSuccess { get; }
        bool IsAborted { get; }
        bool IsFailed { get; }
        string Message { get; }
        int FtpResponseCode { get; }
    }
    #endregion

    [ClassInterface(ClassInterfaceType.None)]
    public class FtpAsyncResult : IFtpAsyncResult
    {
        public const int Complete = 0;
        public const int Fail = 1;
        public const int Abort = 2;
        private readonly BitArray m_result;
        private string m_message;
        private int m_ftpResponse;

        #region Constructors
        internal FtpAsyncResult()
            : this("Success.", (int)FtpResponse.InvalidCode, Complete)
        {
        }
        internal FtpAsyncResult(string message, int result)
            : this(message, (int)FtpResponse.InvalidCode, result)
        {
        }
        /*public FtpAsyncResult(string message) : this(message, null, (int)FtpResponse.InvalidCode, Fail)
        {
        }
        public FtpAsyncResult(string message, string fileName) : this(message, fileName, (int)FtpResponse.InvalidCode, Fail)
        {
        }
        public FtpAsyncResult(string message, int ftpCode) : this(message, null, ftpCode, Fail)
        {
        }
        public FtpAsyncResult(string message, string fileName, int ftpCode) : this(message, fileName, ftpCode, Fail)
        {
        }*/
        internal FtpAsyncResult(string message, int ftpCode, int result)
        {
            m_result = new BitArray(3);
            m_message = message;
            m_ftpResponse = ftpCode;
            m_result[result] = true;
        }
        #endregion

        public bool IsSuccess
        {
            get { return m_result[Complete]; }
        }
        public bool IsFailed
        {
            get { return m_result[Fail]; }
        }
        public bool IsAborted
        {
            get { return m_result[Abort]; }
        }
        public int FtpResponseCode
        {
            get { return m_ftpResponse; }
        }
        public string Message
        {
            get { return m_message; }
        }
    }
}