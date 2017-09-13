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
                this.DataSource = filteredItems;
            }
        }

    }
}