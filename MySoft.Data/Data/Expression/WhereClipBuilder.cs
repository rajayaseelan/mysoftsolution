using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// WhereClip构建类
    /// </summary>
    public class WhereClipBuilder
    {
        private List<WhereClipItem> list;

        /// <summary>
        /// 实例化WhereClipBuilder
        /// </summary>
        public WhereClipBuilder()
        {
            this.list = new List<WhereClipItem>();
        }

        /// <summary>
        /// 实例化WhereClipBuilder
        /// </summary>
        /// <param name="where"></param>
        public WhereClipBuilder(params WhereClip[] where)
            : this()
        {
            this.list.AddRange(where.Select(p => new WhereClipItem { WhereClip = p, IsAnd = true }));
        }

        /// <summary>
        /// 增加一个条件
        /// </summary>
        /// <param name="where"></param>
        public void And(WhereClip where)
        {
            if (DataHelper.IsNullOrEmpty(where)) return;

            if (list.Exists(o =>
            {
                return string.Compare(where.ToString(), o.ToString()) == 0;
            }))
            {
                return;
            }

            this.list.Add(new WhereClipItem { WhereClip = where, IsAnd = true });
        }

        /// <summary>
        /// 增加一个条件
        /// </summary>
        /// <param name="where"></param>
        public void Or(WhereClip where)
        {
            if (DataHelper.IsNullOrEmpty(where)) return;

            if (list.Exists(o =>
            {
                return string.Compare(where.ToString(), o.ToString()) == 0;
            }))
            {
                return;
            }

            this.list.Add(new WhereClipItem { WhereClip = where, IsAnd = false });
        }

        /// <summary>
        /// 返回条件
        /// </summary>
        public WhereClip WhereClip
        {
            get
            {
                var where = WhereClip.None;
                foreach (var item in list)
                {
                    if (item.IsAnd)
                        where &= item.WhereClip;
                    else
                        where |= item.WhereClip;
                }

                return where;
            }
        }

        /// <summary>
        /// 条件组件类
        /// </summary>
        private class WhereClipItem
        {
            /// <summary>
            /// 条件属性
            /// </summary>
            public WhereClip WhereClip { get; set; }

            /// <summary>
            /// 是否And操作
            /// </summary>
            public bool IsAnd { get; set; }
        }
    }
}
