using System;
using System.ComponentModel;
using System.Drawing;
using Autodesk.AutoCAD.DatabaseServices;

namespace ProcessingProgram.ViewModels
{
    public class ProcessArcViewModel : ProcessObjectViewModel
    {
        private readonly Arc _processArc;
        private readonly Arc _toolpathArc;

        private static int _no;

        public ProcessArcViewModel(ProcessObject processObject, string objectName)
            : base(processObject)
        {
            ObjectName = objectName ?? ("Дуга" + ++_no);
            _processArc = new Arc(); //processObject.ProcessCurve as Arc;
            if (_processArc == null)
                throw new Exception("Ошибка приведения к дуге");
            //_toolpathArc = processObject.ToolpathCurve as Arc;
        }

        public override double Length
        {
            get { return _processArc.Length; }
        }

        [Category("2. Геометрия объекта"), DisplayName("Точка центр"), Description("Центр дуги")]
        public PointF Center
        {
            get { return ConvertToPointF(_processArc.Center); }
        }

        [Category("2. Геометрия объекта"), DisplayName("Угол начало"),
         Description("Начальный угол дуги в градусах")]
        public double StartAngle
        {
            get { return Math.Round(_processArc.StartAngle*180/Math.PI, 3); }
        }

        [Category("2. Геометрия объекта"), DisplayName("Угол конец"),
         Description("Конечный угол дуги в градусах")]
        public double EndAngle
        {
            get { return Math.Round(_processArc.EndAngle*180/Math.PI, 3); }
        }

        [Category("2. Геометрия объекта"), DisplayName("Радиус"), Description("Радиус дуги")]
        public double Radius
        {
            get { return _processArc.Radius; }
        }

        [Category("2. Геометрия объекта"), DisplayName("Угол полный"), Description("Угол сектора в градусах")]
        public double TotalAngle
        {
            get { return Math.Round(_processArc.TotalAngle*180/Math.PI, 3); }
        }

        [Category("3. Геометрия траектории"), DisplayName("Точка центр"), Description("Центр дуги")]
        public PointF ToolpathCenter
        {
            get { return _toolpathArc != null ? ConvertToPointF(_toolpathArc.Center) : PointF.Empty; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Угол начало"),
         Description("Начальный угол дуги в градусах")]
        public double ToolpathStartAngle
        {
            get { return _toolpathArc != null ? Math.Round(_toolpathArc.StartAngle*180/Math.PI, 3) : 0; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Угол конец"),
         Description("Конечный угол дуги в градусах")]
        public double ToolpathEndAngle
        {
            get { return _toolpathArc != null ? Math.Round(_toolpathArc.EndAngle*180/Math.PI, 3) : 0; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Радиус"), Description("Радиус дуги")]
        public double ToolpathRadius
        {
            get { return _toolpathArc != null ? _toolpathArc.Radius : 0; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Длина"), Description("Длина дуги")]
        public double ToolpathLength
        {
            get { return _toolpathArc != null ? _toolpathArc.Length : 0; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Угол полный"), Description("Угол сектора в градусах")
        ]
        public double ToolpathTotalAngle
        {
            get { return _toolpathArc != null ? Math.Round(_toolpathArc.TotalAngle*180/Math.PI, 3) : 0; }
        }
    }
}
