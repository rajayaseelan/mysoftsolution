using System;
using System.Collections.Generic;
using System.Data;
using MySoft.Data.Design;
using System.Linq;

namespace MySoft.Data
{
    /// <summary>
    /// Entity状态
    /// </summary>
    [Serializable]
    public enum EntityState
    {
        /// <summary>
        /// 插入状态
        /// </summary>
        Insert,
        /// <summary>
        /// 修改状态
        /// </summary>
        Update
    }

    /// <summary>
    /// Entity基类
    /// </summary>
    [Serializable]
    public abstract class EntityBase : IEntityBase, IEntityInfo, IValidator
    {
        protected List<Field> updatelist = new List<Field>();
        protected List<Field> removeinsertlist = new List<Field>();
        protected bool isUpdate = false;
        protected bool isFromDB = false;

        /// <summary>
        /// 转换成另一对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult As<TResult>()
        {
            lock (this)
            {
                return DataHelper.ConvertType<IEntityBase, TResult>(this);
            }
        }

        /// <summary>
        /// 返回一个行阅读对象
        /// </summary>
        IRowReader IEntityBase.ToRowReader()
        {
            lock (this)
            {
                try
                {
                    SourceList<EntityBase> list = new SourceList<EntityBase>();
                    list.Add(this);

                    DataTable dt = list.GetDataTable(this.GetType());
                    ISourceTable table = new SourceTable(dt);
                    return table[0];
                }
                catch (Exception ex)
                {
                    throw new DataException("数据转换失败！", ex);
                }
            }
        }

        /// <summary>
        /// 返回字典对象
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> IEntityBase.ToDictionary()
        {
            try
            {
                IDictionary<string, object> dict = new Dictionary<string, object>();
                foreach (Field f in GetFields())
                {
                    object value = CoreHelper.GetPropertyValue(this, f.PropertyName);
                    dict[f.OriginalName] = value;
                }
                return dict;
            }
            catch (Exception ex)
            {
                throw new DataException("数据转换失败！", ex);
            }
        }

        /// <summary>
        /// 使用propertyName获取值信息
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        object IEntityBase.GetValue(string propertyName)
        {
            return CoreHelper.GetPropertyValue(this, propertyName);
        }

        /// <summary>
        /// 使用propertyName获设置信息
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        void IEntityBase.SetValue(string propertyName, object value)
        {
            CoreHelper.SetPropertyValue(this, propertyName, value);
        }

        /// <summary>
        /// 使用field获取值信息
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        object IEntityBase.GetValue(Field field)
        {
            return CoreHelper.GetPropertyValue(this, field.PropertyName);
        }

        /// <summary>
        /// 使用field获设置信息
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        void IEntityBase.SetValue(Field field, object value)
        {
            CoreHelper.SetPropertyValue(this, field.PropertyName, value);
        }

        /// <summary>
        /// 通过属性获取字段
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        Field IEntityBase.GetField(string propertyName)
        {
            var field = this.GetFields().FirstOrDefault(p => string.Compare(p.PropertyName, propertyName, true) == 0);
            if ((IField)field == null)
            {
                throw new DataException(string.Format("实体【{0}】中未找到属性为【{1}】的字段信息！", this.GetType().FullName, propertyName));
            }
            return field;
        }

        /// <summary>
        /// 获取对象状态
        /// </summary>
        EntityState IEntityBase.GetObjectState()
        {
            return isUpdate ? EntityState.Update : EntityState.Insert;
        }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <returns></returns>
        EntityBase IEntityBase.CloneObject()
        {
            lock (this)
            {
                return DataHelper.CloneObject(this);
            }
        }

        #region 字段信息

        /// <summary>
        /// 返回标识列的名称（如Oracle中Sequence.nextval）
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSequence()
        {
            return null;
        }

        /// <summary>
        /// 获取标识列
        /// </summary>
        /// <returns></returns>
        protected virtual Field GetIdentityField()
        {
            return null;
        }

        /// <summary>
        /// 获取主键列表
        /// </summary>
        /// <returns></returns>
        protected virtual Field[] GetPrimaryKeyFields()
        {
            return new Field[] { };
        }

        /// <summary>
        /// 获取字段列表
        /// </summary>
        /// <returns></returns>
        internal protected abstract Field[] GetFields();

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <returns></returns>
        protected abstract object[] GetValues();

        #endregion

        /// <summary>
        /// 获取只读属性
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool GetReadOnly()
        {
            return false;
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <returns></returns>
        protected internal virtual Table GetTable()
        {
            return new Table("TempTable");
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="reader"></param>
        protected abstract void SetValues(IRowReader reader);

        #region 内部方法

        /// <summary>
        /// 获取系列的名称
        /// </summary>
        internal string SequenceName
        {
            get
            {
                lock (this)
                {
                    return this.GetSequence();
                }
            }
        }

        /// <summary>
        /// 获取排序或分页字段
        /// </summary>
        /// <returns></returns>
        internal Field PagingField
        {
            get
            {
                lock (this)
                {
                    Field pagingField = this.GetIdentityField();

                    if ((IField)pagingField == null)
                    {
                        Field[] fields = this.GetPrimaryKeyFields();
                        if (fields.Length > 0) pagingField = fields[0];
                    }

                    return pagingField;
                }
            }
        }

        /// <summary>
        /// 获取标识列
        /// </summary>
        internal Field IdentityField
        {
            get
            {
                lock (this)
                {
                    return this.GetIdentityField();
                }
            }
        }

        /// <summary>
        /// 设置所有的值
        /// </summary>
        /// <param name="reader"></param>
        internal void SetDbValues(IRowReader reader)
        {
            lock (this)
            {
                //设置内部的值
                SetValues(reader);

                //设置来自数据库变量为true
                isFromDB = true;
            }
        }

        /// <summary>
        /// 获取字段及值
        /// </summary>
        /// <returns></returns>
        internal List<FieldValue> GetFieldValues()
        {
            lock (this)
            {
                List<FieldValue> fvlist = new List<FieldValue>();

                Field identityField = this.GetIdentityField();
                List<Field> pkFields = new List<Field>(this.GetPrimaryKeyFields());

                Field[] fields = this.GetFields();
                object[] values = this.GetValues();

                if (fields.Length != values.Length)
                {
                    throw new DataException("字段与值无法对应！");
                }

                int index = 0;
                foreach (Field field in fields)
                {
                    FieldValue fv = new FieldValue(field, values[index]);

                    //判断是否为标识列
                    if ((IField)identityField != null)
                        if (identityField.Name == field.Name) fv.IsIdentity = true;

                    //判断是否为主键
                    if (pkFields.Contains(field)) fv.IsPrimaryKey = true;

                    if (isUpdate)
                    {
                        //如果是更新，则将更新的字段改变状态为true
                        if (updatelist.Contains(field)) fv.IsChanged = true;
                    }
                    else
                    {
                        //如果是插入，则将移除插入的字段改变状态为true
                        if (removeinsertlist.Contains(field)) fv.IsChanged = true;
                    }

                    fvlist.Add(fv);
                    index++;
                }

                return fvlist;
            }
        }

        #endregion

        #region IValidator 成员

        /// <summary>
        /// 验证实体的有效性
        /// </summary>
        /// <returns></returns>
        public virtual ValidateResult Validation()
        {
            return ValidateResult.Default;
        }

        #endregion

        #region IEntityInfo 成员

        /// <summary>
        /// 表信息
        /// </summary>
        Table IEntityInfo.Table
        {
            get
            {
                return this.GetTable();
            }
        }

        /// <summary>
        /// 字段信息
        /// </summary>
        Field[] IEntityInfo.Fields
        {
            get
            {
                return this.GetFields();
            }
        }

        /// <summary>
        /// 字段及值信息
        /// </summary>
        FieldValue[] IEntityInfo.FieldValues
        {
            get
            {
                return this.GetFieldValues().ToArray();
            }
        }

        /// <summary>
        /// 更新字段
        /// </summary>
        Field[] IEntityInfo.UpdateFields
        {
            get
            {
                return this.GetFieldValues().FindAll(p => p.IsChanged).ConvertAll<Field>(p => p.Field).ToArray();
            }
        }

        /// <summary>
        /// 更新字段及值信息
        /// </summary>
        FieldValue[] IEntityInfo.UpdateFieldValues
        {
            get
            {
                return this.GetFieldValues().FindAll(p => p.IsChanged).ToArray();
            }
        }

        /// <summary>
        /// 是否修改
        /// </summary>
        bool IEntityInfo.IsUpdate
        {
            get
            {
                return this.GetFieldValues().FindAll(p => p.IsChanged).Count > 0;
            }
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        bool IEntityInfo.IsReadOnly
        {
            get
            {
                return this.GetReadOnly();
            }
        }

        #endregion
    }
}
