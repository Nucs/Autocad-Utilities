using System;
using System.IO;
using System.Threading;
using MailFinder.Helpers;

namespace MailFinder {
    public class DirectoryIndexTask : ParentTask {
        public DirectoryInfo Search { get; set; }
        public DirectoryIndexTask(Action action) : base(action) { }
        public DirectoryIndexTask(Action action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
        public DirectoryIndexTask(CancellableAction<ParentTask> action) : base(action) { }
        public DirectoryIndexTask(CancellableAction<ParentTask> action, ProgressDesciber progress, CancellationToken token) : base(action, progress, token) { }
    }
}