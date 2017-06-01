using System.Configuration;

namespace SBM.Transfer
{
    public class Source : ConfigurationElement
    {
        [ConfigurationProperty("Connection", IsRequired = true)]
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

        [ConfigurationProperty("NetDomain", IsRequired = false)]
        public string NetDomain
        {
            get
            {
                return (string)this["NetDomain"];
            }
            set
            {
                this["NetDomain"] = value;
            }
        }

        [ConfigurationProperty("NetUser", IsRequired = false)]
        public string NetUser
        {
            get
            {
                return (string)this["NetUser"];
            }
            set
            {
                this["NetUser"] = value;
            }
        }

        [ConfigurationProperty("NetPassword", IsRequired = false)]
        public string NetPassword
        {
            get
            {
                return (string)this["NetPassword"];
            }
            set
            {
                this["NetPassword"] = value;
            }
        }
    }
}
