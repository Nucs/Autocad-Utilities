using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using autonet.lsp;
using Common;

namespace LispDebugAssistant {
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