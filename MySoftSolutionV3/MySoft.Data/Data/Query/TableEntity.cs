using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    [Serializable]
    internal class TableEntity
    {
        /// <summary>
        /// 表名
        /// </summary>
        public Table Table { get; set; }

        /// <summary>
        /// 实体对象
        /// </summary>
        public Entity Entity { get; set; }
    }
}
