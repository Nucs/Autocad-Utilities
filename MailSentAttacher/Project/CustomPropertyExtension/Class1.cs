using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpShell.SharpContextMenu;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using DSOFile;

using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace CustomPropertyExtension
{
    public class Program
    {
        public static void Main()
        {
            const string s = "C:/Users/eli/Documents/m.msg";

            string filePath = s;
            var file = ShellFile.FromFilePath(filePath);

            // Read and Write:

            string[] oldAuthors = file.Properties.System.Author.Value;
            string oldTitle = file.Properties.System.Title.Value;

            file.Properties.System.Author.Value = new string[] { "Author #1", "Author #2" };
            ShellPropertyWriter propertyWriter = file.Properties.GetPropertyWriter();
            propertyWriter.WriteProperty(SystemProperties.System.Message.DateSent, DateTime.Now);
            propertyWriter.Close();
        }
    }
}