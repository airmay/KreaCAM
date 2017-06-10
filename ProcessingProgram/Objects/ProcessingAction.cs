using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ProcessingProgram.Constants;

namespace ProcessingProgram.Objects
{
    /// <summary>
    /// Действие процесса обработки
    /// </summary>
    public class ProcessingAction
    {
        public int No { get; set; }
        public ObjectId ObjectId { get; set; } 

        /// <summary>
        /// Направление
        /// </summary>
        public ObjectId DirectObjectId { get; set; }

        /// <summary>
        /// Инструмент
        /// </summary>
        public ObjectId ToolObjectId { get; set; }

        public ActionType ActionType { get; set; }
        public String Param { get; set; }

        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string I { get; set; }
        public string J { get; set; }
        public string Irel { get; set; }
        public string Jrel { get; set; }

        public int? Speed { get; set; }
        public ToolpathCurveType? ToolpathCurveType;
        public CompensationSide CompensationSide;

        public String Note { get; set; }
        public Boolean IsError { get; set; }
    }
}
