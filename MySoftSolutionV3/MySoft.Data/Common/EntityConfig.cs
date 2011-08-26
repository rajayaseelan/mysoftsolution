using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MySoft.Data.Mapping;

namespace MySoft.Data
{
    /// <summary>
    /// 映像配置类
    /// </summary>
    [Serializable]
    public class EntityConfig
    {
        public static EntityConfig Instance = new EntityConfig();
        private TableSetting[] _Settings;
        private EntityConfig()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            string configPath = ConfigurationManager.AppSettings["EntityConfigPath"];
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\EntityConfig.xml";
            }
            else
            {
                //如果是~则表示当前目录
                if (configPath.Contains("~/") || configPath.Contains("~\\"))
                {
                    configPath = configPath.Replace("/", "\\").Replace("~\\", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\");
                }
            }

            if (File.Exists(configPath))
            {
                XmlTextReader reader = new XmlTextReader(configPath);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TableSetting[]));
                    _Settings = serializer.Deserialize(reader) as TableSetting[];
                }
                catch { }
                finally
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 刷新配置
        /// </summary>
        public void Refresh()
        {
            LoadConfig();
        }

        /// <summary>
        /// 获取表超时时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int GetTableTimeout<T>()
            where T : class
        {
            //如果设置为空返回null
            if (_Settings == null || _Settings.Length == 0)
            {
                return 60;
            }

            //通过Namespace与ClassName来获取映射的表名
            string Namespace = typeof(T).Namespace;
            string ClassName = typeof(T).Name;

            var settings = new List<TableSetting>(_Settings);
            TableSetting setting = settings.Find(p => p.Namespace == Namespace);
            if (setting != null)
            {
                if (setting.Mappings != null && setting.Mappings.Length > 0)
                {
                    //查询mapping的表名
                    var mappings = new List<TableMapping>(setting.Mappings);
                    TableMapping mapping = mappings.Find(p => p.ClassName == ClassName);
                    if (mapping != null)
                    {
                        return mapping.Timeout;
                    }

                    return 60;
                }

                return 60;
            }

            return 60;
        }

        /// <summary>
        /// 获取映射的表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public Table GetMappingTable<T>(string tableName)
            where T : class
        {
            //如果设置为空返回null
            if (_Settings == null || _Settings.Length == 0)
            {
                return new Table(tableName);
            }

            //通过Namespace与ClassName来获取映射的表名
            string Namespace = typeof(T).Namespace;
            string ClassName = typeof(T).Name;

            Table table = new Table(tableName);
            var settings = new List<TableSetting>(_Settings);
            TableSetting setting = settings.Find(p => p.Namespace == Namespace);
            if (setting != null)
            {
                table.Prefix = setting.Prefix;
                table.Suffix = setting.Suffix;

                if (setting.Mappings != null && setting.Mappings.Length > 0)
                {
                    //查询mapping的表名
                    var mappings = new List<TableMapping>(setting.Mappings);
                    TableMapping mapping = mappings.Find(p => p.ClassName == ClassName);
                    if (mapping != null)
                    {
                        if (!string.IsNullOrEmpty(mapping.MappingName))
                        {
                            table.TableName = mapping.MappingName;

                            if (!mapping.UsePrefix) table.Prefix = null;
                            if (!mapping.UseSuffix) table.Suffix = null;
                        }
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// 获取映射的字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public Field GetMappingField<T>(string propertyName, string fieldName)
            where T : class
        {
            //如果设置为空返回null
            if (_Settings == null || _Settings.Length == 0)
            {
                return new Field(fieldName);
            }

            //通过Namespace与ClassName来获取映射的表名
            string Namespace = typeof(T).Namespace;
            string ClassName = typeof(T).Name;

            Field field = new Field(fieldName);
            var settings = new List<TableSetting>(_Settings);
            var setting = settings.Find(p => p.Namespace == Namespace);
            if (setting != null)
            {
                if (setting.Mappings != null && setting.Mappings.Length > 0)
                {
                    //查询mapping的表名
                    var mappings = new List<TableMapping>(setting.Mappings);
                    var mapping = mappings.Find(p => p.ClassName == ClassName);
                    if (mapping != null)
                    {
                        if (mapping.Mappings != null && mapping.Mappings.Length > 0)
                        {
                            var fmappings = new List<FieldMapping>(mapping.Mappings);
                            var fmapping = fmappings.Find(p => p.PropertyName == propertyName);
                            if (fmapping != null)
                            {
                                field = new Field(fmapping.MappingName);
                            }
                        }
                    }
                }
            }

            return field;
        }
    }
}
