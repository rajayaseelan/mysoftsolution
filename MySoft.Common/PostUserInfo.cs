
namespace MySoft.Common
{
    /// <summary>
    /// PostUserInfo : 构造传递用户数据。
    /// </summary>
    public class PostUserInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public PostUserInfo() { }

        // Private Field
        private string _UID;
        private string _UserID;
        private string _UserPwd;
        private string _UserMail;
        private string _OldUserPwd;
        private ServiceMethod _Method;

        #region 用户数据属性

        /// <summary>
        /// 用户编号
        /// </summary>
        public string UID
        {
            get { return _UID; }
            set { _UID = value; }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPwd
        {
            get { return _UserPwd; }
            set { _UserPwd = value; }
        }

        /// <summary>
        /// 用户邮件地址
        /// </summary>
        public string UserMail
        {
            get { return _UserMail; }
            set { _UserMail = value; }
        }

        /// <summary>
        /// 旧密码
        /// </summary>
        public string OldUserPwd
        {
            get { return _OldUserPwd; }
            set { _OldUserPwd = value; }
        }

        /// <summary>
        /// 自定义服务的操作方式
        /// </summary>
        public ServiceMethod Method
        {
            get { return _Method; }
            set { _Method = value; }
        }

        #endregion
    }
}
