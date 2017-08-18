using System;
using System.Collections.Generic;
using System.Threading;
using MailFinder.Helpers;

namespace MailFinder {
    public class SearchTask : ParentTask {
        public string Term { get; set; }

        public IEnumerable<string> Paths { get; set; }
        public SearchTask(Action action) : base(action) { }
        public SearchTask(Action action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
        public SearchTask(CancellableAction<ParentTask> action) : base(action) { }
        public SearchTask(CancellableAction<ParentTask> action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
    }
}