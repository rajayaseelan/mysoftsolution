using System;
using System.Xml;

namespace MySoft.Common
{
    /// <summary>
    /// xml 的摘要说明。
    /// </summary>
    public class XmlHandler
    {
        protected XmlDocument xdoc = new XmlDocument();
        public XmlElement root;
        public XmlHandler()
        {

        }

        /// <summary>
        /// 加载xml文档
        /// </summary>
        /// <param name="xml"></param>
        public void LoadXml(string xml)
        {
            xdoc.LoadXml(xml);
            root = (XmlElement)xdoc.FirstChild;
        }

        /// <summary>
        /// 取得名称为name的结点的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetValue(string name)
        {
            XmlNode xn = FindXnByName(root.ChildNodes, name);
            if (xn == null) return null;
            return xn.InnerText;
        }

        public XmlNode GetNode(string name, string value)
        {
            XmlNode xn = FindXnByXa(root.ChildNodes, name, value);
            return xn;
        }

        /// <summary>
        /// 创建一个包含version和指定根节点的XmlDocument
        /// </summary>
        /// <param name="rootName"></param>
        public void CreateRoot(string rootName)
        {
            XmlElement xe = xdoc.CreateElement(rootName);
            xdoc.AppendChild(xe);
            root = xe;
        }

        /// <summary>
        /// 增加一个子结点
        /// </summary>
        /// <param name="name"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        public XmlElement AppendChild(string name, string _value)
        {
            return AddChild((XmlElement)root, name, _value);
        }

        public override string ToString()
        {
            return xdoc.OuterXml;
        }

        /// <summary>
        /// 为一个XmlElement添加子节点，并返回添加的子节点引用
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="sField"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public XmlElement AddChild(XmlElement xe, string sField, string sValue)
        {
            XmlElement xeTemp = xdoc.CreateElement(sField);
            xeTemp.InnerText = sValue;
            xe.AppendChild(xeTemp);
            return xeTemp;
        }

        /// <summary>
        /// 为一个XmlElement添加子节点，并返回添加的子节点引用
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="xd"></param>
        /// <param name="sField"></param>
        /// <returns></returns>
        protected XmlElement AddChild(XmlElement xe, XmlDocument xd, string sField)
        {
            XmlElement xeTemp = xd.CreateElement(sField);
            xe.AppendChild(xeTemp);
            return xeTemp;
        }

        /// <summary>
        /// 为一个节点添加属性
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        public void AddAttribute(XmlElement xe, string strName, string strValue)
        {
            //判断属性是否存在
            string s = GetXaValue(xe.Attributes, strName);
            //属性已经存在
            if (s != null)
            {
                throw new System.Exception("attribute exists");
            }
            XmlAttribute xa = xdoc.CreateAttribute(strName);
            xa.Value = strValue;
            xe.Attributes.Append(xa);
        }

        /// <summary>
        /// 为一个节点添加属性，不是系统表
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="xe"></param>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        protected void AddAttribute(XmlDocument xdoc, XmlElement xe, string strName, string strValue)
        {
            //判断属性是否存在
            string s = GetXaValue(xe.Attributes, strName);
            //属性已经存在
            if (s != null)
            {
                throw new Exception("Error:The attribute '" + strName + "' has been existed!");
            }
            XmlAttribute xa = xdoc.CreateAttribute(strName);
            xa.Value = strValue;
            xe.Attributes.Append(xa);
        }

        /// <summary>
        /// 通过节点名称找到指定的节点
        /// </summary>
        /// <param name="xnl"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        protected XmlNode FindXnByName(XmlNodeList xnl, string strName)
        {
            for (int i = 0; i < xnl.Count; i++)
            {
                if (xnl.Item(i).LocalName == strName) return xnl.Item(i);
            }
            return null;
        }

        /// <summary>
        /// 找到指定名称属性的值
        /// </summary>
        /// <param name="xac"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        protected string GetXaValue(XmlAttributeCollection xac, string strName)
        {
            for (int i = 0; i < xac.Count; i++)
            {
                if (xac.Item(i).LocalName == strName) return xac.Item(i).Value;
            }
            return null;
        }

        /// <summary>
        /// 找到指定名称属性的值
        /// </summary>
        /// <param name="xnl"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        protected string GetXnValue(XmlNodeList xnl, string strName)
        {
            for (int i = 0; i < xnl.Count; i++)
            {
                if (xnl.Item(i).LocalName == strName) return xnl.Item(i).InnerText;
            }
            return null;
        }

        //为一个节点指定值
        protected void SetXnValue(XmlNodeList xnl, string strName, string strValue)
        {
            for (int i = 0; i < xnl.Count; i++)
            {
                if (xnl.Item(i).LocalName == strName)
                {
                    xnl.Item(i).InnerText = strValue;
                    return;
                }
            }
            return;
        }

        /// <summary>
        /// 为一个属性指定值
        /// </summary>
        /// <param name="xac"></param>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        protected void SetXaValue(XmlAttributeCollection xac, string strName, string strValue)
        {
            for (int i = 0; i < xac.Count; i++)
            {
                if (xac.Item(i).LocalName == strName)
                {
                    xac.Item(i).Value = strValue;
                    return;
                }
            }
            return;
        }


        /// <summary>
        /// 寻找具有指定名称和属性/值组合的节点
        /// </summary>
        /// <param name="xnl"></param>
        /// <param name="strXaName"></param>
        /// <param name="strXaValue"></param>
        /// <returns></returns>
        protected XmlNode FindXnByXa(XmlNodeList xnl, string strXaName, string strXaValue)
        {
            string xa;
            for (int i = 0; i < xnl.Count; i++)
            {
                xa = GetXaValue(xnl.Item(i).Attributes, strXaName);
                if (xa != null)
                {
                    if (xa == strXaValue) return xnl.Item(i);
                }
            }
            return null;
        }
    }
}
