using System;
using System.Collections.Generic;
using System.Linq;

namespace Linq.Extras {
    public static class XEnumerable {
        /// <summary>
        /// Produces the set intersection of two sequences, based on the specified key and key comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <c>source</c>.</typeparam>
        /// <typeparam name="TKey">The type of the key used to test for equality between elements.</typeparam>
        /// <param name="source">The first sequence.</param>
        /// <param name="other">The second sequence.</param>
        /// <param name="keySelector">A delegate that returns the key used to test for equality between elements.</param>
        /// <param name="keyComparer">A comparer used to test for equality between keys.</param>
        /// <returns>The set intersection of <c>source</c> and <c>other</c>, based on the specified key and key comparer.</returns>
        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(
             this IEnumerable<TSource> source,
             IEnumerable<TSource> other,
             Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> keyComparer = null) {
            var comparer = XEqualityComparer.By(keySelector, keyComparer);
            return source.Intersect(other, comparer);
        }
    }
}