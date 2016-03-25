using System;
using System.Threading.Tasks;

namespace CSharp.Gists.Tasks
{
    public static class TaskCompletionSourceExtensions
    {
        public static void FromTask<T>(this TaskCompletionSource<T> tcs, Task<T> task)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    // Flattern exception this way
                    var ae = task.Exception;
                    var e = ae.InnerExceptions.Count == 1 ? ae.InnerExceptions[0] : ae;
                    tcs.TrySetException(e);
                    break;
                case TaskStatus.Canceled:
                    tcs.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    tcs.TrySetResult(task.Result);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("The task is expected to be in final state. Actual status is '{0}'", task.Status));
            }
        }

        public static void FromTask(this TaskCompletionSource<object> tcs, Task task)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    // Flattern exception this way
                    var ae = task.Exception;
                    var e = ae.InnerExceptions.Count == 1 ? ae.InnerExceptions[0] : ae;
                    tcs.TrySetException(e);
                    break;
                case TaskStatus.Canceled:
                    tcs.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    tcs.TrySetResult(default(object));
                    break;
                default:
                    throw new InvalidOperationException(string.Format("The task is expected to be in final state. Actual status is '{0}'", task.Status));
            }
        }
    }
}