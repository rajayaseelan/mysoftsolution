using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using MySoft.IoC;
using MySoft.IoC.Aspect;
using System.Collections.Specialized;

namespace MySoft.PlatformService.UserService
{
    public class AspectLog : AspectInterceptor
    {
        protected override void PreProceed(IInvocation invocation)
        {
            base.PreProceed(invocation);
        }
    }

    [AspectProxy(typeof(AspectLog))]
    public class UserService : IUserService, IInitializable, IStartable
    {
        //private DateTime startTime;
        //public UserService()
        //{
        //    this.startTime = DateTime.Now;
        //}

        public string GetUser(UserInfo user)
        {
            return user.Name;
        }

        public string GetUser(NameValueCollection nv)
        {
            return "maoyong";
        }

        public string GetSex(Sex value)
        {
            return Convert.ToString(value);
        }

        //[AspectSwitcher(true)]
        public virtual UserInfo GetUserInfo(string name, out string userid, out Guid guid, out UserInfo user)
        {
            //Thread.Sleep(60000);

            var context = OperationContext.Current;

            //var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            //if (count % 5 == 0)
            //{
            //    throw new UserException("Error Count: " + count);
            //}
            //else if (count % 6 == 0)
            //{
            //    Thread.Sleep(new Random().Next(1, 10) * 1000);
            //}

            user = new UserInfo()
            {
                Name = name,
                Description = string.Format("您的用户名为：{0}", name)
            };

            userid = user.Name;

            try
            {
                guid = new Guid(userid.Split('_')[1]);
            }
            catch
            {
                guid = Guid.NewGuid();
            }

            //int value = new Random().Next(1, 5);
            //Thread.Sleep(value * 1000);
            Console.WriteLine("{0} => {1}", DateTime.Now, guid);

            //Thread.Sleep(10);

            return user;
        }

        public string GetDateTime(Guid guid, DateTime time, UserInfo user, Sex sex)
        {
            return string.Format("{0} => {1}", guid, DateTime.Now);
        }

        private static string value;
        private static readonly object syncRoot = new object();
        public string GetUsersString()
        {
            //return "asfsadf";
            Thread.Sleep(1000);

            try
            {
                if (value == null)
                {
                    lock (syncRoot)
                    {
                        if (value == null)
                        {
                            value = System.IO.File.ReadAllText("c:\\test.htm");
                        }
                    }
                }
                return value;
            }
            catch (Exception ex)
            {
                //return ErrorHelper.GetHtmlError(ex);

                throw ex;
            }
        }

        public IList<UserInfo> GetUsers()
        {
            var list = new List<UserInfo>();

            for (int i = 0; i < 100; i++)
            {
                var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
                list.Add(new UserInfo { Name = "test_" + count, Description = "test_" + count + "_" + Thread.CurrentThread.ManagedThreadId });
            }

            Console.WriteLine("{0} => {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
            int value = new Random().Next(1, 10);
            Thread.Sleep(value * 1000);

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
            Console.WriteLine("{0}: Initialize...", DateTime.Now);
        }

        #endregion

        #region IStartable 成员

        public void Start()
        {
            //throw new NotImplementedException();
            Console.WriteLine("{0}: Start...", DateTime.Now);
        }

        public void Stop()
        {
            //throw new NotImplementedException();
            Console.WriteLine("{0}: Stop...", DateTime.Now);
        }

        #endregion
    }
}
