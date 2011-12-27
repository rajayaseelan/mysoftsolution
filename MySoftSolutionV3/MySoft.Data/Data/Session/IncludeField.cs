using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// 用于被选择的Field
    /// </summary>
    [Serializable]
    public class IncludeField : IFieldFilter
    {
        private Field[] fields;
        internal List<Field> Fields
        {
            get
            {
                if (fields == null || fields.Length == 0)
                    return new List<Field>();

                return new List<Field>(fields);
            }
        }

        /// <summary>
        /// 实例化被选择的Field
        /// </summary>
        /// <param name="fields"></param>
        internal IncludeField(Field[] fields)
        {
            this.fields = fields;
        }

        #region IFieldFilter 成员

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Field[] GetFields(Field[] fields)
        {
            List<Field> list = new List<Field>(fields);
            list = list.FindAll(f =>
            {
                if (this.Fields.Contains(f)) return true;
                return false;
            });

            return list.ToArray();
        }

        #endregion
    }
}
