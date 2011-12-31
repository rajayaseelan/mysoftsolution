using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Threading;

namespace MySoft.Data
{
    /// <summary>
    /// 批处理
    /// </summary>
    public class DbBatch : IDbBatch
    {
        private bool useBatch = false;
        private int batchSize;
        private DbProvider dbProvider;
        private DbTrans dbTrans;
        private List<DbCommand> commandList = new List<DbCommand>();

        internal DbBatch(DbProvider dbProvider, DbTrans dbTran, int batchSize)
        {
            this.dbProvider = dbProvider;
            this.batchSize = batchSize;
            this.dbTrans = dbTran;
            this.useBatch = true;

            if (batchSize < 0 || batchSize > 100)
            {
                throw new DataException("请设置batchSize的值在1-100之间！");
            }
        }

        internal DbBatch(DbProvider dbProvider, DbTrans dbTran)
        {
            this.dbProvider = dbProvider;
            this.dbTrans = dbTran;
            this.useBatch = false;
        }

        #region Trans操作

        /// <summary>
        /// 执行批处理操作
        /// </summary>
        /// <param name="errors">输出的错误</param>
        /// <returns></returns>
        public int Execute(out IList<DataException> errors)
        {
            //实例化errors
            errors = new List<DataException>();
            int rowCount = 0;

            if (commandList.Count == 0)
            {
                //如果命令列表为空，则直接返回
                return rowCount;
            }

            //Access不能进行多任务处理
            if (!dbProvider.SupportBatch)
            {
                foreach (DbCommand cmd in commandList)
                {
                    try
                    {
                        //执行成功，则马上退出
                        rowCount += dbProvider.ExecuteNonQuery(cmd, dbTrans);
                    }
                    catch (DbException ex)
                    {
                        errors.Add(new DataException(ex.Message, ex));
                    }

                    //执行一次休眠一下
                    Thread.Sleep(10);
                }
            }
            else
            {
                int size = Convert.ToInt32(Math.Ceiling(commandList.Count * 1.0 / batchSize));
                for (int index = 0; index < size; index++)
                {
                    DbCommand mergeCommand = dbProvider.CreateSqlCommand("init");
                    List<DbCommand> cmdList = new List<DbCommand>();
                    int getSize = batchSize;
                    if ((index + 1) * batchSize > commandList.Count)
                    {
                        getSize = commandList.Count - index * batchSize;
                    }
                    cmdList.AddRange(commandList.GetRange(index * batchSize, getSize));
                    StringBuilder sb = new StringBuilder();

                    int pIndex = 0;
                    foreach (DbCommand cmd in cmdList)
                    {
                        string cmdText = cmd.CommandText;
                        foreach (DbParameter p in cmd.Parameters)
                        {
                            DbParameter newp = (DbParameter)((ICloneable)p).Clone();
                            mergeCommand.Parameters.Add(newp);
                        }
                        sb.Append(cmdText);
                        sb.Append(";\r\n");

                        pIndex++;
                    }

                    mergeCommand.CommandText = sb.ToString();

                    try
                    {
                        //执行成功，则马上退出
                        rowCount += dbProvider.ExecuteNonQuery(mergeCommand, dbTrans);
                    }
                    catch (DbException ex)
                    {
                        errors.Add(new DataException(ex.Message, ex));
                    }

                    //执行一次休眠一下
                    Thread.Sleep(10);
                }
            }

            //结束处理,清除命令列表
            commandList.Clear();

            return rowCount;
        }

        #region 增删改操作(指定表名)

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Save<T>(Table table, T entity)
            where T : Entity
        {
            List<FieldValue> fvlist = entity.GetFieldValues();
            WhereClip where = null;
            int value = 0;

            #region 实体验证处理

            //对实体进行验证
            ValidateResult result = entity.Validation();
            if (!result.IsSuccess)
            {
                List<string> msgs = new List<string>();
                foreach (var msg in result.InvalidValues)
                {
                    msgs.Add(msg.Field.PropertyName + " : " + msg.Message);
                }
                string message = string.Join("\r\n", msgs.ToArray());
                throw new DataException(message);
            }

            #endregion

            //获取实体状态
            EntityState state = entity.As<IEntityBase>().GetObjectState();

            //判断实体的状态
            if (state == EntityState.Insert)
            {
                object retVal;
                fvlist.RemoveAll(fv => fv.IsChanged);

                value = Insert<T>(table, fvlist, out retVal);

                //给标识列赋值
                if (retVal != null)
                {
                    CoreHelper.SetPropertyValue(entity, entity.IdentityField.PropertyName, retVal);
                }
            }
            else
            {
                where = DataHelper.GetPkWhere<T>(entity.GetTable(), entity);
                fvlist.RemoveAll(fv => !fv.IsChanged || fv.IsIdentity || fv.IsPrimaryKey);

                value = Update<T>(table, fvlist, where);
            }
            entity.AttachSet();

            return value;
        }

        #region 私有方法

        /// <summary>
        /// 添加命令到队列中
        /// </summary>
        /// <param name="cmd"></param>
        private void AddCommand(DbCommand cmd)
        {
            commandList.Add(cmd);
        }

        /// <summary>
        /// 插入值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fvlist"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        internal int Insert<T>(Table table, List<FieldValue> fvlist, out object retVal)
            where T : Entity
        {
            int val = 0;
            retVal = null;

            T entity = CoreHelper.CreateInstance<T>();
            if (useBatch)
            {
                DbCommand cmd = dbProvider.CreateInsert<T>(table, fvlist, entity.IdentityField, entity.SequenceName);
                AddCommand(cmd);
            }
            else
            {
                val = dbProvider.Insert<T>(table, fvlist, dbTrans, entity.IdentityField, entity.SequenceName, true, out retVal);
            }

            return val;
        }

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fvlist"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private int Update<T>(Table table, List<FieldValue> fvlist, WhereClip where)
            where T : Entity
        {
            int val = 0;

            if (useBatch)
            {
                DbCommand cmd = dbProvider.CreateUpdate<T>(table, fvlist, where);
                AddCommand(cmd);
            }
            else
            {
                val = dbProvider.Update<T>(table, fvlist, where, dbTrans);
            }

            return val;
        }

        #endregion

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, T entity)
             where T : Entity
        {
            WhereClip where = DataHelper.GetPkWhere<T>(table, entity);
            int val = 0;
            if (useBatch)
            {
                DbCommand cmd = dbProvider.CreateDelete<T>(table, where);
                AddCommand(cmd);
            }
            else
            {
                val = dbProvider.Delete<T>(table, where, dbTrans);
            }
            return val;
        }

        /// <summary>
        /// 按主键删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, params object[] pkValues)
            where T : Entity
        {
            WhereClip where = DataHelper.GetPkWhere<T>(table, pkValues);
            return dbProvider.Delete<T>(table, where, dbTrans);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(Table table, T entity, params Field[] fields)
            where T : Entity
        {
            if (Exists<T>(table, entity, fields))
            {
                if (fields != null && fields.Length > 0)
                {
                    var list = entity.As<IEntityInfo>().UpdateFieldValues.ToList();
                    WhereClip where = DataHelper.GetAllWhere<T>(table, entity, fields);
                    return Update<T>(table, list, where);
                }
                else
                {
                    entity.Attach();
                }
            }
            else
            {
                entity.Detach();
            }

            return Save<T>(table, entity);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(Table table, FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            if (Exists<T>(table, where))
                return Update<T>(table, fvs.ToList(), where);
            else
            {
                object retVal;
                return Insert<T>(table, fvs.ToList(), out retVal);
            }
        }

        #endregion

        #region 增删改操作

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Save<T>(T entity)
            where T : Entity
        {
            return Save<T>(null, entity);
        }

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Delete<T>(T entity)
             where T : Entity
        {
            return Delete<T>(null, entity);
        }

        /// <summary>
        /// 按主键删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public int Delete<T>(params object[] pkValues)
            where T : Entity
        {
            return Delete<T>(null, pkValues);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(T entity, params Field[] fields)
            where T : Entity
        {
            return InsertOrUpdate(null, entity, fields);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            return InsertOrUpdate<T>(null, fvs, where);
        }

        #endregion

        #endregion

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool Exists<T>(Table table, T entity, Field[] fields)
            where T : Entity
        {
            WhereClip where = WhereClip.None;
            if (fields != null && fields.Length > 0)
                where = DataHelper.GetAllWhere<T>(table, entity, fields);
            else
                where = DataHelper.GetPkWhere<T>(table, entity);
            return Exists<T>(table, where);
        }

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private bool Exists<T>(Table table, WhereClip where)
            where T : Entity
        {
            if (where == null || where == WhereClip.None)
            {
                throw new DataException("在判断记录是否存在时出现异常，条件为null或WhereClip.None！");
            }

            FromSection<T> fs = new FromSection<T>(dbProvider, dbTrans, table);
            return fs.Where(where).Count() > 0;
        }
    }
}
