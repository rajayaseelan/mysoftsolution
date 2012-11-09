using System;
using System.Collections;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// The RedirectPageRuleCollection models a set of RedirectPageRules in the Web.config file.
    /// </summary>
    /// <remarks>
    /// The RedirectPageRuleCollection is expressed in XML as:
    /// <code>
    /// &lt;RedirectPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RedirectPageRule&gt;
    /// &lt;RedirectPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RedirectPageRule&gt;
    /// ...
    /// &lt;RedirectPageRule&gt;
    ///   &lt;LookFor&gt;<i>pattern to search for</i>&lt;/LookFor&gt;
    ///   &lt;SendTo&gt;<i>string to redirect to</i>&lt;/LookFor&gt;
    /// &lt;RedirectPageRule&gt;
    /// </code>
    /// </remarks>
    [Serializable]
    public class RedirectPageRuleCollection : CollectionBase
    {
        /// <summary>
        /// Adds a new RedirectPageRule to the collection.
        /// </summary>
        /// <param name="r">A RedirectPageRule instance.</param>
        public virtual void Add(RedirectPageRule r)
        {
            this.InnerList.Add(r);
        }

        /// <summary>
        /// Gets or sets a RedirectPageRule at a specified ordinal index.
        /// </summary>
        public RedirectPageRule this[int index]
        {
            get
            {
                return (RedirectPageRule)this.InnerList[index];
            }
            set
            {
                this.InnerList[index] = value;
            }
        }
    }
}
