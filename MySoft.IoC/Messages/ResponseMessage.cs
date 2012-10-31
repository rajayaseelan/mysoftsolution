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
        private long elapsedTime;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public long ElapsedTime
        {
            get
            {
                return elapsedTime;
            }
            set
            {
                elapsedTime = value;

                if (_value != null)
                {
                    if (_value is InvokeData)
                    {
                        (_value as InvokeData).ElapsedTime = value;
                    }
                }
            }
        }

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

                if (_count < 0)
                {
                    _count = GetCount(_value);
                }
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

        private int _count = -1;

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get
            {
                if (_count < 0)
                {
                    _count = GetCount(_value);
                }

                return _count;
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
                    var ex = ErrorHelper.GetInnerException(_error);
                    return string.Format("(Type: {0}) Elapsed Time: {1} ms, Error: {2}", base.ReturnType, this.elapsedTime, ex.Message);
                }
                else
                {
                    return string.Format("(Type: {0}) Elapsed Time: {1} ms, RowCount: {2} row(s)", base.ReturnType, this.elapsedTime, this.Count);
                }
            }
        }

        private int GetCount(object val)
        {
            if (val == null) return 0;

            try
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
            }
            catch (Exception ex)
            {
            }

            return 1;
        }
    }
}