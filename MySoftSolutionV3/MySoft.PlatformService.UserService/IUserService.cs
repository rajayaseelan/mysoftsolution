using System;
using System.Collections.Generic;
using System.Text;
using MySoft.Remoting;
using System.Data;
using MySoft.IoC;
using MySoft.RESTful;
using System.Net;
using System.Runtime.Serialization;

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

    [ServiceContract]
    public interface IUserService
    {
        UserInfo GetUserInfo(string name);

        [OperationContract(CacheTime = 10)]
        IList<UserInfo> GetUsers();
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
