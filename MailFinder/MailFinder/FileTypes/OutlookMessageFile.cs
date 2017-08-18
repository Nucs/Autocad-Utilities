using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using autonet.Extensions;
using MailFinder.Helpers;
using MsgReader.Outlook;

namespace MailFinder.FileTypes {
    public class OutlookMessageFile : SearchableFile {
        public string[] Attachments { get; set; }

        private OutlookMessageFile() { } //for json

        public OutlookMessageFile(FileInfo file, bool includeattachments = true) : base(file) {
            using (var stream = FileShared.OpenRead(file))
                _load(file, stream, includeattachments);
        }

        public OutlookMessageFile(FileInfo file, Stream stream, bool includeattachments = true) : base(file, stream) {
            _load(file, stream, includeattachments);
        }

        private void _load(FileInfo file, Stream stream, bool includeattachments) {
            using (var msg = new Storage.Message(stream)) {
                var main = ConvertToString(msg);
                var attchs = (includeattachments ? _deep_attachments(msg, new[] {"msg"}) : extractMessages(msg, new[] {"msg"})).Select(ConvertToString).ToArray();
                Title = msg.Subject;
                Version = Version;
                Content = main;
                Attachments = attchs;
                Path = file.FullName;
                Date = msg.SentOn?.ToString("G", Program.ILCulture) ?? DateTime.MinValue.ToString("G", Program.ILCulture);
                try {
                    MD5 = file.CalculateMD5(stream);
                } catch (SecurityException) { } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        public override int SearchInside(string str, bool ignorecase) {
            return StringHelper.CountInside(Content.ConcatSafe(this.Attachments).ToArray(), str, ignorecase);
        }

        public override IndexedFile ToIndexedFile() {
            return new IndexedFile() {
                Title = this.Title,
                Content = this.Content,
                MD5 = this.MD5,
                InnerContent = string.Join("\n\n", this.Attachments ?? new string[0]),
                Path = this.Path,
                Version = this.Version,
                Date = this.Date,
                Directory = System.IO.Path.GetDirectoryName(this.Path)
            };
        }


        public static string ConvertToString(Storage.Message msg) {
            var sb = new StringBuilder();
            var from = msg.Sender;
            var sentOn = msg.SentOn;


            var subject = msg.Subject;
            var body = msg.BodyText;
            // etc...
            sb.AppendLine($"{(sentOn ?? DateTime.MinValue).ToString("g", Program.ILCulture)}");
            sb.AppendLine($"{from.DisplayName} {from.Email}");
            try {
                var recipientsTo = msg.GetEmailRecipients(Storage.Recipient.RecipientType.To, false, false);
                sb.AppendLine($"{recipientsTo}");
            } catch { }
            try {
                var recipientsCc = msg.GetEmailRecipients(Storage.Recipient.RecipientType.Cc, false, false);
                sb.AppendLine($"{recipientsCc}");
            } catch { }
            sb.AppendLine($"{subject}");
            sb.AppendLine($"{string.Join("", msg.GetAttachmentNames())}");
            sb.AppendLine($"{body}");

            return sb.ToString().Trim('\n', '\r', '\t').Replace("\n\n", "\n").Replace("\n\n", "\n").ToString();
        }

        private static List<Storage.Message> _deep_attachments(Storage.Message msg, string[] supported, List<Storage.Message> l = null) {
            if (msg == null) throw new ArgumentNullException(nameof(msg));
            if (supported == null) throw new ArgumentNullException(nameof(supported));
            if (supported.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(supported));

            if (l == null)
                l = new List<Storage.Message>();

            var attch = new List<object>(msg.Attachments);
            var innermessages = attch.TakeoutWhereType<object, Storage.Message>()
                .Concat(
                    attch.Cast<Storage.Attachment>()
                        .Where(att => supported.Any(s => System.IO.Path.GetExtension(att.FileName).EndsWith(s, true, CultureInfo.InvariantCulture)))
                        .Select(att => {
                            using (var ms = new MemoryStream(att.Data, false))
                                return new Storage.Message(ms);
                        }))
                .ToList();
            l.AddRange(innermessages);

            foreach (var im in innermessages) {
                _deep_attachments(im, supported, l);
            }

            return l;
        }

        private List<Storage.Message> extractMessages(Storage.Message msg, string[] supported) {
            var l = new List<Storage.Message>();

            var attch = new List<object>(msg.Attachments);
            var innermessages = attch.TakeoutWhereType<object, Storage.Message>()
                .Concat(
                    attch.Cast<Storage.Attachment>()
                        .Where(att => supported.Any(s => System.IO.Path.GetExtension(att.FileName).EndsWith(s, true, CultureInfo.InvariantCulture)))
                        .Select(att => {
                            using (var ms = new MemoryStream(att.Data, false))
                                return new Storage.Message(ms);
                        }))
                .ToList();
            l.AddRange(innermessages);
            return l;
        }
    }
}