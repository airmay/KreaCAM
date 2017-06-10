using System.ComponentModel;
using System.Drawing;
using Autodesk.AutoCAD.Geometry;

namespace ProcessingProgram.ViewModels
{
    /// <summary>
    /// Базовый класс для редактирования обрабатываемого объекта
    /// </summary>
    [DefaultProperty("Название")]
    abstract public class ProcessObjectViewModel
    {
        public readonly ProcessObject ProcessObject;
        private PointF _pointF;

        protected ProcessObjectViewModel(ProcessObject processObject)
        {
            ProcessObject = processObject;
        }

        protected PointF ConvertToPointF(Point3d point3D)
        {
            _pointF.X = (float)point3D.X;
            _pointF.Y = (float)point3D.Y;
            return _pointF;
        }

        public override string ToString()
        {
            return ObjectName;
        }

/*        [Browsable(false)]
        public Curve ToolpathCurve
        {
            get
            {
                return toolpathCurve;
            }
            set
            {
                toolpathCurve = value;
                ToolpathLine = toolpathCurve as Line;
                ToolpathArc = toolpathCurve as Arc;
            }
        }*/
        [Category("1. Общие"), DisplayName("Название"), Description("Название объекта")]
        public string ObjectName { get; set; }

        //[CategoryAttribute("1. Общие"), DisplayName("Тип"), DescriptionAttribute("Тип объекта")]
        //public abstract string TypeName { get; protected set; }

        [Category("2. Геометрия объекта"), DisplayName("Точка начало"), Description("Начальная вершина")]
        public PointF StartPoint
        {
            get { return ConvertToPointF(ProcessObject.ProcessCurve.StartPoint); }
        }

        [Category("2. Геометрия объекта"), DisplayName("Точка конец"), Description("Конечная вершина")]
        public PointF EndPoint
        {
            get { return ConvertToPointF(ProcessObject.ProcessCurve.EndPoint); }
        }

        [Category("2. Геометрия объекта"), DisplayName("Длина"), Description("Длина объекта")]
        public abstract double Length { get; }

        [Category("3. Геометрия траектории"), DisplayName("Точка начало"), Description("Начальная вершина")]
        public PointF ToolpathStartPoint
        {
            get { return ProcessObject.ToolpathCurve != null ? ConvertToPointF(ProcessObject.ToolpathCurve.StartPoint) : PointF.Empty; }
        }

        [Category("3. Геометрия траектории"), DisplayName("Точка конец"), Description("Конечная вершина")]
        public PointF ToolpathEndPoint
        {
            get { return ProcessObject.ToolpathCurve != null ? ConvertToPointF(ProcessObject.ToolpathCurve.EndPoint) : PointF.Empty; }
        }

        [Category("4. Инструмент"), DisplayName("Номер"), Description("Номер используемого инструмента, мм")]
        public int ToolNo
        {
            get { return ProcessObject.Tool.No; }
        }
        
        [Category("4. Инструмент"), DisplayName("Наименование"), Description("Наименование используемого инструмента")]
        public string ToolName 
        { 
            get { return ProcessObject.Tool != null ? ProcessObject.Tool.Name : null; }
        }

        [Category("4. Инструмент"), DisplayName("Диаметр"), Description("Диаметр используемого инструмента, мм")]
        public double? Diameter
        {
            get { return ProcessObject.Tool != null ? ProcessObject.Tool.Diameter : null; }
        }

        [Category("4. Инструмент"), DisplayName("Позиция в магазине"), Description("Позиция инструмента в магазине")]
        public int Position
        {
            get { return ProcessObject.Tool.Position; }
        }
        
        [Category("4. Инструмент"), DisplayName("Кромка"), Description("Номер используемой крмки инструмента")]
        public int Kromka
        {
            get { return ProcessObject.Tool.Kromka; }
        }

/*        [CategoryAttribute("4. Инструмент"), DisplayName("Толщина"), DescriptionAttribute("Толщина используемого инструмента, мм")]
        public double Thickness
        {
            get { return ToolNo <= processOptions.ToolsList.Count ? processOptions.ToolsList[ToolNo - 1].Thickness : 0; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Скорость большая"), DescriptionAttribute("Скорость реза большая, мм/мин")]
        public int GreatSpeed 
        {
            get { return _processObject.ProcessingParams.GreatSpeed; }
            set { _processObject.ProcessingParams.GreatSpeed = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Скорость малая"), DescriptionAttribute("Скорость реза малая, мм/мин")]
        public int SmallSpeed
        {
            get { return _processObject.ProcessingParams.SmallSpeed; }
            set { _processObject.ProcessingParams.SmallSpeed = value; }
        }*/
        [Category("5. Параметры обработки"), DisplayName("Скорость подачи"), Description("Скорость подачи инструмента, мм/мин")]
        public int GreatSpeed 
        {
            get { return ProcessObject.Tool.WorkSpeed; }
            set { ProcessObject.Tool.WorkSpeed = value; }
        }

        [Category("5. Параметры обработки"), DisplayName("Скорость опускания"), Description("Скорость опускания инструмента, мм/мин")]
        public int SmallSpeed
        {
            get { return ProcessObject.Tool.DownSpeed; }
            set { ProcessObject.Tool.DownSpeed = value; }
        }

        [Category("5. Параметры обработки"), DisplayName("Шпиндель"), Description("Скорость вращения шпинделя, об/мин")]
        public int Frequency
        {
            get { return ProcessObject.Tool.Frequency; }
            set { ProcessObject.Tool.Frequency = value; }
        }

        [Category("5. Параметры обработки"), DisplayName("Отступ"), Description("Отступ от детали, мм")]
        public int DepthAll
        {
            get { return ProcessObject.ProcessingParams.DepthAll; }
            set { ProcessObject.ProcessingParams.DepthAll = value; }
        }

        [Category("5. Параметры обработки"), DisplayName("Шаг"), Description("Глубина реза за один проход, мм")]
        public int Depth
        {
            get { return ProcessObject.ProcessingParams.Depth; }
            set { ProcessObject.ProcessingParams.Depth = value; }
        }
        /*        
                [CategoryAttribute("5. Параметры обработки"), DisplayName("Глубина реза"), DescriptionAttribute("Суммарная глубина реза, мм")]
                public int DepthAll
                {
                    get { return _processObject.ProcessingParams.DepthAll; }
                    set { _processObject.ProcessingParams.DepthAll = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Глубина построчно"), DescriptionAttribute("Глубина реза за один проход, мм")]
                public int Depth
                {
                    get { return _processObject.ProcessingParams.Depth; }
                    set { _processObject.ProcessingParams.Depth = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Точно начало"), DescriptionAttribute("Крайняя точка реза соответствует начальной точке объекта")]
                public bool IsBeginExactly
                {
                    get { return IsExactly[VertexType.Start.Index()]; }
                    set { IsExactly[VertexType.Start.Index()] = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Точно конец"), DescriptionAttribute("Крайняя точка реза соответствует конечной точке объекта")]
                public bool IsEndExactly
                {
                    get { return IsExactly[VertexType.End.Index()]; }
                    set { IsExactly[VertexType.End.Index()] = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Медленно начало"), DescriptionAttribute("Движение инструмента к начальной точке с пониженной скоростью подачи")]
                public bool IsBeginSlowly
                {
                    get { return IsSlowly[VertexType.Start.Index()]; }
                    set { IsSlowly[VertexType.Start.Index()] = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Медленно конец"), DescriptionAttribute("Движение инструмента к конечной точке с пониженной скоростью подачи")]
                public bool IsEndSlowly
                {
                    get { return IsSlowly[VertexType.End.Index()]; }
                    set { IsSlowly[VertexType.End.Index()] = value; }
                }

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Скорость на концах"), DescriptionAttribute("Скорость реза на концах, мм/мин")]
                public int SafetySpeed
                {
                    get { return processOptions.SafetySpeed; }
                    set { processOptions.SafetySpeed = value; }
                }

                private bool isEngineOutside;

                [CategoryAttribute("5. Параметры обработки"), DisplayName("Двигатель снаружи"), DescriptionAttribute("Расположение двигателя с наружной стороны по отношению к детали")]
                public bool IsEngineOutside 
                {
                    get { return isEngineOutside; }
                    set
                    {
                        if (CanEditEngineOutside)
                            isEngineOutside = value;
                    }
                }

                public bool CanEditEngineOutside;*/
    }

}
