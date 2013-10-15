using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// 属性映射关系
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// 创建映射关系
        /// </summary>
        /// <param name="mapping">示例[field1>field2、field1|field2、field1 field2、field1=field2]</param>
        /// <returns></returns>
        public static PropertyMapping Create(string mapping)
        {
            var arr = mapping.Split('>', '|', ' ', '=');
            if (arr.Length != 2)
            {
                throw new Exception("映射关系有误，支持>、|、=或空格分隔两个属性！");
            }

            //返回属性映射关系
            return new PropertyMapping { From = arr[0], To = arr[1] };
        }

        /// <summary>
        /// 创建映射关系
        /// </summary>
        /// <param name="mapping">示例[field1>field2、field1|field2、field1 field2、field1=field2]</param>
        /// <returns></returns>
        public static PropertyMapping[] Create(params string[] mappings)
        {
            var list = new List<PropertyMapping>();

            if (mappings != null && mappings.Length > 0)
            {
                foreach (var mapping in mappings)
                {
                    list.Add(Create(mapping));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 从某属性名称
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 转换到的属性名称
        /// </summary>
        public string To { get; set; }
    }
}
