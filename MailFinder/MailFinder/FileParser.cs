using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using autonet.Extensions;
using MailFinder.FileTypes;
using MsgReader.Outlook;
using nucs.Filesystem;

namespace MailFinder {
    public class FileParser {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="base">The base directory of current application</param>
        /// <param name="file">The file to parse</param>
        public static SearchableFile Parse(FileInfo file) {
            switch (file.Extension.TrimStart('.')) {
                case "msg":
                    var msg = new OutlookMessageFile(file);
                    return msg;
                case "pdf":
                default:
                    return null;
            }
        }


        public const string Version = "1.0.0.0";
        /*private static void ProcessMessage(string term)
        {
            var current = Program.CurrentFolder;
            if (current == null)
                return;
            var index = current.SubFolder("$index").EnsureCreated();

            index.Attributes = FileAttributes.Hidden;

            lblPath.Invoke(new MethodInvoker(() => lblPath.Text = current.FullName));
            var recusive = Bag.Data["deepfolder"] as bool? ?? false;
            var attachments = Bag.Data["deepattachments"] as bool? ?? false;
            var files = recusive ? FileSearch.EnumerateFilesDeep(current, "*.msg") : FileSearch.GetFiles(current, "*.msg");
            foreach (var file in files)
            {
                var f = file.FullName;
                if (hoot.IsIndexed(f)) continue;
                using (var msg = new Storage.Message(f))
                {
                    var main = MessageToString(msg);
                    var attchs = (attachments ? _deep_attachments(msg, new[] { "msg" }) : extractMessages(msg, new[] { "msg" })).Select(MessageToString).ToArray();
                    var n = new Message() { Version = Version, Content = main, Attachments = attchs, Path = file.FullName };
                    try
                    {
                        n.MD5 = file.CalculateMD5();
                    }
                    catch (SecurityException) { }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            hoot.Save();
            //todo add hidden msgtext file 
        }*/
    }
}