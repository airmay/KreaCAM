using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    public partial class ProcessingForm : UserControl
    {
        private List<ProcessingAction> _processingActions;
        private bool _isProgrammFocus;

        public event EventHandler<EventArgs<ProcessingAction>> CurrentChanged;
        public event EventHandler DeleteProcessing;
        public event EventHandler Calculate;

        public ProcessingForm()
        {
            InitializeComponent();
        }

        public void ShowData(List<ProcessingAction> data)
        {
            _processingActions = data;
            bindingSource.DataSource = _processingActions;
            bindingSource.ResetBindings(false);
        }

        private void bindingSource_CurrentChanged(object sender, EventArgs e)
        {
            if (_isProgrammFocus)
                return;
            var action = bindingSource.Current as ProcessingAction;
            if (action != null)
                OnCurrentChanged(new EventArgs<ProcessingAction>(action));
        }

        private void toolStripButtonCalculate_Click(object sender, EventArgs e)
        {
            OnCalculate(new EventArgs());
        }

        private void toolStripButtonDeleteProcessing_Click(object sender, EventArgs e)
        {
            OnDeleteProcessing(new EventArgs());
        }

        public void SelectObjects(List<ObjectId> objectIds)
        {
            if (_processingActions == null)
                return;
            gridView.BeginSelection();
            gridView.ClearSelection();
            if (objectIds != null)
                _processingActions.FindAll(p => objectIds.Contains(p.ObjectId)).ConvertAll(p => gridView.GetRowHandle(_processingActions.IndexOf(p)))
                    .ForEach(p => gridView.SelectRow(p));
            gridView.EndSelection();
        }

        public void SetFocus(ObjectId objectId)
        {
            if (_processingActions == null)
                return;
            _isProgrammFocus = true;
            gridView.FocusedRowHandle = gridView.GetRowHandle(_processingActions.FindIndex(p => p.ObjectId == objectId));
            _isProgrammFocus = false;
        }

        #region Инициаторы событий формы

        private void OnCurrentChanged(EventArgs<ProcessingAction> e)
        {
            if (CurrentChanged != null)
                CurrentChanged(this, e);
        }
        private void OnDeleteProcessing(EventArgs e)
        {
            if (DeleteProcessing != null)
                DeleteProcessing(this, e);
        }

        private void OnCalculate(EventArgs e)
        {
            if (Calculate != null)
                Calculate(this, e);
        }

        #endregion
    }
}
