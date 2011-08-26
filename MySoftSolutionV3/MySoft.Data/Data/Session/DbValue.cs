using System;

namespace MySoft.Data
{
    /// <summary>
    /// 数据库值
    /// </summary>
    [Serializable]
    public class DbValue
    {
        /// <summary>
        /// 系统时间
        /// </summary>
        public static DbValue DateTime
        {
            get
            {
                return new DbValue("getdate()");
            }
        }

        /// <summary>
        /// 返回默认值
        /// </summary>
        public static DbValue Default
        {
            get
            {
                return new DbValue("$$$___$$$___$$$");
            }
        }

        private string dbvalue;
        public DbValue(string dbvalue)
        {
            this.dbvalue = dbvalue;
        }

        internal string Value
        {
            get { return dbvalue; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DbValue)) return false;
            return string.Compare(this.Value, (obj as DbValue).Value, true) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}