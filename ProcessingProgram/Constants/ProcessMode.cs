using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingProgram.Constants
{
    public enum ProcessMode
    {
        /// <summary>
        /// В плоскости
        /// </summary>
        InPlane,

        /// <summary>
        /// Непрерывное опускание
        /// </summary>
        ContinuousDescent,

        /// <summary>
        /// Пошаговое опускание
        /// </summary>
        StepByStepDescent,

        /// <summary>
        /// Профиль диском
        /// </summary>
        ProfileDisc,
    }
}
