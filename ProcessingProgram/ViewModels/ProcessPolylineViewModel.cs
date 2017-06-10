using System;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;

namespace ProcessingProgram.ViewModels
{
    class ProcessPolylineViewModel : ProcessObjectViewModel
    {
        private readonly Polyline _processPolyine;
        private readonly Polyline _toolpathPolyine;

        private static int _no;

        public ProcessPolylineViewModel(ProcessObject processObject, string objectName)
            : base(processObject)
        {
            ObjectName = objectName ?? ("Полилиния" + ++_no);
            _processPolyine = new Polyline(); //processObject.ProcessCurve as Polyline;
            if (_processPolyine == null)
                throw new Exception("Ошибка приведения к полилинии");
            //_toolpathPolyine = processObject.ToolpathCurve as Polyline;
        }

        public override double Length
        {
            get { return _processPolyine.Length; }
        }

        [Category("2. Геометрия объекта"), DisplayName("Количество вершин"), Description("Количество вершин полилинии")]
        public int VerticesCount
        {
            get { return _processPolyine.NumberOfVertices; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Длина"), Description("Длина полилинии")]
        public double? ToolpathLength
        {
            get { return _toolpathPolyine != null ? (double?)_toolpathPolyine.Length : null; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Количество вершин"), Description("Количество вершин полилинии")]
        public int? ToolpathVerticesCount
        {
            get { return  _toolpathPolyine != null ? (int?)_toolpathPolyine.NumberOfVertices : null; }
        }
    }
}

