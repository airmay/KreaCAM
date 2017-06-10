using System;
using System.Collections.Generic;
using System.Linq;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    public static class ProgramGenerator
    {
        private static int _lineNo;
        private static readonly List<string> MachineProgram = new List<string>();
        private static Settings Settings { get; set; }
        private static ActionType _actionType;

        private static readonly Action<string> AddLine = line => MachineProgram.Add(String.Format("N{0}0 {1}", ++_lineNo, line));

        public static List<string> Generate(IEnumerable<ProcessingAction> actions)
        {
            Settings = Settings.GetInstance();
            MachineProgram.Clear();
            _lineNo = 0;

            foreach (var action in actions)
            {
                switch (action.ActionType)
                {
                    // Начало программы
                    case ActionType.StartOfProgram:
                        AddLine("; Start of Program \"" + Settings.MachineName + "\"");
                        AddLine(";");
                        AddLine("; PART NAME   :");
                        AddLine("; DATE TIME   : " + DateTime.Now);
                        AddLine(";");
                        if (Settings.Machine == MachineKind.Krea)
                        {
                            AddLine("(UAO,E30)");
                            AddLine("(UIO,Z(E31))");
                        }
                        if (Settings.Machine == MachineKind.Ravelli)
                        {
                            AddLine("G54G17G90G642");
                            AddLine("G0G90G153D0Z0");
                        }
                        break;

                    // Конец программы
                    case ActionType.EndOfProgram:
                        AddLine("; End of Program");
                        AddLine("M30");
                        break;

                    // Плоскость обработки
                    case ActionType.PlaneProcessing:
                        AddLine("G17");
                        break;

                    // Смена инструмента
                    case ActionType.ChangeTool:
                        AddLine("; Start of Path");
                        AddLine(";");
                        AddLine("; Tool Change");
                        var par = action.Param.Split(';');
                        AddLine("T" + (par.Any() ? par[0] : "?"));
                        if (Settings.Machine == MachineKind.Ravelli)
                        {
                            AddLine("M6");
                            AddLine("D" + (par.Count() > 1 ? par[1] : "?"));
                            AddLine("G54");
                            AddLine("G0 B0 C0");
                        }
                        break;

                    // Включение шпинделя
                    case ActionType.SpindleStart:
                        AddLine(String.Format("S{0} M3", action.Param));
                        AddLine("M7");
                        AddLine("M8");
                        break;

                    // Выключение шпинделя
                    case ActionType.SpindleStop:
                        AddLine("; End of Path");
                        AddLine("M5");              // выключение шпинделя                       
                        if (Settings.Machine == MachineKind.Krea)
                        {
                            AddLine("M9 M10");          // выключение воды
                            AddLine("G0 G79 Z(@ZUP)");  // подъем в верхнюю точку
                        }
                        if (Settings.Machine == MachineKind.Ravelli)
                        {
                            AddLine("M9");
                            AddLine("G0G90G153D0Z0");
                        }
                        break;

                    // Компенсация
                    case ActionType.Compensation:
                        AddLine(action.CompensationSide == CompensationSide.Left ? "G41" : (action.CompensationSide == CompensationSide.Right ? "G42" : "G40"));
                        break;

                    // Подвод к точке опускания, Перемещение инструмента, Опускание инструмента, Подъем инструмента
                    // Заход, Резка, Выход, Опускание инструмента, Подъем инструмента
                    case ActionType.InitialMove:
                    case ActionType.Move:
                    case ActionType.EngageMove:
                    case ActionType.Cutting:
                    case ActionType.RetractMove:
                    case ActionType.AapproachMove:
                    case ActionType.DepartureMove:
                        AddGCommand(action);
                        break;

                    default:
                        AutocadUtils.ShowError(String.Format("Ошибка при генерации программы: не распознана команда \"{0}\"", action.ActionType));
                        break;
                }
            }

            return MachineProgram;
        }

        private static void AddGCommand(ProcessingAction processingAction)
        {
            if (processingAction.ActionType != _actionType)
                AddLine("; " + processingAction.ActionType);
            _actionType = processingAction.ActionType;

            var line = processingAction.ToolpathCurveType == null
                ? processingAction.Param
                : processingAction.ToolpathCurveType == ToolpathCurveType.Line
                    ? "G1"
                    : (processingAction.ToolpathCurveType == ToolpathCurveType.ArcClockwise ? "G2" : "G3");
            if (processingAction.X != null)
                line += " X" + processingAction.X;
            if (processingAction.Y != null)
                line += " Y" + processingAction.Y;
            if (processingAction.Z != null)
                line += " Z" + processingAction.Z;
            if (processingAction.I != null)
                line += " I" + (Settings.Machine == MachineKind.Ravelli ? processingAction.Irel : processingAction.I);
            if (processingAction.J != null)
                line += " J" + (Settings.Machine == MachineKind.Ravelli ? processingAction.Jrel : processingAction.J);
            if (processingAction.Speed != null)
                line += " F" + processingAction.Speed;

            AddLine(line);
        }
    }
}
