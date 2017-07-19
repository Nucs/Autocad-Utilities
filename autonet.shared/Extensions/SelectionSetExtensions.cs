using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace autonet.Extensions {
    public static class SelectionSetExtensions {
        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr) {
            return Quick.GetImpliedOrSelect();
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, SelectionFilter f) {
            return Quick.GetImpliedOrSelect(f);
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, PromptSelectionOptions f) {
            return Quick.GetImpliedOrSelect(f);
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, PromptSelectionOptions f, SelectionFilter ff) {
            return Quick.GetImpliedOrSelect(f, ff);
        }

        public static void SetSelected(this QuickTransaction tr, SelectionSet ss, bool runsssetfirst = false) {
            Quick.SetSelected(ss, runsssetfirst);
        }
    }
}