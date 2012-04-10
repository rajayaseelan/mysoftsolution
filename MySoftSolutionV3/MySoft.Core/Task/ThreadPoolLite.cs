using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MySoft.Task
{
    internal class SemaphoreLite
    {
        private int _count;
        private object _sem = new object();

        #region ctor.
        public SemaphoreLite(int count)
        {
            _count = count;
        }
        #endregion

        #region methods
        public bool Wait()
        {
            return Wait(Timeout.Infinite);
        }

        public bool Wait(int millisecondsTimeout)
        {
            if (millisecondsTimeout < 0 && millisecondsTimeout != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout 值应该大于0或等于Timeout.Infinite。");
            }

            lock (_sem)
            {
                if (_count <= 0)
                {
                    if (!Monitor.Wait(_sem, millisecondsTimeout))
                    {
                        return false;
                    }
                }
                _count--;
                return true;
            }
        }

        public void WaitUntil(int amount)
        {
            lock (_sem)
            {
                while (_count < amount)
                {
                    Monitor.Wait(_sem, Timeout.Infinite);
                }
                _count = _count - amount;
            }

        }

        public void Pulse()
        {
            lock (_sem)
            {
                _count++;
                Monitor.Pulse(_sem);
            }
        }
        #endregion
    }

    internal class AutoResetEventLite
    {
        private object _sem = new object();
        private bool _isSet;

        private static int id = 0;
        private int _currentId;

        #region public methods
        public int ID
        {
            get { return _currentId; }
        }

        public AutoResetEventLite(bool initialState)
        {
            if (initialState)
            {
                _isSet = true;
            }
            else
            {
                _isSet = false;
            }
            Interlocked.Exchange(ref _currentId, id);
            Interlocked.Increment(ref id);
        }

        public bool Wait()
        {
            return Wait(Timeout.Infinite);
        }

        public bool Wait(int millisecondsTimeout)
        {
            if (millisecondsTimeout < 0 && millisecondsTimeout != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout 参数应该大于0或等于Timeout.Infinite。");
            }

            lock (_sem)
            {
                bool result;

                if (!_isSet)
                {
                    result = Monitor.Wait(_sem, millisecondsTimeout);
                    if (!result)
                    {
                        return false;
                    }
                }
                _isSet = false;
                return true;
            }
        }

        public void Set()
        {
            lock (_sem)
            {
                if (_isSet)
                {
                    return;
                }
                Monitor.Pulse(_sem);
                _isSet = true;
            }
        }

        #endregion

    }

    //根据需求，可以改成非static的，可以增加管理功能，可以依据计算密集型应用、IO密集型应用作出不同优化等等等等
    public static class ThreadPoolLite
    {
        public static readonly int MaxThreadCount = Environment.ProcessorCount * 2 + 2;
        public static readonly int InitThreadCount = Environment.ProcessorCount;
        public static readonly int MaxQueueLength = Environment.ProcessorCount * 2 + 2;
        private const int MaxMillisecondsTimeoutToQueueItem = 30000;
        private const int MaxMillisecondsTimeoutToKillWorker = 180000;
        private const int MaxMillisecondsTimeoutWaitingForWorker = 3000;

        private static Stack<AutoResetEventLite> _threads = new Stack<AutoResetEventLite>();
        private static Queue<WorkItem> _workItems = new Queue<WorkItem>();

        //queue's empty count.
        private static SemaphoreLite _queueSemR = new SemaphoreLite(MaxQueueLength);
        //queue's count. 
        private static SemaphoreLite _queueSemP = new SemaphoreLite(0);
        private static SemaphoreLite _threadP = new SemaphoreLite(0);

        private static int _aliveThreads = 0;
        public static int AliveThreads
        {
            get
            {
                return _aliveThreads;
            }
        }

        public static int CurrentQueueLength
        {
            get { return _workItems.Count; }
        }

        static ThreadPoolLite()
        {
            Thread dispatcher = new Thread(new ThreadStart(Dispatcher));
            dispatcher.IsBackground = true;
            dispatcher.Start();

            for (int i = 0; i < InitThreadCount; i++)
            {
                AddNewThread();
            }
        }

        public static bool QueueWorkItem(WaitCallback waitCallback)
        {
            return QueueWorkItem(waitCallback, null);
        }

        public static bool QueueWorkItem(WaitCallback waitCallback, object state)
        {
            if (waitCallback == null)
            {
                throw new ArgumentNullException("waitCallback");
            }

            WorkItem item = new WorkItem(waitCallback, state);

            //wait for the queue.
            if (_queueSemR.Wait(MaxMillisecondsTimeoutToQueueItem))
            {
                lock (_workItems)
                {
                    _workItems.Enqueue(item);
                }
                _queueSemP.Pulse();
                return true;
            }
            else
            {
                return false;
            }
        }

        #region private methods
        private static void Worker(object state)
        {
            AutoResetEventLite are = (AutoResetEventLite)state;
            while (are.Wait(MaxMillisecondsTimeoutToKillWorker))
            {
                WorkItem item;
                lock (_workItems)
                {
                    item = _workItems.Dequeue();
                }
                _queueSemR.Pulse();

                //不考虑线程上下文的改变
                try
                {
                    item.WaitCallback(item.State);
                }
                catch
                {
                    //是不是要扔个异步事件出去再说.
                    continue;
                }
                finally
                {
                    lock (_threads)
                    {
                        _threads.Push(are);
                        _threadP.Pulse();
                    }

                    if (AliveThreads == ThreadPoolLite.MaxThreadCount)
                    {
                        Thread.Sleep(0);
                    }
                }
            }

            lock (_threads)
            {
                if (are.Wait(0))
                {
                    //中了五百万
                    _queueSemP.Pulse();
                }
                Interlocked.Decrement(ref _aliveThreads);
                //压缩stack
                Stack<AutoResetEventLite> tmp = new Stack<AutoResetEventLite>();
                while (_threads.Count > 0)
                {
                    AutoResetEventLite areT = _threads.Pop();
                    if (object.ReferenceEquals(areT, are))//感觉好多了
                    {
                        continue;
                    }
                    tmp.Push(areT);
                }
                _threads.Clear();
                while (tmp.Count > 0)
                {
                    _threads.Push(tmp.Pop());
                }
            }
        }

        private static void Dispatcher()
        {
            while (true)
            {
                _queueSemP.Wait();
                if (_threads.Count != 0)
                {
                    ActiveThread();
                    continue;
                }
                else
                {
                    if (_threadP.Wait(MaxMillisecondsTimeoutWaitingForWorker))
                    {
                        ActiveThread();
                        continue;
                    }
                    else
                    {
                        if (AliveThreads < MaxThreadCount)
                        {
                            AddNewThread();
                            ActiveThread();
                            continue;
                        }
                        else
                        {
                            _threadP.Wait();
                            ActiveThread();
                            continue;
                        }
                    }
                }
            }
        }

        private static void ActiveThread()
        {
            _threadP.Wait();

            AutoResetEventLite are;
            lock (_threads)
            {
                are = _threads.Pop();
                are.Set();
            }
            //Thread.Sleep(1);
        }

        private static void AddNewThread()
        {
            AutoResetEventLite are = new AutoResetEventLite(false);
            Thread t = new Thread(new ParameterizedThreadStart(Worker));
            t.IsBackground = true;
            t.Start(are);
            Interlocked.Increment(ref _aliveThreads);
            lock (_threads)
            {
                _threads.Push(are);
            }
            _threadP.Pulse();
        }
        #endregion

        #region WorkItem
        private class WorkItem
        {
            public WorkItem(WaitCallback waitCallback, object state)
            {
                _waitCallback = waitCallback;
                _state = state;
            }

            private WaitCallback _waitCallback;
            public WaitCallback WaitCallback
            {
                get { return _waitCallback; }
            }

            private object _state;
            public object State
            {
                get { return _state; }
            }
        }
        #endregion
    }

}
