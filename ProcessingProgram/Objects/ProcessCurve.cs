using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ProcessingProgram.Constants;
using System;

namespace ProcessingProgram.Objects
{
    /// <summary>
    /// Обрабатываемая кривая
    /// </summary>
    public class ProcessCurve //: IEquatable<ProcessCurve>
    {
        /// <summary>
        /// Тип кривой
        /// </summary>
        public CurveType Type;

        public string TypeName { get; set; }

        public string Name { get; set; }

        public Point3d StartPoint { get; set; }

        public Point3d EndPoint { get; set; }

        public double Length { get; set; }

        /// <summary>
        /// Знак внешней стороны
        /// </summary>
        public int OutsideSign;

        /// <summary>
        /// Ид объекта автокада
        /// </summary>
        public readonly ObjectId ObjectId;

        public ProcessCurve(Curve curve, int no)
        {
            Name = TypeName + no;
            StartPoint = curve.StartPoint;
            EndPoint = curve.EndPoint;
            ObjectId = curve.ObjectId;

            var line = curve as Line;
            if (line != null)
            {
                Type = CurveType.Line;
                TypeName = "Отрезок";
                Length = line.Length;
            }
            var arc = curve as Arc;
            if (arc != null)
            {
                Type = CurveType.Arc;
                TypeName = "Дуга";
                Length = arc.Length;
            }
            var circle = curve as Circle;
            if (circle != null)
            {
                Type = CurveType.Circle;
                TypeName = "Окружность";
                Length = circle.Diameter * Math.PI;
            }
            var polyline = curve as Polyline;
            var polyline2d = curve as Polyline2d;
            if (polyline != null || polyline2d != null)
            {
                Type = CurveType.Polyline;
                TypeName = "Полилиния";
                Length = polyline != null ? polyline.Length : polyline2d.Length;
                if ((polyline != null && polyline.Closed) || (polyline2d != null && polyline2d.Closed))
                    EndPoint = StartPoint;
            }
        }

        public override string ToString()
        {
            return Name;
        }

/*        public bool Equals(ProcessCurve other)
        {
            return !Equals(other, null) && ObjectId == other.ObjectId;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return base.Equals(obj);

            if (!(obj is ProcessCurve))
                throw new InvalidCastException("The 'obj' argument is not a ProcessCurve object.");
            else
                return Equals(obj as ProcessCurve);
        }*/

        public override int GetHashCode()
        {
            return ObjectId.GetHashCode();
        }

/*        public static bool operator ==(ProcessCurve curve1, ProcessCurve curve2)
        {
            return !Equals(curve1, null) ? curve1.Equals(curve2) : Equals(curve2, null);
        }
        
        public static bool operator !=(ProcessCurve curve1, ProcessCurve curve2)
        {
            return !Equals(curve1, null) ? !curve1.Equals(curve2) : !Equals(curve2, null);
        }*/
    }
}
