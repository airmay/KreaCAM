using System;
using ProcessingProgram.Constants;

namespace ProcessingProgram.Objects
{
    public class Settings
    {
        // TODO переделать Settings

        private static Settings _instance;

        public String MachineName { get; set; }
        public MachineKind Machine
        {
            get
            {
                return MachineName == "Denver" ? MachineKind.Denver : MachineKind.Krea;
            }
        }
        public String MachineIPAddress { get; set; }
        public ProcessMode ProcessMode { get; set; }

        public Double SafetyHeight { get; set; }
        public Double Thickness { get; set; }
        public int Frequency { get; set; }

        // параметры для обрабтки диском
        public double VerticalStep { get; set; }
        public double HeightMin { get; set; }
        public double HeightMax { get; set; }
        public double Pripusk { get; set; }

        public bool OneDirection { get; set; }
        public bool WithCompensation { get; set; }

        public Double SafetyZ
        {
            get { return Thickness + SafetyHeight; }
        }

        public static Settings GetInstance()
        {
            return _instance ?? (_instance = new Settings());
        }

        private Settings()
        {
            MachineName = Properties.Settings.Default.MachineName;
            MachineIPAddress = Properties.Settings.Default.MachineIPAddress;
            ProcessMode = (ProcessMode) Properties.Settings.Default.ProcessMode;
            SafetyHeight = Properties.Settings.Default.SafetyHeight;
            Thickness = Properties.Settings.Default.Thickness;
            Frequency = Properties.Settings.Default.Frequency;

            HeightMax = 10;
            VerticalStep = 5;
            Pripusk = 2;

            OneDirection = false;
            WithCompensation = true;
        }

        public static void Save()
        {
            Properties.Settings.Default.MachineName = _instance.MachineName;
            Properties.Settings.Default.ProcessMode = (int)_instance.ProcessMode;
            Properties.Settings.Default.MachineIPAddress = _instance.MachineIPAddress;
            Properties.Settings.Default.SafetyHeight = _instance.SafetyHeight;
            Properties.Settings.Default.Thickness = _instance.Thickness;
            Properties.Settings.Default.Frequency = _instance.Frequency;
            Properties.Settings.Default.Save();
        }
    }
}
