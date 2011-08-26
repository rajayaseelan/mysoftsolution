using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using MySoft.Data.Design;
using MySoft.Logger;
using MySoft.Cache;
using System.Diagnostics;

namespace MySoft.Data
{
    public abstract class DbProvider : IDbProvider
    {
        private DbHelper dbHelper;
        private char leftToken;
        private char rightToken;
        private char paramPrefixToken;
        private IExcutingLog logger;

        /// <summary>
        /// 日志依赖
        /// </summary>
        internal IExcutingLog Logger
        {
            set
            {
                logger = value;
            }
        }

        /// <summary>
        /// 缓存依赖
        /// </summary>
        internal ICacheDependent Cache { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        internal int Timeout { get; set; }

        protected DbProvider(string connectionString, System.Data.Common.DbProviderFactory dbFactory, char leftToken, char rightToken, char paramPrefixToken)
        {
            this.leftToken = leftToken;
            this.rightToken = rightToken;
            this.paramPrefixToken = paramPrefixToken;
            this.dbHelper = new DbHelper(connectionString, dbFactory);
            this.Timeout = -1;
        }

        internal void SetDecryptHandler(DecryptEventHandler handler)
        {
            this.dbHelper.SetDecryptHandler(handler);
        }

        #region 增加DbCommand参数

        private string FormatParameter(string parameterName)
        {
            if (parameterName.IndexOf(paramPrefixToken) == 0)
                return parameterName;
            if (parameterName.IndexOf('$') == 0)
                return paramPrefixToken + parameterName.TrimStart('$');
            return paramPrefixToken + parameterName;
        }

        /// <summary>
        /// 给命令添加参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        public void AddParameter(DbCommand cmd, DbParameter[] parameters)
        {
            foreach (DbParameter p in parameters)
            {
                if (p.Value == null)
                    p.Value = DBNull.Value;
                else if (p.Value is Enum)
                {
                    //对枚举进行特殊处理
                    p.Value = Convert.ToInt32(p.Value);
                }

                cmd.Parameters.Add(p);
            }
        }

        /// <summary>
        /// 给命令添加参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public void AddParameter(DbCommand cmd, SQLParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return;

            List<DbParameter> list = new List<DbParameter>();
            foreach (SQLParameter p in parameters)
            {
                DbParameter dbParameter = CreateParameter(p.Name, p.Value);
                dbParameter.Direction = p.Direction;

                list.Add(dbParameter);
            }

            AddParameter(cmd, list.ToArray());
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        public void AddInputParameter(DbCommand cmd, string parameterName, DbType dbType, int size, object value)
        {
            dbHelper.AddInputParameter(cmd, parameterName, dbType, size, value);
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public void AddInputParameter(DbCommand cmd, string parameterName, DbType dbType, object value)
        {
            dbHelper.AddInputParameter(cmd, parameterName, dbType, value);
        }

        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        public void AddOutputParameter(DbCommand cmd, string parameterName, DbType dbType, int size)
        {
            dbHelper.AddOutputParameter(cmd, parameterName, dbType, size);
        }

        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        public void AddOutputParameter(DbCommand cmd, string parameterName, DbType dbType)
        {
            dbHelper.AddOutputParameter(cmd, parameterName, dbType);
        }

        /// <summary>
        /// 添加输入输出参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        public void AddInputOutputParameter(DbCommand cmd, string parameterName, DbType dbType, int size, object value)
        {
            dbHelper.AddInputOutputParameter(cmd, parameterName, dbType, size, value);
        }

        /// <summary>
        /// 添加输入输出参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public void AddInputOutputParameter(DbCommand cmd, string parameterName, DbType dbType, object value)
        {
            dbHelper.AddInputOutputParameter(cmd, parameterName, dbType, value);
        }

        /// <summary>
        /// 添加返回参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        public void AddReturnValueParameter(DbCommand cmd, string parameterName, DbType dbType, int size)
        {
            dbHelper.AddReturnValueParameter(cmd, parameterName, dbType, size);
        }

        /// <summary>
        /// 添加返回参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        public void AddReturnValueParameter(DbCommand cmd, string parameterName, DbType dbType)
        {
            dbHelper.AddReturnValueParameter(cmd, parameterName, dbType);
        }

        public DbParameter GetParameter(DbCommand cmd, string parameterName)
        {
            return dbHelper.GetParameter(cmd, parameterName);
        }

        #endregion

        #region 执行SQL语句

        internal int ExecuteNonQuery(DbCommand cmd, DbTrans trans)
        {
            //调整DbCommand;
            PrepareCommand(cmd);

            //执行命令前的事件
            if (!StartExcuteCommand(cmd))
            {
                throw new DataException("当前数据库操作被拦截 ==> " + GetLog(cmd));
            }

            int retVal = -1;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (trans.Connection == null && trans.Transaction == null)
                {
                    retVal = dbHelper.ExecuteNonQuery(cmd);
                }
                else
                {
                    retVal = dbHelper.ExecuteNonQuery(cmd, trans);
                }
            }
            catch (Exception ex)
            {
                WriteErrorCommand(ex, cmd);

                throw ex;
            }
            finally
            {
                watch.Stop();

                //执行命令后的事件
                EndExcuteCommand(cmd, retVal, (int)watch.ElapsedMilliseconds);
            }

            return retVal;
        }

        internal SourceReader ExecuteReader(DbCommand cmd, DbTrans trans)
        {
            //调整DbCommand;
            PrepareCommand(cmd);

            //执行命令前的事件
            if (!StartExcuteCommand(cmd))
            {
                throw new DataException("当前数据库操作被拦截 ==> " + GetLog(cmd));
            }

            SourceReader retVal = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                IDataReader reader;
                if (trans.Connection == null && trans.Transaction == null)
                {
                    reader = dbHelper.ExecuteReader(cmd);
                }
                else
                {
                    reader = dbHelper.ExecuteReader(cmd, trans);
                }
                retVal = new SourceReader(reader);
            }
            catch (Exception ex)
            {
                WriteErrorCommand(ex, cmd);

                throw ex;
            }
            finally
            {
                watch.Stop();

                //执行命令后的事件
                EndExcuteCommand(cmd, retVal, (int)watch.ElapsedMilliseconds);
            }

            return retVal;
        }

        internal DataSet ExecuteDataSet(DbCommand cmd, DbTrans trans)
        {
            //调整DbCommand;
            PrepareCommand(cmd);

            //执行命令前的事件
            if (!StartExcuteCommand(cmd))
                return null;

            DataSet retVal = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (trans.Connection == null && trans.Transaction == null)
                {
                    retVal = dbHelper.ExecuteDataSet(cmd);
                }
                else
                {
                    retVal = dbHelper.ExecuteDataSet(cmd, trans);
                }
            }
            catch (Exception ex)
            {
                WriteErrorCommand(ex, cmd);

                throw ex;
            }
            finally
            {
                watch.Stop();

                //执行命令后的事件
                EndExcuteCommand(cmd, retVal, (int)watch.ElapsedMilliseconds);
            }

            return retVal;
        }

        internal DataTable ExecuteDataTable(DbCommand cmd, DbTrans trans)
        {
            //调整DbCommand;
            PrepareCommand(cmd);

            //执行命令前的事件
            if (!StartExcuteCommand(cmd))
            {
                throw new DataException("当前数据库操作被拦截 ==> " + GetLog(cmd));
            }

            DataTable retVal = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (trans.Connection == null && trans.Transaction == null)
                {
                    retVal = dbHelper.ExecuteDataTable(cmd);
                }
                else
                {
                    retVal = dbHelper.ExecuteDataTable(cmd, trans);
                }
            }
            catch (Exception ex)
            {
                WriteErrorCommand(ex, cmd);

                throw ex;
            }
            finally
            {
                watch.Stop();

                //执行命令后的事件
                EndExcuteCommand(cmd, retVal, (int)watch.ElapsedMilliseconds);
            }

            return retVal;
        }

        internal object ExecuteScalar(DbCommand cmd, DbTrans trans)
        {
            //调整DbCommand;
            PrepareCommand(cmd);

            //执行命令前的事件
            if (!StartExcuteCommand(cmd))
            {
                throw new DataException("当前数据库操作被拦截 ==> " + GetLog(cmd));
            }

            object retVal = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (trans.Connection == null && trans.Transaction == null)
                {
                    retVal = dbHelper.ExecuteScalar(cmd);
                }
                else
                {
                    retVal = dbHelper.ExecuteScalar(cmd, trans);
                }
            }
            catch (Exception ex)
            {
                WriteErrorCommand(ex, cmd);

                throw ex;
            }
            finally
            {
                watch.Stop();

                //执行命令后的事件
                EndExcuteCommand(cmd, retVal, (int)watch.ElapsedMilliseconds);
            }

            return retVal;
        }

        /// <summary>
        /// 调整DbCommand命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private void PrepareCommand(DbCommand cmd)
        {
            //处理command命令
            cmd.CommandText = FormatCommandText(cmd.CommandText);
            if (Timeout > 0) //如果超时时间设置大于0，则使用指定的超时时间
            {
                cmd.CommandTimeout = Timeout;
            }

            int index = 0;
            foreach (DbParameter p in cmd.Parameters)
            {
                string oldName = p.ParameterName;

                //如果sql方式，才进行参数处理
                if (cmd.CommandType == CommandType.Text)
                {
                    if (oldName.Length >= 100)
                        p.ParameterName = FormatParameter("p" + index++);
                    else
                        p.ParameterName = FormatParameter(oldName);

                    cmd.CommandText = cmd.CommandText.Replace(oldName, p.ParameterName);
                }
                else
                {
                    p.ParameterName = FormatParameter(oldName);
                }
            }

            //调整参数
            PrepareParameter(cmd);
        }

        #endregion

        #region 创建DbConnection及DbParameter

        /// <summary>
        /// 创建DbConnection
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection()
        {
            return dbHelper.CreateConnection();
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <returns></returns>
        public DbParameter CreateParameter()
        {
            return dbHelper.CreateParameter();
        }

        #endregion

        #region 公用增删改方法

        #region Insert

        internal int Insert<T>(Table table, List<FieldValue> fvlist, DbTrans trans, Field identityfield, string autoIncrementName, bool isOutValue, out object retVal)
            where T : Entity
        {
            retVal = null;
            T entity = CoreHelper.CreateInstance<T>();

            int returnValue = 0;
            DbCommand cmd = CreateInsert<T>(table, fvlist, identityfield, autoIncrementName);
            string tableName = table == null ? entity.GetTable().Name : table.Name;

            if ((IField)identityfield != null)
            {
                //Access获取最大的记录号
                if (AccessProvider)
                {
                    returnValue = ExecuteNonQuery(cmd, trans);
                    if (isOutValue)
                    {
                        cmd = CreateSqlCommand(string.Format(AutoIncrementValue, identityfield.Name, tableName));
                        retVal = ExecuteScalar(cmd, trans);
                    }
                }
                else
                {
                    if (AllowAutoIncrement)
                    {
                        returnValue = ExecuteNonQuery(cmd, trans);

                        if (isOutValue)
                        {
                            if (!string.IsNullOrEmpty(autoIncrementName))
                            {
                                cmd = CreateSqlCommand(string.Format(AutoIncrementValue, autoIncrementName));
                                retVal = ExecuteScalar(cmd, trans);
                            }
                        }
                    }
                    else
                    {
                        if (isOutValue)
                        {
                            cmd.CommandText += ";" + string.Format(AutoIncrementValue, identityfield.Name, tableName);
                            retVal = ExecuteScalar(cmd, trans);
                            returnValue = 1;
                        }
                        else
                        {
                            returnValue = ExecuteNonQuery(cmd, trans);
                        }
                    }
                }
            }
            else
            {
                returnValue = ExecuteNonQuery(cmd, trans);
            }

            return returnValue;
        }

        internal DbCommand CreateInsert<T>(Table table, List<FieldValue> fvlist, Field identityfield, string autoIncrementName)
            where T : Entity
        {
            T entity = CoreHelper.CreateInstance<T>();

            if (entity.GetReadOnly())
            {
                throw new DataException("只读实体" + typeof(T).Name + "只能用于查询！");
            }

            //移除缓存
            if (this.Cache != null) Cache.RemoveCache(typeof(T));

            string tableName = table == null ? entity.GetTable().Name : table.Name;

            List<SQLParameter> plist = new List<SQLParameter>();
            StringBuilder sbsql = new StringBuilder();
            StringBuilder sbparam = new StringBuilder();

            if (AllowAutoIncrement)
            {
                //如果标识列和标识名称都不为null
                if ((IField)identityfield != null && !string.IsNullOrEmpty(autoIncrementName))
                {
                    string identityName = GetAutoIncrement(autoIncrementName);
                    bool exist = false;
                    fvlist.ForEach(fv =>
                    {
                        if (fv.IsIdentity)
                        {
                            fv.Value = new DbValue(identityName);
                            fv.IsIdentity = false;
                            exist = true;
                        }
                    });

                    if (!exist)
                    {
                        object value = new DbValue(identityName);
                        FieldValue fv = new FieldValue(identityfield, value);
                        fvlist.Insert(0, fv);
                    }
                }
            }

            sbsql.Append("INSERT INTO " + tableName + "(");
            sbparam.Append(" VALUES (");

            fvlist.ForEach(fv =>
            {
                if (fv.IsIdentity)
                    return;

                sbsql.Append(fv.Field.At((string)null).Name);
                if (CheckValue(fv.Value))
                {
                    sbparam.Append(DataHelper.FormatValue(fv.Value));
                }
                else
                {
                    SQLParameter p = null;
                    if (CoreHelper.CheckStructType(fv.Value))
                        p = CreateOrmParameter(DataHelper.FormatValue(fv.Value));
                    else
                        p = CreateOrmParameter(fv.Value);

                    sbparam.Append(p.Name);
                    plist.Add(p);
                }

                sbsql.Append(",");
                sbparam.Append(",");
            });

            sbsql.Remove(sbsql.Length - 1, 1).Append(")");
            sbparam.Remove(sbparam.Length - 1, 1).Append(")");

            string cmdText = string.Format("{0}{1}", sbsql, sbparam);

            return CreateSqlCommand(cmdText, plist.ToArray());
        }

        #endregion

        #region Delete

        internal int Delete<T>(Table table, WhereClip where, DbTrans trans)
            where T : Entity
        {
            DbCommand cmd = CreateDelete<T>(table, where);
            return ExecuteNonQuery(cmd, trans);
        }

        internal DbCommand CreateDelete<T>(Table table, WhereClip where)
            where T : Entity
        {
            T entity = CoreHelper.CreateInstance<T>();

            if (entity.GetReadOnly())
            {
                throw new DataException("只读实体" + typeof(T).Name + "只能用于查询！");
            }

            if ((object)where == null)
            {
                throw new DataException("删除条件不能为null！");
            }

            //移除缓存
            if (this.Cache != null) Cache.RemoveCache(typeof(T));

            StringBuilder sb = new StringBuilder();
            string tableName = table == null ? entity.GetTable().Name : table.Name;
            sb.Append("DELETE FROM " + tableName);

            if (!DataHelper.IsNullOrEmpty(where))
            {
                sb.Append(" WHERE " + where.ToString());
            }

            return CreateSqlCommand(sb.ToString(), where.Parameters);
        }

        #endregion

        #region Update

        internal int Update<T>(Table table, List<FieldValue> fvlist, WhereClip where, DbTrans trans)
            where T : Entity
        {
            //如果没有设置更新的字段，抛出异常
            if (fvlist.FindAll(fv => { return fv.IsChanged; }).Count == 0)
            {
                //throw new DataException("更新数据异常，没有需要更新的数据！");
                return -1; //-1表示没有需要更新的列
            }

            DbCommand cmd = CreateUpdate<T>(table, fvlist, where);
            return ExecuteNonQuery(cmd, trans);
        }

        internal DbCommand CreateUpdate<T>(Table table, List<FieldValue> fvlist, WhereClip where)
            where T : Entity
        {
            T entity = CoreHelper.CreateInstance<T>();

            if (entity.GetReadOnly())
            {
                throw new DataException("只读实体" + typeof(T).Name + "只能用于查询！");
            }

            if ((object)where == null)
            {
                throw new DataException("更新条件不能为null！");
            }

            //移除缓存
            if (this.Cache != null) Cache.RemoveCache(typeof(T));

            string tableName = table == null ? entity.GetTable().Name : table.Name;

            List<SQLParameter> plist = new List<SQLParameter>();
            StringBuilder sb = new StringBuilder();

            sb.Append("UPDATE " + tableName + " SET ");

            fvlist.ForEach(fv =>
            {
                if (fv.IsPrimaryKey || fv.IsIdentity)
                    return;

                if (fv.IsChanged)
                {
                    if (CheckValue(fv.Value))
                    {
                        sb.Append(fv.Field.At((string)null).Name + " = " + DataHelper.FormatValue(fv.Value));
                    }
                    else
                    {
                        SQLParameter p = null;
                        if (CoreHelper.CheckStructType(fv.Value))
                            p = CreateOrmParameter(DataHelper.FormatValue(fv.Value));
                        else
                            p = CreateOrmParameter(fv.Value);

                        sb.Append(fv.Field.At((string)null).Name + " = " + p.Name);
                        plist.Add(p);
                    }

                    sb.Append(",");
                }
            });

            sb.Remove(sb.Length - 1, 1);

            if (!DataHelper.IsNullOrEmpty(where))
            {
                sb.Append(" WHERE " + where.ToString());
                plist.AddRange(where.Parameters);
            }

            return CreateSqlCommand(sb.ToString(), plist.ToArray());
        }

        #endregion

        /// <summary>
        /// 返回最终排序的SQL
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        internal string FormatCommandText(string cmdText)
        {
            return DataHelper.FormatSQL(cmdText, leftToken, rightToken, AccessProvider);
        }

        internal DbCommand CreateSqlCommand(string cmdText)
        {
            return dbHelper.CreateSqlStringCommand(cmdText);
        }

        /// <summary>
        /// 创建SQL命令
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        internal DbCommand CreateSqlCommand(string cmdText, SQLParameter[] parameters)
        {
            DbCommand cmd = dbHelper.CreateSqlStringCommand(cmdText);
            AddParameter(cmd, parameters);

            return cmd;
        }

        /// <summary>
        /// 创建存储过程命令
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        internal DbCommand CreateProcCommand(string procName)
        {
            return dbHelper.CreateStoredProcCommand(procName);
        }

        #endregion

        #region ILogable Members

        /// <summary>
        /// Writes the log.
        /// </summary>
        /// <param name="command">The command.</param>
        private bool StartExcuteCommand(DbCommand command)
        {
            if (logger != null)
            {
                try
                {
                    //输出sql日志
                    logger.WriteLog(GetLog(command), LogType.Information);

                    string cmdText = command.CommandText;
                    List<SQLParameter> parameters = new List<SQLParameter>();
                    foreach (DbParameter p in command.Parameters)
                    {
                        parameters.Add(new SQLParameter(p));
                    }
                    return logger.StartExcute(cmdText, parameters.ToArray());
                }
                catch { }
            }

            return true;
        }

        /// <summary>
        /// 结束时执行的操作
        /// </summary>
        /// <param name="command"></param>
        private void EndExcuteCommand(DbCommand command, object result, int elapsedTime)
        {
            if (logger != null)
            {
                try
                {
                    string cmdText = command.CommandText;
                    List<SQLParameter> parameters = new List<SQLParameter>();
                    foreach (DbParameter p in command.Parameters)
                    {
                        parameters.Add(new SQLParameter(p));
                    }
                    logger.EndExcute(cmdText, parameters.ToArray(), result, elapsedTime);
                }
                catch { }
            }
        }

        /// <summary>
        /// Writes the exception log.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="command"></param>
        private void WriteErrorCommand(Exception ex, DbCommand command)
        {
            if (logger != null)
            {
                try
                {
                    //输出错误日志
                    logger.WriteLog(GetLog(command) + " ==> Error: " + ex.Message, LogType.Error);

                    var exception = new DataException(GetLog(command), ex);
                    logger.WriteError(exception);
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取输出的日志
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected string GetLog(DbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("{0}\t{1}\t\r\n", command.CommandType, command.CommandText));
            if (command.Parameters != null && command.Parameters.Count > 0)
            {
                sb.Append("Parameters:\r\n");
                foreach (DbParameter p in command.Parameters)
                {
                    if (p.Size > 0)
                        sb.Append(string.Format("{0}[{1}][{2}({3})] = {4}\r\n", p.ParameterName, p.DbType, GetParameterType(p), p.Size, p.Value));
                    else
                        sb.Append(string.Format("{0}[{1}][{2}] = {3}\r\n", p.ParameterName, p.DbType, GetParameterType(p), p.Value));
                }
            }
            sb.Append("\r\n");

            return sb.ToString();
        }

        #endregion

        #region 需重写的方法

        /// <summary>
        /// 获取IdentityName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual string GetAutoIncrement(string name)
        {
            return name;
        }

        /// <summary>
        /// 创建分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="itemCount"></param>
        /// <param name="skipCount"></param>
        /// <returns></returns>
        protected internal virtual QuerySection<T> CreatePageQuery<T>(QuerySection<T> query, int itemCount, int skipCount)
            where T : Entity
        {
            if (skipCount == 0)
            {
                ((IPaging)query).Prefix("top " + itemCount);
                return query;
            }
            else
            {
                ((IPaging)query).Prefix("top " + itemCount);

                Field pagingField = query.PagingField;

                if ((IField)pagingField == null)
                {
                    throw new DataException("SqlServer2000或Access请使用SetPagingField设定分页主键！");
                }

                QuerySection<T> jquery = query.CreateQuery<T>();
                ((IPaging)jquery).Prefix("top " + skipCount);
                jquery.Select(pagingField);

                //如果是联合查询，则需要符值整个QueryString
                if (query.UnionQuery)
                {
                    jquery.QueryString = query.QueryString;
                }

                query.PageWhere = !pagingField.In(jquery);

                return query;
            }
        }

        #endregion

        #region 抽像方法

        /// <summary>
        /// 是否为Access驱动
        /// </summary>
        protected virtual bool AccessProvider
        {
            get { return false; }
        }

        /// <summary>
        /// 是否使用自增列
        /// </summary>
        protected virtual bool AllowAutoIncrement
        {
            get { return false; }
        }

        /// <summary>
        /// 是否支持批处理
        /// </summary>
        protected internal abstract bool SupportBatch { get; }

        /// <summary>
        /// 返回自动ID的sql语句
        /// </summary>
        protected abstract string AutoIncrementValue { get; }

        /// <summary>
        /// 获取参数类型
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected abstract object GetParameterType(DbParameter parameter);

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected abstract DbParameter CreateParameter(string parameterName, object val);

        /// <summary>
        /// 调整DbCommand参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected abstract void PrepareParameter(DbCommand cmd);

        #endregion

        /// <summary>
        /// 创建OrmParameter
        /// </summary>
        /// <returns></returns>
        private SQLParameter CreateOrmParameter(object value)
        {
            string pName = CoreHelper.MakeUniqueKey(100, "$");
            return new SQLParameter(pName, value);
        }

        /// <summary>
        /// 检测是否为非法值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool CheckValue(object value)
        {
            if (value == null || value == DBNull.Value || value is Field || value is DbValue)
            {
                return true;
            }
            return false;
        }
    }
}
