
namespace MySoft.RESTful
{
    /// <summary>
    /// 系列化接口
    /// </summary>
    public interface ISerializer
    {
        string Serialize(object data, bool jsonp);
    }
}
