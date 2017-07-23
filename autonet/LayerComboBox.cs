using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace autonet {
    public class LayerComboBox : ComboBox {

        public List<object> Objects { get; set; }
        public Func<object,string,bool> Comparer { get; set; }

        protected override void OnTextUpdate(EventArgs e) {
            base.OnTextUpdate(e);
            if (Objects != null && Comparer !=null) {
                
                string filter_param = this.Text;

                List<object> filteredItems = Objects.Where(x => Comparer(x,filter_param)).ToList();
                // another variant for filtering using StartsWith:
                // List<string> filteredItems = arrProjectList.FindAll(x => x.StartsWith(filter_param));

                this.DataSource = filteredItems;

                // if all values removed, bind the original full list again
/*                if (String.IsNullOrWhiteSpace())
                {
                    this.DataSource = Objects;
                }*/

            }
            // this line will make sure, that the ComboBox opens itself to show the filtered results     
        }

    }
}