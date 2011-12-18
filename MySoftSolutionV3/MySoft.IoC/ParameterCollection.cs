using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC
{
    /// <summary>
    /// The parameter collection type used by request msg.
    /// </summary>
    [Serializable]
    public class ParameterCollection
    {
        private Hashtable parmValues = new Hashtable();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection() { }

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

        private string paramString = string.Empty;
        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(paramString))
            {
                return paramString;
            }

            if (parmValues.Keys.Count == 0)
            {
                paramString = "{}";
            }
            else
            {
                JObject json = new JObject();
                foreach (string key in parmValues.Keys)
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

                paramString = json.ToString(Formatting.Indented);
            }

            return paramString;
        }
    }
}
