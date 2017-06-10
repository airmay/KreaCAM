using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    public partial class ToolStoreForm : Form  // TODO сделать User Control
    {
        private List<Tool> Tools;

        public ToolStoreForm(List<Tool> data)
        {
            InitializeComponent();
            Tools = data;
            toolBindingSource.DataSource = Tools;
        }

        public IEnumerable<Tool> GetSelectedToos()
        {
            return gridView.GetSelectedRows().ToList().ConvertAll(p => gridView.GetRow(p) as Tool);
        }

        private void SimpleButtonSaveToolsClick(object sender, EventArgs e)
        {
            Tool.SaveTools(toolBindingSource.DataSource as List<Tool>);
        }

        private void ToolStoreForm_Load(object sender, EventArgs e)
        {
            //toolBindingSource.ResetBindings(true);
            //toolBindingSource.DataSource = Tool.LoadTools();
        }
    }
}
