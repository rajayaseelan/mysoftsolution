using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace MySoft
{
    /// <summary>
    /// Xml工具
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// 创建一个根节点
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlRootNode CreateRoot(string path)
        {
            return new XmlRootNode(path);
        }
    }

    /// <summary>
    /// XmlChildNode 的摘要说明
    /// </summary>
    public class XmlChildNode
    {
        private XmlDocument doc;
        private string element;
        private XmlNode node;

        /// <summary>
        /// 实例化 XmlChildNode
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="node"></param>
        public XmlChildNode(XmlDocument doc, XmlNode node)
        {
            this.doc = doc;
            this.element = node.Name;
            this.node = node;
        }

        /// <summary>
        /// 获取一个节点
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public XmlChildNode GetNode(string element)
        {
            string el = null;
            if (string.IsNullOrEmpty(this.element))
                el = "/" + element.TrimStart('/');
            else
                el = string.Format("/{0}/{1}", this.element.TrimStart('/'), element.TrimStart('/'));

            var node = doc.SelectSingleNode(el);
            if (node == null) return null;

            return new XmlChildNode(doc, node);
        }

        #region 根据属性获取值

        /// <summary>
        /// 通过属性的值获取另一属性的值
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="outAttribute"></param>
        /// <returns></returns>
        public string GetValueByAttributeValue(string attribute, string value, string outAttribute)
        {
            var nodes = GetNodesByAttributeValue(attribute, value);
            if (nodes.Length > 0)
            {
                return nodes[0].GetAttribute(outAttribute);
            }

            return null;
        }

        /// <summary>
        /// 通过属性的值获取另一属性的值
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        /// <param name="outAttribute"></param>
        /// <returns></returns>
        public string GetValueByAttributeValue(string[] attributes, string[] values, string outAttribute)
        {
            var nodes = GetNodesByAttributeValue(attributes, values);
            if (nodes.Length > 0)
            {
                return nodes[0].GetAttribute(outAttribute);
            }

            return null;
        }

        #endregion

        #region 根据属性获取对象

        /// <summary>
        /// 通过属性的值获取节点
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlChildNode[] GetNodesByAttributeValue(string attribute, string value)
        {
            return GetNodesByAttributeValue(new string[] { attribute }, new string[] { value });
        }

        /// <summary>
        /// 通过属性的值获取节点
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public XmlChildNode[] GetNodesByAttributeValue(string[] attributes, string[] values)
        {
            var list = new List<XmlChildNode>();

            foreach (XmlNode nd in node.ChildNodes)
            {
                int index = 0;
                int count = 0;
                foreach (string attribute in attributes)
                {
                    if (nd.Attributes[attribute].Value.ToLower() == values[index].ToLower())
                    {
                        count++;
                    }
                    index++;
                }

                if (count == attributes.Length)
                {
                    var helper = new XmlChildNode(doc, nd);
                    list.Add(helper);
                }
            }

            return list.ToArray();
        }

        #endregion

        /// <summary>
        /// 获取节点列表
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public XmlChildNode[] GetNodes(string element)
        {
            var list = new List<XmlChildNode>();
            foreach (XmlNode nd in node.ChildNodes)
            {
                if (nd.Name == element)
                {
                    var helper = new XmlChildNode(doc, nd);
                    list.Add(helper);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public string GetAttribute(string attribute)
        {
            try
            {
                return node.Attributes[attribute].Value;
            }
            catch (Exception ex)
            {
                throw new MySoftException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public string this[string attribute]
        {
            get
            {
                return GetAttribute(attribute);
            }
        }

        #region 插入节点

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        public XmlChildNode Create(string element)
        {
            return Insert(element, (string[])null, (string[])null);
        }

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        public XmlChildNode Create(string element, string value)
        {
            var node = Create(element);
            node.Text = value;

            return node;
        }

        /// <summary>
        /// 插入节点及值
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlChildNode Insert(string attribute, string value)
        {
            return Insert(null, attribute, value);
        }

        /// <summary>
        /// 插入属性及值
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        public XmlChildNode Insert(string[] attributes, string[] values)
        {
            return Insert(null, attributes, values);
        }

        /// <summary>
        /// 插入节点、属性及值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public XmlChildNode Insert(string element, string attribute, string value)
        {
            return Insert(element, new string[] { attribute }, new string[] { value });
        }

        /// <summary>
        /// 插入节点、属性及值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        public XmlChildNode Insert(string element, string[] attributes, string[] values)
        {
            try
            {
                XmlElement xe;
                bool isRoot = true;
                if (string.IsNullOrEmpty(element))
                    xe = (XmlElement)node;
                else
                {
                    isRoot = false;
                    xe = doc.CreateElement(element);
                }

                if (attributes != null)
                {
                    int index = 0;
                    foreach (string attribute in attributes)
                    {
                        xe.SetAttribute(attribute, values[index]);
                        index++;
                    }
                }

                if (isRoot)
                {
                    return this;
                }
                else
                {
                    node.AppendChild(xe);
                    return new XmlChildNode(doc, (XmlNode)xe);
                }
            }
            catch (Exception ex)
            {
                throw new MySoftException(ex.Message, ex);
            }
        }

        #endregion

        #region 更新节点

        /// <summary>
        /// 更新属性值
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public XmlChildNode Update(string attribute, string value)
        {
            return Update(new string[] { attribute }, new string[] { value });
        }

        /// <summary>
        /// 更新属性值
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        public XmlChildNode Update(string[] attributes, string[] values)
        {
            try
            {
                XmlElement xe = (XmlElement)node;
                int index = 0;
                foreach (string attribute in attributes)
                {
                    xe.SetAttribute(attribute, values[index]);
                    index++;
                }

                return this;
            }
            catch (Exception ex)
            {
                throw new MySoftException(ex.Message, ex);
            }
        }

        #endregion

        #region 删除节点

        /// <summary>
        /// 删除节点
        /// </summary>
        public XmlChildNode Delete()
        {
            return Delete(null);
        }

        /// <summary>
        /// 删除属性
        /// </summary>
        /// <param name="attribute"></param>
        public XmlChildNode Delete(string attribute)
        {
            try
            {
                XmlElement xe = (XmlElement)node;
                if (string.IsNullOrEmpty(attribute))
                    node.ParentNode.RemoveChild(node);
                else
                    xe.RemoveAttribute(attribute);

                return this;
            }
            catch (Exception ex)
            {
                throw new MySoftException(ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// 获取节点值
        /// </summary>
        public string Text
        {
            get
            {
                return node.InnerText;
            }
            set
            {
                node.InnerText = value;
            }
        }

        /// <summary>
        /// 获取节点xml
        /// </summary>
        public string XML
        {
            get
            {
                return node.OuterXml;
            }
        }
    }

    /// <summary>
    /// XmlRootNode 的摘要说明
    /// </summary>
    public class XmlRootNode : IDisposable
    {
        XmlDocument doc = new XmlDocument();
        private string path;
        private string content;

        /// <summary>
        /// 实例化XmlRootNode
        /// </summary>
        /// <param name="path"></param>
        public XmlRootNode(string path)
        {
            this.path = path;

            if (File.Exists(path))
            {
                try
                {
                    doc.Load(path);
                    content = doc.InnerXml;
                }
                catch { }
            }
        }

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        public XmlChildNode Create(string element)
        {
            return Create(element, (string[])null, (string[])null);
        }

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        public XmlChildNode Create(string element, string value)
        {
            var node = Create(element);
            node.Text = value;

            return node;
        }

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlChildNode Create(string element, string attribute, string value)
        {
            return Create(element, new string[] { attribute }, new string[] { value });
        }

        /// <summary>
        /// 创建element根节点
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributes"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public XmlChildNode Create(string element, string[] attributes, string[] values)
        {
            try
            {
                MemoryStream ms = new MemoryStream();

                var xw = new XmlTextWriter(ms, Encoding.UTF8);
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument();
                xw.WriteStartElement(element);

                if (attributes != null)
                {
                    int index = 0;
                    foreach (string attribute in attributes)
                    {
                        xw.WriteAttributeString(attribute, values[index]);
                        index++;
                    }
                }

                xw.WriteEndElement();
                xw.WriteEndDocument();
                xw.Flush();

                ms.Position = 0;
                var xr = XmlReader.Create(ms);
                doc.Load(xr);
                xr.Close();

                ms.Close();
                xw.Close();

                return new XmlChildNode(doc, (XmlNode)doc.DocumentElement);
            }
            catch (Exception ex)
            {
                throw new MySoftException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取一个节点
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public XmlChildNode this[string element]
        {
            get
            {
                return GetNode(element);
            }
        }

        /// <summary>
        /// 获取一个节点
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public XmlChildNode GetNode(string element)
        {
            if (doc.ChildNodes.Count == 0) return null;

            //如果只有2个节点，说明是要节点
            if (doc.ChildNodes.Count == 2)
            {
                return new XmlChildNode(doc, (XmlNode)doc.DocumentElement);
            }

            string[] elements = element.Split(new char[] { '.', '/', '|' });
            if (elements.Length == 1)
            {
                var node = doc.SelectSingleNode(elements[0]);
                if (node == null) return null;

                return new XmlChildNode(doc, node);
            }

            XmlChildNode help = null;
            foreach (string el in elements)
            {
                var node = doc.SelectSingleNode(elements[0]);
                if (node == null) return null;

                if (help == null)
                    help = new XmlChildNode(doc, node);
                else
                    help = help.GetNode(el);
            }

            return help;
        }

        #region 保存节点

        /// <summary>
        /// 将对象设置到当前文档
        /// </summary>
        /// <param name="value"></param>
        public void SetObject(object value)
        {
            try
            {
                string xml = SerializationManager.SerializeXml(value);
                doc.InnerXml = xml;
            }
            catch { }
        }

        /// <summary>
        /// 保存更新
        /// </summary>
        public void Save()
        {
            try
            {
                //有更改时才保存
                if (content != doc.InnerXml)
                {
                    if (!string.IsNullOrEmpty(doc.InnerXml))
                    {
                        if (!File.Exists(path))
                        {
                            //是否存在指定文件路径的目录
                            if (!Directory.Exists(Path.GetDirectoryName(path)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                            }

                            var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                            fs.SetLength(0);

                            using (var sw = new StreamWriter(fs, Encoding.UTF8))
                            {
                                sw.Write(doc.InnerXml);
                                sw.Flush();
                            }
                        }
                        else
                        {
                            doc.Save(path);
                        }
                    }
                }
            }
            catch { };
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Save();
        }

        #endregion
    }
}
