using System;
using System.Windows.Forms;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;
using DevExpress.XtraLayout.Utils;

namespace ProcessingProgram.Forms
{
    public partial class SettingForm : UserControl
    {
        public event EventHandler AddSectionObjects;

        public SettingForm()
        {
            InitializeComponent();

            bindingSourceSettings.DataSource = Settings.GetInstance(); // Properties.Settings.Default;
            bindingSourceProcessParams.DataSource = ProcessingParams.GetDefault(); // Properties.Settings.Default;

            comboBoxEdit1.SelectedIndex = (int)Settings.GetInstance().ProcessMode;

            // ReSharper disable once CoVariantArrayConversion
            object[] items = Enum.GetNames(typeof(FeedType));
            comboBoxEditFeedType.Properties.Items.AddRange(items);
            comboBoxEditRetractType.Properties.Items.AddRange(items);
        }

        /// <summary>
        /// Обновить настройки в соответствии с формой настроек
        /// </summary>
        public void RefreshSettings()
        {
            bindingSourceSettings.EndEdit();
            bindingSourceProcessParams.EndEdit();
        }

        public void RefreshForm()
        {
            bindingSourceSettings.ResetBindings(false);
            bindingSourceProcessParams.ResetBindings(false);
        }

        /// <summary>
        /// Отобразить описание сечения
        /// </summary>
        /// <param name="sectorDesc"></param>
        public void SetSectionDesc(string sectionDesc)
        {
            if (sectionDesc == "")
                return;
            sectorItem.Text = sectionDesc;
            sectorItem.TextVisible = true;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((Settings)bindingSourceSettings.DataSource).ProcessMode = (ProcessMode)comboBoxEdit1.SelectedIndex;
            layoutControlGroupProfileDisc.Visibility = 
                ((Settings)bindingSourceSettings.DataSource).ProcessMode == ProcessMode.ProfileDisc 
                ? LayoutVisibility.Always 
                : LayoutVisibility.Never;
        }

        private void simpleButtonSetSector_Click(object sender, EventArgs e)
        {
            AddSectionObjects(this, e);
        }
    }
}
