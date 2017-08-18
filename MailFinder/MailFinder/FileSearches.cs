// Decompiled with JetBrains decompiler
// Type: nucs.Filesystem.FileSearch
// Assembly: nucs.Filesystem, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A0366B70-708B-414F-BC71-38D0DE3B9031
// Assembly location: C:\Users\Eli\Desktop\autocad\autoload\MailFinder\packages\nucs.Filesystem.1.1.0\lib\net452\nucs.Filesystem.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MailFinder {
    public static class FileSearches {
        public static IEnumerable<FileInfo> EnumerateDrive(DriveInfo drive, CancellationToken token, string searchPattern = "*.*") {
            return EnumerateFilesDeep(drive.RootDirectory, token, searchPattern);
        }

        public static IEnumerable<FileInfo> EnumerateDrive(char drivechar, CancellationToken token, string searchPattern = "*.*") {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(di => (int) di.RootDirectory.FullName.First() == (int) drivechar);
            if (driveInfo == null)
                return new FileInfo[0];
            return EnumerateFilesDeep(driveInfo.RootDirectory, token, searchPattern);
        }

        public static IEnumerable<FileInfo> EnumerateFilesDeep(DirectoryInfo @base, CancellationToken token, string searchPattern = "*.*") {
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(@base);
            while (queue.Count > 0) {
                if (token.IsCancellationRequested)
                    yield break;
                var num = 0;
                var @this = queue.Dequeue();
                label_3:
                try {
                    foreach (var directory in @this.GetDirectories())
                        queue.Enqueue(directory);
                } catch (UnauthorizedAccessException ex) {
                    continue;
                } catch (IOException ex) {
                    if (ex.Message.Contains("device is not ready")) {
                        Thread.Sleep(1);
                        if (++num != 1000) {
                            if (token.IsCancellationRequested)
                                yield break;
                            goto label_3;
                        }
                        continue;
                    }
                }
                var fileInfoArray = @this.GetFiles(searchPattern.Split('|'));
                for (var index = 0; index < fileInfoArray.Length; ++index) {
                    if (token.IsCancellationRequested)
                        yield break;
                    yield return fileInfoArray[index];
                }
                fileInfoArray = null;
            }
        }

        public static FileInfo[] GetFiles(this DirectoryInfo @this, params string[] searchPatterns) {
            if (searchPatterns == null || searchPatterns.Length == 0)
                return @this.GetFiles();
            return searchPatterns.SelectMany(@this.GetFiles).Distinct().ToArray();
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectoriesDeep(DirectoryInfo @base, CancellationToken token, string searchPattern = "*") {
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(@base);
            while (queue.Count > 0) {
                if (token.IsCancellationRequested)
                    yield break;
                var num = 0;
                var directoryInfo1 = queue.Dequeue();
                label_3:
                DirectoryInfo[] directoryInfoArray1 = null;
                try {
                    directoryInfoArray1 = directoryInfo1.GetDirectories("*", SearchOption.TopDirectoryOnly);
                } catch (UnauthorizedAccessException ex) {
                    continue;
                } catch (IOException ex) {
                    if (ex.Message.Contains("device is not ready")) {
                        Thread.Sleep(1);
                        if (++num != 1000) {
                            if (token.IsCancellationRequested)
                                yield break;
                            goto label_3;
                        }
                        continue;
                    }
                }

                var directoryInfoArray = directoryInfoArray1;
                if (directoryInfoArray != null)
                    foreach (var directoryInfo2 in directoryInfoArray) {
                        if (token.IsCancellationRequested)
                            yield break;
                        queue.Enqueue(directoryInfo2);
                        yield return directoryInfo2;
                    }
            }
        }
    }
}