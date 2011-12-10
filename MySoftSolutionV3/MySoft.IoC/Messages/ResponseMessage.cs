using System;
using System.Collections;
using System.Data;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// The response msg.
    /// </summary>
    [Serializable]
    public class ResponseMessage : MessageBase
    {
        private string assemblyName;
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string AssemblyName
        {
            get
            {
                return assemblyName;
            }
            set
            {
                assemblyName = value;
            }
        }

        private object data;

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                error = null;
            }
        }

        private Exception error;

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Error
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
                data = null;
            }
        }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool IsError
        {
            get
            {
                return this.error != null;
            }
        }

        /// <summary>
        /// 是否是业务异常
        /// </summary>
        public bool IsBusinessError
        {
            get
            {
                return this.error is BusinessException;
            }
        }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get
            {
                if (data == null)
                    return 0;
                else
                    return GetCount(data);
            }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public override string Message
        {
            get
            {
                if (error != null)
                {
                    return string.Format("Error: {0} (Type:{1}).", ErrorHelper.GetInnerException(error).Message, base.ReturnType);
                }
                else
                {
                    return string.Format("RowCount: {0} (Type:{1}).", this.Count, base.ReturnType);
                }
            }
        }

        private int GetCount(object val)
        {
            if (val is ICollection)
            {
                return (val as ICollection).Count;
            }
            else if (val is Array)
            {
                return (val as Array).Length;
            }
            else if (val is DataTable)
            {
                return (val as DataTable).Rows.Count;
            }
            else if (val is DataSet)
            {
                var ds = val as DataSet;
                if (ds.Tables.Count > 0)
                {
                    int count = 0;
                    foreach (DataTable table in ds.Tables)
                    {
                        count += table.Rows.Count;
                    }
                    return count;
                }
            }

            return 0;
        }
    }
}