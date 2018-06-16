using System;
using System.Collections.Generic;
using System.Data;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    /// <summary>
    /// Методы генерации действий станка
    /// </summary>
    public class Machine //: IMachine
    {
        private static Settings Settings { get; set; }
        private readonly List<ProcessingAction> _processingActions;

        public event EventHandler<EventArgs<int>> ChangeActionsCount;

        private Tool _tool;
        private int? _speed;
        private bool _isSpeedChanged;
        private Point3d _position;
        private CompensationSide _compensation;
        private Boolean _isSpindleStarted;

        /// <summary>
        /// Плоскость обработки
        /// </summary>
        private String _planeProcessing;

        public Machine()
        {
            Settings = Settings.GetInstance();
            _processingActions = new List<ProcessingAction>();
        }

        #region IMachine implementation

        public List<ProcessingAction> GetProcessingActions()
        {
            return _processingActions;
        }

        private void OnChangeActionsCount(int count)
        {
            if (ChangeActionsCount != null)
                ChangeActionsCount(null, new EventArgs<int>(count));
        }

        public void Start()
        {
            _processingActions.Clear();
            _compensation = CompensationSide.None;
            _isSpindleStarted = false;
            _tool = null;

            CreateProcessAction(ActionType.StartOfProgram);
        }

        public void Stop()
        {
            DepartureMove();
            CreateProcessAction(ActionType.SpindleStop);
            CreateProcessAction(ActionType.EndOfProgram);
        }

        public void Clear()
        {
            _processingActions.Clear();
        }

        /// <summary>
        /// Установить компенсацию
        /// </summary>
        /// <param name="compensation"></param>
        public void SetCompensation(CompensationSide compensation)
        {
            if (compensation == _compensation || !Settings.WithCompensation)
                return;
            _compensation = compensation;
            var action = CreateProcessAction(ActionType.Compensation, compensation.ToString());
            action.CompensationSide = compensation;
        }

        public void EngageMove(Curve path, int? speed)
        {
            PathMovement(ActionType.EngageMove, path, speed);
        }

        public ProcessingAction Cutting(Curve path, int? speed)
        {
            return PathMovement(ActionType.Cutting, path, speed);
        }

        public List<ProcessingAction> Cutting(List<Curve> pathsList, int? speed)
        {
            return pathsList.ConvertAll(p => PathMovement(ActionType.Cutting, p, speed));
        }

        public void RetractMove(Curve path, int? speed)
        {
            PathMovement(ActionType.RetractMove, path, speed);
        }

        /// <summary>
        /// Установить параметры обработки элемента
        /// </summary>
        /// <param name="tool">Инструмент</param>
        public void SetTool(Tool tool)
        {
            if (tool == _tool)
                return;

            if (_isSpindleStarted)
            {
                DepartureMove();
                CreateProcessAction(ActionType.SpindleStop);
                _isSpindleStarted = false;
            }
            _tool = tool;
            CreateProcessAction(ActionType.ChangeTool, _tool.No.ToString());
            if (_planeProcessing == null)
            {
                _planeProcessing = "XY";
                CreateProcessAction(ActionType.PlaneProcessing, _planeProcessing);
            }
        }

        /// <summary>
        /// Установить подачу
        /// </summary>
        /// <param name="speed">Скорость подачи</param>
        public void SetSpeed(int speed)
        {
            if (speed == _speed) return;
            _speed = speed;
            _isSpeedChanged = true;
        }

        /// <summary>
        /// Установить инструмент в позицию
        /// </summary>
        /// <param name="position">Позиция</param>
        public void SetPosition(Point3d position, int speed)
        {
            if (_isSpindleStarted)
            {
                if (_position == position)
                    return;
                DepartureMove();
                ChangePosition(ActionType.Move, "G0", position.X, position.Y);
            }
            else
            {
                ChangePosition(ActionType.Move, "G0", position.X, position.Y);
                //CreateProcessAction(ActionType.InitialMove, x: position.X, y: position.Y, speed: speed);
                CreateProcessAction(ActionType.SpindleStart, Settings.Frequency.ToString());
                _isSpindleStarted = true;

                CreateProcessAction(ActionType.AapproachMove, "G0", z: Settings.SafetyZ);
                _position = new Point3d(position.X, position.Y, Settings.SafetyZ);
            }
            SetSpeed(speed);
            ChangePosition(ActionType.AapproachMove, "G1", z: position.Z, speed: speed);
        }

        public void Move(Point3d position, int? speed)
        {
            ChangePosition(ActionType.Move, "G1", position.X, position.Y, position.Z, speed: speed);
        }

        #endregion

        /// <summary>
        /// Подъем инструмента
        /// </summary>
        private void DepartureMove()
        {
            if (_position.Z >= Settings.SafetyZ)
                return;
            ChangePosition(ActionType.DepartureMove, "G1", z: Settings.SafetyZ, speed: ProcessingParams.GetDefault().GreatSpeed);
            SetCompensation(CompensationSide.None);
            CreateProcessAction(ActionType.DepartureMove, "G0", z: Settings.SafetyZ);
        }

        private void ChangePosition(ActionType actionType, string param = null, double? x = null, double? y = null, double? z = null, int? speed = null)
        {
            var action = CreateProcessAction(actionType, param, speed: speed);           
            var oldPosition = _position;
            _position = new Point3d(x ?? _position.X, y ?? _position.Y, z ?? _position.Z);
            var line = new Line(oldPosition, _position);
            action.ObjectId = AutocadUtils.AddCurve(line, actionType);
            action.DirectObjectId = SetDirectLine(line, x == null && y == null);
            SetCoordinates(action, oldPosition, _position);
        }

        /// <summary>
        /// Движение инструмента по траектории
        /// </summary>
        /// <param name="actionType">Тип действия</param>
        /// <param name="path">Траектория</param>
        private ProcessingAction PathMovement(ActionType actionType, Curve path, int? speed = null)
        {
            if (path == null)
                return null;

            var oldPosition = _position;
            if ((_position == path.StartPoint) || (Math.Abs(_position.X - path.StartPoint.X) < CalcUtils.Tolerance) && (Math.Abs(_position.Y - path.StartPoint.Y) < CalcUtils.Tolerance))
                _position = path.EndPoint;
            else if ((_position == path.EndPoint) || (Math.Abs(_position.X - path.EndPoint.X) < CalcUtils.Tolerance) && (Math.Abs(_position.Y - path.EndPoint.Y) < CalcUtils.Tolerance))
                _position = path.StartPoint;
            else
            {
                AutocadUtils.ShowError("Ошибка: не соответствие позиции при расчете траектории");
                return null;
            }

            var action = CreateProcessAction(actionType, speed: speed);
            action.ObjectId = path.ObjectId != ObjectId.Null ? path.ObjectId : AutocadUtils.AddCurve(path, actionType);
            action.DirectObjectId = SetDirectLine(path);
            SetCoordinates(action, oldPosition, _position, path is Arc);

            if (path is Line)
            {
                action.ToolpathCurveType = ToolpathCurveType.Line;
            }
            if (path is Arc)
            {
                var arc = path as Arc;
                action.ToolpathCurveType = (_position == arc.StartPoint
                                                              ? ToolpathCurveType.ArcClockwise
                                                              : ToolpathCurveType.ArcCounterclockwise);
                if (action.ToolpathCurveType == ToolpathCurveType.ArcClockwise ^ _compensation == CompensationSide.Left) // инструмент внутри дуги
                {
                    if (arc.Radius <= _tool.Diameter/2)
                    {
                        action.Note = "Радиус дуги меньше или равен радиусу инструмента";
                        action.IsError = true;
                        var message = String.Format("Строка {0}: {1}", action.No, action.Note);
                        AutocadUtils.ShowError(message);
                    }
                    if (arc.Radius <= 1.5*_tool.Diameter)
                    {
                        action.Speed = 200;
                        _isSpeedChanged = true;
                    }
                }
                action.I = Round(arc.Center.X);
                action.J = Round(arc.Center.Y);
                var vector = oldPosition.GetVectorTo(arc.Center);
                action.Irel = Round(vector.X);
                action.Jrel = Round(vector.Y);
            }
            action.Param = action.ToolpathCurveType.ToString();

            return action;
        }

        private static void SetCoordinates(ProcessingAction action, Point3d startPosition, Point3d endPosition, bool flag = false)
        {
            if (Math.Abs(startPosition.X - endPosition.X) > CalcUtils.Tolerance || flag)
                action.X = Round(endPosition.X);
            if (Math.Abs(startPosition.Y - endPosition.Y) > CalcUtils.Tolerance || flag)
                action.Y = Round(endPosition.Y);
            if (Math.Abs(startPosition.Z - endPosition.Z) > CalcUtils.Tolerance)
                action.Z = Round(endPosition.Z);
        }

        private static readonly Func<double?, string> Round = v => v != null ? v.Value.ToString("0.#####") : null;

        /// <summary>
        /// Создать действие процесса обработки
        /// </summary>
        /// <param name="actionType">Действие</param>
        /// <param name="param">Параметр</param>
        private ProcessingAction CreateProcessAction(ActionType actionType, string param = null, double? x = null, double? y = null, double? z = null, int? speed = null)
        {
            var action = new ProcessingAction
            {
                No = _processingActions.Count + 1,
                ActionType = actionType,
                Param = param,
                Speed = speed, //_speed, //_isSpeedChanged ? _speed : null, 
                X = Round(x),
                Y = Round(y),
                Z = Round(z)
            };
            _processingActions.Add(action);
            if (_processingActions.Count % 100 == 0)
                OnChangeActionsCount(_processingActions.Count);
            _isSpeedChanged = false;
            return action;

        }

        /// <summary>
        /// Установить стрелку направления обработки
        /// </summary>
        /// <param name="curve">Траектория обработки</param>
        /// <param name="isVertical">Вертикальная прямая</param>
        private ObjectId SetDirectLine(Curve curve, bool isVertical = false)
        {
            var curveLength = curve.GetLength();
            if (curveLength < 10)
                return ObjectId.Null;
            var dist = Math.Min(50, curveLength/2);
            if (_position == curve.StartPoint)
                dist = curveLength - dist;
            var points = new Point3dCollection {curve.GetPointAtDist(dist)};
            var arrowLength =  Math.Min(10, curveLength/3);
            var vector = curve.GetFirstDerivative(points[0]).GetNormal() * arrowLength;
            if (_position == curve.EndPoint)
                vector = vector.Negate();
            var sidn = _compensation == CompensationSide.Left ? -1 : 1;
            const double angle = 0.4;
            var axis = isVertical ? Vector3d.YAxis : Vector3d.ZAxis;
            points.Add(points[0] + vector.RotateBy(angle * sidn, axis));
            points.Add(_compensation == CompensationSide.None
                    ? points[0] + vector.RotateBy(-angle * sidn, axis) 
                    : curve.GetClosestPointTo(points[1], false));
            var directCurve = new Polyline3d(Poly3dType.SimplePoly, points, true);

            return AutocadUtils.AddCurve(directCurve, ActionType.Direction);
        }
    }
}