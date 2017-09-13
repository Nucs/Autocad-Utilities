using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;

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

        public static SelectionSet ToSelectionSet<T>(this IEnumerable<T> list, SelectionMethod? method = SelectionMethod.Crossing) where T : Drawable {
            var ss = SelectionSet.FromObjectIds(list.Select(t => t.Id).ToArray());
            if (method!=null)
                foreach (SelectedObject o in ss) 
                    o.GetType().GetField("m_method", BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(o, (SelectionMethod)method);
            
            return ss;
        }
        public static SelectionSet ToSelectionSet(this IEnumerable<ObjectId> list, SelectionMethod? method = SelectionMethod.Crossing) {
            var ss = SelectionSet.FromObjectIds(list.ToArray());
            if (method!=null)
                foreach (SelectedObject o in ss) 
                    o.GetType().GetField("m_method", BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(o, (SelectionMethod)method);
            
            return ss;
        }

        public static List<T> LInsert<T>(this List<T> l, int index, T obj) {
            l.Insert(index, obj);
            return l;
        }
    }
}