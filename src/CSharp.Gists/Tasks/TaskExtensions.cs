using System;
using System.Threading.Tasks;

namespace CSharp.Gists.Tasks
{
    public static class TaskExtensions
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == null) throw new ArgumentNullException("task");

            var tcs = new TaskCompletionSource<T>();

            task.ContinueWith(tcs.FromTask, TaskContinuationOptions.ExecuteSynchronously);

            Task.Delay(timeout)
                .ContinueWith(t => tcs.TrySetCanceled(), TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (task == null) throw new ArgumentNullException("task");

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(tcs.FromTask, TaskContinuationOptions.ExecuteSynchronously);

            Task.Delay(timeout)
                .ContinueWith(t => tcs.TrySetCanceled(), TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}