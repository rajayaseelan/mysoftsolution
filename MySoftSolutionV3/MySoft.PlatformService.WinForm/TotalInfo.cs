using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.PlatformService.WinForm
{
    /// <summary>
    /// 统计信息
    /// </summary>
    public class TotalInfo
    {
        public string AppName { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public long ElapsedTime { get; set; }
        public int Count { get; set; }
        public int Times { get; set; }
    }

    /// <summary>
    /// 用户排序
    /// </summary>
    public class OrderTotalInfo : IComparable<OrderTotalInfo>
    {
        public long ElapsedTime { get; set; }
        public int Count { get; set; }
        public int Times { get; set; }

        #region IComparable<OrderTotalInfo> 成员

        public int CompareTo(OrderTotalInfo other)
        {
            var ret = this.ElapsedTime.CompareTo(other.ElapsedTime);
            if (ret == 0)
            {
                ret = this.Times.CompareTo(other.Times);
                if (ret == 0)
                    ret = this.Count.CompareTo(other.Count);
            }

            return ret;
        }

        #endregion
    }
}
