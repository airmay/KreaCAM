using ProcessingProgram.Constants;

namespace ProcessingProgram.Objects
{
    /// <summary>
    /// Параметры обработки
    /// </summary>
    public class ProcessingParams
    {
        private static ProcessingParams _default;

        public int GreatSpeed { get; set; }
        public int SmallSpeed { get; set; }
        public double DepthAll { get; set; }
        public double Depth { get; set; }

        public FeedType FeedType { get; set; }
        public int FeedRadius { get; set; }
        public int FeedAngle { get; set; }
        public int FeedLength { get; set; }

        public FeedType RetractionType { get; set; }
        public int RetractionRadius { get; set; }
        public int RetractionAngle { get; set; }
        public int RetractionLength { get; set; }

        private ProcessingParams()
        {
        }

        public static ProcessingParams Create()
        {
            return _default.MemberwiseClone() as ProcessingParams;
        }

        public static ProcessingParams GetDefault()
        {
            return _default ?? (_default = new ProcessingParams
                                               {
                                                   GreatSpeed = Properties.Settings.Default.GreatSpeed,
                                                   SmallSpeed = Properties.Settings.Default.SmallSpeed,
                                                   DepthAll = Properties.Settings.Default.DepthAll,
                                                   Depth = Properties.Settings.Default.Depth,
                                                   FeedType = (FeedType)Properties.Settings.Default.FeedType,
                                                   FeedRadius = Properties.Settings.Default.FeedRadius,
                                                   FeedAngle = Properties.Settings.Default.FeedAngle,
                                                   FeedLength = Properties.Settings.Default.FeedLength,
                                                   RetractionType = (FeedType)Properties.Settings.Default.RetractionType,
                                                   RetractionRadius = Properties.Settings.Default.RetractionRadius,
                                                   RetractionAngle = Properties.Settings.Default.RetractionAngle,
                                                   RetractionLength = Properties.Settings.Default.RetractionLength
                                               });
        }

        public static void SaveDefault()
        {
            Properties.Settings.Default.GreatSpeed = _default.GreatSpeed;
            Properties.Settings.Default.SmallSpeed = _default.SmallSpeed;
            Properties.Settings.Default.DepthAll = _default.DepthAll;
            Properties.Settings.Default.Depth = _default.Depth;
            Properties.Settings.Default.GreatSpeed = _default.GreatSpeed;
            Properties.Settings.Default.GreatSpeed = _default.GreatSpeed;
            Properties.Settings.Default.FeedType = (int)_default.FeedType;
            Properties.Settings.Default.FeedRadius = _default.FeedRadius;
            Properties.Settings.Default.FeedAngle = _default.FeedAngle;
            Properties.Settings.Default.FeedLength = _default.FeedLength;
            Properties.Settings.Default.RetractionType = (int)_default.RetractionType;
            Properties.Settings.Default.RetractionRadius = _default.RetractionRadius;
            Properties.Settings.Default.RetractionAngle = _default.RetractionAngle;
            Properties.Settings.Default.RetractionLength = _default.RetractionLength;            
        }
    }
}
