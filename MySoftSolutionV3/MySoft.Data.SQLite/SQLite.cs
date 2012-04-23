using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace MySoft.Data.SQLite
{
    /// <summary>
    /// SQLite 驱动
    /// </summary>
    public class SQLiteProvider : DbProvider
    {
        public SQLiteProvider(string connectionString)
            : base(connectionString, SQLiteFactory.Instance, '[', ']', '?')
        {
        }

        /// <summary>
        /// 是否支持批处理
        /// </summary>
        protected override bool SupportBatch
        {
            get { return true; }
        }

        /// <summary>
        /// 返回自动ID的sql语句
        /// </summary>
        protected override string AutoIncrementValue
        {
            get { return "SELECT LAST_INSERT_ROWID()"; }
        }

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected override DbParameter CreateParameter(string parameterName, object val)
        {
            SQLiteParameter p = new SQLiteParameter();
            p.ParameterName = parameterName;
            if (val == null || val == DBNull.Value)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                if (val is Enum)
                {
                    p.Value = Convert.ToInt32(val);
                }
                else
                {
                    p.Value = val;
                }
            }
            return p;
        }

        /// <summary>
        /// 调整DbCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected override void PrepareParameter(DbCommand cmd)
        {
            //替换系统日期值
            cmd.CommandText = cmd.CommandText.Replace("GETDATE()", "CURRENT_TIMESTAMP");

            for (int index = cmd.Parameters.Count - 1; index >= 0; index--)
            {
                var p = cmd.Parameters[index];

                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue) continue;
                if (p.Value == DBNull.Value) continue;

                //SQLite不带参数名
                cmd.CommandText = cmd.CommandText.Replace(p.ParameterName, "?");
                p.ParameterName = "?";
            }
        }

        /// <summary>
        /// 创建分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="itemCount"></param>
        /// <param name="skipCount"></param>
        /// <returns></returns>
        protected override QuerySection<T> CreatePageQuery<T>(QuerySection<T> query, int itemCount, int skipCount)
        {
            if (skipCount == 0)
            {
                ((IPaging)query).End("LIMIT " + itemCount);
                return query;
            }
            else
            {
                string suffix = string.Format("LIMIT {0} OFFSET {1}", itemCount, skipCount);
                ((IPaging)query).End(suffix);
                return query;
            }
        }
    }
}
