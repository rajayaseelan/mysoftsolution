using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using MySoft.Logger;
using System.Collections;
using MySoft.Threading;

namespace MySoft.Web
{
    /// <summary>
    /// 更新项
    /// </summary>
    public interface IUpdateItem
    {
        /// <summary>
        /// 重试间隔(单位:分钟)
        /// </summary>
        int RetryInterval { get; set; }

        /// <summary>
        /// 立即更新页面
        /// </summary>
        void Update();

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        void Update(DateTime updateTime);

        /// <summary>
        /// 异步更新，TimeSpan表示延迟更新的时间，所有依赖失效
        /// </summary>
        void Update(TimeSpan timeSpan);

        /// <summary>
        /// 当前是否可以更新
        /// </summary>
        bool NeedUpdate(DateTime updateTime);

        /// <summary>
        /// 设置域名
        /// </summary>
        /// <param name="domainUri"></param>
        void SetDomain(string domainUri);
    }

    /// <summary>
    /// 静态页子项接口
    /// </summary>
    public interface IStaticPageItem : IUpdateItem
    {
        /// <summary>
        /// 开始处理
        /// </summary>
        event ExecuteEventHandler OnStart;

        /// <summary>
        /// 结束处理
        /// </summary>
        event ExecuteEventHandler OnComplete;

        /// <summary>
        /// 生成时的回调
        /// </summary>
        event CallbackEventHandler Callback;

        /// <summary>
        /// 静态页生成依赖
        /// </summary>
        IUpdateDependency StaticPageDependency { get; set; }

        /// <summary>
        /// 输出编码
        /// </summary>
        Encoding OutEncoding { get; set; }

        /// <summary>
        /// 输入编码
        /// </summary>
        Encoding InEncoding { get; set; }

        /// <summary>
        /// 是否为远程页面
        /// </summary>
        bool IsRemote { get; set; }
    }

    /// <summary>
    /// 执行生成时的委托
    /// </summary>
    /// <param name="createTime"></param>
    /// <param name="dynamicurl"></param>
    /// <param name="staticurl"></param>
    public delegate void ExecuteEventHandler(DateTime createTime, string dynamicurl, string staticurl);

    /// <summary>
    /// 生成页面时委托
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public delegate string CallbackEventHandler(string content);

    /// <summary>
    /// 返回值数组的委托
    /// </summary>
    /// <returns></returns>
    public delegate object[] GetResultEventHandler(object state);

    /// <summary>
    /// 通用静态页子项
    /// </summary>
    public sealed class SingleStaticPageItem : IStaticPageItem
    {
        /// <summary>
        /// 回调
        /// </summary>
        public event ExecuteEventHandler OnStart;

        /// <summary>
        /// 结束处理
        /// </summary>
        public event ExecuteEventHandler OnComplete;

        /// <summary>
        /// 生成时的回调
        /// </summary>
        public event CallbackEventHandler Callback;

        #region 属性

        private string query;
        private string templatePath;
        private string savePath;
        private string validateString;
        private bool updateComplete;

        private IUpdateDependency staticPageDependency;
        /// <summary>
        /// 静态页依赖方案
        /// </summary>
        public IUpdateDependency StaticPageDependency
        {
            get { return staticPageDependency; }
            set { staticPageDependency = value; }
        }

        private Encoding outEncoding;
        /// <summary>
        /// 输出编码
        /// </summary>
        public Encoding OutEncoding
        {
            get { return outEncoding; }
            set { outEncoding = value; }
        }

        private Encoding inEncoding;
        /// <summary>
        /// 输入编码
        /// </summary>
        public Encoding InEncoding
        {
            get { return inEncoding; }
            set { inEncoding = value; }
        }

        private bool isRemote;
        /// <summary>
        /// 是否为远程页面
        /// </summary>
        public bool IsRemote
        {
            get { return isRemote; }
            set { isRemote = value; }
        }

        private int retryInterval = 5;
        /// <summary>
        /// 重试间隔(单位:分钟)
        /// </summary>
        public int RetryInterval
        {
            get { return retryInterval; }
            set { retryInterval = value; }
        }

        private int inMinutes = 5;
        /// <summary>
        /// 多少分钟之内才生成
        /// </summary>
        public int InMinutes
        {
            get { return inMinutes; }
            set { inMinutes = value; }
        }

        /// <summary>
        /// 当前是否可以更新
        /// </summary>
        bool IUpdateItem.NeedUpdate(DateTime updateTime)
        {
            //如果没更新完成，则返回
            if (!updateComplete) return false;

            //判断策略是否已经达到更新值
            return staticPageDependency.HasUpdate(updateTime, inMinutes);
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        private SingleStaticPageItem()
        {
            this.outEncoding = Encoding.UTF8;
            this.inEncoding = Encoding.UTF8;
            this.staticPageDependency = new SlidingUpdateTime(new TimeSpan(1, 0, 0));
            this.isRemote = false;
            this.updateComplete = true;
        }

        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        /// <param name="templatePath">模板路径</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="validateString">验证字符串</param>
        public SingleStaticPageItem(string templatePath, string savePath, string validateString)
            : this()
        {
            this.templatePath = templatePath;
            this.savePath = savePath;
            this.validateString = validateString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePath">模板路径</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="createSpan">生成间隔时间</param>
        public SingleStaticPageItem(string templatePath, string savePath, string validateString, IUpdateDependency staticPageDependency)
            : this(templatePath, savePath, validateString)
        {
            this.staticPageDependency = staticPageDependency;
        }

        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        /// <param name="templatePath">模板页路径</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="createSpan">生成间隔时间</param>
        /// <param name="validateString">验证字符串</param>
        /// <param name="query">查询参数字符串</param>
        public SingleStaticPageItem(string templatePath, string query, string savePath, string validateString)
            : this(templatePath, savePath, validateString)
        {
            this.query = query;
        }


        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        /// <param name="templatePath">模板页路径</param>
        /// <param name="query">查询参数字符串</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="createSpan">生成间隔时间</param>
        /// <param name="validateString">验证字符</param>
        public SingleStaticPageItem(string templatePath, string query, string savePath, string validateString, IUpdateDependency staticPageDependency)
            : this(templatePath, query, savePath, validateString)
        {
            this.staticPageDependency = staticPageDependency;
        }

        #endregion

        /// <summary>
        /// 设置域名
        /// </summary>
        /// <param name="domainUri"></param>
        public void SetDomain(string domainUri)
        {
            //如果是远程请求，则跳过
            if (isRemote) return;

            isRemote = true;
            templatePath = string.Concat(domainUri.TrimEnd('/'), "/", templatePath.TrimStart(new char[] { '~', '/' }));
        }

        /// <summary>
        /// 立即更新页面
        /// </summary>
        public void Update()
        {
            (this as IUpdateItem).Update(DateTime.MaxValue);
        }

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        void IUpdateItem.Update(DateTime updateTime)
        {
            updateComplete = false;

            string dynamicurl = templatePath;
            string staticurl = savePath;
            if (!string.IsNullOrEmpty(query))
                dynamicurl = string.Format("{0}?{1}", dynamicurl, query);

            var item = new UpdateItem { DynamicUrl = dynamicurl, StaticPath = staticurl };

            try
            {
                string content = null;

                if (isRemote)
                    content = StaticPageManager.GetRemotePageString(item.DynamicUrl, inEncoding, validateString);
                else
                    content = StaticPageManager.GetLocalPageString(item.Path, item.Query, inEncoding, validateString);

                DateTime createTime = DateTime.Now;

                //开始生成
                if (OnStart != null)
                {
                    try
                    {
                        OnStart(createTime, item.DynamicUrl, item.StaticUrl);
                    }
                    catch (Exception ex)
                    {
                    };
                }

                //生成时回调
                if (Callback != null)
                {
                    try
                    {
                        content = Callback(content);
                    }
                    catch (Exception ex)
                    {
                    };
                }

                string extension = Path.GetExtension(item.StaticPath);
                if (extension != null && extension.ToLower() == ".js")
                {
                    //加入静态页生成元素
                    content = string.Format("{3}\r\n\r\n//<!-- 生成方式：主动生成 -->\r\n//<!-- 更新时间：{0} -->\r\n//<!-- 动态URL：{1} -->\r\n//<!-- 静态URL：{2} -->",
                                        createTime.ToString("yyyy-MM-dd HH:mm:ss"), item.DynamicUrl, item.StaticUrl, content.Trim());
                }
                else
                {
                    //加入静态页生成元素
                    content = string.Format("{3}\r\n\r\n<!-- 生成方式：主动生成 -->\r\n<!-- 更新时间：{0} -->\r\n<!-- 动态URL：{1} -->\r\n<!-- 静态URL：{2} -->",
                                        createTime.ToString("yyyy-MM-dd HH:mm:ss"), item.DynamicUrl, item.StaticUrl, content.Trim());
                }

                StaticPageManager.SaveFile(content, item.StaticPath, outEncoding);

                //结束生成
                if (OnComplete != null)
                {
                    try
                    {
                        OnComplete(createTime, item.DynamicUrl, item.StaticUrl);
                    }
                    catch (Exception ex)
                    {
                    };
                }

                staticPageDependency.UpdateSuccess = true;
            }
            catch (Exception ex)
            {
                StaticPageManager.SaveError(new StaticPageException(string.Format("单个生成静态文件失败，【{2}】分钟后重新生成！\r\n{0} => {1}",
                    item.DynamicUrl, item.StaticUrl, retryInterval), ex));

                //如果出错，则继续往下执行
                staticPageDependency.UpdateSuccess = false;
            }
            finally
            {
                //设置最后更新时间
                //staticPageDependency.LastUpdateTime = updateTime;
            }

            //全部生成成功才设置最后更新时间
            if (updateTime == DateTime.MaxValue)
                staticPageDependency.LastUpdateTime = DateTime.Now;
            else
                staticPageDependency.LastUpdateTime = updateTime;

            if (!staticPageDependency.UpdateSuccess)
            {
                //全部生成成功才设置最后更新时间,否则往后推5分钟重新生成
                staticPageDependency.LastUpdateTime = staticPageDependency.LastUpdateTime.AddMinutes(retryInterval);
            }

            updateComplete = true;
        }

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        public void Update(TimeSpan timeSpan)
        {
            ManagedThreadPool.QueueUserWorkItem(state =>
            {
                ArrayList arr = state as ArrayList;
                IStaticPageItem item = (IStaticPageItem)arr[0];
                TimeSpan span = (TimeSpan)arr[1];
                Thread.Sleep(span);

                item.Update();
            }, new ArrayList { this, timeSpan });
        }
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    public sealed class StaticPageParamInfo
    {
        private object state;
        private string paramName;
        /// <summary>
        /// 参数名
        /// </summary>
        internal string ParamName
        {
            get { return paramName; }
        }

        private GetResultEventHandler getResult;
        /// <summary>
        /// 获取值委托
        /// </summary>
        internal object[] GetResult()
        {
            if (getResult != null)
                return getResult(state);
            else
                return new object[0];
        }

        public StaticPageParamInfo(string paramName, object[] values)
        {
            this.paramName = paramName;
            this.getResult = delegate(object state) { return values; };
        }

        public StaticPageParamInfo(string paramName, GetResultEventHandler getResult)
        {
            this.paramName = paramName;
            this.getResult = getResult;
        }

        public StaticPageParamInfo(string paramName, GetResultEventHandler getResult, object state)
            : this(paramName, getResult)
        {
            this.state = state;
        }
    }

    /// <summary>
    /// 分页静态页子项
    /// </summary>
    public sealed class ParamStaticPageItem : IStaticPageItem
    {
        /// <summary>
        /// 回调
        /// </summary>
        public event ExecuteEventHandler OnStart;

        /// <summary>
        /// 结束处理
        /// </summary>
        public event ExecuteEventHandler OnComplete;

        /// <summary>
        /// 生成时的回调
        /// </summary>
        public event CallbackEventHandler Callback;

        #region 属性

        private string templatePath;
        private string savePath;
        private string query;
        private string validateString;
        private bool updateComplete;
        private List<UpdateItem> updateErrorList;
        private IUpdateDependency staticPageDependency;
        /// <summary>
        /// 静态页生成依赖
        /// </summary>
        public IUpdateDependency StaticPageDependency
        {
            get { return staticPageDependency; }
            set { staticPageDependency = value; }
        }

        private StaticPageParamInfo[] paramInfos;
        /// <summary>
        /// 静态页参数
        /// </summary>
        public StaticPageParamInfo[] ParamInfos
        {
            get { return paramInfos; }
            set { paramInfos = value; }
        }

        private Encoding outEncoding;
        /// <summary>
        /// 输出编码
        /// </summary>
        public Encoding OutEncoding
        {
            get { return outEncoding; }
            set { outEncoding = value; }
        }

        private Encoding inEncoding;
        /// <summary>
        /// 输入编码
        /// </summary>
        public Encoding InEncoding
        {
            get { return inEncoding; }
            set { inEncoding = value; }
        }

        private bool isRemote;
        /// <summary>
        /// 是否为远程页面
        /// </summary>
        public bool IsRemote
        {
            get { return isRemote; }
            set { isRemote = value; }
        }

        private int retryInterval = 5;
        /// <summary>
        /// 重试间隔(单位:分钟)
        /// </summary>
        public int RetryInterval
        {
            get { return retryInterval; }
            set { retryInterval = value; }
        }

        private int inMinutes = 5;
        /// <summary>
        /// 多少分钟之内才生成
        /// </summary>
        public int InMinutes
        {
            get { return inMinutes; }
            set { inMinutes = value; }
        }

        /// <summary>
        /// 默认线程数
        /// </summary>
        private const int DEFAULT_THREAD = 1;

        /// <summary>
        /// 最小线程数
        /// </summary>
        private const int MIN_THREAD = 1;

        /// <summary>
        /// 最大线程数
        /// </summary>
        private const int MAX_THREAD = 10;

        private int threadCount = DEFAULT_THREAD;
        /// <summary>
        /// 生成页面的线程数，默认为1
        /// </summary>
        public int ThreadCount
        {
            get { return threadCount; }
            set
            {
                if (threadCount > MAX_THREAD)
                    throw new WebException(string.Format("生成线程数不能大于{0}！", MAX_THREAD));

                if (threadCount < MIN_THREAD)
                    throw new WebException(string.Format("生成线程数不能小于{0}！", MIN_THREAD));

                threadCount = value;
            }
        }

        /// <summary>
        /// 当前是否可以更新
        /// </summary>
        bool IUpdateItem.NeedUpdate(DateTime updateTime)
        {
            //如果没更新完成，则返回
            if (!updateComplete) return false;

            //判断策略是否已经达到更新值
            return staticPageDependency.HasUpdate(updateTime, inMinutes);
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        private ParamStaticPageItem()
        {
            this.outEncoding = Encoding.UTF8;
            this.inEncoding = Encoding.UTF8;
            this.staticPageDependency = new SlidingUpdateTime(new TimeSpan(1, 0, 0));
            this.isRemote = false;
            this.updateComplete = true;
            this.updateErrorList = new List<UpdateItem>();
        }

        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        /// <param name="templatePath">模板页路径</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="createSpan">生成间隔时间</param>
        /// <param name="validateString">验证字符串</param>
        /// <param name="query">查询参数字符串</param>
        public ParamStaticPageItem(string templatePath, string query, string savePath, string validateString, params StaticPageParamInfo[] paramInfos)
            : this()
        {
            this.templatePath = templatePath;
            this.query = query;
            this.savePath = savePath;
            this.validateString = validateString;
            this.paramInfos = paramInfos;
        }


        /// <summary>
        /// 初始化静态页生成类
        /// </summary>
        /// <param name="templatePath">模板页路径</param>
        /// <param name="query">查询参数字符串</param>
        /// <param name="savePath">生成文件路径</param>
        /// <param name="createSpan">生成间隔时间</param>
        /// <param name="validateString">验证字符</param>
        public ParamStaticPageItem(string templatePath, string query, string savePath, string validateString, IUpdateDependency staticPageDependency, params StaticPageParamInfo[] paramInfos)
            : this(templatePath, query, savePath, validateString, paramInfos)
        {
            this.staticPageDependency = staticPageDependency;
        }

        #endregion

        /// <summary>
        /// 设置域名
        /// </summary>
        /// <param name="domainUri"></param>
        public void SetDomain(string domainUri)
        {
            //如果是远程请求，则跳过
            if (isRemote) return;

            isRemote = true;
            templatePath = string.Concat(domainUri.TrimEnd('/'), "/", templatePath.TrimStart(new char[] { '~', '/' }));
        }

        /// <summary>
        /// 立即更新页面
        /// </summary>
        public void Update()
        {
            (this as IUpdateItem).Update(DateTime.MaxValue);
        }

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        void IUpdateItem.Update(DateTime updateTime)
        {
            updateComplete = false;

            //如果未能全部生成成功，则生成失败列表
            if (updateTime != DateTime.MaxValue && !staticPageDependency.UpdateSuccess)
            {
                var errors = Update(updateTime, updateErrorList);

                //添加到异常列表
                if (errors.Count > 0)
                {
                    updateErrorList.AddRange(errors);
                }
            }
            else
            {
                var dictPosition = new Dictionary<string, int>();
                var dictValues = new Dictionary<string, IList<object>>();
                foreach (var paramInfo in paramInfos)
                {
                    try
                    {
                        if (!dictValues.ContainsKey(paramInfo.ParamName))
                        {
                            var objlist = new List<object>(paramInfo.GetResult());
                            dictValues[paramInfo.ParamName] = objlist;
                            dictPosition[paramInfo.ParamName] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        StaticPageManager.SaveError(new StaticPageException(string.Format("获取参数{0}的值出错，URL：{1}！", paramInfo.ParamName, templatePath), ex));
                        return;
                    }
                }

                try
                {
                    int count = GetPageCount(dictValues);
                    var items = new List<UpdateItem>();

                    string dynamicurl = templatePath;
                    string staticurl = savePath;
                    if (!string.IsNullOrEmpty(query))
                        dynamicurl = string.Format("{0}?{1}", dynamicurl, query);

                    for (int index = 0; index < count; index++)
                    {
                        var tmpDynamicUrl = dynamicurl;
                        var tmpStaticUrl = staticurl;

                        //生成对应的url
                        foreach (string key in dictValues.Keys)
                        {
                            var value = dictValues[key][dictPosition[key]].ToString();

                            //动态地址
                            tmpDynamicUrl = tmpDynamicUrl.Replace(key, value);

                            //静态地址
                            tmpStaticUrl = tmpStaticUrl.Replace(key, value);
                        }

                        //添加到队列中
                        items.Add(new UpdateItem { DynamicUrl = tmpDynamicUrl, StaticPath = tmpStaticUrl });

                        //定位处理
                        SetPosition(dictPosition, dictValues, dictPosition.Keys.Count - 1);
                    }

                    if (items.Count > 0)
                    {
                        //更新页面, 只有一个线程时
                        if (threadCount == 1)
                        {
                            var errors = Update(updateTime, items);

                            //添加到异常列表
                            if (errors.Count > 0)
                            {
                                updateErrorList.AddRange(errors);
                            }
                        }
                        else
                        {
                            int pageSize = (int)Math.Ceiling(items.Count / (threadCount * 1.0));

                            //分页生成静态页
                            var events = new AutoResetEvent[threadCount];

                            for (int index = 0; index < threadCount; index++)
                            {
                                events[index] = new AutoResetEvent(false);

                                var updateItems = new List<UpdateItem>();
                                if (items.Count >= (index + 1) * pageSize)
                                    updateItems = items.GetRange(index * pageSize, pageSize);
                                else
                                    updateItems = items.GetRange(index * pageSize, items.Count - (index * pageSize));

                                var thread = new Thread(state =>
                                {
                                    if (state == null) return;

                                    var arr = state as ArrayList;
                                    var list = arr[0] as List<UpdateItem>;
                                    var reset = arr[1] as AutoResetEvent;

                                    var errors = Update(updateTime, list);

                                    //添加到异常列表
                                    if (errors.Count > 0)
                                    {
                                        lock (updateErrorList)
                                        {
                                            updateErrorList.AddRange(errors);
                                        }
                                    }

                                    reset.Set();
                                });

                                //启动线程
                                thread.Start(new ArrayList { updateItems, events[index] });
                            }

                            //等待所有响应
                            WaitHandle.WaitAll(events);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticPageManager.SaveError(new StaticPageException("创建URL列表出错！ Error => " + ex.Message, ex));
                }
            }

            //全部生成成功才设置最后更新时间
            if (updateTime == DateTime.MaxValue)
                staticPageDependency.LastUpdateTime = DateTime.Now;
            else
                staticPageDependency.LastUpdateTime = updateTime;

            if (!staticPageDependency.UpdateSuccess)
            {
                //全部生成成功才设置最后更新时间,否则往后推5分钟重新生成
                staticPageDependency.LastUpdateTime = staticPageDependency.LastUpdateTime.AddMinutes(retryInterval);
            }

            //更新完成
            updateComplete = true;
        }

        /// <summary>
        /// 获取总循环次数
        /// </summary>
        /// <returns></returns>
        private int GetPageCount(IDictionary<string, IList<object>> dictValues)
        {
            int count = 1;
            foreach (string paramName in dictValues.Keys)
            {
                count *= dictValues[paramName].Count;
            }
            return count;
        }

        /// <summary>
        /// 设置对应的坐标值
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        /// <param name="index"></param>
        private void SetPosition(IDictionary<string, int> positions, IDictionary<string, IList<object>> values, int index)
        {
            if (index < 0) return;
            string key = new List<string>(positions.Keys)[index];
            if (positions[key] < values[key].Count - 1)
            {
                positions[key]++;
            }
            else
            {
                positions[key] = 0;
                SetPosition(positions, values, --index);
            }
        }

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        private IList<UpdateItem> Update(DateTime updateTime, IList<UpdateItem> items)
        {
            var errorList = new List<UpdateItem>();

            try
            {
                foreach (var item in items)
                {
                    try
                    {
                        string content = null;
                        if (isRemote)
                            content = StaticPageManager.GetRemotePageString(item.DynamicUrl, inEncoding, validateString);
                        else
                            content = StaticPageManager.GetLocalPageString(item.Path, item.Query, inEncoding, validateString);

                        DateTime createTime = DateTime.Now;

                        //开始生成
                        if (OnStart != null)
                        {
                            try
                            {
                                OnStart(createTime, item.DynamicUrl, item.StaticUrl);
                            }
                            catch { };
                        }

                        //生成时回调
                        if (Callback != null)
                        {
                            try
                            {
                                content = Callback(content);
                            }
                            catch { };
                        }

                        string extension = Path.GetExtension(item.StaticPath);
                        if (extension != null && extension.ToLower() == ".js")
                        {
                            //加入静态页生成元素
                            content = string.Format("{3}\r\n\r\n//<!-- 生成方式：主动生成 -->\r\n//<!-- 更新时间：{0} -->\r\n//<!-- 动态URL：{1} -->\r\n//<!-- 静态URL：{2} -->",
                                                createTime.ToString("yyyy-MM-dd HH:mm:ss"), item.DynamicUrl, item.StaticUrl, content.Trim());
                        }
                        else
                        {
                            //加入静态页生成元素
                            content = string.Format("{3}\r\n\r\n<!-- 生成方式：主动生成 -->\r\n<!-- 更新时间：{0} -->\r\n<!-- 动态URL：{1} -->\r\n<!-- 静态URL：{2} -->",
                                                createTime.ToString("yyyy-MM-dd HH:mm:ss"), item.DynamicUrl, item.StaticUrl, content.Trim());
                        }

                        StaticPageManager.SaveFile(content, item.StaticPath, outEncoding);

                        //结束生成
                        if (OnComplete != null)
                        {
                            try
                            {
                                OnComplete(createTime, item.DynamicUrl, item.StaticUrl);
                            }
                            catch { };
                        }

                        //置状态为生成成功
                        item.UpdateSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        //置状态为生成失败
                        item.UpdateSuccess = false;

                        StaticPageManager.SaveError(new StaticPageException(string.Format("单个生成静态文件失败，等待重新生成！\r\n{0} => {1}",
                                item.DynamicUrl, item.StaticUrl), ex));
                    }
                }

                //未全部更新成功
                if (items.Any(p => !p.UpdateSuccess))
                {
                    errorList = items.Where(p => !p.UpdateSuccess).ToList();
                    string html = string.Join("\r\n", errorList.Select(p => string.Format("{0} => {1}", p.DynamicUrl, p.StaticUrl)).ToArray());
                    throw new StaticPageException(string.Format("批量生成【{0}】个静态页失败，【{1}】分钟后重新生成！\r\n{2}",
                        errorList.Count, retryInterval, html));
                }

                staticPageDependency.UpdateSuccess = true;
            }
            catch (Exception ex)
            {
                //如果出错，则继续往下执行
                if (ex is StaticPageException)
                    StaticPageManager.SaveError(ex as StaticPageException);

                staticPageDependency.UpdateSuccess = false;
            }
            finally
            {
                //设置最后更新时间
                //staticPageDependency.LastUpdateTime = updateTime;
            }

            return errorList;
        }

        /// <summary>
        /// 对页面进行更新
        /// </summary>
        public void Update(TimeSpan timeSpan)
        {
            ManagedThreadPool.QueueUserWorkItem(state =>
            {
                TimeSpan span = (TimeSpan)state;
                Thread.Sleep(span);

                (this as IUpdateItem).Update(DateTime.MaxValue);
            }, timeSpan);
        }
    }
}
