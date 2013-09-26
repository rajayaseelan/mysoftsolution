using MySoft.RESTful.Business.Pool;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 业务注册器
    /// </summary>
    public interface IBusinessRegister : IContainer
    {
        /// <summary>
        /// 注册业务
        /// </summary>
        void Register(IBusinessPool businessPool);
    }
}
