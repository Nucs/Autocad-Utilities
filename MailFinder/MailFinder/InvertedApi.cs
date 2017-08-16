using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using MailFinder.Helpers;
using nucs.Database;

namespace MailFinder {
    public static class InvertedApi {
        /// <summary>
        ///     Searches the given query
        /// </summary>
        /// <returns></returns>
        public static IndexedFile[] Search(string query) {
            const string q = @"
                SELECT 
                    *,
                    MATCH (content, innercontent) AGAINST (@query IN NATURAL LANGUAGE MODE) AS score
                ORDER BY score DESC";
            return Db.Query<ScoredIndexedFile>(q, new {query}).Cast<IndexedFile>().ToArray();
        }

        public static void IndexFile(FileInfo f) {
            IndexFiles(f.ToEnumerable());
        }

        /// <summary>
        ///     Indexes the files that are not indexed yet.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="verifyexisting"></param>
        public static void IndexFiles(IEnumerable<FileInfo> f, bool verifyexisting=false) {
            if (f == null) throw new ArgumentNullException(nameof(f));
            
            var allfiles = f.Where(fn => fn != null).Select(ff => ff.FullName).ToArray();
            var tocreate = allfiles.Except(FilterIndexed(allfiles, verifyexisting));

            foreach (var createme in tocreate) {
                var a = FileParser.Parse(new FileInfo(createme));
                var pp = a.ToIndexedFile();
                var b = Db.Insert(pp);
                if (b==null)
                    throw new InvalidOperationException("Failed Inserting");
            }
        }

        //todo add a force refresh function that will re-add the data to the database

        /// <summary>
        ///     Returns a list of those that are already indexed
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="verifyexisting"></param>
        /// <returns></returns>
        public static string[] FilterIndexed(IEnumerable<string> paths, bool verifyexisting = false) {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            if (verifyexisting)
                paths = paths.Where(File.Exists);
            paths = paths.Select(MySQLEscape).Select(s=>$"'{s}'");
            return Db.Query<string>($"SELECT * FROM mailfinder.files WHERE path IN ({string.Join(",", paths)});").ToArray();
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

        public static IEnumerable<string> And(this Dictionary<string, List<string>> index, string firstTerm, string secondTerm) {
            return (from d in index
                    where d.Key.Equals(firstTerm)
                    select d.Value).SelectMany(x => x)
                .Intersect
                ((from d in index
                    where d.Key.Equals(secondTerm)
                    select d.Value).SelectMany(x => x));
        }

        public static IEnumerable<string> Or(this Dictionary<string, List<string>> index, string firstTerm, string secondTerm) {
            //return (from d in index
            //        where d.Key.Equals(firstTerm)
            //        select d.Value).SelectMany(x => x).ToList().Union
            //                ((from d in index
            //                  where d.Key.Equals(secondTerm)
            //                  select d.Value).SelectMany(x => x).ToList()).Distinct();

            return (from d in index
                    where d.Key.Equals(firstTerm) || d.Key.Equals(secondTerm)
                    select d.Value).SelectMany(x => x)
                .Distinct();
        }
    }

    public class IndexedFile {
        public string MD5 { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public string InnerContent { get; set; }
    }

    public class ScoredIndexedFile : IndexedFile {
        public double Score { get; set; }
    }

    class EntryPoint {
        public static Dictionary<string, List<string>> invertedIndex;

        /*static void Main(string[] args) {
            invertedIndex = new Dictionary<string, List<string>>();
            string folder = "C:\\Users\\Elena\\Documents\\Visual Studio 2013\\Projects\\InvertedIndex\\Files\\";

            foreach (string file in Directory.EnumerateFiles(folder, "*.txt")) {
                List<string> content = System.IO.File.ReadAllText(file).Split(' ').Distinct().ToList();
                addToIndex(content, file.Replace(folder, ""));
            }

            var resAnd = invertedIndex.And("star", "sparkling");
            var resOr = invertedIndex.Or("star", "sparkling");

            Console.ReadLine();
        }

        private static void addToIndex(List<string> words, FileInfo document) {
            foreach (var word in words) {
                if (!invertedIndex.ContainsKey(word)) {
                    invertedIndex.Add(word, new List<string> {document});
                } else {
                    invertedIndex[word].Add(document);
                }
            }
        }*/
    }
}