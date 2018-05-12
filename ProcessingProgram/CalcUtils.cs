using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;

namespace ProcessingProgram
{
    /// <summary>
    /// Расчетные методы
    /// </summary>
    public static class CalcUtils
    {
        public const double Tolerance = 2e-6;

        private const double Pi = Math.PI;

        /// <summary>
        /// 2 * PI
        /// </summary>
        private const double PiPi = Math.PI*2;

        /// <summary>
        /// PI / 2
        /// </summary>
        private const double Pi2 = Math.PI/2;

        public static int RadiansToDegrees(double radians)
        {
            return Convert.ToInt32(Math.Round(radians*180/Pi));
        }

        public static double DegreesToRadians(int degrees)
        {
            return degrees*Pi/180;
        }

        // richard durant, friends, dubstar
        private static List<ProcessObject> ProcessObjects { get; set; }
        private static List<ProcessCurve> ProcessCurves { get; set; }

        public static void Init(List<ProcessObject> processObjects, List<ProcessCurve> processCurves)
        {
            ProcessObjects = processObjects;
            ProcessCurves = processCurves;
        }

        #region Outside utils

        /// <summary>
        /// Расчет внешней стороны для цепочки объектов
        /// </summary>
        /// <param name="curve">Начальный объект</param>
        public static void CalcOutside(ProcessCurve curve)
        {
            if (curve.Type == CurveType.Circle)
            {
                curve.OutsideSign = -1;
                return;
            }
            var vector = AutocadUtils.GetFirstDerivative(curve.ObjectId, curve.StartPoint);
            // набор вершин
            var vertexSet = new HashSet<Point3d>(ProcessCurves.SelectMany(p => p.Type == CurveType.Polyline
                ? AutocadUtils.GetPoints(p.ObjectId)
                : new List<Point3d> {p.StartPoint, p.EndPoint}));

            var sign = -Math.Sign(vertexSet.Sum(p => Math.Sign(vector.CrossProduct(p - curve.StartPoint).Z)));
            curve.OutsideSign = sign != 0 ? sign : 1; //если не определили то берем 1

            if (!SetOutsideConnectObjects(curve, curve.EndPoint)) // если не замкнута
                SetOutsideConnectObjects(curve, curve.StartPoint); // то проставляем с другого конца
        }

        /// <summary>
        /// Установить внешнюю сторону связанных объектов
        /// </summary>
        /// <param name="firstCurve">Начальный объект</param>
        /// <param name="point">Точка связки с цепочкой</param>
        /// <returns>Признак замкнутой цепочки</returns>
        private static bool SetOutsideConnectObjects(ProcessCurve firstCurve, Point3d point)
        {
            var curve = firstCurve;
            while (true)
            {
                var connectCurve = ProcessCurves.FirstOrDefault(p => p.ObjectId != curve.ObjectId && (p.StartPoint == point || p.EndPoint == point));
                if (connectCurve == null || connectCurve == firstCurve)
                    return connectCurve == firstCurve; // если тру значит замкнута
                var direct = (curve.EndPoint == connectCurve.StartPoint || curve.StartPoint == connectCurve.EndPoint)
                    ? 1 // направление не меняется
                    : -1;
                //if (obj.ProcessCurve is Line ^ connectObject.ProcessCurve is Line) // если разные типы кривой
                //    k = -k;
                connectCurve.OutsideSign = direct*curve.OutsideSign;

                curve = connectCurve;
                point = point == curve.StartPoint ? curve.EndPoint : curve.StartPoint;
            }
        }

        /// <summary>
        /// Изменить сторону обработки объекта и связанных с ним
        /// </summary>
        /// <param name="objectId">ИД кривой</param>
        public static void ChangeOutside(ObjectId objectId)
        {
            var curve = ProcessCurves.Single(p => p.ObjectId == objectId);
            curve.OutsideSign = -curve.OutsideSign;
            if (!SetOutsideConnectObjects(curve, curve.EndPoint)) // если не замкнута
                SetOutsideConnectObjects(curve, curve.StartPoint); // то проставляем с другого конца
        }

        #endregion

        #region Feed

        public struct FeedGroup
        {
            public Line Line;
            public Arc Arc;
            public Point3d Point;
        }

        public static FeedGroup CalcFeedGroup(Curve curve, bool isStartCurve, int sign, FeedType feedType, int radius, int angle, int length)
        {
            var feedGroup = new FeedGroup();
            Vector3d vector;
            Point3d point = isStartCurve ? curve.StartPoint : curve.EndPoint;
            if (!curve.Closed || isStartCurve)
            {
                vector = curve.GetFirstDerivative(point);
            }
            else if (curve is Circle)
            {
                vector = new Vector3d(0, 1, 0);
            }
            else // расчет касательной в конце замкнутой полилинии
            {
                int param;
                var polyline = curve as Polyline;
                if (polyline != null)
                    param = polyline.NumberOfVertices - 1;
                else
                {
                    var polyline2d = curve as Polyline2d;
                    if (polyline2d != null)
                        param = polyline2d.Cast<object>().Count();
                    else
                    {
                        AutocadUtils.ShowError("Ошибка в расчете подвода-отвода");
                        feedGroup.Point = point;
                        return feedGroup;
                    }
                }
                vector = curve.GetFirstDerivative(param);
            }

            switch (feedType)
            {
                case FeedType.None:
                    feedGroup.Point = point;
                    break;
                case FeedType.Line:
                    vector = (isStartCurve ? -1 : 1) * vector.GetNormal() * length;
                    feedGroup.Point = point + vector;
                    feedGroup.Line = new Line(point, feedGroup.Point);
                    break;
                case FeedType.Arc:
                    vector = vector.GetPerpendicularVector() * sign * radius;
                    feedGroup.Point = point + vector;
                    vector = vector.Negate();
                    var angle1 = vector.GetAngleTo(Vector3d.XAxis, Vector3d.ZAxis.Negate());
                    var turnSign = (isStartCurve ? -1 : 1) * sign;
                    vector = vector.RotateBy(DegreesToRadians(angle) * turnSign, Vector3d.ZAxis);
                    feedGroup.Line = new Line(feedGroup.Point, feedGroup.Point + vector);
                    var angle2 = feedGroup.Line.Angle;
                    var isCrossAxis = Math.Abs(angle2 - angle1) > Pi;
                    feedGroup.Arc = new Arc(feedGroup.Point, radius,
                        isCrossAxis ? Math.Max(angle1, angle2) : Math.Min(angle1, angle2),
                        isCrossAxis ? Math.Min(angle1, angle2) : Math.Max(angle1, angle2));
                    break;
            }
            return feedGroup;
        }
        #endregion

        internal static double? GetSectionDepth(List<Curve> sectionCurves, double z)
        {
            Line line = new Line(new Point3d(0, z, 0), new Point3d(1, z, 0));
            Point3dCollection points = new Point3dCollection();
            foreach (Curve curve in sectionCurves)
            {
                curve.IntersectWith(line, Intersect.ExtendArgument, points, 0, 0);
                if (points.Count > 0)
                    return points[0].X;
            }
            return null;
        }
    }
}
