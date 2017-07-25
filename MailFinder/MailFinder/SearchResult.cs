using System.Drawing;

namespace MailFinder {
    public class SearchResult {
        public string Path { get; set; }
        public string Term { get; set; }
        public Image Image { get; set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        /*public override string ToString() {
            return 
        }*/
    }
}