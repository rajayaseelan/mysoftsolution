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
    [ServiceContract(CallbackType = typeof(IMessageListener))]
    public interface IMessagePublishService
    {
        void Subscribe();

        void Unsubscribe();

        void Compute(int x, int y);
    }

    [Serializable]
    public class UserException2 : Exception
    {
        public UserException2() : base() { }

        public UserException2(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected UserException2(SerializationInfo info, StreamingContext context)
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

    public class MessagePublishService : IMessagePublishService
    {

        #region IMessagePublishService 成员

        public void Subscribe()
        {
            IMessageListener callback = OperationContext.Current.GetCallbackChannel<IMessageListener>();
            var endPoint = OperationContext.Current.RemoteEndPoint as IPEndPoint;
            MessageCenter.Instance.AddListener(new MessageListener(endPoint.Address.ToString(), endPoint.Port, callback));

            Console.WriteLine("订阅服务IMessageListener成功! {0}:{1}", endPoint.Address, endPoint.Port);
        }

        public void Unsubscribe()
        {
            IMessageListener callback = OperationContext.Current.GetCallbackChannel<IMessageListener>();
            var endPoint = OperationContext.Current.RemoteEndPoint as IPEndPoint;
            MessageCenter.Instance.RemoveListener(new MessageListener(endPoint.Address.ToString(), endPoint.Port, callback));

            Console.WriteLine("退订服务IMessageListener成功! {0}:{1}", endPoint.Address, endPoint.Port);
        }

        public void Compute(int x, int y)
        {
            IMessageListener callback = OperationContext.Current.GetCallbackChannel<IMessageListener>();
            callback.ShowData(x, y, x + y);
        }

        #endregion
    }

    public interface IMessageListener
    {
        void Publish(string message);

        void ShowData(int x, int y, int value);
    }

    [PublishKind("user", Description = "用户管理")]
    [ServiceContract]
    public interface IUserService
    {
        //[PublishMethod("getuser", Description = "获取用户信息", UserParameter = "name")]
        //UserInfo GetUser(string name);

        //[PublishMethod("getusers", Description = "获取用户信息")]
        //IList<UserInfo> GetUsers();

        [OperationContract(CacheTime = 30000)]
        UserInfo GetUserInfo(string name);

        [PublishMethod("getusers", Description = "获取用户信息")]
        IList<UserInfo> GetUsers();

        IDictionary<string, UserInfo> GetDictUsers();

        DataTable GetDataTable();

        void SetUser(UserInfo user, ref int userid);

        int GetUserID();

        void SendMessage(string message);
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
