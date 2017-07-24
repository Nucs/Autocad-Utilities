using System.Collections.Generic;

namespace nucs.Collections {
    public delegate bool EqualsDelegate<in T>(T x, T y);

    public delegate int GetHashCodeDelegate<in T>(T obj);

    /// <summary>
    ///     Equality comparer using lambda actions. saves time 
    /// </summary>
    /// <typeparam name="T">Compared type</typeparam>
    public class DynamicEqualityComparer<T> : IEqualityComparer<T> {
        public EqualsDelegate<T> EqualityMethod;
        public GetHashCodeDelegate<T> GetHashCodeMethod;

        public DynamicEqualityComparer(EqualsDelegate<T> equalityMethod, GetHashCodeDelegate<T> hashcodeMethod) {
            EqualityMethod = equalityMethod;
            GetHashCodeMethod = hashcodeMethod;
        }

        public bool Equals(T x, T y) {
            return EqualityMethod(x, y);
        }

        public int GetHashCode(T obj) {
            return GetHashCodeMethod(obj);
        }
    }
}