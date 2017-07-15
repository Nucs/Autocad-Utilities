using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LispDebugAssistant {
    public class LspFileWatcher : IDisposable {
        
        public string[] Files => Directory.GetFiles(this.watcher.Path, ".lsp");

        public delegate void FileDeletedEvent(string filename, DateTime when);

        public delegate void FileAddedEvent(string filename, DateTime when);

        public delegate void FileChangedEvent(string filename, DateTime when);

        public event FileChangedEvent FileChanged;
        public event FileAddedEvent FileAdded;
        public event FileDeletedEvent FileDeleted;

        public LspFileWatcher(DirectoryInfo path) : this(path.FullName) { }

        public LspFileWatcher(string path) {
            if (Directory.Exists(path) == false)
                throw new DirectoryNotFoundException("Cant find directory: " + path);
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.lsp"
            };

            /* Watch for changes in LastAccess and LastWrite times, and 
                the renaming of files or directories. */
            // Only watch text files.

            // Add event handlers.
            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnCreated;
            watcher.Deleted += WatcherOnDeleted;
            watcher.Renamed += WatcherOnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private FileSystemWatcher watcher { get; set; }

        private void WatcherOnRenamed(object sender, RenamedEventArgs args) {
            FileDeleted?.Invoke(args.OldFullPath, DateTime.Now);
            FileAdded?.Invoke(args.FullPath, DateTime.Now);
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs args) {
            FileDeleted?.Invoke(args.FullPath, DateTime.Now);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs args) {
            FileAdded?.Invoke(args.FullPath, DateTime.Now);
        }

        private Hashtable fileWriteTime = new Hashtable();

        private void WatcherOnChanged(object sender, FileSystemEventArgs args) {
            string path = args.FullPath.ToString();
            string currentLastWriteTime = File.GetLastWriteTime(args.FullPath).ToString();

            // if there is no path info stored yet
            // or stored path has different time of write then the one now is inspected
            if (!fileWriteTime.ContainsKey(path) || fileWriteTime[path].ToString() != currentLastWriteTime) {
                //then we do the main thing
                FileChanged?.Invoke(args.FullPath, DateTime.Now);

                //lastly we update the last write time in the hashtable
                fileWriteTime[path] = currentLastWriteTime;
            }
        }

        public bool IsDisposed { get; set; }

        public void Dispose() {
            if (watcher != null) {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
                fileWriteTime.Clear();
            }
        }
    }
}