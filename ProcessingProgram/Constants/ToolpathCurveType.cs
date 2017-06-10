namespace ProcessingProgram.Constants
{
    /// <summary>
    /// Вид траектории обработки
    /// </summary>
    public enum ToolpathCurveType
    {
        /// <summary>
        /// Прямая
        /// </summary>
        Line,

        /// <summary>
        /// Дуга По часовой стрелке
        /// </summary>
        ArcClockwise,

        /// <summary>
        /// Дуга Против часовой стрелки
        /// </summary>
        ArcCounterclockwise
    }
}