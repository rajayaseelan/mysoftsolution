using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 自启动接口
    /// </summary>
    public interface IStartable : Castle.Core.IStartable
    {
        //TO DO
    }

    /// <summary>
    /// 初始化接口
    /// </summary>
    public interface IInitializable : Castle.Core.IInitializable
    {
        //TO DO
    }
}

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// Aspect调用接口
    /// </summary>
    public interface IInvocation : Castle.DynamicProxy.IInvocation
    {
        /// <summary>
        /// 参数集合信息
        /// </summary>
        ParameterCollection Parameters { get; }

        /// <summary>
        /// 操作描述信息
        /// </summary>
        string Description { get; }
    }
}
