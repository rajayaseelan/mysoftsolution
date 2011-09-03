using System;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace MySoft.Data.MySql
{
    /// <summary>
    /// MySql 驱动
    /// </summary>
    public class MySqlProvider : DbProvider
    {
        public MySqlProvider(string connectionString)
            : base(connectionString, MySqlClientFactory.Instance, '`', '`', '?')
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
            get { return "SELECT LAST_INSERT_ID()"; }
        }

        /// <summary>
        /// 获取参数类型
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected override object GetParameterType(DbParameter parameter)
        {
            return (parameter as MySqlParameter).MySqlDbType;
        }

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected override DbParameter CreateParameter(string parameterName, object val)
        {
            MySqlParameter p = new MySqlParameter();
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
            cmd.CommandText = cmd.CommandText.Replace("GETDATE()", "NOW()");

            foreach (MySqlParameter p in cmd.Parameters)
            {
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue) continue;
                if (p.Value == DBNull.Value) continue;

                if (p.DbType == DbType.Guid)
                {
                    p.MySqlDbType = MySqlDbType.VarChar;
                    p.Size = 36;
                    p.Value = p.Value.ToString();
                }
                else if (p.DbType == DbType.String || p.DbType == DbType.AnsiString || p.DbType == DbType.AnsiStringFixedLength)
                {
                    if (p.Size > 4000)
                    {
                        p.MySqlDbType = MySqlDbType.Text;
                    }
                    else
                    {
                        p.MySqlDbType = MySqlDbType.VarChar;
                    }
                }
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
                string suffix = string.Format("LIMIT {0} ,{1}", skipCount, itemCount);
                ((IPaging)query).End(suffix);
                return query;
            }
        }
    }
}
