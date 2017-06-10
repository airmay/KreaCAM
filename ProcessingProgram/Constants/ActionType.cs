namespace ProcessingProgram.Constants
{
    /// <summary>
    /// Тип действия процесса обработки
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Начало программы
        /// </summary>
        StartOfProgram,

        /// <summary>
        /// Конец программы
        /// </summary>
        EndOfProgram,

        /// <summary>
        /// Начало пути
        /// </summary>
        StartOfPath,

        /// <summary>
        /// Конец пути
        /// </summary>
        EndOfPath,

        /// <summary>
        /// Смена инструмента
        /// </summary>
        ChangeTool,

        /// <summary>
        /// Плоскость обработки
        /// </summary>
        PlaneProcessing,

        /// <summary>
        /// Включение шпинделя
        /// </summary>
        SpindleStart,

        /// <summary>
        /// Выключение шпинделя
        /// </summary>
        SpindleStop,

        /// <summary>
        /// Включение воды
        /// </summary>
        WaterStart,

        /// <summary>
        /// Выключение воды
        /// </summary>
        WaterStop,

        /// <summary>
        /// Компенсация диаметра
        /// </summary>
        Compensation,

        /// <summary>
        /// Подвод к точке опускания
        /// </summary>
        InitialMove,

        /// <summary>
        /// Опускание инструмента
        /// </summary>
        AapproachMove,

        /// <summary>
        /// Подъем инструмента
        /// </summary>
        DepartureMove,

        /// <summary>
        /// Заход
        /// </summary>
        EngageMove,

        /// <summary>
        /// Выход
        /// </summary>
        RetractMove,

        /// <summary>
        /// Резка
        /// </summary>
        Cutting,

        /// <summary>
        /// Направавление реза
        /// </summary>
        Direction,

        /// <summary>
        /// Заглубление
        /// </summary>
        Penetration,

        /// <summary>
        /// Перемещение инструмента
        /// </summary>
        Move

    }
}
