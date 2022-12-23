using System.Collections.Generic;
using System.Linq;

namespace Kumo.Routing.API
{
    public class KumoEvent
    {
        public int temperatureValue;
        public Dictionary<int, List<int>> portMap;
        public IEnumerable<KumoText> textValues;
        public IEnumerable<KumoColor> colorValues;
        public IEnumerable<KumoLock> lockValue;

        public KumoEvent()
        {
            this.temperatureValue = -1;
            this.portMap = new Dictionary<int, List<int>>();
            this.textValues = Enumerable.Empty<KumoText>();
            this.colorValues = Enumerable.Empty<KumoColor>();
            this.lockValue = Enumerable.Empty<KumoLock>();
        }
        public KumoEvent(int tValue, Dictionary<int, List<int>> pM, List<KumoText> tV, List<KumoColor> kC, List<KumoLock> kL)
        {
            this.temperatureValue = tValue;
            this.portMap = pM;
            this.textValues = tV;
            this.colorValues = kC;
            this.lockValue = kL;
        }

        public static KumoEvent Empty { get; set; } = new KumoEvent();
    }
}
