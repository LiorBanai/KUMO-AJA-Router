using System.Drawing;

namespace Kumo.Routing.API
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
        public string DisableColorName { get; set; }
        public string LoginPassword { get; set; }
        public bool ShowFormXCloseButton { get; set; }
        public string Title { get; set; }
        public int LocationOffsetFromTop { get; set; }
        public bool RedirectKeyboardKeyPresses { get; set; }
        public string Splitter { get; set; }
        public KumoSettings()
        {
            DisableColorName = nameof(Color.DimGray);
            Title = "Video Routing";
            LocationOffsetFromTop = 60;
            Splitter = "##";
        }

        public override string ToString()
        {
            return $"{nameof(KumoAPIMode)}: {KumoAPIMode}, {nameof(ConnectionSettings)}: {ConnectionSettings}, {nameof(UseColorsForButtonsText)}: {UseColorsForButtonsText}, {nameof(UseTransparentImages)}: {UseTransparentImages}, {nameof(FlatButtons)}: {FlatButtons}, {nameof(DrawConnectionLines)}: {DrawConnectionLines}, {nameof(ShowOKCloseButton)}: {ShowOKCloseButton}, {nameof(CircleSize)}: {CircleSize}, {nameof(DisableColorName)}: {DisableColorName}, {nameof(ShowFormXCloseButton)}: {ShowFormXCloseButton}, {nameof(Title)}: {Title}, {nameof(Title)}: {LocationOffsetFromTop}, {nameof(RedirectKeyboardKeyPresses)}: {RedirectKeyboardKeyPresses}, {nameof(Splitter)}: {Splitter}";
        }
    }
}
