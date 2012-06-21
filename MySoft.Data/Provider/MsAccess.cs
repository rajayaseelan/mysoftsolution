using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using MySoft.Data.SqlServer;

namespace MySoft.Data.MsAccess
{
    /// <summary>
    /// Access 驱动
    /// </summary>
    public class MsAccessProvider : SqlServerProvider
    {
        public MsAccessProvider(string connectionString)
            : base(connectionString, OleDbFactory.Instance)
        {
        }

        /// <summary>
        /// 是否为access驱动
        /// </summary>
        protected override bool AccessProvider
        {
            get { return true; }
        }

        /// <summary>
        /// 是否支持批处理
        /// </summary>
        protected internal override bool SupportBatch
        {
            get { return false; }
        }

        /// <summary>
        /// 返回自动ID的sql语句
        /// </summary>
        protected override string AutoIncrementValue
        {
            get { return "SELECT MAX({0}) FROM {1}"; }
        }

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected override DbParameter CreateParameter(string parameterName, object val)
        {
            OleDbParameter p = new OleDbParameter();
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
            //替换系统日期值
            cmd.CommandText = cmd.CommandText.Replace("GETDATE()", "DATE()");

            foreach (OleDbParameter p in cmd.Parameters)
            {
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue) continue;
                if (p.Value == DBNull.Value) continue;

                if (p.DbType == DbType.String || p.DbType == DbType.AnsiString || p.DbType == DbType.AnsiStringFixedLength)
                {
                    if (p.Size > 4000)
                    {
                        p.OleDbType = OleDbType.LongVarWChar;
                    }
                    else
                    {
                        p.OleDbType = OleDbType.VarWChar;
                    }
                }
                else if (p.DbType == DbType.Binary)
                {
                    if (p.Size > 8000)
                    {
                        p.OleDbType = OleDbType.LongVarWChar;
                    }
                }
                else if (p.DbType == DbType.Date || p.DbType == DbType.Time || p.DbType == DbType.DateTime)
                {
                    p.Value = p.Value.ToString();
                }
            }
        }
    }
}
