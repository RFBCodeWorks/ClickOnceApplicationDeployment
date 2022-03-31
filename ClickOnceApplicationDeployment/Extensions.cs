using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBApplicationDeployment
{
    static class Extensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StartAction"></param>
        /// <param name="CancelAction"></param>
        /// <param name="tc"><see cref="TaskCompletionSource{TResult}"/> object set within the event handler </param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>TRUE if the task ran to completion, otherwise false.</returns>
        public static Task<bool> RunCancellableTask<T>(Action StartAction, Action CancelAction, TaskCompletionSource<T> tc, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                bool stayRunning = true;
                bool Result = false;
                StartAction();
                while (stayRunning)
                {
                    if (tc.Task.Status >= TaskStatus.RanToCompletion)
                    {
                        Result = true;
                        stayRunning = false;
                    }
                    if (token.IsCancellationRequested)
                    {
                        CancelAction();
                        Result = false;
                        stayRunning = false;
                    }
                    if (stayRunning) Thread.Sleep(100);
                }
                return Result;
            }, token, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
        }

    }
}
