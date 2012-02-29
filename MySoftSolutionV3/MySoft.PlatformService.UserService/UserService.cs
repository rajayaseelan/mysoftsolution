using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using MySoft.IoC;
using System.Collections.Specialized;
using Castle.Core;

namespace MySoft.PlatformService.UserService
{
    public class UserService : BaseContainer, IUserService, IInitializable
    {
        private DateTime startTime;
        public UserService()
        {
            this.startTime = DateTime.Now;
        }

        public string GetUser(UserInfo user)
        {
            return user.Name;
        }

        public string GetUser(NameValueCollection nv)
        {
            return "maoyong";
        }

        public int GetSex(Sex value)
        {
            return Convert.ToInt32(value);
        }

        public virtual UserInfo GetUserInfo(string name, ref int length, out UserInfo user, params int[] ids)
        {
            var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            if (count % 5 == 0)
            {
                throw new UserException("Error Count: " + count);
            }
            else if (count % 6 == 0)
            {
                Thread.Sleep(new Random().Next(1, 10) * 1000);
            }

            user = new UserInfo()
            {
                Name = name,
                Description = string.Format("您的用户名为：{0}", name)
            };

            length = user.Description.Length;

            Thread.Sleep(2000);

            return user;
        }

        public string GetDateTime(Guid guid, DateTime time, UserInfo user, Sex sex)
        {
            return string.Format("{0} => {1}", guid, DateTime.Now);
        }

        public IList<UserInfo> GetUsers()
        {
            var list = new List<UserInfo>();

            for (int i = 0; i < 100; i++)
            {
                var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
                list.Add(new UserInfo { Name = "test_" + count, Description = "test_" + count + "_" + Thread.CurrentThread.ManagedThreadId });
            }

            return list;
        }

        public UserInfo GetUser(int id, string name, IList<string> list, int[] values)
        {
            return new UserInfo { Name = id.ToString(), Description = id.ToString() + "你好！" + name };
        }

        public IDictionary<Sex, IList<UserInfo>> GetDict()
        {
            return null;
        }

        #region IInitializable 成员

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
