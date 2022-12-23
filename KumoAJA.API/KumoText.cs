namespace Kumo.Routing.API
{
   public class KumoText
    {
        public portType pT;
        public int portNum;

        public string Line1Text
        {
            get => _line1Text;
            set
            {
                _line1Text = value;
                Line1Changed = true;
            }
        }

        public string Line2Text
        {
            get => _line2Text;
            set
            {
                _line2Text = value;
                Line2Changed = true;
            }
        }

        private string _line1Text;
        private string _line2Text;
        public bool Line1Changed { get; set; }
        public bool Line2Changed { get; set; }
    }
}
