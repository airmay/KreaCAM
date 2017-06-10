namespace ProcessingProgram.Constants
{
    /// <summary>
    /// Тип кривой
    /// </summary>
    public enum CurveType
    {
        /// <summary>
        /// Не определен
        /// </summary>
        None,

        /// <summary>
        /// Прямая
        /// </summary>
        Line,

        /// <summary>
        /// Дуга
        /// </summary>
        Arc,

        /// <summary>
        /// Полилиния
        /// </summary>
        Polyline,

        /// <summary>
        /// Окружность
        /// </summary>
        Circle
    };
}