
namespace MySoft.RESTful
{
    /// <summary>
    /// 序列化接口
    /// </summary>
    public interface ISerializer
    {
        string Serialize(object data);
    }
}
