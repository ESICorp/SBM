using System.Configuration;

namespace SBM.Transfer
{
    public class Field : ConfigurationElement
    {
        [ConfigurationProperty("Source", IsRequired = true)]
        public string Source
        {
            get
            {
                return (string)this["Source"];
            }
            set
            {
                this["Source"] = value;
            }
        }

        [ConfigurationProperty("Target", IsRequired = true)]
        public string Target
        {
            get
            {
                return (string)this["Target"];
            }
            set
            {
                this["Target"] = value;
            }
        }

        [ConfigurationProperty("Type", IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)this["Type"];
            }
            set
            {
                this["Type"] = value;
            }
        }
    }
}
