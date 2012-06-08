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
        bool HasUpdate(DateTime currentDate, int inMinutes);

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
        public abstract bool HasUpdate(DateTime currentDate, int inMinutes);

        protected UpdateType updateType;
        /// <summary>
        /// 更新类型
        /// </summary>
        public UpdateType UpdateType
        {
            get { return updateType; }
            set { updateType = value; }
        }

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
            this.updateType = UpdateType.Day;
            this.updateSuccess = true;
        }
    }

    /// <summary>
    /// 定时生成策略，用于控制区间
    /// </summary>
    public sealed class SlidingDateTimeRegion
    {
        private DateTime beginDateTime;
        private DateTime endDateTime;

        public SlidingDateTimeRegion(DateTime beginDateTime, DateTime endDateTime)
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
            var checkTime = updateTime;
            var beginTime = beginDateTime;
            var endTime = endDateTime;

            switch (updateType)
            {
                case UpdateType.Year:
                    checkTime = DateTime.Parse(checkTime.ToString("1900-MM-dd HH:mm:ss"));
                    beginTime = DateTime.Parse(beginTime.ToString("1900-MM-dd HH:mm:ss"));
                    endTime = DateTime.Parse(endTime.ToString("1900-MM-dd HH:mm:ss"));
                    break;
                case UpdateType.Month:
                    checkTime = DateTime.Parse(checkTime.ToString("1900-01-dd HH:mm:ss"));
                    beginTime = DateTime.Parse(beginTime.ToString("1900-01-dd HH:mm:ss"));
                    endTime = DateTime.Parse(endTime.ToString("1900-01-dd HH:mm:ss"));
                    break;
                case UpdateType.Day:
                    checkTime = DateTime.Parse(checkTime.ToString("1900-01-01 HH:mm:ss"));
                    beginTime = DateTime.Parse(beginTime.ToString("1900-01-01 HH:mm:ss"));
                    endTime = DateTime.Parse(endTime.ToString("1900-01-01 HH:mm:ss"));
                    break;
            }

            //判断是否在区间内
            return checkTime.Ticks >= beginTime.Ticks && checkTime.Ticks <= endTime.Ticks;
        }
    }

    /// <summary>
    /// 定时生成策略（按间隔时间）
    /// </summary>
    public sealed class SlidingUpdateDependency : AbstractUpdateDependency
    {
        private TimeSpan slidingTimeSpan;
        private SlidingDateTimeRegion[] slidingTimeParams;

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
        public SlidingDateTimeRegion[] SlidingTimeParams
        {
            get { return slidingTimeParams; }
            set { slidingTimeParams = value; }
        }

        public SlidingUpdateDependency() { }

        public SlidingUpdateDependency(TimeSpan slidingTimeSpan)
        {
            this.slidingTimeSpan = slidingTimeSpan;
        }

        public SlidingUpdateDependency(TimeSpan slidingTimeSpan, DateTime lastUpdateTime)
            : this(slidingTimeSpan)
        {
            this.lastUpdateTime = lastUpdateTime;
        }

        public SlidingUpdateDependency(TimeSpan slidingTimeSpan, params SlidingDateTimeRegion[] slidingTimeParams)
            : this(slidingTimeSpan)
        {
            this.slidingTimeParams = slidingTimeParams;
        }

        public SlidingUpdateDependency(TimeSpan slidingTimeSpan, DateTime lastUpdateTime, params SlidingDateTimeRegion[] slidingTimeParams)
            : this(slidingTimeSpan, lastUpdateTime)
        {
            this.slidingTimeParams = slidingTimeParams;
        }

        #region 带类型参数

        public SlidingUpdateDependency(UpdateType updateType, TimeSpan slidingTimeSpan)
            : this(slidingTimeSpan)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateDependency(UpdateType updateType, TimeSpan slidingTimeSpan, DateTime lastUpdateTime)
            : this(slidingTimeSpan, lastUpdateTime)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateDependency(UpdateType updateType, TimeSpan slidingTimeSpan, params SlidingDateTimeRegion[] slidingTimeParams)
            : this(slidingTimeSpan, slidingTimeParams)
        {
            this.updateType = updateType;
        }

        public SlidingUpdateDependency(UpdateType updateType, TimeSpan slidingTimeSpan, DateTime lastUpdateTime, params SlidingDateTimeRegion[] slidingTimeParams)
            : this(slidingTimeSpan, lastUpdateTime, slidingTimeParams)
        {
            this.updateType = updateType;
        }

        #endregion

        public override bool HasUpdate(DateTime currentDate, int inMinutes)
        {
            //更新时间为最大更新时间，直接返回true
            if (currentDate == DateTime.MaxValue) return true;

            //如果更新失败，判断时间后返回true
            if (!updateSuccess && currentDate.Ticks > lastUpdateTime.Ticks)
                return true;

            if (!updateSuccess) return false;

            DateTime updateTime = lastUpdateTime.Add(slidingTimeSpan);

            var span = currentDate.Subtract(updateTime);
            bool isUpdate = span.Ticks > 0 && span.TotalMinutes < inMinutes;
            if (isUpdate)
            {
                if (slidingTimeParams != null && slidingTimeParams.Length > 0)
                {
                    isUpdate = false;
                    foreach (SlidingDateTimeRegion slidingTimeParam in slidingTimeParams)
                    {
                        isUpdate = slidingTimeParam.CheckUpdate(updateType, currentDate);
                        if (isUpdate) break;
                    }
                }
            }
            return isUpdate;
        }
    }

    /// <summary>
    /// 绝对时间生成策略
    /// </summary>
    public sealed class AbsoluteUpdateDependency : AbstractUpdateDependency
    {
        private DateTime[] absoluteDateTimes;

        /// <summary>
        /// 时间间隔
        /// </summary>
        public DateTime[] AbsoluteDateTimes
        {
            get { return absoluteDateTimes; }
            set { absoluteDateTimes = value; }
        }

        public AbsoluteUpdateDependency() { }

        public AbsoluteUpdateDependency(params DateTime[] absoluteDateTimes)
        {
            this.absoluteDateTimes = absoluteDateTimes;
        }

        public AbsoluteUpdateDependency(UpdateType updateType, params DateTime[] absoluteDateTimes)
            : this(absoluteDateTimes)
        {
            this.updateType = updateType;
        }

        public override bool HasUpdate(DateTime currentDate, int inMinutes)
        {
            //更新时间为最大更新时间，直接返回true
            if (currentDate == DateTime.MaxValue) return true;

            //如果更新失败，判断时间后返回true
            if (!updateSuccess && currentDate.Ticks > lastUpdateTime.Ticks)
                return true;

            if (!updateSuccess) return false;

            bool isUpdate = false;
            var checkTime = currentDate;

            foreach (DateTime absoluteDateTime in absoluteDateTimes)
            {
                var absoluteTime = absoluteDateTime;

                switch (updateType)
                {
                    case UpdateType.Year:
                        checkTime = DateTime.Parse(checkTime.ToString("1900-MM-dd HH:mm:ss"));
                        absoluteTime = DateTime.Parse(absoluteTime.ToString("1900-MM-dd HH:mm:ss"));
                        break;
                    case UpdateType.Month:
                        checkTime = DateTime.Parse(checkTime.ToString("1900-01-dd HH:mm:ss"));
                        absoluteTime = DateTime.Parse(absoluteTime.ToString("1900-01-dd HH:mm:ss"));
                        break;
                    case UpdateType.Day:
                        checkTime = DateTime.Parse(checkTime.ToString("1900-01-01 HH:mm:ss"));
                        absoluteTime = DateTime.Parse(absoluteTime.ToString("1900-01-01 HH:mm:ss"));
                        break;
                }

                var span = checkTime.Subtract(absoluteTime);
                isUpdate = span.Ticks > 0 && span.TotalMinutes < inMinutes;
                if (isUpdate) break;
            }

            return isUpdate;
        }
    }
}
