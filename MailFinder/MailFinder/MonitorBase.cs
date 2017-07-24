using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using nucs.Collections;
using nucs.SystemCore.Boolean;

namespace nucs.Monitoring.Inline {
    public abstract class MonitorBase<T> : IMonitor<T>, IDisposable {
        private readonly DynamicEqualityComparer<T> comparer;
        private readonly BoolAction<T> isblacklisted;

        /// <summary>
        ///     When an exception occurs on fetching, howmuch to sleep.
        /// </summary>
        protected int exceptionOnFetchDelayMS = 50;

        private T[] @last = new T[0];


        private int last_hashsum = -1;

        /// <summary>
        ///     When the @new list equals to @last, how much to sleep.
        /// </summary>
        protected int noChangeDelayMs = 200;

        private bool stopflag;


        private Thread t;

        protected MonitorBase() {
            comparer = new DynamicEqualityComparer<T>(((x, y) => x?.Equals(y) == true), x => x?.GetHashCode() ?? 0);
        }

        protected MonitorBase(DynamicEqualityComparer<T> comparer, BoolAction<T> isblacklisted) {
            this.comparer = comparer;
            this.isblacklisted = isblacklisted;
        }

        public abstract IEnumerable<T> FetchCurrent();

        public event EnteredHandler<T> Entered;

        public event LeftHandler<T> Left;

        /// <summary>
        ///     Begins the monitoring thread.
        /// </summary>
        public virtual void Start() {
            if (t != null)
                return;
            t = new Thread(detector);
            t.Start();
        }

        /// <summary>
        ///     Signals that the monitoring thread should be stopped.
        /// </summary>
        public virtual void Stop() {
            stopflag = true;
        }

        private void detector() {
            while (true) {
                if (stopflag)
                    break;
                T[] @new;
                try {
                    @new = FetchCurrent().Distinct(comparer).Where(o => {
                        if (isblacklisted == null)
                            return true;
                        return !isblacklisted(o);
                    }).ToArray();
                }
                catch (Exception) {
                    Thread.Sleep(exceptionOnFetchDelayMS);
                    continue;
                }

                var hashsum = calculatesum(@new);
                if (last_hashsum == hashsum) {
                    Thread.Sleep(noChangeDelayMs);
                    continue;
                }
                //not equals
                last_hashsum = hashsum;

                var unnotified = @new.Except(@last, comparer).ToArray();
                var deleted = @last.Except(@new, comparer).ToArray();
                @last = @new;

                if (Entered != null)
                    foreach (var item in unnotified)
                        Entered(item);

                if (Left != null)
                    foreach (var item in deleted)
                        Left(item);
            }
            stopflag = false;
            t = null;
        }

        private int calculatesum(IEnumerable<T> itemsEnumerable) {
            var items = itemsEnumerable.ToArray();
            if (items.Length == 0)
                return 0;
            var hash = comparer.GetHashCode(items[0]);
            hash = items.Skip(1).Aggregate(hash, (current, item) => current ^ comparer.GetHashCode(item));
            return hash;
        }

        public static MonitorBase<T> operator +(MonitorBase<T> c1, EnteredHandler<T> c2) {
            c1.Entered += c2;
            return c1;
        }

        public static MonitorBase<T> operator +(MonitorBase<T> c1, LeftHandler<T> c2) {
            c1.Left += c2;
            return c1;
        }

        public static MonitorBase<T> operator +(MonitorBase<T> c1, Action<T> c2) {
            c1.Entered += item => c2(item);
            return c1;
        }

        public void Dispose() {
            this.Stop();
        }
    }
}