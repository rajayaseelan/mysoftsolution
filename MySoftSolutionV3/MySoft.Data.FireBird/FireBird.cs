using System;
using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace MySoft.Data.FireBird
{
    /// <summary>
    /// FireBird 驱动
    /// </summary>
    public class FireBirdProvider : DbProvider
    {
        public FireBirdProvider(string connectionString)
            : base(connectionString, FirebirdClientFactory.Instance, ' ', ' ', '@')
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
            get { return "SELECT GEN_ID({0}, 1) FROM RDB$DATABASE"; }
        }

        /// <summary>
        /// 格式化IdentityName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override string GetAutoIncrement(string name)
        {
            return string.Format("GEN_ID({0}, 1)", name);
        }

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected override DbParameter CreateParameter(string parameterName, object val)
        {
            FbParameter p = new FbParameter();
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
        /// 调整命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected override void PrepareParameter(DbCommand cmd)
        {
            //replace "N'" to "'"
            cmd.CommandText = cmd.CommandText.Replace("N'", "'");

            //替换系统日期值
            cmd.CommandText = cmd.CommandText.Replace("GETDATE()", "CURRENT_TIMESTAMP");

            foreach (FbParameter p in cmd.Parameters)
            {
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue) continue;
                if (p.Value == DBNull.Value) continue;

                if (p.DbType == DbType.String || p.DbType == DbType.AnsiString || p.DbType == DbType.AnsiStringFixedLength)
                {
                    if (p.Size > 4000)
                    {
                        p.FbDbType = FbDbType.Text;
                    }
                    else
                    {
                        p.FbDbType = FbDbType.VarChar;
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
                ((IPaging)query).Prefix("FIRST " + itemCount);
                return query;
            }
            else
            {
                string prefix = string.Format("FIRST {0} SKIP {1}", itemCount, skipCount);
                ((IPaging)query).Prefix(prefix);
                return query;
            }
        }
    }
}
