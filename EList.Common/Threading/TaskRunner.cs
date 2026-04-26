using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace EList.Common.Threading
{
    public static class TaskRunner
    {
        public static TResult WaitOne<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, TResult> taskAction,
            Func<Task<TResult>, bool> checkResultAction = null)
        {
            var results = WaitInTimeout(sources, taskAction, checkResultAction, null, 1);
            return results.FirstOrDefault();
        }

        public static TResult[] WaitAll<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, TResult> taskAction)
        {
            return WaitInTimeout(sources, taskAction, null, null, null);
        }

        public static TResult WaitOneInTimeout<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, TResult> taskAction,
            TimeSpan timeout,
            Func<Task<TResult>, bool> checkResultAction = null)
        {
            var results = WaitInTimeout(sources, taskAction, checkResultAction, timeout, 1);
            return results.FirstOrDefault();
        }

        public static TResult[] WaitAllInTimeout<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, TResult> taskAction,
            TimeSpan timeout)
        {
            return WaitInTimeout(sources, taskAction, null, timeout);
        }

        public static TResult[] WaitInTimeout<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, TResult> taskAction,
            Func<Task<TResult>, bool> checkResultAction = null,
            TimeSpan? timeout = null,
            int? maxResultCount = null)
        {
            #region Validation
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (taskAction == null)
                throw new ArgumentNullException(nameof(taskAction));

            if (maxResultCount.HasValue &&
                (maxResultCount < 0 || maxResultCount > sources.Length))
                throw new ArgumentException(nameof(maxResultCount));
            #endregion

            if (maxResultCount == 0)
                return new TResult[0];

            if (timeout.HasValue && timeout == TimeSpan.Zero)
                return new TResult[0];

            if (maxResultCount == null)
                maxResultCount = sources.Length;

            if (checkResultAction == null)
            {
                checkResultAction = (res) => { return true; };
            }

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var tasks = new List<Task<TResult>>();
                var isTimeOut = false;
                var tasksCountToCheck = 0;

                if (timeout.HasValue)
                {
                    var task = new Task<TResult>(() =>
                    {
                        Thread.Sleep((int)timeout.Value.TotalMilliseconds);
                        isTimeOut = true;
                        return default;
                    });

                    tasks.Add(task);
                    task.Start();

                    tasksCountToCheck = 1;
                }

                foreach (var source in sources)
                {
                    var token = cancellationTokenSource.Token;
                    var task = new Task<TResult>(() => taskAction(source, token));
                    tasks.Add(task);
                    task.Start();
                }

                var completedTasks = new List<Task<TResult>>();

                while (completedTasks.Count < maxResultCount && tasks.Count > tasksCountToCheck)
                {
                    var indexOfCompletedTask = Task.WaitAny(tasks.ToArray());

                    if (isTimeOut)
                    {
                        break;
                    }

                    var completedTask = tasks[indexOfCompletedTask];

                    if (checkResultAction(completedTask))
                    {
                        completedTasks.Add(completedTask);
                    }

                    tasks.RemoveAt(indexOfCompletedTask);
                }

                cancellationTokenSource.Cancel();
                return completedTasks?.Select(t => t.Result).ToArray();
            }
        }

        public async static Task<TResult> WaitOneAsync<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, Task<TResult>> taskAction,
            Func<TResult, Task<bool>> checkResultAction = null)
        {
            var results = await WaitInTimeoutAsync(sources, taskAction, null, checkResultAction, 1);
            return results.FirstOrDefault();
        }

        public async static Task<TResult[]> WaitAllAsync<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, Task<TResult>> taskAction,
            Func<TResult, Task<bool>> checkResultAction = null)
        {
            return await WaitInTimeoutAsync(sources, taskAction, null, checkResultAction);
        }

        public async static Task<TResult> WaitOneInTimeoutAsync<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, Task<TResult>> taskAction,
            TimeSpan timeout,
            Func<TResult, Task<bool>> checkResultAction = null)
        {
            var results = await WaitInTimeoutAsync(sources, taskAction, timeout, checkResultAction, 1);
            return results.FirstOrDefault();
        }

        public async static Task<TResult[]> WaitAllInTimeoutAsync<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, Task<TResult>> taskAction,
            TimeSpan timeout,
            Func<TResult, Task<bool>> checkResultAction = null)
        {
            return await WaitInTimeoutAsync(sources, taskAction, timeout, checkResultAction);
        }

        private async static Task<TResult[]> WaitInTimeoutAsync<TResult, TSource>(
            TSource[] sources,
            Func<TSource, CancellationToken, Task<TResult>> taskAction,
            TimeSpan? timeout = null,
            Func<TResult, Task<bool>> checkResultAction = null,
            int? maxResultCount = null)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (taskAction == null)
                throw new ArgumentNullException(nameof(taskAction));

            if (maxResultCount.HasValue &&
                (maxResultCount < 0 || maxResultCount > sources.Length))
                throw new ArgumentException(nameof(maxResultCount));

            if (maxResultCount == 0)
                return new TResult[0];

            if (timeout.HasValue && timeout == TimeSpan.Zero)
                return new TResult[0];

            if (maxResultCount == null)
                maxResultCount = sources.Length;

            if (checkResultAction == null)
                checkResultAction = (result) => Task.FromResult(true);

            var results = new List<TResult>();
            var tasks = new List<Task<TResult>>();
            var cts = new CancellationTokenSource();

            try
            {
                if (timeout.HasValue)
                    cts.CancelAfter(timeout.Value);

                foreach (var source in sources)
                    tasks.Add(taskAction(source, cts.Token));

                var finishedTasks = new List<Task<TResult>>();

                while (finishedTasks.Count < maxResultCount && tasks.Any())
                {
                    var finishedTask = await Task.WhenAny(tasks);
                    finishedTasks.Add(finishedTask);
                    tasks.Remove(finishedTask);

                    var finishedTaskResult = await finishedTask;

                    if (await checkResultAction(finishedTaskResult))
                        results.Add(finishedTaskResult);
                }

                if (tasks.Any())
                    cts.Cancel();
            }
            catch (TaskCanceledException) { }
            finally
            {
                cts.Dispose();
            }

            return results.ToArray();
        }
    }
}
