using System;
using System.Collections.Generic;
using System.Xml;

namespace MySoft
{
    /// <summary>
    /// 配置基类
    /// </summary>
    public abstract class ConfigurationBase
    {
        /// <summary>
        /// 获取属性（string类型）
        /// </summary>
        public static string GetStringAttribute(XmlAttributeCollection attributes, string key, string defaultValue)
        {
            if (attributes[key] != null
                && !string.IsNullOrEmpty(attributes[key].Value))
                return attributes[key].Value;
            return defaultValue;
        }

        /// <summary>
        /// 获取属性（int类型）
        /// </summary>
        public static int GetIntAttribute(XmlAttributeCollection attributes, string key, int defaultValue)
        {
            int val = defaultValue;

            if (attributes[key] != null
                && !string.IsNullOrEmpty(attributes[key].Value))
            {
                int.TryParse(attributes[key].Value, out val);
            }
            return val;
        }

        /// <summary>
        /// 获取属性（bool类型）
        /// </summary>
        public static bool GetBoolAttribute(XmlAttributeCollection attributes, string key, bool defaultValue)
        {
            bool val = defaultValue;

            if (attributes[key] != null
                && !string.IsNullOrEmpty(attributes[key].Value))
            {
                bool.TryParse(attributes[key].Value, out val);
            }
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, T> LoadModules<T>(XmlNode node)
        {
            Dictionary<string, T> modules = new Dictionary<string, T>();

            if (node != null)
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.NodeType != XmlNodeType.Comment)
                    {
                        switch (n.Name)
                        {
                            case "clear":
                                modules.Clear();
                                break;
                            case "remove":
                                XmlAttribute removeNameAtt = n.Attributes["name"];
                                string removeName = removeNameAtt == null ? null : removeNameAtt.Value;

                                if (!string.IsNullOrEmpty(removeName) && modules.ContainsKey(removeName))
                                {
                                    modules.Remove(removeName);
                                }

                                break;
                            case "add":

                                XmlAttribute en = n.Attributes["enabled"];
                                if (en != null && en.Value == "false")
                                    continue;

                                XmlAttribute nameAtt = n.Attributes["name"];
                                XmlAttribute typeAtt = n.Attributes["type"];
                                string name = nameAtt == null ? null : nameAtt.Value;
                                string itype = typeAtt == null ? null : typeAtt.Value;

                                if (string.IsNullOrEmpty(name))
                                {
                                    continue;
                                }

                                if (string.IsNullOrEmpty(itype))
                                {
                                    continue;
                                }

                                Type type = Type.GetType(itype);

                                if (type == null)
                                {
                                    continue;
                                }

                                T mod = default(T);

                                try
                                {
                                    mod = (T)Activator.CreateInstance(type);
                                }
                                catch
                                {
                                    //todo: log
                                }

                                if (mod == null)
                                {
                                    continue;
                                }

                                modules.Add(name, mod);
                                break;

                        }
                    }
                }
            }
            return modules;
        }

    }
}
