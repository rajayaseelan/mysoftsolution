using System;
using System.Collections;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// The RewriterRuleCollection models a set of RewriterRules in the Web.config file.
    /// </summary>
    /// <remarks>
    /// The RewriterRuleCollection is expressed in XML as:
    /// <code>
    /// &lt;RewriterRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RewriterRule&gt;
    /// &lt;RewriterRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RewriterRule&gt;
    /// ...
    /// &lt;RewriterRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RewriterRule&gt;
    /// </code>
    /// </remarks>
    [Serializable]
    public class CacheControlRuleCollection : CollectionBase
    {
        /// <summary>
        /// Adds a new RewriterRule to the collection.
        /// </summary>
        /// <param name="r">A RewriterRule instance.</param>
        public virtual void Add(CacheControlRule r)
        {
            this.InnerList.Add(r);
        }

        /// <summary>
        /// Gets or sets a RewriterRule at a specified ordinal index.
        /// </summary>
        public CacheControlRule this[int index]
        {
            get
            {
                return (CacheControlRule)this.InnerList[index];
            }
            set
            {
                this.InnerList[index] = value;
            }
        }
    }
}
