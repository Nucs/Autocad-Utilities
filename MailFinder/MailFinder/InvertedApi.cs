using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using autonet.Settings;
using MailFinder.Helpers;
using nucs.Database;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace MailFinder {
    public static class InvertedApi {

        public static SettingsBag Bag => MainForm.Bag;

        public static IndexedFile[] Search(string query, string path) {
            return Search(query, path.ToEnumerable());
        }

        public static IndexedFile[] Search(string query, DirectoryInfo path) {
            return Search(query, path.ToEnumerable());
        }

        /// <summary>
        ///     Searches the given query
        /// </summary>
        /// <returns></returns>
        public static IndexedFile[] Search(string query, IEnumerable<DirectoryInfo> paths) {
            return Search(query, paths.Where(d => d != null).Select(d => d.FullName));
        }

        /// <summary>
        ///     Searches the given query
        /// </summary>
        /// <returns></returns>
        public static IndexedFile[] Search(string query, IEnumerable<string> paths) {
            if (string.IsNullOrEmpty(query)) return new IndexedFile[0];
            var p = paths?.Select(pa => (Path.HasExtension(pa) ? Path.GetDirectoryName(pa) : pa)?.Replace('/','\\')).Where(pa=>pa!=null && Path.GetDirectoryName(pa)!=null).ToArray() ?? new string[0];
            if (p.Length==0)
                return new IndexedFile[0];
            bool attachments = Bag.Get("deepattachments", false);
            bool like = Bag.Get("regex", false) && query.IndexOfAny(new []{'%','_'})!=-1;
            string q = $@"
                SELECT
		            *,
		            {(like? "(Content LIKE @query) AS score" : "MATCH (Content) AGAINST (@query IN NATURAL LANGUAGE MODE) AS score")}
                    {(attachments? $",{(like ? "(Innercontent LIKE @query) AS innerscore" : "MATCH (Innercontent) AGAINST (@query IN NATURAL LANGUAGE MODE) AS innerscore")}" : ",0 AS innerscore")}
	            FROM mailfinder.files
	            WHERE {string.Join(" OR ", p.Select(path=>$"`Directory`='{MySQLEscape(path)}'"))}  
                GROUP BY (MD5)
	            HAVING score > 0 OR innerscore > 0
	            ORDER BY score DESC ;";
            //Console.WriteLine(q);
            return Db.Query<ScoredIndexedFile>(q, new { query, paths = string.Join("|", p)}).Cast<IndexedFile>().ToArray();
        }

        public static void IndexFile(FileInfo f) {
            IndexFiles(f.ToEnumerable());
        }

        /// <summary>
        ///     Indexes the files that are not indexed yet.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="verifyexisting"></param>
        public static void IndexFiles(IEnumerable<FileInfo> f, bool verifyexisting = false) {
            IndexFiles(f,CancellationToken.None, verifyexisting);
        }

        /// <summary>
        ///     Indexes the files that are not indexed yet.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="verifyexisting"></param>
        public static void IndexFiles(IEnumerable<FileInfo> f,CancellationToken token, bool verifyexisting = false) {
            if (token.IsCancellationRequested)
                return;
            if (f == null) throw new ArgumentNullException(nameof(f));

            var allfiles = f.Where(fn => fn != null).Select(ff => ff.FullName).ToArray();
            if (allfiles.Length == 0)
                return;
            var alreadyexist = FilterIndexed(allfiles, verifyexisting).ToArray();
            var tocreate = allfiles.Except(alreadyexist).ToArray();
            if (tocreate.Length == 0)
                return;

            foreach (var createme in tocreate) {
                if (token.IsCancellationRequested)
                    return;
                var a = FileParser.Parse(new FileInfo(createme));
                var pp = a.ToIndexedFile();
                try {
                    var b = Db.Insert(pp);
                } catch (MySqlException e) when (e.Message.Contains("MD5_UNIQUE")) {
                    //already indexed.
                }
            }
        }

        /// <summary>
        ///     Returns a list of those that are already indexed
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="verifyexisting"></param>
        /// <returns></returns>
        public static string[] FilterIndexed(IEnumerable<string> paths, bool verifyexisting = false) {
            return FilterIndexed(paths, CancellationToken.None, verifyexisting);
        }

        /// <summary>
        ///     Returns a list of those that are already indexed
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="verifyexisting"></param>
        /// <returns></returns>
        public static string[] FilterIndexed(IEnumerable<string> paths,CancellationToken token, bool verifyexisting = false) {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            if (verifyexisting)
                paths = paths.Where(File.Exists);
            paths = paths.Select(MySQLEscape).Select(s => $"'{s}'");
            var p = paths.ToArray();
            if (p.Length == 0)
                return new string[0];
            return Db.Query<string>($"SELECT Path FROM mailfinder.files WHERE path IN ({string.Join(",", p)});").ToArray();
        }

        private static string MySQLEscape(string str) {
            return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_]",
                delegate(Match match) {
                    string v = match.Value;
                    switch (v) {
                        case "\x00": // ASCII NUL (0x00) character
                            return "\\0";
                        case "\b": // BACKSPACE character
                            return "\\b";
                        case "\n": // NEWLINE (linefeed) character
                            return "\\n";
                        case "\r": // CARRIAGE RETURN character
                            return "\\r";
                        case "\t": // TAB
                            return "\\t";
                        case "\u001A": // Ctrl-Z
                            return "\\Z";
                        default:
                            return "\\" + v;
                    }
                });
        }
    }

    [Table("files")]
    public class IndexedFile {
        public int Id { get; set; }
        public string Title { get; set; }
        public string MD5 { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        [IgnoreSelect]
        public string Content { get; set; }
        [IgnoreSelect]
        public string InnerContent { get; set; }
        public string Date { get; set; }
        public string Directory { get; set; }
    }

    public class ScoredIndexedFile : IndexedFile {
        public double Score { get; set; }
        public double Innerscore { get; set; }

        [IgnoreInsert, IgnoreSelect, IgnoreUpdate, JsonIgnore]
        public double TotalScore => Score + Innerscore;
    }
}