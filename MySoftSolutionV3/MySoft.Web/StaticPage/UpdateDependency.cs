using System;

namespace MySoft.Web
{
    /// <summary>
    /// 更新依赖，包含 SlidingUpdateTime 和 AbsoluteUpdateTime 
    /// </summary>
    public interface IUpdateDependency
    {
        /// <summary>
        /// 更新类型
        /// </summary>
        UpdateType UpdateType { get; set; }

        /// <summary>
        /// 判断是否需要更新
        /// </summary>
        /// <param name="currentDate"></param>
        /// <returns></returns>
        bool HasUpdate(DateTime currentDate);

        /// <summary>
        /// 最后更新时间
        /// </summary>
        DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 是否更新成功
        /// </summary>
        bool UpdateSuccess { get; set; }
    }

    /// <summary>
    /// 更新类型
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// 只生成一次
        /// </summary>
        None,
        /// <summary>
        /// 每年定时生成一次
        /// </summary>
        Year,
        /// <summary>
        /// 每月定时生成一次
        /// </summary>
        Month,
        /// <summary>
        /// 每天定时生成一次
        /// </summary>
        Day
    }

    /// <summary>
    /// 定时生成基类
    /// </summary>
    public abstract class AbstractUpdateDependency : IUpdateDependency
    {
        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <returns></returns>
        public abstract bool HasUpdate(DateTime currentDate);

        /// <summary>
        /// 更新类型
        /// </summary>
        public abstract UpdateType UpdateType { get; set; }

        protected DateTime lastUpdateTime = DateTime.Now;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get { return lastUpdateTime; }
            set { lastUpdateTime = value; }
        }

        protected bool updateSuccess;
        /// <summary>
        /// 是否更新成功
        /// </summary>
        public bool UpdateSuccess
        {
            get { return updateSuccess; }
            set { updateSuccess = value; }
        }

        public AbstractUpdateDependency()
        {
            this.updateSuccess = true;
        }
    }

    /// <summary>
    /// 定时生成策略
    /// </summary>
    public sealed class SlidingParamInfo
    {
        private DateTime beginDateTime;
        private DateTime endDateTime;

        public SlidingParamInfo(DateTime beginDateTime, DateTime endDateTime)
        {
            this.beginDateTime = beginDateTime;
            this.endDateTime = endDateTime;
        }

        /// <summary>
        /// 检测当前更新时间是否在设置的范围内
        /// </summary>
        /// <param name="updateTime"></param>
        /// <returns></returns>
        internal bool CheckUpdate(UpdateType updateType, DateTime updateTime)
        {
            switch (updateType)
            {
                case UpdateType.Year:
                    if (beginDateTime.Year != updateTime.Year)
                    {
                        beginDateTime = beginDateTime.AddYears(1);
                        endDateTime = endDateTime.AddYears(1);
                    }
                    break;
                case UpdateType.Month:
                    if (beginDateTime.Month != updateTime.Month)
                    {
                        beginDateTime = beginDateTime.AddMonths(1);
                        endDateTime = endDateTime.AddMonths(1);
                    }
                    break;
                case UpdateType.Day:
                    if (beginDateTime.Day != updateTime.Day)
                    {
                        beginDateTime = beginDateTime.AddDays(1);
                        endDateTime = endDateTime.AddDays(1);
                    }
                    break;
            }

            return updateTime.Ticks >= beginDateTime.Ticks && updateTime.Ticks <= endDateTime.Ticks;
        }
    }

    /// <summary>
    /// 定时生成策略（按间隔时间）
    /// </summary>
    public sealed class SlidingUpdateTime : AbstractUpdateDependency
    {
        private TimeSpan slidingTimeSpan;
        private SlidingParamInfo[] slidingTimeParams;

        private UpdateType updateType;
        /// <summary>
        /// 更新类型
        /// </summary>
        public override UpdateType UpdateType
        {
            get { return updateType; }
            set { updateType = value; }
        }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public TimeSpan SlidingTimeSpan
        {
            get { return slidingTimeSpan; }
            set { slidingTimeSpan = value; }
        }

        /// <summary>
        /// 定时生成时间段参数
        /// </summary>
        public SlidingParamInfo[] SlidingTimeParams
        {
            get { return slidingTimeParams; }
            set { slidingTimeParams = value; }
        }

        public SlidingUpdateTime() { }

        public SlidingUpdateTime(TimeSpan slidingTimeSpan)
        {
            this.slidingTimeSpan = slidingTimeSpan;
            this.updateType = UpdateType.None;
        }

        public SlidingUpdateTime(TimeSpan slidingTimeSpan, DateTime lastUpdateTime)
            : this(slidingTimeSpan)
        {
            this.lastUpdateTime = lastUpdateTime;
        }

        public SlidingUpdateTime(TimeSpan slidingTimeSpan, params SlidingParamInfo[] slidingTimeParams)
            : this(slidingTimeSpan)
        {
            this.slidingTimeParams = slidingTimeParams;
        }

        public SlidingUpdateTime(TimeSpan slidingTimeSpan, DateTime lastUpdateTime, params SlidingParamInfo[] slidingTimeParams)
            : this(slidingTimeSpan, lastUpdateTime)
        {
            this.slidingTimeParams = slidingTimeParams;
        }

        #region 带类型参数

        public SlidingUpdateTime(UpdateType updateType, TimeSpan slidingTimeSpan)
            : this(slidingTimeSpan)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateTime(UpdateType updateType, TimeSpan slidingTimeSpan, DateTime lastUpdateTime)
            : this(slidingTimeSpan, lastUpdateTime)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateTime(UpdateType updateType, TimeSpan slidingTimeSpan, params SlidingParamInfo[] slidingTimeParams)
            : this(slidingTimeSpan, slidingTimeParams)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateTime(UpdateType updateType, TimeSpan slidingTimeSpan, DateTime lastUpdateTime, params SlidingParamInfo[] slidingTimeParams)
            : this(slidingTimeSpan, lastUpdateTime, slidingTimeParams)
        {
            this.updateType = updateType;
        }

        #endregion

        public override bool HasUpdate(DateTime currentDate)
        {
            //更新时间为最大更新时间，直接返回true
            if (currentDate == DateTime.MaxValue) return true;

            //如果更新失败，判断时间后返回true
            if (!updateSuccess && currentDate.Ticks > lastUpdateTime.Ticks)
                return true;

            if (!updateSuccess) return false;

            DateTime updateTime = lastUpdateTime.Add(slidingTimeSpan);

            bool isUpdate = currentDate.Ticks >= updateTime.Ticks;
            if (isUpdate && lastUpdateTime != DateTime.MinValue)
            {
                if (slidingTimeParams != null)
                {
                    foreach (SlidingParamInfo slidingTimeParam in slidingTimeParams)
                    {
                        if (slidingTimeParam.CheckUpdate(updateType, currentDate)) return true;
                    }
                    isUpdate = false;
                }
            }
            return isUpdate;
        }
    }

    /// <summary>
    /// 绝对时间生成策略
    /// </summary>
    public sealed class AbsoluteUpdateTime : AbstractUpdateDependency
    {
        private DateTime[] absoluteDateTimes;
        private UpdateType updateType;
        /// <summary>
        /// 更新类型
        /// </summary>
        public override UpdateType UpdateType
        {
            get { return updateType; }
            set { updateType = value; }
        }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public DateTime[] AbsoluteDateTimes
        {
            get { return absoluteDateTimes; }
            set { absoluteDateTimes = value; }
        }

        public AbsoluteUpdateTime() { }

        public AbsoluteUpdateTime(params DateTime[] absoluteDateTimes)
        {
            this.absoluteDateTimes = absoluteDateTimes;
            this.updateType = UpdateType.None;
        }

        public AbsoluteUpdateTime(UpdateType updateType, params DateTime[] absoluteDateTimes)
            : this(absoluteDateTimes)
        {
            this.updateType = updateType;
        }

        public override bool HasUpdate(DateTime currentDate)
        {
            //更新时间为最大更新时间，直接返回true
            if (currentDate == DateTime.MaxValue) return true;

            //如果更新失败，判断时间后返回true
            if (!updateSuccess && currentDate.Ticks > lastUpdateTime.Ticks)
                return true;

            if (!updateSuccess) return false;

            int index = 0;
            bool isUpdate = false;
            foreach (DateTime absoluteDateTime in absoluteDateTimes)
            {
                //如果日期不一致，则把日期先变成一致
                if (absoluteDateTime.Day != currentDate.Day)
                {
                    absoluteDateTimes[index] = absoluteDateTime.AddDays(currentDate.Day - absoluteDateTime.Day);
                }

                var span = currentDate.Subtract(absoluteDateTime);
                isUpdate = span.Ticks > 0 && span.TotalMinutes < 5;
                if (isUpdate)
                {
                    switch (updateType)
                    {
                        case UpdateType.Year:
                            absoluteDateTimes[index] = absoluteDateTime.AddYears(1);
                            break;
                        case UpdateType.Month:
                            absoluteDateTimes[index] = absoluteDateTime.AddMonths(1);
                            break;
                        case UpdateType.Day:
                            absoluteDateTimes[index] = absoluteDateTime.AddDays(1);
                            break;
                        case UpdateType.None:
                            absoluteDateTimes[index] = currentDate;
                            break;
                    }
                    break;
                }
                index++;
            }
            return isUpdate;
        }
    }
}
