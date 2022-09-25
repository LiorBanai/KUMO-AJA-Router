using System.Drawing;

namespace KumoAJA.API
{
    public class KumoSettings
    {
        public KumoAPIMode KumoAPIMode { get; set; }
        public KumoConnectionSettings ConnectionSettings { get; set; }

        public bool UseColorsForButtonsText { get; set; }
        public bool UseTransparentImages { get; set; }
        public bool FlatButtons { get; set; }
        public bool DrawConnectionLines { get; set; }
        public bool ShowOKCloseButton { get; set; }
        public int CircleSize { get; set; } = 7;
        public string DisableColorName { get; set; } = nameof(Color.DimGray);
    }
}
