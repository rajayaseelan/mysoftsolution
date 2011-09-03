using System;
using System.Collections;
using System.Xml.Serialization;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// The StaticPageRuleCollection models a set of StaticPageRules in the Web.config file.
    /// </summary>
    /// <remarks>
    /// The StaticPageRuleCollection is expressed in XML as:
    /// <code>
    /// &lt;StaticPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;StaticPageRule&gt;
    /// &lt;StaticPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;StaticPageRule&gt;
    /// ...
    /// &lt;StaticPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;StaticPageRule&gt;
    /// </code>
    /// </remarks>
    [Serializable]
    public class StaticPageRuleCollection : CollectionBase
    {
        /// <summary>
        /// Adds a new StaticPageRule to the collection.
        /// </summary>
        /// <param name="r">A StaticPageRule instance.</param>
        public virtual void Add(StaticPageRule r)
        {
            this.InnerList.Add(r);
        }

        /// <summary>
        /// Gets or sets a StaticPageRule at a specified ordinal index.
        /// </summary>
        public StaticPageRule this[int index]
        {
            get
            {
                return (StaticPageRule)this.InnerList[index];
            }
            set
            {
                this.InnerList[index] = value;
            }
        }
    }
}
