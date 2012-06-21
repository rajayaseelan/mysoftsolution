
namespace MySoft.Common
{
    /// <summary>
    /// ServiceMethod : 自定义服务的操作方式。
    /// </summary>
    public enum ServiceMethod : int
    {
        /// <summary>
        /// 验证用户登陆
        /// </summary>
        CheckUser = 0,
        /// <summary>
        /// 添加用户
        /// </summary>
        AddUser,
        /// <summary>
        /// 修改用户信息
        /// </summary>
        ChangeUser,
        /// <summary>
        /// 注销删除用户
        /// </summary>
        DeleteUser
    }
}
