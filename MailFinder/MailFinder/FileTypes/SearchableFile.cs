using System;
using System.Collections.Generic;
using System.IO;

namespace MailFinder.FileTypes {
    public abstract class SearchableFile : IEquatable<SearchableFile>, IComparable<SearchableFile>, IComparable {
        public string MD5 { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }

        protected SearchableFile() {}
        protected SearchableFile(FileInfo file) {}

        protected SearchableFile(FileInfo file, Stream stream) { }

        /// <summary>
        ///     Search inside the file and rate the occurances - higher, the more chance for it to be it.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ignorecase"></param>
        /// <returns></returns>
        public abstract int SearchInside(string str, bool ignorecase);

        public int CompareTo(SearchableFile other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var md5Comparison = string.Compare(MD5, other.MD5, StringComparison.Ordinal);
            if (md5Comparison != 0) return md5Comparison;
            return string.Compare(Version, other.Version, StringComparison.Ordinal);
        }

        public int CompareTo(object obj) {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is SearchableFile)) throw new ArgumentException($"Object must be of type {nameof(SearchableFile)}");
            return CompareTo((SearchableFile) obj);
        }

        public static bool operator <(SearchableFile left, SearchableFile right) {
            return Comparer<SearchableFile>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(SearchableFile left, SearchableFile right) {
            return Comparer<SearchableFile>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(SearchableFile left, SearchableFile right) {
            return Comparer<SearchableFile>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(SearchableFile left, SearchableFile right) {
            return Comparer<SearchableFile>.Default.Compare(left, right) >= 0;
        }

        public bool Equals(SearchableFile other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(MD5, other.MD5) && string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SearchableFile) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((MD5 != null ? MD5.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(SearchableFile left, SearchableFile right) {
            return Equals(left, right);
        }

        public static bool operator !=(SearchableFile left, SearchableFile right) {
            return !Equals(left, right);
        }

        public abstract IndexedFile ToIndexedFile();
    }
}