using System.Collections.Generic;

namespace MySoft
{
    /// <summary>
    /// 排序数据属性
    /// </summary>
    public class SortProperty
    {
        private string propertyName;
        internal string PropertyName
        {
            get { return propertyName; }
        }

        private bool desc;
        internal bool IsDesc
        {
            get { return desc; }
        }

        public SortProperty(string propertyName)
        {
            this.propertyName = propertyName;
            this.desc = false;
        }

        private SortProperty(string propertyName, bool desc)
            : this(propertyName)
        {
            this.desc = desc;
        }

        /// <summary>
        /// 从小到大
        /// </summary>
        public SortProperty Asc
        {
            get
            {
                return new SortProperty(this.propertyName, false);
            }
        }

        /// <summary>
        /// 从大到小
        /// </summary>
        public SortProperty Desc
        {
            get
            {
                return new SortProperty(this.propertyName, true);
            }
        }
    }

    /// <summary>
    /// 自定义数据排序算法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortComparer<T> : IComparer<T>
    {
        private List<SortProperty> sorts;
        /// <summary>
        /// 初始化自定义比较类
        /// </summary>
        /// <param name="sorts"></param>
        public SortComparer(params SortProperty[] sorts)
        {
            this.sorts = new List<SortProperty>(sorts);
        }

        /// <summary>
        /// 添加排序属性
        /// </summary>
        /// <param name="sorts"></param>
        public void AddProperty(params SortProperty[] sorts)
        {
            if (sorts != null && sorts.Length > 0)
            {
                this.sorts.AddRange(sorts);
            }
        }

        /// <summary>
        /// 实现Compare比较两个值的大小
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            return CompareValue(x, y, 0);
        }

        #region 值比较

        /// <summary>
        /// 进行深层排序
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int CompareValue(T x, T y, int index)
        {
            int ret = 0;
            if (sorts.Count - 1 >= index)
            {
                ret = CompareProperty(x, y, sorts[index]);
                if (ret == 0)
                {
                    ret = CompareValue(x, y, ++index);
                }
            }

            return ret;
        }

        /// <summary>
        /// 比较两个值的大小(从小到大)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private int CompareProperty(T x, T y, SortProperty property)
        {
            object value1 = CoreHelper.GetPropertyValue(x, property.PropertyName);
            object value2 = CoreHelper.GetPropertyValue(y, property.PropertyName);

            //比较两个值的大小
            int ret = CoreHelper.Compare(value1, value2);

            if (property.IsDesc) return -ret;
            return ret;
        }

        #endregion
    }
}
