
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
        //TO DO
    }
}
