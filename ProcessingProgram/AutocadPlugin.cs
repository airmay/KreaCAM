using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using ProcessingProgram.Forms;
using ProcessingProgram.Objects;
using Autodesk.AutoCAD.DatabaseServices;

namespace ProcessingProgram
{
    public class AutocadPlugin : IExtensionApplication
    {
        private static List<ProcessingAction> ProcessingActions;
        private static readonly List<ProcessObject> ProcessObjects = new List<ProcessObject>();
        private static readonly List<ProcessCurve> ProcessCurves = new List<ProcessCurve>();
        private static readonly List<Curve> SectionCurves = new List<Curve>();
        private static List<Tool> Tools = Tool.LoadTools();

        private static readonly ObjectForm ObjectForm = new ObjectForm(ProcessObjects);
        private static readonly ProcessingForm ProcessingForm = new ProcessingForm();
        private static readonly ProgramForm ProgramForm = new ProgramForm();
        private static readonly SettingForm SettingForm = new SettingForm();
        private static readonly ToolStoreForm ToolStoreForm = new ToolStoreForm(Tools);

        public void Initialize()
        {
            AutocadUtils.WriteMessage("Инициализация плагина ProcessingProgram. Версия с режимом обработки."); // + DateTime.Today.ToShortDateString()); TODO Assemlly.DateTime()
            
            ObjectForm.DeleteAll += (sender, args) => DeleteAll();
            ObjectForm.DeleteProcessing += (sender, args) => DeleteProcessing();
            ObjectForm.Calculate += (sender, args) => Calculate();
            ObjectForm.Quit += (sender, args) => AutocadUtils.Close();
            ObjectForm.AddObjects += (sender, args) => AddObjects();
            ObjectForm.ShowTools += (sender, args) => AutocadUtils.ShowModalDialog(ToolStoreForm);
            ObjectForm.CurrentChanged += (sender, args) => AutocadUtils.SelectObject(args.Data.Curve.ObjectId);
            ObjectForm.ChangeOutside += (sender, args) =>
                {
                    CalcUtils.ChangeOutside(args.Data.Curve.ObjectId);
                    Calculate();
                };
            ObjectForm.ReverseProcess += (sender, args) => 
                {
                    args.Data.Direction *= -1;
                    Calculate();
                };
            ObjectForm.DeleteObject += (sender, args) =>
                {
                    ProcessObjects.Remove(args.Data);
                    if (ProcessObjects.All(p => p.Curve != args.Data.Curve))
                        ProcessCurves.Remove(args.Data.Curve);
                    Calculate();
                };
            SettingForm.AddSectionObjects += (sender, args) =>
                {
                    AddSectionObjects();
                    SettingForm.SetSectionDesc(SectionCurves.Any() ? String.Format("Сечение установлено. {0} элемента", SectionCurves.Count) : "");
                };

            AutocadUtils.AddPaletteSet("Объекты", ObjectForm);

            ProcessingForm.CurrentChanged += (sender, args) => AutocadUtils.SelectObjects(args.Data.ObjectId, args.Data.ToolObjectId);
            ProcessingForm.DeleteProcessing += (sender, args) => DeleteProcessing();
            ProcessingForm.Calculate += (sender, args) => Calculate();
            AutocadUtils.AddPaletteSet("Обработка", ProcessingForm);

            AutocadUtils.AddPaletteSet("Программа", ProgramForm);
            AutocadUtils.AddPaletteSet("Настройки", SettingForm);

            AutocadUtils.Selected += (sender, args) => ProcessingForm.SelectObjects(args.Data);
            AutocadUtils.Focused += (sender, args) => ProcessingForm.SetFocus(args.Data);
            AutocadUtils.Focused += (sender, args) => ObjectForm.SetFocus(args.Data);

            CalcUtils.Init(ProcessObjects, ProcessCurves);
            ProcessObjectFactory.Init(ProcessObjects, ProcessCurves);

            var machine = new Machine();
            machine.ChangeActionsCount += (sender, args) => ObjectForm.ShowProgress(String.Format("Генерация обработки... {0} строк", args.Data));
            ActionGenerator.SetMachine(machine);

            //AutocadUtils.CreateTest();
            //RunTest();
        }

        public void Terminate()
        {
            SettingForm.RefreshSettings();
            ProcessingParams.SaveDefault();
            Settings.Save();
        }

        [CommandMethod("show")]
        public void ShowPaletteSet()
        {
            AutocadUtils.ShowPaletteSet();
        }

        /// <summary>
        /// Добавить объекты
        /// </summary>
        //[CommandMethod("add")]
        //, CommandFlags.UsePickSet)] // | CommandFlags.Redraw | CommandFlags.Modal)] //CommandFlags.Redraw SetImpliedSelection()SelectImplied
        private void AddObjects()
        {
            var selectedObjects = AutocadUtils.GetSelectedObjects();
            if (selectedObjects == null)
                return;
            if (AutocadUtils.ShowModalDialog(ToolStoreForm) != DialogResult.OK) return;
            var tools = ToolStoreForm.GetSelectedToos();
            SettingForm.RefreshSettings();
            ProcessObjectFactory.Create(selectedObjects, tools);
            ObjectForm.RefreshList();
            ShowPaletteSet();
        }

        /// <summary>
        /// Удалить все
        /// </summary>
        private void DeleteAll()
        {
            //AutocadUtils.Test();
            //return;
            DeleteProcessing();
            ProcessObjects.Clear();
            ProcessCurves.Clear();
            ObjectForm.RefreshList();
        }

        /// <summary>
        /// Удалить обработку
        /// </summary>
        private void DeleteProcessing()
        {
            if (ActionGenerator.IsCalculation)
            {
                ActionGenerator.Abort();
                AutocadUtils.ShowError("Расчет обработки прерван");
                return;
            }
            if (ProcessingActions == null)
                return;
            AutocadUtils.DeleteCurves(ProcessingActions.ConvertAll(p => p.ObjectId));
            AutocadUtils.DeleteCurves(ProcessingActions.ConvertAll(p => p.DirectObjectId));
            AutocadUtils.DeleteCurves(ProcessingActions.ConvertAll(p => p.ToolObjectId));
            ProcessingActions.Clear();
            ProcessingForm.ShowData(ProcessingActions);
        }

        /// <summary>
        /// Расчет обработки
        /// </summary>
        private void Calculate()
        {
            ObjectForm.SetProgressVisible(true);
            DeleteProcessing();
            SettingForm.RefreshSettings();
            ObjectForm.RefreshObjects();

            ProcessingActions = ActionGenerator.Generate(ProcessObjects, SectionCurves);

            ProcessingForm.ShowData(ProcessingActions);
            var machineProgram = ProgramGenerator.Generate(ProcessingActions);
            ProgramForm.SetProgram(machineProgram);
            ObjectForm.SetProgressVisible(false);
        }

        [Conditional("DEBUG")]
        private void RunTest()
        {
            var selectedObjects = AutocadUtils.GetAllCurves();
            if (selectedObjects == null || !Tools.Any())
                return;
            ProcessObjectFactory.Create(selectedObjects.FindAll(p => p.GetLength() > 100), Tools.Where(p => p.No == 2));
            /*
            SectionCurves.AddRange(selectedObjects.FindAll(p => p.GetLength() < 100).Cast<Curve>().ToList());
            var points = SectionCurves.Select(p => p.StartPoint.Y).Concat(SectionCurves.Select(p => p.EndPoint.Y));
            Settings.GetInstance().HeightMax = points.Max();
            Settings.GetInstance().HeightMin = points.Min();
            SettingForm.RefreshForm();
             * */
            Calculate();
            ObjectForm.RefreshList();
        }

        /// <summary>
        /// Добавить объекты сечения
        /// </summary>
        //[CommandMethod("adds")]
        private void AddSectionObjects()
        {
            var selectedObjects = AutocadUtils.GetSelectedObjects();
            if (selectedObjects == null)
                return;
            SectionCurves.Clear();
            SectionCurves.AddRange(selectedObjects.Cast<Curve>().ToList());
            var points = SectionCurves.Select(p => p.StartPoint.Y).Concat(SectionCurves.Select(p => p.EndPoint.Y));
            Settings.GetInstance().HeightMax = points.Max();
            Settings.GetInstance().HeightMin = points.Min();
            SettingForm.RefreshForm();
            AutocadUtils.WriteMessage(String.Format("Добавлено сечение: {0} объектов. Диапазон по высоте: {1}-{2}",
                SectionCurves.Count, Settings.GetInstance().HeightMin, Settings.GetInstance().HeightMax));
        }
    }
}
