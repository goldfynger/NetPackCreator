#define PARALLELAWAITABLE_SHOW_EXCEPTION_MSG

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NetPackCreator
{
    /// <summary></summary>
    internal abstract class ParallelAwaitableBase
    {
        /// <summary></summary>
        protected readonly object _locker = new object();

        /// <summary></summary>
        protected object _compareObject;

        /// <summary></summary>
        protected CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary></summary>
    internal sealed class ParallelAwaitable : ParallelAwaitableBase
    {
        /// <summary></summary>
        private readonly Action _awaitAction;
        /// <summary></summary>
        private readonly Action _resultAction;

        /// <summary></summary>
        /// <param name="awaitAction"></param>
        /// <param name="resultAction"></param>
        public ParallelAwaitable(Action awaitAction, Action resultAction)
        {
            if (awaitAction == null) throw new ArgumentNullException("awaitAction");
            if (resultAction == null) throw new ArgumentNullException("resultAction");

            this._awaitAction = awaitAction;
            this._resultAction = resultAction;
        }

        /// <summary></summary>
        public void Run()
        {
            this.RunAsync();
        }

        /// <summary>Должен запускаться только в главном потоке.</summary>
        private async void RunAsync()
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess()) throw new InvalidOperationException("Function called in non-UI thread");

            var compareObject = new object();
            this._compareObject = compareObject;

            this._cancellationTokenSource.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            Exception exception = null;

            try
            {
                await Task.Run(this._awaitAction, this._cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!ReferenceEquals(this._compareObject, compareObject)) return;

            if (exception != null)
            {

#if PARALLELAWAITABLE_SHOW_EXCEPTION_MSG

                Debug.Fail(exception.Message);

#endif

                return;
            }

            this._resultAction();
        }
    }

    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ParallelAwaitable<T> : ParallelAwaitableBase
    {
        /// <summary></summary>
        private readonly Action<T> _awaitAction;
        /// <summary></summary>
        private readonly Action _resultAction;

        /// <summary></summary>
        /// <param name="awaitAction"></param>
        /// <param name="resultAction"></param>
        public ParallelAwaitable(Action<T> awaitAction, Action resultAction)
        {
            if (awaitAction == null) throw new ArgumentNullException("awaitAction");
            if (resultAction == null) throw new ArgumentNullException("resultAction");

            this._awaitAction = awaitAction;
            this._resultAction = resultAction;
        }

        /// <summary></summary>
        /// <param name="parameter"></param>
        public void Run(T parameter)
        {
            this.RunAsync(parameter);
        }

        /// <summary>Должен запускаться только в главном потоке.</summary>
        /// <param name="parameter"></param>
        private async void RunAsync(T parameter)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess()) throw new InvalidOperationException("Function called in non-UI thread");

            var compareObject = new object();
            this._compareObject = compareObject;

            this._cancellationTokenSource.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            Exception exception = null;

            try
            {
                await Task.Run(() => this._awaitAction(parameter), this._cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!ReferenceEquals(this._compareObject, compareObject)) return;

            if (exception != null)
            {

#if PARALLELAWAITABLE_SHOW_EXCEPTION_MSG

                Debug.Fail(exception.Message);

#endif

                return;
            }

            this._resultAction();
        }
    }

    /// <summary></summary>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class ParallelAwaitableWithResult<TResult> : ParallelAwaitableBase
    {
        /// <summary></summary>
        private readonly Func<TResult> _awaitFunc;
        /// <summary></summary>
        private readonly Action<TResult> _resultAction;

        /// <summary></summary>
        /// <param name="awaitFunc"></param>
        /// <param name="resultAction"></param>
        public ParallelAwaitableWithResult(Func<TResult> awaitFunc, Action<TResult> resultAction)
        {
            if (awaitFunc == null) throw new ArgumentNullException("awaitFunc");
            if (resultAction == null) throw new ArgumentNullException("resultAction");

            this._awaitFunc = awaitFunc;
            this._resultAction = resultAction;
        }

        /// <summary></summary>
        public void Run()
        {
            this.RunAsync();
        }

        /// <summary>Должен запускаться только в главном потоке.</summary>
        private async void RunAsync()
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess()) throw new InvalidOperationException("Function called in non-UI thread");
            
            var compareObject = new object();
            this._compareObject = compareObject;

            this._cancellationTokenSource.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            TResult result = default(TResult);
            Exception exception = null;

            try
            {
                result = await Task<TResult>.Run(this._awaitFunc, this._cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!ReferenceEquals(this._compareObject, compareObject)) return;

            if (exception != null)
            {

#if PARALLELAWAITABLE_SHOW_EXCEPTION_MSG

                Debug.Fail(exception.Message);

#endif

                return;
            }

            this._resultAction(result);
        }
    }

    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class ParallelAwaitableWithResult<T, TResult> : ParallelAwaitableBase
    {
        /// <summary></summary>
        private readonly Func<T, TResult> _awaitFunc;
        /// <summary></summary>
        private readonly Action<TResult> _resultAction;

        /// <summary></summary>
        /// <param name="awaitFunc"></param>
        /// <param name="resultAction"></param>
        public ParallelAwaitableWithResult(Func<T, TResult> awaitFunc, Action<TResult> resultAction)
        {
            if (awaitFunc == null) throw new ArgumentNullException("awaitFunc");
            if (resultAction == null) throw new ArgumentNullException("resultAction");

            this._awaitFunc = awaitFunc;
            this._resultAction = resultAction;
        }

        /// <summary></summary>
        /// <param name="parameter"></param>
        public void Run(T parameter)
        {
            this.RunAsync(parameter);
        }

        /// <summary>Должен запускаться только в главном потоке.</summary>
        /// <param name="parameter"></param>
        private async void RunAsync(T parameter)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess()) throw new InvalidOperationException("Function called in non-UI thread");
            
            var compareObject = new object();
            this._compareObject = compareObject;

            this._cancellationTokenSource.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            TResult result = default(TResult);
            Exception exception = null;

            try
            {
                result = await Task<TResult>.Run(() => this._awaitFunc(parameter), this._cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!ReferenceEquals(this._compareObject, compareObject)) return;

            if (exception != null)
            {

#if PARALLELAWAITABLE_SHOW_EXCEPTION_MSG

                Debug.Fail(exception.Message);

#endif

                return;
            }

            this._resultAction(result);
        }
    }
}