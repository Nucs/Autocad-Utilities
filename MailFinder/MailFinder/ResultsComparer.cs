using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace MailFinder {
    internal class ResultsComparer : IComparer, IComparer<string> {
        private readonly SortOrder _sortOrder;

        public ResultsComparer(SortOrder sortOrder) {
            _sortOrder = sortOrder;
        }

        public int Compare(object x, object y) {
            var dir = SortOrder.Descending;
            const string format = "dd/MM/yyyy HH:mm:ss";
            if (string.IsNullOrEmpty(x?.ToString()) && string.IsNullOrEmpty(y?.ToString()))
                return 0;
            if (string.IsNullOrEmpty(x?.ToString()))
                return -1;
            if (string.IsNullOrEmpty(y?.ToString()))
                return 1;
            if (x is OLVListItem _x && y is OLVListItem _y) {
                x = _x.Text;
                y = _y.Text;
            }
            if (x is double __x && y is double __y) {
                return (__x as IComparable).CompareTo(__y);
            }

            try {
                var a = DateTime.ParseExact(x.ToString(), format, null);
                var b = DateTime.ParseExact(y.ToString(), format, null);
                return _sortOrder == SortOrder.Ascending ? DateTime.Compare(a, b) : DateTime.Compare(b,a);
            } catch { }

            return String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
        }

        public int Compare(string x, string y) {
            return Compare((object) x, (object) y);
        }
    }
}