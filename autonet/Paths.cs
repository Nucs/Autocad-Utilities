using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Common {
    /// <summary>
    ///     Class that determines paths.
    /// </summary>
    public static class Paths {
#pragma warning disable CS0169 // The field 'Paths._cacheprogress' is never used
        private static Task _cacheprogress;
#pragma warning restore CS0169 // The field 'Paths._cacheprogress' is never used

        private static readonly string _location = Assembly.GetEntryAssembly().Location;

        /// <summary>
        ///     Gives the path to windows dir, most likely to be 'C:/Windows/'
        /// </summary>
        public static DirectoryInfo WindowsDirectory => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));

        /// <summary>
        ///     The path to the entry exe.
        /// </summary>
        public static FileInfo ExecutingExe => new FileInfo(_location);

        /// <summary>
        ///     The path to the entry exe's directory.
        /// </summary>
        public static DirectoryInfo ExecutingDirectory => ExecutingExe.Directory;

        /// <summary>
        ///     Checks the ability to create and write to a file in the supplied directory.
        /// </summary>
        /// <param name="directory">String representing the directory path to check.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public static bool IsDirectoryWritable(this DirectoryInfo directory) {
            var success = false;
            var fullPath = directory + "testicales.exe";

            if (directory.Exists)
                try {
                    using (var fs = new FileStream(fullPath, FileMode.CreateNew,
                        FileAccess.Write)) {
                        fs.WriteByte(0xff);
                    }

                    if (File.Exists(fullPath)) {
                        File.Delete(fullPath);
                        success = true;
                    }
                }
                catch (Exception) {
                    success = false;
                }
            return success;
        }

        /// <summary>
        ///     Combines the file name with the dir of <see cref="Paths.ExecutingExe" />, resulting in path of a file inside the
        ///     directory of the executing exe file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static FileInfo CombineToExecutingBase(string filename) {
            if (ExecutingExe.DirectoryName != null)
                return new FileInfo(Path.Combine(ExecutingExe.DirectoryName, filename));
            return null;
        }

        /// <summary>
        ///     Combines the file name with the dir of <see cref="Paths.ExecutingExe" />, resulting in path of a file inside the
        ///     directory of the executing exe file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DirectoryInfo CombineToExecutingBaseDir(string filename) {
            if (ExecutingExe.DirectoryName != null)
                return new DirectoryInfo(Path.Combine(ExecutingExe.DirectoryName, filename));
            return null;
        }

        /// <summary>
        ///     Compares two FileSystemInfos the right way.
        /// </summary>
        /// <returns></returns>
        public static bool CompareTo(this FileSystemInfo fi, FileSystemInfo fi2) {
            return NormalizePath(fi.FullName).Equals(NormalizePath(fi2.FullName), StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Normalizes path to prepare for comparison or storage
        /// </summary>
        public static string NormalizePath(string path) {
            path = path.Replace("/", "\\")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
            if (path.Contains("\\"))
                if (Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
                    try {
                        path = Path.GetFullPath(new Uri(path).LocalPath);
                    }
                    catch { }
            //is root, fix.
            if ((path.Length == 2) && (path[1] == ':') && char.IsLetter(path[0]) && char.IsUpper(path[0]))
                path = path + "\\";

            return path;
        }

        /// <summary>
        ///     Removes or replaces all illegal characters for path in a string.
        /// </summary>
        public static string RemoveIllegalPathCharacters(string filename, string replacewith = "") => string.Join(replacewith, filename.Split(Path.GetInvalidFileNameChars()));
    }
}