using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autonet.Extensions {
    public static class PathExtensions {
        public static DirectoryInfo EnsureCreated(this DirectoryInfo dir) {
            var root = dir.Root.FullName.Replace("\\", "/");
            var parts = dir.FullName.Replace("\\","/").TrimEnd('/').Split('/').Skip(1).ToArray();
            parts[0] = parts[0].Replace(root, "");
            var p = root;
            if (Directory.Exists(p) == false)
                throw new InvalidOperationException("The root doesn't exist!");
            foreach (var part in parts) {
                p += part + "/";
                if (Directory.Exists(p) == false)
                    Directory.CreateDirectory(p);
            }

            return dir;
        }
        /// <summary>
        /// return dirinfo of subfolder
        /// </summary>
        /// <param name="base"></param>
        /// <param name="sub">somefoldername</param>
        /// <returns></returns>
        public static DirectoryInfo SubFolder(this DirectoryInfo @base, string sub) {
            if (sub == null) throw new ArgumentNullException(nameof(sub));
            if (string.IsNullOrEmpty(sub)) throw new ArgumentException("Value cannot be null or empty.", nameof(sub));
            return new DirectoryInfo(Path.Combine(@base.FullName, sub));
        }
        /// <summary>
        ///     Fileinfo of a file inside folder.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileInfo SubFile(this DirectoryInfo @base, string file) {
            if (string.IsNullOrEmpty(file)) throw new ArgumentException("Value cannot be null or empty.", nameof(file));
            return new FileInfo(Path.Combine(@base.FullName, file));
        }
    }
}
