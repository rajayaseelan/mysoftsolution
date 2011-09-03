using System;
using System.Collections.Generic;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class Entity : EntityBase, IEntity
    {
        #region 公用方法

        #region 实体更新插入操作(对原有的字段)

        #region 移除指定的列

        /// <summary>
        /// 将实体置为修改状态并移除指定的列
        /// </summary>
        /// <param name="removeFields"></param>
        public void Attach(params Field[] removeFields)
        {
            lock (this)
            {
                isUpdate = true;
                RemoveFieldsToUpdate(removeFields);
            }
        }

        /// <summary>
        /// 将实体置为插入状态并移除指定的列
        /// </summary>
        /// <param name="removeFields"></param>
        public void Detach(params Field[] removeFields)
        {
            lock (this)
            {
                isUpdate = false;
                RemoveFieldsToInsert(removeFields);
            }
        }

        #endregion

        #endregion

        #region 实体更新插入操作(对所有的字段)

        #region 移除指定的列

        /// <summary>
        /// 将实体置为修改状态并移除指定的列(所有字段)
        /// </summary>
        /// <param name="removeFields"></param>
        public void AttachAll(params Field[] removeFields)
        {
            lock (this)
            {
                AddFieldsToUpdate(this.GetFields());
                Attach(removeFields);
            }
        }

        /// <summary>
        /// 将实体置为插入状态并移除指定的列(所有字段)
        /// </summary>
        /// <param name="removeFields"></param>
        public void DetachAll(params Field[] removeFields)
        {
            lock (this)
            {
                removeinsertlist.Clear();
                Detach(removeFields);
            }
        }

        #endregion

        #region 指定移除以外的列

        /// <summary>
        /// 将实体置为修改状态并更新指定的列
        /// </summary>
        /// <param name="setFields"></param>
        public void AttachSet(params Field[] setFields)
        {
            lock (this)
            {
                updatelist.Clear();
                AddFieldsToUpdate(setFields);
                Attach();
            }
        }

        /// <summary>
        /// 将实体置为插入状态并插入指定的列
        /// </summary>
        /// <param name="setFields"></param>
        public void DetachSet(params Field[] setFields)
        {
            lock (this)
            {
                removeinsertlist.Clear();
                List<Field> fields = new List<Field>(setFields);
                List<Field> list = new List<Field>(this.GetFields());
                list.RemoveAll(f =>
                {
                    if (fields.Contains(f)) return true;
                    return false;
                });
                RemoveFieldsToInsert(list);
                Detach();
            }
        }

        #endregion

        #endregion

        #endregion

        #region 重载的方法

        /// <summary>
        /// 增加单个属性到修改列表
        /// </summary>
        /// <param name="field"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected void OnPropertyValueChange(Field field, object oldValue, object newValue)
        {
            lock (this)
            {
                //如果是从数据库中读出的实体，则判断是否需要更新
                if (isFromDB)
                {
                    if (oldValue != null)
                    {
                        if (!oldValue.Equals(newValue))
                        {
                            AddFieldsToUpdate(field);
                        }
                    }
                    else if (newValue != null)
                    {
                        if (!newValue.Equals(oldValue))
                        {
                            AddFieldsToUpdate(field);
                        }
                    }
                }
                else
                {
                    AddFieldsToUpdate(field);
                }
            }
        }

        /// <summary>
        /// 添加字段到修改列表
        /// </summary>
        /// <param name="fields"></param>
        private void AddFieldsToUpdate(IList<Field> fields)
        {
            if (fields == null || fields.Count == 0) return;

            foreach (Field field in fields)
            {
                AddFieldsToUpdate(field);
            }
        }

        /// <summary>
        /// 添加字段到修改列表
        /// </summary>
        /// <param name="fields"></param>
        private void AddFieldsToUpdate(Field field)
        {
            if (!updatelist.Exists(p => p.Name == field.Name))
            {
                lock (updatelist)
                {
                    updatelist.Add(field);
                }
            }
        }

        /// <summary>
        /// 移除字段到修改列表
        /// </summary>
        /// <param name="fields"></param>
        private void RemoveFieldsToUpdate(IList<Field> fields)
        {
            if (fields == null || fields.Count == 0) return;

            foreach (Field field in fields)
            {
                lock (updatelist)
                {
                    updatelist.RemoveAll(p => p.Name == field.Name);
                }
            }
        }

        /// <summary>
        /// 移除字段到修改列表
        /// </summary>
        /// <param name="fields"></param>
        private void RemoveFieldsToInsert(IList<Field> fields)
        {
            if (fields == null || fields.Count == 0) return;

            foreach (Field field in fields)
            {
                if (!removeinsertlist.Exists(p => p.Name == field.Name))
                {
                    lock (removeinsertlist)
                    {
                        removeinsertlist.Add(field);
                    }
                }
            }
        }

        #endregion
    }
}
