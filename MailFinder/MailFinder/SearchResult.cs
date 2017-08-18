using System.Drawing;
using Dapper;

namespace MailFinder {
    public class SearchResult {
        public double Score { get; set; }
        public Image Image { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        [IgnoreSelect]
        public string Directory => System.IO.Path.GetDirectoryName(Path);
        /// <summary>
        /// Date it was sent on.
        /// </summary>
        public string Sent { get; set; }
        /*public override string ToString() {
            return 
        }*/
    }
}