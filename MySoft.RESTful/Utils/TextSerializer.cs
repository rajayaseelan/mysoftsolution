using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
