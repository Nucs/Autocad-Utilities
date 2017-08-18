using System;
using System.Threading;

namespace MailFinder {
    /// <summary>
    ///     A combo of <see cref="CancellationTokenSource"/> and <see cref="ManualResetEventSlim"/>
    /// </summary>
    public class ProgressDesciber : IDisposable {
        private ManualResetEventSlim Ended { get; }
        private CancellationTokenSource Cancellation { get; }

        public bool HasFinished => Ended.IsSet;

        public void Cancel() {
            Cancellation.Cancel();
            Ended.Set();
        }

        public void Cancel(bool throwOnFirstException) {
            Cancellation.Cancel(throwOnFirstException);
            Ended.Set();
        }

        public void CancelAfter(TimeSpan delay) {
            Cancellation.CancelAfter(delay);
            Ended.Set();
        }

        public void CancelAfter(int millisecondsDelay) {
            Cancellation.CancelAfter(millisecondsDelay);
            Ended.Set();
        }

        public bool IsCancellationRequested => Cancellation.IsCancellationRequested;

        public CancellationToken Token => Cancellation.Token;

        public ProgressDesciber(ManualResetEventSlim ended, CancellationTokenSource cancellation) {
            Ended = ended;
            Cancellation = cancellation;
        }

        public ProgressDesciber() : this(new ManualResetEventSlim(false), new CancellationTokenSource()) { }
        public ProgressDesciber(ManualResetEventSlim ended) : this(ended, new CancellationTokenSource()) { }

        

        public ProgressDesciber(CancellationTokenSource cancellation) : this(new ManualResetEventSlim(false), cancellation) { }

        public void Wait() {
            Ended.Wait(Token);
        }

        internal void SetAsFinished() {
            if (!Ended.IsSet)
                Ended.Set();
        }
        
        public void Wait(CancellationToken cancellationToken) {
            Ended.Wait(cancellationToken);
        }

        public bool Wait(TimeSpan timeout) {
            return Ended.Wait(timeout, Token);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken) {
            return Ended.Wait(timeout, cancellationToken);
        }

        public bool Wait(int millisecondsTimeout) {
            return Ended.Wait(millisecondsTimeout, Token);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) {
            return Ended.Wait(millisecondsTimeout, cancellationToken);
        }

        public void Dispose() {
            Ended?.Dispose();
            Cancellation?.Dispose();
        }
    }
}