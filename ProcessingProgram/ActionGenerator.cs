using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    /// <summary>
    /// Генератор последовательности действий станка для выполнения процесса обработки
    /// </summary>
    public static class ActionGenerator
    {
        private static IMachine Machine { get; set; }
        private static Settings Settings { get; set; }

        public static void SetMachine(IMachine machine)
        {
            Machine = machine;
            Settings = Settings.GetInstance();
        }

        /// <summary>
        /// Расчет обработки
        /// </summary>
        public static List<ProcessingAction> Generate(IEnumerable<ProcessObject> processObjects, List<Curve> sectionCurves)
        {
            try
            {
                IsCalculation = true;
                AutocadUtils.StartTransaction();
                Machine.Start();
                foreach (var processObject in processObjects)
                {
                    if (!IsCalculation)
                        break;
                    if (!CheckParams(processObject))
                        continue;

                    Machine.SetTool(processObject.Tool);

                    if (processObject.Curve.OutsideSign == 0)
                        CalcUtils.CalcOutside(processObject.Curve);

                    var curve = AutocadUtils.GetCurve(processObject.Curve.ObjectId);

                    if (Settings.ProcessMode == ProcessMode.ContinuousDescent)
                    {
                        if (!curve.Closed)
                            throw new Exception("Расчет в режиме непрерывного опускания возможен только для замкнутой кривой");
                        CuttingContinuousDescent(processObject, curve);
                    }
                    else if (Settings.ProcessMode == ProcessMode.ProfileDisc)
                        CuttingProfileDisc(processObject, curve, sectionCurves);
                    else
                        Cutting(processObject, curve);
                }
                Machine.Stop();
                AutocadUtils.CommitTransaction();
            }
            catch (Exception e)
            {
                Machine.Clear();
                var error = String.Format("Ошибка при расчете обработки: {0}", e.Message);
                AutocadUtils.ShowError(error);
            }
            AutocadUtils.DisposeTransaction();
            IsCalculation = false;

            return Machine.GetProcessingActions();
        }

        private static bool CheckParams(ProcessObject processObject)
        {
            var par = processObject.ProcessingParams;
            var error = String.Empty;
            if ((Settings.ProcessMode == ProcessMode.StepByStepDescent || Settings.ProcessMode == ProcessMode.ContinuousDescent) 
                && Settings.Thickness < 0.1)
                error = "укажите толщину";
            if (Settings.ProcessMode == ProcessMode.ContinuousDescent && par.DepthAll == 0)
                error = "укажите суммарную глубину";
            if (par.DepthAll > 0 && par.Depth < 0.1)
                error = "укажите шаг";
            if (!String.IsNullOrEmpty(error))
                AutocadUtils.ShowError(String.Format("Объект {0}: {1}", processObject.Curve.Name, error));
            return String.IsNullOrEmpty(error);
        }

        private static void Cutting(ProcessObject processObject, Curve curve)
        {
            var par = processObject.ProcessingParams;
            var s = par.DepthAll;
            var direction = processObject.Direction;
            var isFirstPass = true;
            do
            {
                if (!IsCalculation)
                    return;

                if (!isFirstPass)
                {
                    s -= par.Depth;
                    if (s < 0) 
                        s = 0;
                    if (!Settings.OneDirection)
                        direction = -direction;
                }
                Curve toolpathCurve = Settings.ProcessMode == ProcessMode.StepByStepDescent
                    ? AutocadUtils.GetDisplacementCopy(curve, Settings.Thickness - par.DepthAll + s)
                    : AutocadUtils.GetOffsetCopy(curve, processObject.Curve.OutsideSign*s);
                if (!Settings.WithCompensation)
                    toolpathCurve = AutocadUtils.GetOffsetCopy(toolpathCurve, processObject.Curve.OutsideSign * processObject.Tool.Diameter.Value/2);

                Feed(toolpathCurve, direction, processObject.Curve.OutsideSign, par, isFirstPass || Settings.OneDirection);

                if (processObject.Curve.Type == CurveType.Polyline || processObject.Curve.Type == CurveType.Circle)
                    Machine.Cutting(AutocadUtils.Explode(toolpathCurve, direction == -1));
                else
                    Machine.Cutting(toolpathCurve);

                Retract(toolpathCurve, direction, processObject.Curve.OutsideSign, par);
                isFirstPass = false;
            } while (s > 0);
        }

        private static void CuttingContinuousDescent(ProcessObject processObject, Curve curve)
        {
            var par = processObject.ProcessingParams;
            var toolpathCurve = AutocadUtils.GetDisplacementCopy(curve, Settings.Thickness);

            Feed(toolpathCurve, processObject.Direction, processObject.Curve.OutsideSign, par, true);

            var toolpathCurves = AutocadUtils.Explode(curve, processObject.Direction == -1);

            var z0 = Settings.Thickness - par.DepthAll;
            var z = Settings.Thickness;
            var k = par.Depth/curve.GetLength();
            IEnumerator<Curve> enumerator = toolpathCurves.GetEnumerator();
            enumerator.MoveNext();
            do
            {
                if (!IsCalculation)
                    return;
                z -= k * enumerator.Current.GetLength();
                if (z - z0 < CalcUtils.Tolerance) z = z0;
                toolpathCurve = AutocadUtils.GetDisplacementCopy(enumerator.Current, z);
                Machine.Cutting(toolpathCurve);
                MoveCycle(enumerator);
            } while (z > z0);

            var startCurve = enumerator.Current;
            do
            {
                toolpathCurve = AutocadUtils.GetDisplacementCopy(enumerator.Current, z0);
                Machine.Cutting(toolpathCurve);
                MoveCycle(enumerator);
            } while (enumerator.Current != startCurve);
            enumerator.Dispose();

            Retract(toolpathCurve, processObject.Direction, processObject.Curve.OutsideSign, par);
        }

        private static readonly Action<IEnumerator> MoveCycle = e =>
        {
            if (e.MoveNext()) return;
            e.Reset();
            e.MoveNext();
        };

        private static void Feed(Curve toolpathCurve, int direction, int outsideSign, ProcessingParams par, bool isFirstPass)
        {
            var feedGroup = CalcUtils.CalcFeedGroup(toolpathCurve, direction == 1, outsideSign, par.FeedType, par.FeedRadius, par.FeedAngle, par.FeedLength);

            if (isFirstPass)
                Machine.SetPosition(feedGroup.Point, par.SmallSpeed);
            else
                Machine.Move(feedGroup.Point);

            if (Settings.WithCompensation)
                Machine.SetCompensation(direction * outsideSign == 1 ? CompensationSide.Left : CompensationSide.Right);

            Machine.EngageMove(feedGroup.Line);
            Machine.EngageMove(feedGroup.Arc);

            Machine.SetSpeed(par.GreatSpeed);
        }

        private static void Retract(Curve toolpathCurve, int direction, int outsideSign, ProcessingParams par)
        {
            if (par.RetractionType == FeedType.None)
                return;
            var feedGroup = CalcUtils.CalcFeedGroup(toolpathCurve, direction == -1, outsideSign, par.RetractionType, par.RetractionRadius, par.RetractionAngle, par.RetractionLength);
            Machine.RetractMove(feedGroup.Arc);
            if (Settings.WithCompensation)
                Machine.SetCompensation(CompensationSide.None);
            Machine.RetractMove(feedGroup.Line);
        }

        private static void CuttingProfileDisc(ProcessObject processObject, Curve curve, List<Curve> sectionCurves)
        {
            if (sectionCurves.Count == 0)
            {
                AutocadUtils.ShowError("Не указано сечение");
                return;
            }
            var par = processObject.ProcessingParams;
            var z = Settings.HeightMax;
            do
            {
                if (!IsCalculation)
                    return;
                var d1 = CalcUtils.GetSectionDepth(sectionCurves, z);
                var d2 = CalcUtils.GetSectionDepth(sectionCurves, z - processObject.Tool.Thickness.GetValueOrDefault());
                double d;
                if (d1.HasValue && d2.HasValue)
                    d = d1 > d2 ? d1.Value : d2.Value;
                else if (d1.HasValue)
                    d = d1.Value;
                else if (d2.HasValue)
                    d = d2.Value;
                else
                    return;

                var toolpathCurve = AutocadUtils.GetDisplacementCopy(curve, z);

                AlternatingCuttingCurve(processObject, toolpathCurve, d + Settings.Pripusk);

                z -= Settings.VerticalStep;
            } 
            while (z >= Settings.HeightMin);
        }

        private static void AlternatingCuttingCurve(ProcessObject processObject, Curve curve, double d)
        {
            var par = processObject.ProcessingParams;
            // s - текущее смещение от кривой контура детали curve
            // d - заданная величина смещения
            var s = par.Depth > 0 
                ? (par.DepthAll > d ? par.DepthAll : d) 
                : d;
            s -= par.Depth;
            if (s < d) s = d;
            var direction = processObject.Direction;
            var isFirstPass = true;
            do
            {
                if (!IsCalculation)
                    return;
                if (!isFirstPass)
                {
                    s -= par.Depth;
                    if (s < d) s = d;
                    direction = -direction;
                }

                Curve toolpathCurve = AutocadUtils.GetOffsetCopy(curve, processObject.Curve.OutsideSign * s);
                ObjectId toolObjectId = AutocadUtils.CreateToolCurve(s, toolpathCurve.StartPoint.Z, processObject.Tool.Thickness);

                Feed(toolpathCurve, direction, processObject.Curve.OutsideSign, par, isFirstPass);

                List<ProcessingAction> actions = new List<ProcessingAction>();
                if (processObject.Curve.Type == CurveType.Polyline || processObject.Curve.Type == CurveType.Circle)
                    actions = Machine.Cutting(AutocadUtils.Explode(toolpathCurve, direction == -1));
                else
                    actions.Add(Machine.Cutting(toolpathCurve));

                actions.ForEach(p => p.ToolObjectId = toolObjectId);

                Retract(toolpathCurve, direction, processObject.Curve.OutsideSign, par);
                
                isFirstPass = false;
            } 
            while (s > d);
        }

        public static bool IsCalculation { get; set; }

        internal static void Abort()
        {
            IsCalculation = false;
        }
    }
}