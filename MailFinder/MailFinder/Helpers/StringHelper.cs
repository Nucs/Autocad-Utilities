using System;

namespace MailFinder.Helpers {
    public static class StringHelper {
        public static int CountInside(string haystack, string needle, bool ignorecase = false) {
            if (string.IsNullOrEmpty(haystack)) return 0;
            if (string.IsNullOrEmpty(needle)) return 0;
            return CountInside(haystack, new[] {needle}, ignorecase);
        }

        public static int CountInside(string haystack, string[] needles, bool ignorecase = false) {
            if (string.IsNullOrEmpty(haystack)) return 0;
            if (needles == null || needles.Length == 0) return 0;
            var ret = CountInside(new[] {haystack}, needles, ignorecase);
            if (ret.Length == 0)
                return 0;
            return ret[0];
            /* var strcomp = ignorecase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
             int count = 0;
             foreach (var needle in needles) {
                 var n = 0;
                 while ((n = haystack.IndexOf(needle, n, strcomp)) != -1) {
                     n += needle.Length;
                     ++count;
                 }
             }
 
             return count;*/
        }

        public static int CountInside(string[] haystacks, string needle, bool ignorecase = false) {
            if (string.IsNullOrEmpty(needle)) return 0;
            if (haystacks == null || haystacks.Length == 0) return 0;
            var ret = CountInside(haystacks, new[] {needle}, ignorecase);
            if (ret.Length == 0)
                return 0;
            return ret[0];
        }

        /// <summary>
        ///     returns first, index of haystack
        /// </summary>
        /// <param name="haystacks"></param>
        /// <param name="needles"></param>
        /// <param name="ignorecase"></param>
        /// <returns></returns>
        public static int[] CountInside(string[] haystacks, string[] needles, bool ignorecase = false) {
            if (haystacks == null || haystacks.Length == 0) return new int[0];
            if (needles == null || needles.Length == 0) return new int[0];
            var strcomp = ignorecase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            int[] @out = new int[haystacks.Length];

            for (int i = 0; i < haystacks.Length; i++) {
                var haystack = haystacks[i];
                foreach (var needle in needles) {
                    int count = 0;
                    var n = 0;
                    while ((n = haystack.IndexOf(needle, n, strcomp)) != -1) {
                        n += needle.Length;
                        ++count;
                    }
                    @out[i] = count;
                }
            }

            return @out;
        }
    }
}