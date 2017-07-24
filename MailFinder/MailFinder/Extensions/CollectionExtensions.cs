using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace autonet.Extensions {
    public static class CollectionExtensions {
        public static List<TOut> TakeoutWhere<TIn,TOut>(this List<TIn> @in, Func<TIn, bool> verify) {
            var @out = new List<TOut>();
            foreach (var o in @in.ToArray().Where(verify)) {
                @out.Add((TOut) (object) o);
                @in.Remove(o);
            }
            return @out;
        }

        public static List<TOut> TakeoutWhereType<TIn, TOut>(this List<TIn> @in, Type t) {
            return TakeoutWhere<TIn,TOut>(@in, @i=>@i.GetType() == t);
        }
        public static List<TOut> TakeoutWhereType<TIn, TOut>(this List<TIn> @in) {
            return TakeoutWhere<TIn,TOut>(@in, @i=>@i.GetType() == typeof(TOut));
        }
         public static List<TIn> WhereType<TIn>(this List<TIn> @in, Type t) {
            return @in.Where(_in => _in.GetType() == t).ToList();
        }

        public static List<T> LInsert<T>(this List<T> l, int index, T obj) {
            l.Insert(index, obj);
            return l;
        }
    }
}