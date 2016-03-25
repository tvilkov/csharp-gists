using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CSharp.Gists.Tasks;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Tasks
{
    // ReSharper disable InconsistentNaming
    public class TaskExtensionsTests
    {
        [TestFixture(Category = "WithTimeout", Description = "WithTimeout еask extention tests")]
        public class WithTimeout
        {
            [Test]
            public async void TimeoutExpire()
            {
                Exception exptectedException = null;
                try
                {
                    await Task
                        .Delay(TimeSpan.FromSeconds(3))
                        .WithTimeout(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    exptectedException = ex;
                }

                Assert.IsNotNull(exptectedException, "Expected exception was not raised");
                Assert.IsInstanceOf<TaskCanceledException>(exptectedException, "Expected exception is not a TaskCancelledException");
            }

            [Test]
            public async void TimeoutExpireTaskWithResult()
            {
                Exception exptectedException = null;
                string taskResult = null;
                try
                {
                    taskResult = await CreateTask(TimeSpan.FromSeconds(3), "Task result")
                        .WithTimeout(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    exptectedException = ex;
                }

                Assert.IsNotNull(exptectedException, "Expected exception was not raised");
                Assert.IsInstanceOf<TaskCanceledException>(exptectedException, "Expected exception is not a TaskCancelledException");
                Assert.IsNull(taskResult);
            }

            [Test]
            public async void RunToCompletion()
            {
                await Task
                    .Delay(TimeSpan.FromSeconds(1))
                    .WithTimeout(TimeSpan.FromSeconds(3));
            }

            [Test]
            public async void RunToCompletionTaskWithResult()
            {
                var result = await CreateTask(TimeSpan.FromSeconds(1), "Task result")
                    .WithTimeout(TimeSpan.FromSeconds(3));

                Assert.AreEqual("Task result", result);
            }

            [TestCase(true, TestName = "Void task")]
            [TestCase(false, TestName = "Task with result")]
            public async void TaskShouldNotRaiseExceptionWhenFaultedAfterTimeout(bool voidTask)
            {
                var unobservedExceptionWasRaised = false;
                var timeout = false;
                TaskScheduler.UnobservedTaskException += (sender, args) =>
                {
                    Debug.WriteLine("Unobserved exception!");
                    unobservedExceptionWasRaised = true;
                };
                try
                {
                    if (voidTask)
                    {
                        await CreateVoidTaskWhichFail(TimeSpan.FromSeconds(3), new Exception("Error in task which is to be timeouted"))
                        .WithTimeout(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        await CreateTaskWhichFail(TimeSpan.FromSeconds(3), new Exception("Error in task which is to be timeouted"))
                            .WithTimeout(TimeSpan.FromSeconds(1));
                    }
                }
                catch (TaskCanceledException)
                {
                    timeout = true;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));

                Assert.IsTrue(timeout, "Task timeout didn't happen");
                Assert.IsFalse(unobservedExceptionWasRaised, "An unobserved task exception was raised by task");
            }

            protected async Task<string> CreateTask(TimeSpan executionDuration, string result = null)
            {
                await Task.Delay(executionDuration);
                return result;
            }

            protected async Task CreateVoidTaskWhichFail(TimeSpan executionDuration, Exception error)
            {
                await Task.Delay(executionDuration);
                throw error;
            }

            protected async Task<string> CreateTaskWhichFail(TimeSpan executionDuration, Exception error)
            {
                await Task.Delay(executionDuration);
                throw error;
            }
        }

    }
    // ReSharper restore InconsistentNaming
}