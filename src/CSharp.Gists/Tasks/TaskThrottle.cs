using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Gists.Tasks
{
    /// <summary>
    /// Provides throttling capabilities for tasks.
    /// TODO[tv]: Introduce different throttle strategies (tasks per minute, for instance)
    /// </summary>
    [DebuggerDisplay("Throttle: {ActiveTaskCount} active tasks of {m_ConcurencyLimit} allowed")]
    class TaskThrottle : IDisposable
    {
        private readonly int m_ConcurencyLimit;
        private SemaphoreSlim m_Semaphore;
        private CancellationTokenSource m_DisposeCancellationTokenSource;

        public TaskThrottle(int concurencyLimit)
        {
            if (concurencyLimit <= 0)
                throw new ArgumentException(@"concurencyLimit is expected to be a positive integer value", "concurencyLimit");
            m_ConcurencyLimit = concurencyLimit;
        }

        public int ActiveTaskCount
        {
            [DebuggerStepThrough]
            get { return m_ConcurencyLimit - m_Semaphore.CurrentCount; }
        }

        private void ensureInitialized()
        {
            LazyInitializer.EnsureInitialized(ref m_Semaphore, () => new SemaphoreSlim(m_ConcurencyLimit));
            LazyInitializer.EnsureInitialized(ref m_DisposeCancellationTokenSource, () => new CancellationTokenSource());
        }

        public void Throttle(Action action, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (action == null) throw new ArgumentNullException("action");

            ThrottleCore(cancellationToken);

            action();
        }

        public T Throttle<T>(Func<T> action, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (action == null) throw new ArgumentNullException("action");

            ThrottleCore(cancellationToken);

            return action();
        }

        protected void ThrottleCore(CancellationToken cancellationToken)
        {
            ensureInitialized();

            var signalled = WaitHandle.WaitAny(new[]
                {
                    m_DisposeCancellationTokenSource.Token.WaitHandle,
                    cancellationToken.WaitHandle,
                    m_Semaphore.AvailableWaitHandle
                });
            if (signalled == 0) throw new ObjectDisposedException("Throttle was disposed");
            if (signalled == 1) throw new TaskCanceledException("The task was cancelled");
        }

        public Task ThrottleAsync(Action action, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null) throw new ArgumentNullException("action");

            return ThrottleAsync(() => Task.Factory.StartNew(action), cancellationToken);
        }

        public Task<T> ThrottleAsync<T>(Func<T> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null) throw new ArgumentNullException("action");

            return ThrottleAsync(() => Task.Factory.StartNew(action), cancellationToken);
        }

        public async Task ThrottleAsync(Func<Task> actionFactory, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            ensureInitialized();

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_DisposeCancellationTokenSource.Token, cancellationToken))
            {
                await m_Semaphore.WaitAsync(tokenSource.Token);
                try
                {
                    var task = actionFactory();
                    Debug.Assert(task != null, "Null task returned from the factory");

                    await task;
                }
                finally
                {
                    m_Semaphore.Release();
                }
            }
        }

        public async Task<T> ThrottleAsync<T>(Func<Task<T>> actionFactory, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            ensureInitialized();

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_DisposeCancellationTokenSource.Token, cancellationToken))
            {
                await m_Semaphore.WaitAsync(tokenSource.Token);
                try
                {
                    var task = actionFactory();
                    Debug.Assert(task != null, "Null task returned from the factory");

                    var result = await task;
                    return result;
                }
                finally
                {
                    m_Semaphore.Release();
                }
            }
        }

        public void Dispose()
        {
            m_DisposeCancellationTokenSource.Cancel();
            m_Semaphore.Dispose();
        }
    }
}