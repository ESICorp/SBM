using System.Configuration;

namespace SBM.Transfer
{
    public class Target : ConfigurationElement
    {
        [ConfigurationProperty("Connection", IsRequired=true)]
        public string Connection
        {
            get
            {
                return (string)this["Connection"];
            }
            set
            {
                this["Connection"] = value;
            }
        }

        [ConfigurationProperty("SQLBefore", IsRequired = false)]
        public string SQLBefore
        {
            get
            {
                return (string)this["SQLBefore"];
            }
            set
            {
                this["SQLBefore"] = value;
            }
        }

        [ConfigurationProperty("SQLAfter", IsRequired = false)]
        public string SQLAfter
        {
            get
            {
                return (string)this["SQLAfter"];
            }
            set
            {
                this["SQLAfter"] = value;
            }
        }
    }
}
