using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ProcessingProgram.Objects
{
    /// <summary>
    /// Объект обработки
    /// </summary>
    public class ProcessObject
    {
        /// <summary>
        /// Обрабатываемая кривая
        /// </summary>
        public ProcessCurve Curve { get; private set; }

        /// <summary>
        /// Параметры обработки
        /// </summary>
        public ProcessingParams ProcessingParams { get; private set; }

        /// <summary>
        /// Инструмент
        /// </summary>
        public Tool Tool { get; private set; }

        /// <summary>
        /// Направление обработки
        /// </summary>
        public int Direction { get; set; }

        /// <summary>
        /// Начальная точка
        /// </summary>
        public Point3d ProcessStartPoint
        {
            get { return Direction == 1 ? Curve.StartPoint : Curve.EndPoint; }
        }

        /// <summary>
        /// Конечная точка
        /// </summary>
        public Point3d ProcessEndPoint
        {
            get { return Direction == 1 ? Curve.EndPoint : Curve.StartPoint; }
        }

        public ProcessObject(ProcessCurve curve, Tool tool)
        {
            Curve = curve;
            ProcessingParams = ProcessingParams.Create();        
            Tool = tool;
            Direction = 1;
        }
    }
}