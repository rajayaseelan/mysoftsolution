using System;
using System.Data;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace MySoft.Data.PostgreSQL
{
    /// <summary>
    /// PostgreSQL 驱动
    /// </summary>
    public class PostgreSQLProvider : DbProvider
    {
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="connectionString"></param>
        public PostgreSQLProvider(string connectionString)
            : base(connectionString, NpgsqlFactory.Instance, '`', '`', '?')
        {
        }

        /// <summary>
        /// 是否支持批处理
        /// </summary>
        protected override bool SupportBatch
        {
            get { return false; }
        }

        /// <summary>
        /// 是否使用自增列
        /// </summary>
        protected override bool AllowAutoIncrement
        {
            get { return true; }
        }

        /// <summary>
        /// 返回自动ID的sql语句
        /// </summary>
        protected override string AutoIncrementValue
        {
            get { return "SELECT CURRVAL('{0}')"; }
        }

        /// <summary>
        /// 格式化IdentityName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override string GetAutoIncrement(string name)
        {
            return string.Format("NEXTVAL({0})", name);
        }

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected override DbParameter CreateParameter(string parameterName, object val)
        {
            NpgsqlParameter p = new NpgsqlParameter();
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

            foreach (NpgsqlParameter p in cmd.Parameters)
            {
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue) continue;
                if (p.Value == DBNull.Value) continue;

                if (p.DbType == DbType.Guid)
                {
                    p.NpgsqlDbType = NpgsqlDbType.Char;
                    p.Size = 36;
                    p.Value = p.Value.ToString();
                }
                else if (p.DbType == DbType.String || p.DbType == DbType.AnsiString || p.DbType == DbType.AnsiStringFixedLength)
                {
                    if (p.Size > 4000)
                    {
                        p.NpgsqlDbType = NpgsqlDbType.Text;
                    }
                    else
                    {
                        p.NpgsqlDbType = NpgsqlDbType.Varchar;
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
                string suffix = string.Format("LIMIT {0} OFFSET {1}", itemCount, skipCount);
                ((IPaging)query).End(suffix);
                return query;
            }
        }
    }
}
