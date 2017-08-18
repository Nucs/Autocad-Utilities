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
        
    }
}