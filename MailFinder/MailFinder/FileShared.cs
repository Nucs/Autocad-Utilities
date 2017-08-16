using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MailFinder {
    public static class FileShared {
        public static FileStream OpenRead(FileInfo f) {
            return new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static FileStream OpenWrite(FileInfo f) {
            return new FileStream(f.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        public static (bool, Exception) WriteAllText(this FileInfo f, string txt, Encoding enc = null) {
            enc = enc ?? Encoding.UTF8;
            try {
                using (var stream = OpenWrite(f)) {
                    var txtbytes = enc.GetBytes(txt);
                    stream.Write(txtbytes,0,txtbytes.Length);
                }
                return (true, null);
            }
            catch (Exception e) {
                return (false, e);
            }
        }

        public static (bool, Task<(bool, AggregateException)>) WriteAllTextAsync(this FileInfo f, string txt, Encoding enc = null) {
            enc = enc ?? Encoding.UTF8;
            try {
                using (var stream = OpenWrite(f)) {
                    var txtbytes = enc.GetBytes(txt);
                    return (true, stream.WriteAsync(txtbytes,0,txtbytes.Length).ContinueWith(task => (!task.IsFaulted, (task.IsFaulted ? task.Exception : null))));
                }
            }
            catch (Exception e) {
                return (false, Task.FromResult((false,new AggregateException(e))));
            }
        }

        public static (bool, Task<(bool, string, AggregateException)>) ReadAllTextAsync(this FileInfo f, string txt, Encoding enc = null) {
            enc = enc ?? Encoding.UTF8;
            try {
                using (var stream = OpenWrite(f)) {
                    var bytes = new byte[stream.Length];
                    return (true, stream.ReadAsync(bytes, 0, bytes.Length)
                        .ContinueWith(task => 
                            (!task.IsFaulted, 
                            (task.IsFaulted ? null : enc.GetString(bytes)), 
                            (task.IsFaulted ? task.Exception : null))));
                }
            } catch (Exception e) {
                return (false, Task.FromResult<(bool, string, AggregateException)>((false,null,new AggregateException(e))));
            }
        }

        public static (bool,string, Exception) ReadAllText(this FileInfo f, string txt, Encoding enc = null) {
            enc = enc ?? Encoding.UTF8;
            try {
                using (var stream = OpenRead(f)) {
                    var bytes = new byte[stream.Length];
                    var r = stream.Read(bytes,0,bytes.Length);
                    return (true, enc.GetString(bytes),null);
                }
            }
            catch (Exception e) {
                return (false, null, e);
            }
        }

        public static string CalculateMD5(this FileInfo file, Stream fs = null) {
            using (var md5 = MD5.Create()) {
                if (fs == null) {
                    using (var stream = OpenRead(file))
                        return md5.ComputeHash(stream).ToHex(false);
                } else {
                    var p = fs.Position;
                    fs.Position = 0;
                    try {
                        return md5.ComputeHash(fs).ToHex(false);
                    } finally {
                        fs.Position = p;
                    }
                }
            }
        }

        public static string ToHex(this byte[] bytes, bool upperCase) {
            if (bytes == null || bytes.Length == 0) return "";
            var result = new StringBuilder(bytes.Length * 2);

            foreach (byte t in bytes)
                result.Append(t.ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}