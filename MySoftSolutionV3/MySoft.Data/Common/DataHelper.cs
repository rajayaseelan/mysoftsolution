using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using System.Linq;

namespace MySoft.Data
{
    /// <summary>
    /// 数据服务类
    /// </summary>
    public static class DataHelper
    {
        #region 数据转换

        /// <summary>
        /// 从对象obj中获取值传给当前实体,TOutput必须为class或接口
        /// TInput可以为class、NameValueCollection、IDictionary、IRowReader、DataRow
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TOutput ConvertType<TInput, TOutput>(TInput obj)
        {
            if (obj == null) return default(TOutput);

            if (obj is TOutput && typeof(TOutput).IsInterface)
            {
                return (TOutput)(obj as object);
            }
            else
            {
                TOutput t = default(TOutput);

                try
                {
                    //t = CoreHelper.CreateInstance<TOutput>();
                    if (typeof(TOutput) == typeof(TInput))
                    {
                        t = CoreHelper.CreateInstance<TOutput>(obj.GetType());
                    }
                    else
                    {
                        t = CoreHelper.CreateInstance<TOutput>();
                    }
                }
                catch (Exception ex)
                {
                    throw new DataException(string.Format("创建类型对象【{0}】出错，可能不存在构造函数！", typeof(TOutput).FullName), ex);
                }

                //如果当前实体为Entity，数据源为IRowReader的话，可以通过内部方法赋值
                if (t is Entity && obj is IRowReader)
                {
                    (t as Entity).SetDbValues(obj as IRowReader);
                }
                else
                {
                    foreach (PropertyInfo p in CoreHelper.GetPropertiesFromType<TOutput>())
                    {
                        object value = null;
                        if (obj is NameValueCollection)
                        {
                            NameValueCollection reader = obj as NameValueCollection;
                            if (reader[p.Name] == null) continue;
                            value = reader[p.Name];
                        }
                        else if (obj is IDictionary)
                        {
                            IDictionary reader = obj as IDictionary;
                            if (!reader.Contains(p.Name)) continue;
                            if (reader[p.Name] == null) continue;
                            value = reader[p.Name];
                        }
                        else if (obj is IRowReader)
                        {
                            IRowReader reader = obj as IRowReader;
                            if (reader.IsDBNull(p.Name)) continue;
                            value = reader[p.Name];
                        }
                        else if (obj is DataRow)
                        {
                            IRowReader reader = new SourceRow(obj as DataRow);
                            if (reader.IsDBNull(p.Name)) continue;
                            value = reader[p.Name];
                        }
                        else
                        {
                            value = CoreHelper.GetPropertyValue(obj, p.Name);
                        }

                        if (value == null) continue;
                        CoreHelper.SetPropertyValue(t, p, value);
                    }
                }

                //通过此方式处理的对象将修改列清除
                if (t != null && t is Entity) (t as Entity).AttachSet();

                return t;
            }
        }

        #endregion

        #region 判断是否为null或空

        /// <summary>
        /// 判断WhereClip是否为null或空
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(WhereClip where)
        {
            if ((object)where == null || string.IsNullOrEmpty(where.ToString()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断OrderByClip是否为null或空
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(OrderByClip order)
        {
            if ((object)order == null || string.IsNullOrEmpty(order.ToString()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断GroupByClip是否为null或空
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(GroupByClip group)
        {
            if ((object)group == null || string.IsNullOrEmpty(group.ToString()))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 格式化数据为数据库通用格式
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static string FormatValue(object val)
        {
            if (val == null || val == DBNull.Value)
            {
                return "null";
            }

            Type type = val.GetType();

            if (type == typeof(Guid))
            {
                return string.Format("'{0}'", val);
            }
            else if (type == typeof(DateTime))
            {
                return string.Format("'{0}'", ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else if (type == typeof(bool))
            {
                return ((bool)val) ? "1" : "0";
            }
            else if (val is Field)
            {
                return ((Field)val).Name;
            }
            else if (val is DbValue)
            {
                return ((DbValue)val).Value;
            }
            else if (type.IsEnum)
            {
                return Convert.ToInt32(val).ToString();
            }
            else if (type.IsValueType)
            {
                if (CoreHelper.CheckStructType(type))
                {
                    //如果属性是值类型，则进行系列化存储
                    return SerializationManager.SerializeJson(val);
                }

                return val.ToString();
            }
            else
            {
                return string.Format("N'{0}'", val.ToString());
            }
        }

        internal static string FormatSQL(string sql, char leftToken, char rightToken, bool isAccess)
        {
            if (sql == null) return string.Empty;

            if (isAccess)
                sql = sql.Replace("__[[", '('.ToString())
                        .Replace("]]__", ')'.ToString())
                        .Replace("__[", leftToken.ToString())
                        .Replace("]__", rightToken.ToString());

            else
                sql = sql.Replace("__[[", ' '.ToString())
                        .Replace("]]__", ' '.ToString())
                        .Replace("__[", leftToken.ToString())
                        .Replace("]__", rightToken.ToString());

            //string str = sql.Replace(" . ", ".")
            //                .Replace(" , ", ",")
            //                .Replace(" ( ", " (")
            //                .Replace(" ) ", ") ");

            return CoreHelper.RemoveSurplusSpaces(sql);
        }

        internal static object[] CheckAndReturnValues(object[] values)
        {
            //如果值为null，则返回不等条件
            if (values == null)
            {
                throw new DataException("传入的数据不能为null！");
            }

            //如果长度为0，则返回不等条件
            if (values.Length == 0)
            {
                throw new DataException("传入的数据个数不能为0！");
            }

            //如果传的类型不是object,则强制转换
            if (values.Length == 1 && values[0].GetType().IsArray)
            {
                try
                {
                    values = ArrayList.Adapter((Array)values[0]).ToArray();
                }
                catch
                {
                    throw new DataException("传入的数据不能正确被解析！");
                }
            }

            return values;
        }

        internal static WhereClip GetPkWhere<T>(Table table, object[] pkValues)
            where T : Entity
        {
            WhereClip where = null;
            List<FieldValue> list = CoreHelper.CreateInstance<T>().GetFieldValues();
            pkValues = CheckAndReturnValues(pkValues);

            int pkCount = list.FindAll(p => p.IsPrimaryKey).Count;
            if (pkValues.Length != pkCount)
            {
                throw new DataException("传入的数据与主键无法对应，应该传入【" + pkCount + "】个主键值！");
            }

            list.ForEach(fv =>
            {
                int index = 0;
                if (fv.IsPrimaryKey)
                {
                    where &= fv.Field.At(table) == pkValues[index];
                    index++;
                }
            });

            return where;
        }

        internal static WhereClip GetPkWhere<T>(Table table, T entity)
            where T : Entity
        {
            WhereClip where = null;
            List<FieldValue> list = entity.GetFieldValues();

            list.ForEach(fv =>
            {
                if (fv.IsPrimaryKey)
                {
                    where &= fv.Field.At(table) == fv.Value;
                }
            });

            return where;
        }


        internal static WhereClip GetAllWhere<T>(Table table, T entity, Field[] fields)
            where T : Entity
        {
            WhereClip where = null;
            var list = entity.GetFieldValues();
            var flist = fields.ToList();

            list.ForEach(fv =>
            {
                if (flist.Exists(p => fv.Field.Name == p.Name))
                {
                    where &= fv.Field.At(table) == fv.Value;
                }
            });

            return where;
        }

        /// <summary>
        /// 创建一个FieldValue列表
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static List<FieldValue> CreateFieldValue(Field[] fields, object[] values, bool isInsert)
        {
            if (fields == null || values == null)
            {
                throw new DataException("字段及值不能为null！");
            }

            if (fields.Length != values.Length)
            {
                throw new DataException("字段及值长度不一致！");
            }

            int index = 0;
            var fvlist = new List<FieldValue>();
            foreach (Field field in fields)
            {
                FieldValue fv = new FieldValue(field, values[index]);

                if (isInsert && values[index] is Field)
                {
                    fv.IsIdentity = true;
                }
                else if (!isInsert)
                {
                    fv.IsChanged = true;
                }

                fvlist.Add(fv);

                index++;
            }

            return fvlist;
        }

        #endregion
    }
}