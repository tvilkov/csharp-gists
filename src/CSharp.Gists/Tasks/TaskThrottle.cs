using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Gists.Tasks
{
    /// <summary>
    /// Provides throttling capabilities for tasks.
    /// </summary>
    [DebuggerDisplay("Throttle: {ActiveTaskCount} active tasks of {m_ConcurencyLimit} allowed")]
    class TaskThrottle : IDisposable
    {
        private readonly int m_ConcurencyLimit;
        private SemaphoreSlim m_Semaphore;
        private CancellationTokenSource m_CancellationTokenSource;

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
            LazyInitializer.EnsureInitialized(ref m_CancellationTokenSource, () => new CancellationTokenSource());
        }

        public Task<T> Throttle<T>(Func<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            return Throttle(() => Task.Factory.StartNew(action), CancellationToken.None);
        }

        public Task<T> Throttle<T>(Func<T> action, CancellationToken cancellationToken)
        {
            if (action == null) throw new ArgumentNullException("action");

            return Throttle(() => Task.Factory.StartNew(action), cancellationToken);
        }

        public Task Throttle(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            return Throttle(() => Task.Factory.StartNew(action), CancellationToken.None);
        }

        public Task Throttle(Action action, CancellationToken cancellationToken)
        {
            if (action == null) throw new ArgumentNullException("action");

            return Throttle(() => Task.Factory.StartNew(action), cancellationToken);
        }

        public Task Throttle(Func<Task> actionFactory)
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            return Throttle(actionFactory, CancellationToken.None);
        }

        public async Task Throttle(Func<Task> actionFactory, CancellationToken cancellationToken)
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            ensureInitialized();

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token, cancellationToken))
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

        public Task<T> Throttle<T>(Func<Task<T>> actionFactory)
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            return Throttle(actionFactory, CancellationToken.None);
        }

        public async Task<T> Throttle<T>(Func<Task<T>> actionFactory, CancellationToken cancellationToken)
        {
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            ensureInitialized();

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token, cancellationToken))
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
            m_CancellationTokenSource.Cancel();
            m_Semaphore.Dispose();
        }
    }
}