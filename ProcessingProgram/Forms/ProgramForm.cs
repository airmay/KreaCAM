using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    public partial class ProgramForm : UserControl
    {
        private static Settings Settings { get; set; }
        private string ProgramPath { get; set; }

        public ProgramForm()
        {
            InitializeComponent();
            Settings = Settings.GetInstance();
        }

        public void SetProgram(List<string> data)
        {
            textEdit.Lines = data.ToArray();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            string fileExt = "txt";
            if (Settings.Machine == MachineKind.Denver)
                fileExt = "csv";
            if (Settings.Machine == MachineKind.Ravelli)
                fileExt = "mpf";
            if (Settings.Machine == MachineKind.Denver)
                fileExt = "txt";

            saveFileDialog.Filter = String.Format("{0} (*.{1})|*.{2}", Settings.MachineName, fileExt, fileExt);
            saveFileDialog.FileName = "*." + fileExt;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProgramPath = saveFileDialog.FileName;
                File.WriteAllLines(ProgramPath, textEdit.Lines);
            }
        }

        private void toolStripButtonSend_Click(object sender, EventArgs e)
        {
            if (ProgramPath == null)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    ProgramPath = saveFileDialog.FileName;
                else
                    return;
            }
            var file = new FileInfo(ProgramPath);
            if (file.Exists)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    file.CopyTo(Settings.MachineIPAddress + file.Name, true);
                    Cursor = Cursors.Default;
                    MessageBox.Show("Файл программы успешно загружен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(string.Format("Ошибка при копировании: {0}", ex.Message), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(string.Format("Файл \"{0}\" не найден", ProgramPath), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }

}
