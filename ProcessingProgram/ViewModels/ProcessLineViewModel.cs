using System;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;

namespace ProcessingProgram.ViewModels
{
    public class ProcessLineViewModel : ProcessObjectViewModel
    {
        private readonly Line _processLine;
        private readonly Line _toolpathLine;

        private static int _no;

        public ProcessLineViewModel(ProcessObject processObject, string objectName)
            : base(processObject)
        {
            ObjectName = objectName ?? ("Отрезок" + ++_no);
            _processLine = new Line(); //processObject.ProcessCurve as Line;
            if (_processLine == null)
                throw new Exception("Ошибка приведения к линии");
            //_toolpathLine = processObject.ToolpathCurve as Line;
        }

        public override double Length
        {
            get { return _processLine.Length; }
        }

        [Category("2. Геометрия объекта"), DisplayName("Угол"), Description("Угол отрезка в градусах")]
        public double Angle
        {
            get { return Math.Round(_processLine.Angle*180/Math.PI, 3); }
        }

        [Category("3. Геометрия траектории"), DisplayName("Длина"), Description("Длина отрезка")]
        public double? ToolpathLength
        {
            get { return _toolpathLine != null ? (double?)_toolpathLine.Length : null; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Угол фрезы"), Description("Угол фрезы в градусах")]
        public double ToolpathAngle
        {
            get
            {
                return _toolpathLine != null ? Math.Round(((Math.PI * 2 - _toolpathLine.Angle) % Math.PI) * 180 / Math.PI, 3) : 0;
            }
        }
    }
}
