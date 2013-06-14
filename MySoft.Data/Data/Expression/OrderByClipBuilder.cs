using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// OrderByClip构建类
    /// </summary>
    public class OrderByClipBuilder
    {
        private List<OrderByClip> list;

        /// <summary>
        /// 实例化OrderByClipBuilder
        /// </summary>
        public OrderByClipBuilder()
        {
            this.list = new List<OrderByClip>();
        }

        /// <summary>
        /// 实例化OrderByClipBuilder
        /// </summary>
        /// <param name="orderBy"></param>
        public OrderByClipBuilder(params OrderByClip[] orderBy)
            : this()
        {
            this.list.AddRange(orderBy);
        }

        /// <summary>
        /// 增加一个排序
        /// </summary>
        /// <param name="orderBy"></param>
        public void Add(OrderByClip orderBy)
        {
            if (DataHelper.IsNullOrEmpty(orderBy)) return;

            if (list.Exists(o =>
            {
                return string.Compare(orderBy.ToString(), o.ToString()) == 0;
            }))
            {
                return;
            }

            this.list.Add(orderBy);
        }

        /// <summary>
        /// 增加一个排序
        /// </summary>
        /// <param name="field"></param>
        /// <param name="desc"></param>
        public void Add(Field field, bool desc)
        {
            if (desc)
                Add(field.Desc);
            else
                Add(field.Asc);
        }

        /// <summary>
        /// 增加一组排序
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="fields"></param>
        public void Add(bool desc, params Field[] fields)
        {
            foreach (var field in fields)
            {
                Add(field, desc);
            }
        }

        /// <summary>
        /// 返回排序
        /// </summary>
        public OrderByClip OrderByClip
        {
            get
            {
                var order = OrderByClip.None;
                foreach (var orderBy in list)
                {
                    order &= orderBy;
                }

                return order;
            }
        }
    }
}