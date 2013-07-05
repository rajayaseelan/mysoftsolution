using System;
using System.Threading;

namespace MySoft.IoC.Services.Tasks
{
    internal class AsyncResult<TResult> : AsyncResultNoResult
    {
        // Current thread on async result.
        public Thread CurrentThread { get; set; }

        // Field set when operation completes
        private TResult m_result = default(TResult);

        public AsyncResult(AsyncCallback asyncCallback, Object state) :
            base(asyncCallback, state) { }

        public void SetAsCompleted(TResult result,
           Boolean completedSynchronously)
        {
            if (base.IsCompleted) return;

            // Save the asynchronous operation's result
            m_result = result;

            // Tell the base class that the operation completed 
            // sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        new public TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return m_result;  // Return the result (if above didn't throw)
        }
    }
}
