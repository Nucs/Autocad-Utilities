using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autonet.Common.Settings;

namespace autonet
{
    public static class QQManager {
        public static QQAppConfig AppConfig { get; } = JsonConfiguration.Load<QQAppConfig>();

        /// <summary>
        ///     Will get the file, either from cache or from selecting.
        /// </summary>
        public static QQCFile SelectedFile {
            get {
                var path = AppConfig.SelectedPath;
                if (path == null)
                    return Select();
                var qqc = new QQCFile(new FileInfo(path));
                return qqc;
            }
        }

        /// <summary>
        ///     Gets the config file out of the selected file - note: it'll be refreshed.
        /// </summary>
        public static QQConfigurationFile Selected {
            get {
                var s = SelectedFile;
                return s?.Reload();
            }
        }

        public static QQCFile Select() {
            var frm = new QQSelectForm();
            frm.ShowDialog();
            if (frm.Selected == null)
                return null;
            UpdateSelected(frm.Selected);
            return frm.Selected;
        }

        public static void OpenConfiguration() {
            new QQForm().ShowDialog();
        }

        public static void CreateNewConfig() {
            var f = new QQForm();
            f.CreateNew = true;
            f.ShowDialog();
        }

        public static void UpdateSelected(QQCFile file) {
            AppConfig.Selected = file.File.FullName;
            AppConfig.Save();
        }
    }
}
