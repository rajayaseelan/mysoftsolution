using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// GroupByClip构建类
    /// </summary>
    public class GroupByClipBuilder
    {
        private List<GroupByClip> list;

        /// <summary>
        /// 实例化GroupByClipBuilder
        /// </summary>
        public GroupByClipBuilder()
        {
            this.list = new List<GroupByClip>();
        }

        /// <summary>
        /// 实例化GroupByClipBuilder
        /// </summary>
        /// <param name="groupBy"></param>
        public GroupByClipBuilder(params GroupByClip[] groupBy)
            : this()
        {
            this.list.AddRange(groupBy);
        }

        /// <summary>
        /// 增加一个分组
        /// </summary>
        /// <param name="groupBy"></param>
        public void Add(GroupByClip groupBy)
        {
            if (DataHelper.IsNullOrEmpty(groupBy)) return;

            if (list.Exists(o =>
            {
                return string.Compare(groupBy.ToString(), o.ToString()) == 0;
            }))
            {
                return;
            }

            this.list.Add(groupBy);
        }

        /// <summary>
        /// 增加一组分组
        /// </summary>
        /// <param name="fields"></param>
        public void Add(params Field[] fields)
        {
            foreach (var field in fields)
            {
                Add(field.Group);
            }
        }

        /// <summary>
        /// 返回分组
        /// </summary>
        public GroupByClip GroupByClip
        {
            get
            {
                var group = GroupByClip.None;
                foreach (var groupBy in list)
                {
                    group &= groupBy;
                }

                return group;
            }
        }
    }
}
