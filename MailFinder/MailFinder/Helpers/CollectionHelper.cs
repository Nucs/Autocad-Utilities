using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MailFinder.Helpers {
    public static class CollectionHelper {
        public static IEnumerable<T> ConcatSafe<T>(this T a, IEnumerable<T> b) {
            if ((object) a == null && b==null)
                    return new T[0];
            if ((object) a == null)
                return b;
            return a.ToEnumerable().ConcatSafe(b);
        }


        public static IEnumerable<T> ConcatSafe<T>(this IEnumerable<T> a, IEnumerable<T> b) {
            if (a == null && b == null)
                return new T[0];
            if (a == null)
                return b;
            if (b == null)
                return a;
            return a.Concat(b);
        }

        public static IEnumerable<T> ToEnumerable<T>(this T o) {
            return new []{o};
        }
    }
}