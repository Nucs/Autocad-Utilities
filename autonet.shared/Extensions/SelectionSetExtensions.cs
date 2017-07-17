using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace autonet.Extensions {
    public static class SelectionSetExtensions {
        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr) {
            var sel = tr.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count==0) {
                sel = tr.GetSelection();
                if (sel.Status != PromptStatus.OK) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, SelectionFilter f) {
            var sel = tr.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = tr.GetSelection(f);
                if (sel.Status != PromptStatus.OK) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, PromptSelectionOptions f) {
            var sel = tr.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = tr.GetSelection(f);
                if (sel.Status != PromptStatus.OK) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(this QuickTransaction tr, PromptSelectionOptions f, SelectionFilter ff) {
            var sel = tr.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = tr.GetSelection(f, ff);
                if (sel.Status != PromptStatus.OK) {
                    return null;
                }
            }
            return sel.Value;
        }
    }
}