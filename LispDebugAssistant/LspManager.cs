using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using autonet;
using autonet.lsp;
using Common;

namespace LispDebugAssistant {
    public delegate void ExceptionArgs(Exception ex, DateTime when);

    public delegate void LspFileRenamedEvent(string old_filename, LspFile file, DateTime when);

    public delegate void LspDatedEvent(LspFile file, DateTime when);

    public delegate void LspRenamedEvent(string old_fullpath, LspFile file, DateTime when);

    public class LspManager : IDisposable {
        public event LspDatedEvent FileChanged;
        public event LspDatedEvent FileDeleted;
        public event LspFileRenamedEvent FileRenamed;

        /// <summary>
        ///     Fires when a file is called <see cref="Watch(LspFile)"/> upon
        /// </summary>
        public event LspDatedEvent BeganWatchingFile;

        public event LspDatedEvent StoppedWatchingFile;

        public event ExceptionArgs Error;

        /// <summary>
        ///     Shared sync root for adding/removing <see cref="LspFile"/>
        /// </summary>
        public readonly object LockRoot = new object();

        /// <summary>
        ///     List of active watched files.
        /// </summary>
        public List<LspFile> Files { get; } = new List<LspFile>();

        private SafeDictionary<string, LspFileWatcher> Watchers { get; } = new SafeDictionary<string, LspFileWatcher>();


        public LspFile Watch(FileInfo file) {
            return Watch(file.FullName);
        }

        public LspFile Watch(string file) {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (!File.Exists(file))
                throw new FileNotFoundException("Cant start watching cuz file not found: " + file);

            file = Paths.NormalizePath(file);
            LspFile f = new LspFile(file, this);
            lock (LockRoot) {
                Watch(f);
            }
            return f;
        }

        public void Watch(LspFile lsfile) {
            if (lsfile == null) throw new ArgumentNullException(nameof(lsfile));
            lock (LockRoot) {
                Files.Add(lsfile);
                var folder = lsfile.FolderPath;
                if (Watchers.ContainsKey(folder) == false) {
                    var watcher = Watchers[folder] ?? (Watchers[folder] = new LspFileWatcher(folder));
                    watcher.FileAdded += FileAddedHandler;
                    watcher.FileChanged += FileChangedHandler;
                    watcher.FileDeleted += FileDeletedHandler;
                    watcher.FileRenamed += FileRenamedHandler;
                }
            }
            BeganWatchingFile?.Invoke(lsfile, DateTime.Now);
        }

        public bool Remove(FileInfo file) {
            return Remove(file.FullName);
        }

        public bool Remove(string file) {
            lock (LockRoot) {
                var f = FindLspFile(file);
                return Remove(f);
            }
        }

        public bool Remove(LspFile f) {
            if (f == null)
                return false;
            Files.Remove(f);
            var any = FindLspFileByDir(f.FolderPath).Any();
            if (any == false) {
                var watcher = Watchers[f.FolderPath];
                Watchers.Remove(f.FolderPath);
                watcher.Dispose();
            }
            StoppedWatchingFile?.Invoke(f, DateTime.Now);
            return true;
        }

        /// <summary>
        ///     Is this manager paused?
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        /// <summary>
        ///     Pause this <see cref="LspManager"/> from rising events.
        /// </summary>
        /// <param name="ms"></param>
        public void Pause(int ms = -1) {
            lock (LockRoot) {
                foreach (var w in Watchers) {
                    w.Value.RaiseEvents = false;
                }
                IsPaused = true;
                if (ms > 0)
                    Task.Delay(ms).ContinueWith(_ => Resume());
            }
        }

        /// <summary>
        ///     Resume this watcher with rising events.
        /// </summary>
        public void Resume() {
            lock (LockRoot) {
                IsPaused = false;
                foreach (var w in Watchers) {
                    w.Value.RaiseEvents = true;
                }
            }
        }

        /// <summary>
        ///     Clears all listeners.. also, dispose but reusable.
        /// </summary>
        public void Wipe() {
            lock (LockRoot) {
                foreach (var w in Watchers) {
                    w.Value.Dispose();
                }
                Watchers.Clear();
                Files.Clear();
            }
        }

        public LspFile FindLspFile(string filename, DateTime? when = null) {
            LspFile t = null;
            lock (LockRoot) {
                if (filename.Replace('\\', '/').Contains("/"))
                    t = Files.FirstOrDefault(lsfile => Paths.CompareTo(lsfile.FullPath, filename));
                else
                    t = Files.FirstOrDefault(lsfile => Paths.CompareTo(lsfile.FileName, filename));
            }

            if (t == null)
                Error?.Invoke(new ThrowlessException("Couldn't find file inside LspFiles\n" + filename, Environment.StackTrace), when ?? DateTime.Now);
            return t;
        }

        public List<LspFile> FindReferencingFiles(LspFile thisfile) {
            var recuse = new List<LspFile>();
            lock (LockRoot)
                recuse.AddRange(Files.Where(f => f.References.Contains(thisfile.FullPath, new Paths.FilePathEqualityComparer())).ToArray());

            return recuse;
        }

        public LspFile[] FindLspFileByDir(string directory) {
            lock (LockRoot)
                return Files.Where(lsfile => Paths.CompareTo(lsfile.FolderPath, directory)).ToArray();
        }


        private void FileChangedHandler(string filename, DateTime when) {
            var lsfile = FindLspFile(filename, when);
            lsfile?.OnFileChanged(lsfile, when);
            FileChanged?.Invoke(lsfile, when);
        }

        private void FileAddedHandler(string filename, DateTime when) {
            //do nothing.. we ignore it :D
        }

        private void FileDeletedHandler(string filename, DateTime when) {
            var lsfile = FindLspFile(filename, when);
            if (lsfile == null) return;
            Remove(lsfile);
            lsfile.OnFileDeleted(lsfile, when);
            FileDeleted?.Invoke(lsfile, when);
        }

        private void FileRenamedHandler(string old_filename, string new_filename, DateTime when) {
            var lsfile = FindLspFile(old_filename, when);
            if (lsfile == null) return;
            lsfile.FullPath = new_filename;
            lsfile.OnFileRenamed(old_filename, lsfile, when);
            FileRenamed?.Invoke(old_filename, lsfile, when);
        }

        public virtual void OnError(Exception ex, DateTime @when) {
            Error?.Invoke(ex, when);
        }

        public void Dispose() {
            Wipe();
        }

        public void ReloadAll(bool reload_referencers) {
            lock (LockRoot) {
                foreach (var file in Files.ToArray()) {
                    file.Reload(reload_referencers);
                }
            }
        }
    }


    class ThrowlessException : Exception {
        public ThrowlessException(string message, string stackTrace) : base(message) {
            this.StackTrace = stackTrace;
        }

        public ThrowlessException(string message, string stackTrace, Exception inner) : base(message, inner) {
            this.StackTrace = stackTrace;
        }


        public override string StackTrace { get; }
    }

    public static class ExceptionUtilities {
        private static readonly FieldInfo STACK_TRACE_STRING_FI = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Type TRACE_FORMAT_TI = Type.GetType("System.Diagnostics.StackTrace").GetNestedType("TraceFormat", BindingFlags.NonPublic);
        private static readonly MethodInfo TRACE_TO_STRING_MI = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {TRACE_FORMAT_TI}, null);

        public static Exception SetStackTrace(this Exception target, StackTrace stack) {
            var getStackTraceString = TRACE_TO_STRING_MI.Invoke(stack, new object[] {Enum.GetValues(TRACE_FORMAT_TI).GetValue(0)});
            STACK_TRACE_STRING_FI.SetValue(target, getStackTraceString);
            return target;
        }
    }


    /// <summary>
    ///     Represents a file and events on it.
    /// </summary>
    public class LspFile : IEquatable<LspFile>, IComparable<LspFile>, IComparable {
        public event LspDatedEvent FileChanged;
        public event LspDatedEvent FileDeleted;
        public event LspRenamedEvent FileRenamed;

        private LspManager Manager { get; set; }

        public string FullPath { get; set; }
        public string FileName => Path.GetFileName(FullPath);

        public FileInfo FullPathInfo => new FileInfo(FullPath);

        public string FolderPath => Paths.NormalizePath(Path.GetDirectoryName(FullPath));

        public DirectoryInfo FolderPathInfo => new DirectoryInfo(FolderPath);

        public IReadOnlyList<string> References { get; } = new List<string>();

        public LspFile(string fullPath, LspManager manager) {
            FullPath = fullPath;
            Manager = manager;
            FileChanged += (file, when) => UpdateReferenceList();
            UpdateReferenceList();
        }

        private void UpdateReferenceList(DateTime? @when = null) {
            try {
                const int NumberOfRetries = 20;
                const int DelayOnRetry = 100;
                string[] files=null;
                for (int i = 1; i <= NumberOfRetries; ++i) {
                    try {
                        // Do stuff with file
                        files = File.ReadAllText(FullPath).Trim('\n', '\r', ' ').Replace("\r", "").Split('\n').Where(line => line.StartsWith(";#")).Select(line => Paths.NormalizePath(line.Replace(";#", "").Trim())).ToArray();
                        
                        break; // When done we can break loop
                    } catch (IOException e) {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.
                        if (i == NumberOfRetries) // Last one, (re)throw exception and exit
                            Manager?.OnError(new ThrowlessException($"Can't read from file because it is locked (tries {NumberOfRetries} times).", Environment.StackTrace,e), DateTime.Now);

                        Thread.Sleep(DelayOnRetry);
                    }
                }

                //resolve relatives:
                for (int i = 0; i < files.Length; i++) {
                    var file = files[i].Replace("\\", "/");
                    if (file.Contains("/") == false)
                        file = "./" + file;
                    if (file.StartsWith(".") && file.Contains("./")) {
                        files[i] = Path.GetFullPath(Path.Combine(this.FolderPath, file.Replace('/','\\')));
                    }
                }

                lock (References) {
                    ((List<string>) References).Clear();
                    ((List<string>) References).AddRange(files);
                }
            } catch (Exception e) {
                Manager?.OnError(new ThrowlessException("Failed Reading and processing LspFile: " + this.FullPath, Environment.StackTrace, e), @when ?? DateTime.Now);
            }
        }

        public LspFile(FileInfo fullPath, LspManager manager) : this(fullPath.FullName, manager) { }

        public virtual void OnFileChanged(LspFile file, DateTime @when) {
            //reference is already reloaded.
            FileChanged?.Invoke(file, when);
        }

        public virtual void OnFileDeleted(LspFile file, DateTime @when) {
            FileDeleted?.Invoke(file, when);
        }

        public virtual void OnFileRenamed(string old_fullpath, LspFile file, DateTime @when) {
            FileRenamed?.Invoke(old_fullpath, file, when);
        }

        public override int GetHashCode() {
            return (FullPath != null ? FullPath.GetHashCode() : 0);
        }

        /// <summary>
        ///     Reloads using <see cref="LspLoader"/>
        /// </summary>
        public void Reload(bool reload_referencers = true, List<LspFile> alreadyReloaded=null) {
            if (alreadyReloaded != null && alreadyReloaded.Contains(this))
                return;
            LspLoader.LoadFile(FullPath);
            if (reload_referencers) {
                List<LspFile> AlreadyReloaded = alreadyReloaded ?? new List<LspFile>();
                AlreadyReloaded.Add(this);
                Manager?.FindReferencingFiles(this).ForEach(f => f.Reload(true, AlreadyReloaded));
            }
        }

        /// <summary>
        /// Removes this file from listening 
        /// </summary>
        public void Remove() {
            Manager?.Remove(this);
            Manager = null;
        }

        #region Overrides and Equality

        public bool Equals(LspFile other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LspFile) obj);
        }

        public static bool operator ==(LspFile left, LspFile right) {
            return Equals(left, right);
        }

        public static bool operator !=(LspFile left, LspFile right) {
            return !Equals(left, right);
        }

        public int CompareTo(LspFile other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(FullPath, other.FullPath, StringComparison.Ordinal);
        }

        public int CompareTo(object obj) {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is LspFile)) throw new ArgumentException($"Object must be of type {nameof(LspFile)}");
            return CompareTo((LspFile) obj);
        }

        public static bool operator <(LspFile left, LspFile right) {
            return Comparer<LspFile>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(LspFile left, LspFile right) {
            return Comparer<LspFile>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(LspFile left, LspFile right) {
            return Comparer<LspFile>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(LspFile left, LspFile right) {
            return Comparer<LspFile>.Default.Compare(left, right) >= 0;
        }

        private sealed class FullPathEqualityComparer : IEqualityComparer<LspFile> {
            public bool Equals(LspFile x, LspFile y) {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.FullPath, y.FullPath);
            }

            public int GetHashCode(LspFile obj) {
                return (obj.FullPath != null ? obj.FullPath.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<LspFile> FullPathComparer { get; } = new FullPathEqualityComparer();

        #endregion
    }
}