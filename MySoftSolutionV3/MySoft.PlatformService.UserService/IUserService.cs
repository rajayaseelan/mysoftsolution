using System;
using System.Collections.Generic;
using System.Text;
using MySoft.Remoting;
using System.Data;
using MySoft.IoC;
using MySoft.RESTful;

namespace MySoft.PlatformService.UserService
{
    [PublishKind("user")]
    [ServiceContract]
    public interface IUserService
    {
        [PublishMethod("getuserinfo", AuthParameter = "name")]
        [OperationContract(CacheTime = 30000)]
        UserInfo GetUserInfo(string name);

        [PublishMethod("getusers")]
        IList<UserInfo> GetUsers();

        IDictionary<string, UserInfo> GetDictUsers();

        DataTable GetDataTable();

        void SetUser(UserInfo user, ref int userid);

        int GetUserID();
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string FullName
        {
            get
            {
                return typeof(Data.DataHelper).FullName;
            }
        }
    }
}
