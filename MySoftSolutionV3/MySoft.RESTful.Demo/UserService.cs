using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MySoft.RESTful.Auth;

namespace MySoft.RESTful.Demo
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class User
    {
        public int Id;
        public string Name;
        public IList<UserDetail> Details;
        public Sex Sex;
    }

    [Serializable]
    public class UserDetail
    {
        public int Age;
        public IList<UserInfo> Infos;
        public Sex Sex;
    }

    [Serializable]
    public class UserInfo
    {
        public string Address;
        public Sex Sex;
    }

    public enum Sex
    {
        男,
        女
    }

    /// <summary>
    /// 用户接口
    /// </summary>
    [PublishKind("user", Description = "用户接口")]
    public interface IUserService
    {
        /// <summary>
        /// 获取登录用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getloginuser", Description = "获取登录用户")]
        User GetLoginUser();

        /// <summary>
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getuser", Description = "获取一个用户", IsPublic = false)]
        User GetUser(int id, string name);

        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getusers", Description = "获取一组用户", IsPublic = false)]
        IList<User> GetUsers();

        /// <summary>
        /// 保存用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("saveuser", Description = "保存一个用户", Method = HttpMethod.POST)]
        void SaveUser(User user);
    }

    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserService : IUserService
    {
        #region IUserService 成员

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <returns></returns>
        public User GetLoginUser()
        {
            var user = AuthenticationContext.Current.User;
            return new User { Id = user.ID, Name = user.Name };
        }

        /// <summary>
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        public User GetUser(int id, string name)
        {
            return new User { Id = id, Name = name };
        }

        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        public IList<User> GetUsers()
        {
            return new List<User> { new User { Id = 1, Name = "test1" }, new User { Id = 2, Name = "test2" } };
        }

        /// <summary>
        /// 保存用户
        /// </summary>
        /// <returns></returns>
        public void SaveUser(User user)
        {
            //未实现
        }

        #endregion
    }
}
