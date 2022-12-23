using System;

namespace Kumo.Routing.API
{
    [Flags]
    public enum eventType
    {
        onMatrixChanged = 1,
        onTemperatureChanged = 2,
        onTextChanged = 4,
        onColorChanged = 8,
        onLockedChanged = 16,
        ALL = 32
    }
    public enum portType
    {
        Source,
        Destination
    }

    public enum KumoAPIMode
    {
        KUMO,
        Simulator
    }
}
