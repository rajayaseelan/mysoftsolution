using System;
using System.Linq;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 用于被排除的Field
    /// </summary>
    [Serializable]
    public class ExcludeField : IFieldFilter
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
        /// 实例化被排除的Field
        /// </summary>
        /// <param name="fields"></param>
        internal ExcludeField(Field[] fields)
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
            list.RemoveAll(f =>
            {
                if (this.Fields.Any(p => p.Name == f.Name)) return true;
                return false;
            });

            return list.ToArray();
        }

        #endregion
    }
}
