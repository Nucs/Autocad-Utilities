using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace CustomPropertyExtension
{
    public static class Program
    {
        public static void Main()
        {
           var v = new FileSystemWatcher("E:/");
            v.IncludeSubdirectories = true;
            v.Created += VOnCreated;
            v.
            v.Filter = "*.srt";
            v.EnableRaisingEvents = true;
            Console.ReadLine();
        }

        private static void VOnCreated(object sender, FileSystemEventArgs args) {
            Console.WriteLine(args.FullPath);
        }

        public static void SetSendDate(this FileInfo f, DateTime dt) {
            using (var file = ShellFile.FromFilePath(f.FullName)) {
                using (ShellPropertyWriter propertyWriter = file.Properties.GetPropertyWriter()) {
                    propertyWriter.WriteProperty(SystemProperties.System.Message.DateSent, DateTime.Now);
                }
            }

        }
    }
}