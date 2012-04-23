using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// The parameter collection type used by request msg.
    /// </summary>
    [Serializable]
    public class ParameterCollection
    {
        private IDictionary<string, object> parmValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection()
        {
            this.parmValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Get contains key
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public bool ContainsKey(string paramName)
        {
            return parmValues.ContainsKey(paramName);
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified param name.
        /// </summary>
        /// <value></value>
        public object this[string paramName]
        {
            get
            {
                if (parmValues.ContainsKey(paramName))
                {
                    return parmValues[paramName];
                }
                return null;
            }
            set
            {
                //if (value == null) return;
                parmValues[paramName] = value;
            }
        }

        /// <summary>
        /// Removes the specified param name.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        public void Remove(string paramName)
        {
            if (parmValues.ContainsKey(paramName))
            {
                parmValues.Remove(paramName);
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            parmValues.Clear();
        }

        /// <summary>
        /// Get param count.
        /// </summary>
        public int Count
        {
            get
            {
                return parmValues.Count;
            }
        }

        /// <summary>
        /// Get param Keys;
        /// </summary>
        public IList<string> Keys
        {
            get
            {
                return new List<string>(parmValues.Keys);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            if (parmValues.Keys.Count == 0)
            {
                return "{}";
            }
            else
            {
                try
                {
                    return SerializationManager.SerializeJson(parmValues);
                }
                catch
                {
                    JObject json = new JObject();
                    foreach (string key in new List<string>(parmValues.Keys))
                    {
                        //将数据进行系列化
                        var jsonString = string.Empty;
                        try
                        {
                            jsonString = SerializationManager.SerializeJson(parmValues[key]);
                        }
                        catch (Exception ex)
                        {
                            jsonString = SerializationManager.SerializeJson(ex.Message);
                        }

                        //添加到json对象
                        json.Add(key, JToken.Parse(jsonString));
                    }

                    return json.ToString(Formatting.Indented);
                }
            }
        }
    }
}
