using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CSharp.Gists.Tasks;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Tasks
{
    [TestFixture]
    public class TaskThrottleTests
    {
        [Test]
        public async void CanThrottleAsync()
        {
            const int concurrentTaskCount = 20;
            const int concurencyLimit = 5;
            var throttle = new TaskThrottle(concurencyLimit);

            long runningTaskCount = 0;
            bool invariantCondition = true;

            // Run watcher
            var stopEvent = new ManualResetEvent(false);
            var watcher = new Thread(() =>
                {
                    while (!stopEvent.WaitOne(TimeSpan.FromMilliseconds(1)))
                    {
                        if (Interlocked.Read(ref runningTaskCount) <= concurencyLimit) continue;
                        invariantCondition = false;
                        break;
                    }
                }) { Priority = ThreadPriority.AboveNormal, IsBackground = true };
            watcher.Start();

            // Concurrently run tasks with throttlings
            var tasks = new List<Task>(concurrentTaskCount);
            Parallel.For(0, concurrentTaskCount, new ParallelOptions { MaxDegreeOfParallelism = 3 }, i =>
                {
                    var taskNumber = i + 1;
                    var task = throttle.ThrottleAsync(() =>
                        {
                            Debug.WriteLine("Task {0} started", taskNumber);
                            Interlocked.Increment(ref runningTaskCount);
                            Thread.Sleep(TimeSpan.FromSeconds(3));
                            Interlocked.Decrement(ref runningTaskCount);
                            Debug.WriteLine("Task {0} done", taskNumber);
                        });
                    lock (tasks)
                    {
                        tasks.Add(task);
                    }
                });

            await Task.WhenAll(tasks);
            stopEvent.Set();
            watcher.Join();
            throttle.Dispose();

            Assert.IsTrue(invariantCondition, string.Format("There was more than {0} tasks running at the same time", concurencyLimit));
        }
    }
}