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
        private object _value;

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _error = null;
            }
        }

        private Exception _error;

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                _value = null;
            }
        }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool IsError
        {
            get
            {
                return this._error != null;
            }
        }

        /// <summary>
        /// 是否是业务异常
        /// </summary>
        public bool IsBusinessError
        {
            get
            {
                return this._error is BusinessException;
            }
        }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get
            {
                if (_value == null)
                    return 0;
                else
                    return GetCount(_value);
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
                if (_error != null)
                {
                    return string.Format("Error: {0} (Type:{1}).", ErrorHelper.GetInnerException(_error).Message, base.ReturnType);
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

            return 1;
        }
    }
}