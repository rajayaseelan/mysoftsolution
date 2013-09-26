using System;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// Text序列化
    /// </summary>
    public class TextSerializer : ISerializer
    {
        #region ISerializer 成员

        public string Serialize(object data)
        {
            return Convert.ToString(data);
        }

        #endregion
    }
}
