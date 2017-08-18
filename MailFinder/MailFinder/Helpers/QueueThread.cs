using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailFinder.Helpers {
    public delegate void CancellableAction<in T>(T task, CancellationToken token, ProgressDesciber progress) where T : QueuedTask<T>;

    public class QueueThread<T> : IDisposable where T : QueuedTask<T> {
        public event Action<QueuedTask<T>> TaskQueued;

        public delegate void QueuedCancellableAction(T task, CancellationToken token, ProgressDesciber progress);

        protected Queue<QueuedTask<T>> Queue { get; } = new Queue<QueuedTask<T>>();

        private QueuedTask<T> Current { get; set; }

        private readonly ManualResetEventSlim res = new ManualResetEventSlim(false);

        public QueueThread() {
            Thread = new Thread(CoreThread);
            Thread.Start();
        }

        public QueuedTask<T> QueueTask(T queued) {
            if (_threadstop)
                return null;
            
            lock (Queue) {
                if (_threadstop)
                    return null;
                Queue.Enqueue(queued);
                queued.State = TaskState.Queued;
                FireTaskQueued(queued);
                res.Set();
                return queued;
            }
        }

        public QueuedTask<T> QueueTask(Action act) {
            if (_threadstop)
                return null;

            lock (Queue) {
                if (_threadstop)
                    return null;
                var queued = new QueuedTask<T>(act);
                Queue.Enqueue(queued);
                queued.State = TaskState.Queued;
                FireTaskQueued(queued);
                res.Set();

                return queued;
            }
        }

        public QueuedTask<T> QueueTask(CancellableAction<T> act) {
            if (_threadstop)
                return null;
            lock (Queue) {
                if (_threadstop)
                    return null;
                var pd = new ProgressDesciber();
                var queued = new QueuedTask<T>(act);
                Queue.Enqueue(queued);
                queued.State = TaskState.Queued;
                FireTaskQueued(queued);
                res.Set();
                return queued;
            }
        }


        protected virtual void CoreThread() {
            bool @wait = false;
            while (true) {
                if (_threadstop)
                    return;
                QueuedTask<T> a;
                _reload:
                lock (Queue) {
                    _reload_inner_again:
                    if (Queue.Count > 0) {
                        a = Queue.Dequeue();
                        if (a.Progress.IsCancellationRequested) {
                            a.State = TaskState.Cancelled;
                            a = null;
                            goto _reload_inner_again;
                        }
                        goto _work;
                    }
                }
                res.Wait();
                res.Reset();
                goto _reload;
                _work:
                if (_threadstop)
                    return;
                try {
                    Current = a;
                    if (a != null) {
                        a.State = TaskState.Executing;
                        a.Action((T) Current, Current.Token, Current.Progress);
                        Current = null;
                        a.State = a.Progress.IsCancellationRequested ? TaskState.Cancelled : TaskState.Executed;
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    //todo log an error here
                } finally {
                    a = null;
                    Current = null;
                }
            }
        }

        public Thread Thread { get; set; }
        private bool _threadstop = false;

        public void CancelEntireQueue(bool includingcurrent = true) {
            lock (Queue) {
                if (includingcurrent)
                    CancelCurrent();
                while (Queue.Count > 0) {
                    QueuedTask<T> q = Queue.Dequeue();
                    q.Progress.Cancel();
                }
            }
        }

        public void CancelCurrent() {
            var c = Current;
            c?.Progress.Cancel();
        }

        public void Dispose() {
            _threadstop = true;
            res.Set();
        }

        private void FireTaskQueued(QueuedTask<T> t) {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (TaskQueued != null)
                Task.Run(() => TaskQueued?.Invoke(t));
        }

        /// <summary>
        ///     Cancel queued items.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int CancelQueuedWhere(Func<T, bool> @where) {
            QueuedTask<T>[] l;
            lock (Queue)
                l = Queue.ToArray();
            var p = l.Where(task =>@where((T)task)) .ToArray();
            foreach (var q in p) 
                q.Progress.Cancel();
            
            return p.Length;
        }

        /// <summary>
        ///     Cancel queued items.
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns></returns>
        public bool CancelCurrentIf(Func<QueuedTask<T>, bool> boolean) {
            QueuedTask<T> c = Current;
            if (c == null)
                return false;
            if (boolean(c) == true) {
                CancelCurrent();
                return true;
            }
            return false;
        }
    }
}