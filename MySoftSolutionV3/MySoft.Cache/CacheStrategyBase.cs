using System;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存基类
    /// </summary>
    public class CacheStrategyBase
    {
        protected string regionName;
        protected string prefix;
        public CacheStrategyBase(string regionName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new ArgumentNullException("缓存分区名称不能为null！");
            else
                this.prefix = "{" + regionName.ToUpper() + "}|";

            this.regionName = regionName;
        }

        // 默认缓存存活期为1440分钟(24小时)
        private int _timeOut = (int)TimeSpan.FromDays(1).TotalSeconds;

        /// <summary>
        /// 设置到期相对时间[单位：秒] 
        /// </summary>
        public int Timeout
        {
            set { _timeOut = value; }
            get { return _timeOut; }
        }

        /// <summary>
        /// 获取输入的Key
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        protected string GetInputKey(string objId)
        {
            if (string.IsNullOrEmpty(objId)) return objId;
            if (objId.StartsWith(prefix)) return objId;
            return string.Format("{0}{1}", prefix, objId);
        }

        /// <summary>
        /// 获取输出的Key
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        protected string GetOutputKey(string objId)
        {
            if (string.IsNullOrEmpty(objId)) return objId;
            if (!objId.StartsWith(prefix)) return objId;
            return objId.Substring(prefix.Length);
        }
    }
}
