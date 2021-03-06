﻿using System;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 更新创建器
    /// </summary>
    [Serializable]
    public class UpdateCreator : WhereCreator<UpdateCreator>, IUpdateCreator
    {
        /// <summary>
        /// 创建一个新的更新器
        /// </summary>
        public static UpdateCreator NewCreator()
        {
            return new UpdateCreator();
        }

        /// <summary>
        /// 创建一个新的更新器
        /// </summary>
        public static UpdateCreator NewCreator(string tableName)
        {
            return new UpdateCreator(tableName);
        }

        /// <summary>
        /// 创建一个新的更新器
        /// </summary>
        public static UpdateCreator NewCreator(Table table)
        {
            return new UpdateCreator(table);
        }

        private List<FieldValue> fvlist;

        /// <summary>
        /// 实例化UpdateCreator
        /// </summary>
        private UpdateCreator()
            : base()
        {
            this.fvlist = new List<FieldValue>();
        }

        /// <summary>
        /// 实例化UpdateCreator
        /// </summary>
        /// <param name="tableName"></param>
        private UpdateCreator(string tableName)
            : base(tableName, null)
        {
            this.fvlist = new List<FieldValue>();
        }

        /// <summary>
        /// 实例化UpdateCreator
        /// </summary>
        /// <param name="table"></param>
        private UpdateCreator(Table table)
            : base(table)
        {
            this.fvlist = new List<FieldValue>();
        }

        #region 内部属性

        internal Field[] Fields
        {
            get
            {
                return fvlist.ConvertAll<Field>(fv => { return fv.Field; }).ToArray();
            }
        }


        internal object[] Values
        {
            get
            {
                return fvlist.ConvertAll<object>(fv => { return fv.Value; }).ToArray();
            }
        }

        #endregion

        #region 设置表信息

        /// <summary>
        /// 设置表和实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="useKeyWhere"></param>
        /// <returns></returns>
        public UpdateCreator SetEntity<T>(T entity, bool useKeyWhere)
            where T : Entity
        {
            //获取需要更新的值
            this.fvlist = entity.GetFieldValues();
            this.fvlist.RemoveAll(fv => !fv.IsChanged || fv.IsIdentity || fv.IsPrimaryKey);

            if (useKeyWhere)
            {
                WhereClip where = DataHelper.GetPkWhere<T>(entity.GetTable(), entity);

                //返回加入值及条件的对象
                return this.From(Table.GetTable<T>()).AddWhere(where);
            }
            else
            {
                return this.From(Table.GetTable<T>());
            }
        }

        #endregion

        #region 添加字段和值

        /// <summary>
        /// 添加一个数据
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public UpdateCreator AddUpdate(Field field, object value)
        {
            //存在相同字段，则不加入更新列表
            if (!fvlist.Exists(fv => { return fv.Field.OriginalName == field.OriginalName; }))
            {
                FieldValue fv = new FieldValue(field, value);
                fvlist.Add(fv);
            }

            return this;
        }

        /// <summary>
        /// 添加一个数据
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public UpdateCreator AddUpdate(string fieldName, object value)
        {
            return AddUpdate(new Field(fieldName), value);
        }

        /// <summary>
        /// 添加一个数据字典
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public UpdateCreator AddUpdate(IDictionary<string, object> dict)
        {
            string[] fields = new List<string>(dict.Keys).ToArray();
            object[] values = new List<object>(dict.Values).ToArray();
            return AddUpdate(fields, values);
        }

        /// <summary>
        /// 添加一个数据字典
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public UpdateCreator AddUpdate(IDictionary<Field, object> dict)
        {
            Field[] fields = new List<Field>(dict.Keys).ToArray();
            object[] values = new List<object>(dict.Values).ToArray();
            return AddUpdate(fields, values);
        }

        /// <summary>
        /// 添加一个数据
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        public UpdateCreator AddUpdate(Field[] fields, object[] values)
        {
            if (fields == null || values == null)
            {
                throw new DataException("字段和值不能为null;");
            }

            if (fields.Length != values.Length)
            {
                throw new DataException("字段和值的数量必须一致;");
            }

            int index = 0;
            foreach (Field field in fields)
            {
                AddUpdate(field, values[index]);
                index++;
            }

            return this;
        }

        /// <summary>
        /// 添加一个数据
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="values"></param>
        public UpdateCreator AddUpdate(string[] fieldNames, object[] values)
        {
            if (fieldNames == null || values == null)
            {
                throw new DataException("字段和值不能为null;");
            }

            if (fieldNames.Length != values.Length)
            {
                throw new DataException("字段和值的数量必须一致;");
            }

            int index = 0;
            foreach (string fieldName in fieldNames)
            {
                AddUpdate(fieldName, values[index]);
                index++;
            }

            return this;
        }

        /// <summary>
        /// 移除指定的列
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        public UpdateCreator RemoveUpdate(params string[] fieldNames)
        {
            if (fieldNames == null) return this;

            List<Field> fields = new List<Field>();
            foreach (string fieldName in fieldNames)
            {
                fields.Add(new Field(fieldName));
            }

            return RemoveUpdate(fields.ToArray());
        }

        /// <summary>
        /// 移除指定的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public UpdateCreator RemoveUpdate(params Field[] fields)
        {
            if (fields == null) return this;

            foreach (Field field in fields)
            {
                int count = this.fvlist.RemoveAll(fv =>
                {
                    return string.Compare(fv.Field.OriginalName, field.OriginalName, true) == 0;
                });

                if (count == 0)
                {
                    throw new DataException("指定的字段不存在于Update列表中！");
                }
            }

            return this;
        }

        #endregion
    }
}
