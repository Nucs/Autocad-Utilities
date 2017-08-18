using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailFinder.Helpers {
    public class QueuedTask<T> where T : QueuedTask<T> {
        private TaskState _state = TaskState.Idle;
        public event Action<T, TaskState> StateChanged;

        public TaskState State {
            get { return _state; }
            internal set {
                _state = value;
                Task.Run(() => {
                    try {
                        StateChanged?.Invoke((T) this, _state);
                    } catch (Exception) {
                        //todo log exception with state..
                    }
                });
            }
        }

        public CancellableAction<T> Action { get; set; }
        public ProgressDesciber Progress { get; set; }
        public CancellationToken Token { get; set; }

        public QueuedTask(Action action) {
            Action = Wrap(action);
            Progress = new ProgressDesciber();
            Token = Progress.Token;
        }

        public QueuedTask(Action action, ProgressDesciber progress, CancellationToken token) {
            Action = Wrap(action);
            Progress = progress;
            Token = token;
        }

        public QueuedTask(CancellableAction<T> action) {
            Action = Wrap(action);
            Progress = new ProgressDesciber();
            Token = Progress.Token;
        }

        public QueuedTask(CancellableAction<T> action, ProgressDesciber progress, CancellationToken token) {
            Action = Wrap(action);
            Progress = progress;
            Token = token;
        }

        private CancellableAction<T> Wrap(Action act) {
            return (task, _token, _progress) => {
                if (_progress.HasFinished) //was it cancelled prior to calling.
                    return;
                act();
                if (_progress.HasFinished == false)
                    _progress.SetAsFinished();
            };
        }

        private CancellableAction<T> Wrap(CancellableAction<T> act) {
            return (task, _token, _progress) => {
                if (_progress.HasFinished) //was it cancelled prior to calling.
                    return;
                act(task, _token, _progress);
                if (_progress.HasFinished == false)
                    _progress.SetAsFinished();
            };
        }
    }
}