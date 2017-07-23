using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace autonet {
    public static class FileDialogs {
        public static DirectoryInfo GetDirectory(string description = null, bool showNewFolderButton = true) {
            using (var fd = new FolderBrowserDialog()) {
                fd.Description = description ?? "Select a Directory";
                fd.ShowNewFolderButton = showNewFolderButton;
                return fd.ShowDialog() == DialogResult.OK ? new DirectoryInfo(fd.SelectedPath) : null;
            }
        }

        public static FileInfo GetFile(OpenFileDialog fileDialog, FileFilter filter = null) {
            if (filter != null && filter.Count > 0) {
                fileDialog.Filter = filter.ToString();
                if (filter.SelectedIndex + 1 <= filter.Count)
                    fileDialog.FilterIndex = filter.SelectedIndex;
            }
            return fileDialog.ShowDialog() == DialogResult.OK ? new FileInfo(fileDialog.FileName) : null;
        }

        public static FileInfo GetFile(FileFilter filter = null, bool checkFileExists = true, bool restoreDirecory = true, bool MultiSelect = false, string initialdirectory = null) {
            using (var fd = new OpenFileDialog()) {
                fd.RestoreDirectory = restoreDirecory;
                fd.CheckFileExists = checkFileExists;
                fd.CheckPathExists = checkFileExists;
                if (initialdirectory!=null)
                    fd.InitialDirectory = initialdirectory;
                fd.Multiselect = MultiSelect;
                fd.AddExtension = true;
                if (filter != null && filter.Count > 0) {
                    fd.Filter = filter.ToString();
                    if (filter.SelectedIndex + 1 <= filter.Count)
                        fd.FilterIndex = filter.SelectedIndex;
                }
                return fd.ShowDialog() == DialogResult.OK ? new FileInfo(fd.FileName) : null;
            }
        }

        public static FileInfo SaveFile(SaveFileDialog fileDialog, FileFilter filter = null) {
            if (filter != null && filter.Count > 0) {
                fileDialog.Filter = filter.ToString();
                if (filter.SelectedIndex + 1 <= filter.Count)
                    fileDialog.FilterIndex = filter.SelectedIndex;
            }
            return fileDialog.ShowDialog() == DialogResult.OK ? new FileInfo(fileDialog.FileName) : null;
        }

        public static FileInfo SaveFile(FileFilter filter = null, bool checkFileExists = false, bool checkPathExists = false, bool restoreDirecory = true, bool PermissionToCreate = false, bool PermissionToOverwrite = true) {
            using (var fd = new SaveFileDialog()) {
                fd.RestoreDirectory = restoreDirecory;
                fd.CheckFileExists = checkFileExists;
                fd.CheckPathExists = checkPathExists;
                fd.CreatePrompt = PermissionToCreate;
                fd.OverwritePrompt = PermissionToOverwrite;
                fd.AddExtension = true;
                if (filter != null && filter.Count > 0) {
                    fd.Filter = filter.ToString();
                    if (filter.SelectedIndex + 1 <= filter.Count)
                        fd.FilterIndex = filter.SelectedIndex;
                }
                return fd.ShowDialog() == DialogResult.OK ? new FileInfo(fd.FileName) : null;
            }
        }
    }

    /// <summary>
    /// Represents a filter for FileDialog class
    /// that is used when you search for a file path for what ever reason using Microsoft's FileDialog class
    /// , for example: "Excel files(*.xlsx;*.xls)|*.xlsx;*.xls"
    /// </summary>
    public class FileFilter {
        private readonly Dictionary<string, List<string>> filters = new Dictionary<string, List<string>>();

        /// <summary>
        /// What index will be selected on the parent FileDialog; default = 0
        /// </summary>
        public int SelectedIndex { get; set; }

        public int Count {
            get { return filters.Count; }
        }

        /// <summary>
        /// Adds a filter for a 
        /// </summary>
        /// <param name="name">The name of the filter, for example: "Excel Files"</param>
        /// <param name="formats">The format of the filter, for example: "*.xlsx""</param>
        /// <returns>If the filter was successfully added, unless it is already in the object container with the same name.</returns>
        public bool AddFilter(string name, string formats) {
            return AddFilter(name, new[] {formats});
        }

        /// <summary>
        /// Adds a filter for a 
        /// </summary>
        /// <param name="name">The name of the filter, for example: "Excel Files"</param>
        /// <param name="formats">The format of the filter, for example: "*.xlsx""</param>
        /// <returns>If the filter was successfully added, unless it is already in the object container with the same name.</returns>
        public bool AddFilter(string name, params string[] formats) {
            if (filters.ContainsKey(name))
                return false;
            filters.Add(name, formats.ToList());
            return true;
        }

        public bool RemoveFilter(string name) {
            return filters.Remove(name);
        }

        /// <summary>
        /// Represents a filter for FileDialog class
        /// that is used when you search for a file path for what ever reason using Microsoft's FileDialog class
        /// , for example: "Excel files(*.xlsx;*.xls)|*.xlsx;*.xls"
        /// </summary>
        public FileFilter() { }

        /// <summary>
        /// Represents a filter for FileDialog class
        /// that is used when you search for a file path for what ever reason using Microsoft's FileDialog class
        /// , for example: "Excel files(*.xlsx;*.xls)|*.xlsx;*.xls"
        /// </summary>
        public FileFilter(IEnumerable<KeyValuePair<string, IEnumerable<string>>> kvs) {
            foreach (var kv in kvs)
                filters.Add(kv.Key, kv.Value.ToList());
        }

        /// <summary>
        /// Represents a filter for FileDialog class
        /// that is used when you search for a file path for what ever reason using Microsoft's FileDialog class
        /// , for example: "Excel files(*.xlsx;*.xls)|*.xlsx;*.xls"
        /// </summary>
        public FileFilter(string name, params string[] formats) {
            AddFilter(name, formats);
        }

        /// <summary>
        /// Output the combined filters, ready for use 
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (filters.Count == 0)
                return string.Empty;
            return string.Join("|", filters.Select(f => string.Format("{0}({1})|{1}", f.Key, string.Join(";", f.Value.ToArray()))).ToArray());
        }
    }
}