using System;
using System.Collections.Generic;
using System.Text;
using MySoft.Remoting;
using System.Data;
using MySoft.IoC;
using MySoft.RESTful;
using System.Net;

namespace MySoft.PlatformService.UserService
{
    [ServiceContract(CallbackType = typeof(IMessageListener))]
    public interface IMessagePublishService
    {
        void Subscribe();

        void Unsubscribe();

        void Compute(int x, int y);
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

    [ServiceContract]
    public interface IUserService
    {
        [OperationContract(CacheTime = 30000)]
        UserInfo GetUserInfo(string name);

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
