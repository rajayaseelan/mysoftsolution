using System;
using System.Linq;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// FieldValue集合
    /// </summary>
    public class FieldValueCollection<T>
        where T : Entity
    {
        private IDictionary<Field, object> fvValues;
        /// <summary>
        /// 实例化FieldValueCollection
        /// </summary>
        /// <param name="dictValues"></param>
        public FieldValueCollection(IDictionary<string, object> dictValues)
        {
            fvValues = new Dictionary<Field, object>();
            T entity = EntityCache<T>.Get(() => CoreHelper.CreateInstance<T>());
            foreach (var kv in dictValues)
            {
                var field = entity.As<IEntityBase>().GetField(kv.Key);
                if (field != null) fvValues[field] = kv.Value;
            }
        }

        /// <summary>
        /// 实例化FieldValueCollection
        /// </summary>
        /// <param name="dictValues"></param>
        public FieldValueCollection(IDictionary<Field, object> dictValues)
        {
            this.fvValues = dictValues;
        }

        /// <summary>
        /// 返回列表
        /// </summary>
        /// <returns></returns>
        public FieldValue[] ToList()
        {
            List<FieldValue> list = new List<FieldValue>();
            foreach (var kv in fvValues)
            {
                list.Add(new FieldValue(kv.Key, kv.Value));
            }
            return list.ToArray();
        }
    }

    /// <summary>
    /// 字段及值
    /// </summary>
    [Serializable]
    public class FieldValue
    {
        private Field field;
        /// <summary>
        /// 字段
        /// </summary>
        public Field Field
        {
            get { return field; }
        }

        private object fvalue;
        /// <summary>
        /// 值
        /// </summary>
        public object Value
        {
            get { return fvalue; }
            internal set { fvalue = value; }
        }

        private bool isIdentity;
        /// <summary>
        /// 是否标识列
        /// </summary>
        public bool IsIdentity
        {
            get { return isIdentity; }
            set { isIdentity = value; }
        }

        private bool isPrimaryKey;
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set { isPrimaryKey = value; }
        }

        private bool isChanged;
        /// <summary>
        /// 是否更改
        /// </summary>
        internal bool IsChanged
        {
            get { return isChanged; }
            set { isChanged = value; }
        }

        /// <summary>
        /// 实例化FieldValue
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public FieldValue(Field field, object value)
        {
            this.field = field;
            this.fvalue = value;
        }
    }
}
