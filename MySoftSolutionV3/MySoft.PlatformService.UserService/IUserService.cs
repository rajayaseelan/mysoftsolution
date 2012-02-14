using System;
using System.Collections.Generic;
using System.Text;
using MySoft.Remoting;
using System.Data;
using MySoft.IoC;
using MySoft.RESTful;
using System.Net;
using System.Runtime.Serialization;
using System.Collections.Specialized;

namespace MySoft.PlatformService.UserService
{
    [Serializable]
    public class UserException : Exception
    {
        public UserException() : base() { }

        public UserException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected UserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// 重载GetObjectData方法
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [ServiceContract(Name = "user", Description = "用户管理")]
    public interface IUserService
    {
        //[OperationContract(ClientCacheTime = 10, ServerCacheTime = 20, CacheKey = "User_{name}")]
        [OperationContract(ServerCacheTime = 10)]
        UserInfo GetUserInfo(string name, ref int length, out UserInfo user);

        string GetUser(NameValueCollection nv);

        string GetUser(UserInfo user);

        int GetSex(Sex value);

        string GetDateTime(Guid guid, DateTime time, UserInfo user, Sex sex);

        [OperationContract]
        IList<UserInfo> GetUsers();

        [OperationContract(Name = "getuser", Description = "获取用户", HttpEnabled = true)]
        UserInfo GetUser(int id, IList<string> names);
    }

    /// <summary>
    /// 用户服务
    /// </summary>
    [ServiceContract]
    public interface IUserService2
    {
        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        User GetUser(int id);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        User GetUser(string name);
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class User
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Sex Sex { get; set; }
    }

    public enum Sex
    {
        男,
        女
    }
}
