using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram.Forms
{
    public partial class ObjectForm: UserControl
    {
        private readonly List<ProcessObject> _processObjects;

        public event EventHandler DeleteProcessing;
        public event EventHandler AddObjects;
        public event EventHandler DeleteAll;
        public event EventHandler Calculate;
        public event EventHandler Quit;
        public event EventHandler ShowTools;
        public event EventHandler<ProcessObjectEventArgs> CurrentChanged;
        public event EventHandler<EventArgs<ProcessObject>> ChangeOutside;
        public event EventHandler<EventArgs<ProcessObject>> ReverseProcess;
        public event EventHandler<EventArgs<ProcessObject>> DeleteObject;

        public ObjectForm(List<ProcessObject> processObjects)
        {
            InitializeComponent();
            _processObjects = processObjects;
            bindingSource.DataSource = _processObjects;
            // ReSharper disable once CoVariantArrayConversion
            repositoryItemComboBoxFeedType.Items.AddRange(Enum.GetNames(typeof(FeedType)));
        }

        public void ShowProgress(string text)
        {
            progressLabelControl.Text = text;
            Application.DoEvents();
        }

        public void SetProgressVisible(bool isVisible)
        {
            progressPanelControl.Visible = isVisible;
        }

        private bool _isProgrammFocus;
        public void SetFocus(ObjectId objectId)
        {
            if (_processObjects.Exists(p => p.Curve.ObjectId == objectId) &&
                vGridControl.FocusedRecord != -1 && _processObjects[vGridControl.FocusedRecord].Curve.ObjectId != objectId)
            {
                _isProgrammFocus = true;
                vGridControl.FocusedRecord = _processObjects.FindIndex(p => p.Curve.ObjectId == objectId);
                _isProgrammFocus = false;
            }
        }
        
        /// <summary>
        /// Обновить список объектов на форме
        /// </summary>
        public void RefreshList()
        {
            bindingSource.ResetBindings(false);
        }

        /// <summary>
        /// Обновить объекты в соответствии с данными на форме
        /// </summary>
        public void RefreshObjects()
        {
            vGridControl.CloseEditor();
        }

        private void bindingSource_CurrentChanged(object sender, EventArgs e)
        {
            if (_isProgrammFocus)
                return; 
            var processObject = bindingSource.Current as ProcessObject;
            if (processObject != null)
                CurrentChanged.Raise(this, processObject);
                //OnCurrentChanged(new EventArgs<ProcessObject>(processObject));
        }

        #region Обработчики нажатий кнопок тулбара

        private void toolStripButtonMoveLeft_Click(object sender, EventArgs e)
        {
            if (vGridControl.FocusedRecord > 0)
            {
                var current = bindingSource.Current as ProcessObject;
                _processObjects[vGridControl.FocusedRecord] = _processObjects[vGridControl.FocusedRecord - 1];
                _processObjects[vGridControl.FocusedRecord - 1] = current;
                bindingSource.ResetBindings(false);
                vGridControl.FocusedRecord -= 1;
            }
        }

        private void toolStripButtonMoveRight_Click(object sender, EventArgs e)
        {
            if (vGridControl.FocusedRecord < _processObjects.Count - 1)
            {
                var current = bindingSource.Current as ProcessObject;
                _processObjects[vGridControl.FocusedRecord] = _processObjects[vGridControl.FocusedRecord + 1];
                _processObjects[vGridControl.FocusedRecord + 1] = current;
                bindingSource.ResetBindings(false);
                vGridControl.FocusedRecord += 1;
            }
        }

        private void toolStripButtonDeleteAll_Click(object sender, EventArgs e)
        {
            OnDeleteAll(new EventArgs());
        }

        private void toolStripButtonCalc_Click(object sender, EventArgs e)
        {
            OnCalculate(new EventArgs());
        }

        private void toolStripButtonReverseProcess_Click(object sender, EventArgs e)
        {
            var processObject = bindingSource.Current as ProcessObject;
            if (processObject != null)
            {
                OnReverseProcess(new EventArgs<ProcessObject>(processObject));
                vGridControl.Refresh();
            }
        }

        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            OnQuit(new EventArgs());
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            OnAddObjects(new EventArgs());
            vGridControl.Refresh();
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            var processObject = bindingSource.Current as ProcessObject;
            if (processObject != null)
            {
                //OnDeleteObject(new EventArgs<ProcessObject>(processObject));
                DeleteObject.Raise(this, processObject);
                vGridControl.Refresh();
            }
        }

        private void toolStripButtonTools_Click(object sender, EventArgs e)
        {
            OnShowTools(new EventArgs());
        }

        private void toolStripButtonDeleteProcessing_Click(object sender, EventArgs e)
        {
            DeleteProcessing.Raise(this);
            //OnDeleteProcessing(EventArgs.Empty);
        }

        private void toolStripButtonChangeOutside_Click(object sender, EventArgs e)
        {
            var processObject = bindingSource.Current as ProcessObject;
            if (processObject != null)
                OnChangeOutside(new EventArgs<ProcessObject>(processObject));
        }

        #endregion

        #region Инициаторы событий формы

        private void OnDeleteProcessing(EventArgs e)
        {
            if (DeleteProcessing != null)
                DeleteProcessing(this, e);
        }

        private void OnAddObjects(EventArgs e)
        {
            if (AddObjects != null)
                AddObjects(this, e);
        }

        private void OnDeleteAll(EventArgs e)
        {
            if (DeleteAll != null)
                DeleteAll(this, e);
        }

        private void OnCalculate(EventArgs e)
        {
            if (Calculate != null)
                Calculate(this, e);
        }

        private void OnQuit(EventArgs e)
        {
            if (Quit != null)
                Quit(this, e);
        }

        private void OnShowTools(EventArgs e)
        {
            if (ShowTools != null)
                ShowTools(this, e);
        }

/*        private void OnCurrentChanged(EventArgs<ProcessObject> e)
        {
            if (CurrentChanged != null)
                CurrentChanged(this, e);
        }*/

        private void OnChangeOutside(EventArgs<ProcessObject> e)
        {
            if (ChangeOutside != null)
                ChangeOutside(this, e);
        }

        private void OnReverseProcess(EventArgs<ProcessObject> e)
        {
            if (ReverseProcess != null)
                ReverseProcess(this, e);
        }

        private void OnDeleteObject(EventArgs<ProcessObject> e)
        {
            if (DeleteObject != null)
                DeleteObject(this, e);
        }

        #endregion


        /*        [DllImport("user32.dll")]
        extern static IntPtr SetFocus(IntPtr hWnd);

        private void SetFocus()
        {
            SetFocus(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Window.Handle);
        }*/
    }
}
