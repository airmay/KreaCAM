using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ProcessingProgram.Constants;
using ProcessingProgram.Objects;
using System;

namespace ProcessingProgram
{
    /// <summary>
    /// Интерфейс класса содержащего методы генерации действий станка
    /// </summary>
    public interface IMachine
    {
        event EventHandler<EventArgs<int>> ChangeActionsCount;

        /// <summary>
        /// Получить последовательность действий станка
        /// </summary>
        /// <returns>Последовательность действий</returns>
        List<ProcessingAction> GetProcessingActions();

        /// <summary>
        /// Пуск станка
        /// </summary>
        void Start();

        /// <summary>
        /// Останов станка
        /// </summary>
        void Stop();

        /// <summary>
        /// Удалить обработку
        /// </summary>
        void Clear();

        /// <summary>
        /// Установить компенсацию
        /// </summary>
        /// <param name="compensation">Сторона компенсации</param>
        void SetCompensation(CompensationSide compensation);

        /// <summary>
        /// Обработка 
        /// </summary>
        /// <param name="path">Траектория обработки</param>
        ProcessingAction Cutting(Curve path);

        /// <summary>
        /// Обработка 
        /// </summary>
        /// <param name="pathsList">Списое траекторий обработки</param>
        List<ProcessingAction> Cutting(List<Curve> pathsList);

        /// <summary>
        /// Подвод
        /// </summary>
        /// <param name="path">Траектория подвода</param>
        void EngageMove(Curve path);

        /// <summary>
        /// Отвод
        /// </summary>
        /// <param name="path">Траектория отвода</param>
        void RetractMove(Curve path);

        /// <summary>
        /// Переместить инструмент
        /// </summary>
        /// <param name="position">Позиция</param>
        void Move(Point3d position);

        /// <summary>
        /// Установить инструмент
        /// </summary>
        /// <param name="tool">Инструмент</param>
        void SetTool(Tool tool);

        /// <summary>
        /// Установить подачу
        /// </summary>
        /// <param name="speed">Скорость подачи</param>
        void SetSpeed(int speed);

        /// <summary>
        /// Установить инструмент в позицию
        /// </summary>
        /// <param name="position">Позиция</param>
        /// <param name="speed">Подача</param>
        void SetPosition(Point3d position, int speed);
    }
}