using System;
using System.Threading;
using MailFinder.Helpers;

namespace MailFinder {
    public class ParentTask : QueuedTask<ParentTask> {
        public string Description { get; set; }

        public ParentTask(Action action) : base(action) { }
        public ParentTask(Action action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
        public ParentTask(CancellableAction<ParentTask> action) : base(action) { }
        public ParentTask(CancellableAction<ParentTask> action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
    }
}