using System.Configuration;

namespace SBM.Transfer
{
    public class Item : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["Name"];
            }
            set
            {
                this["Name"] = value;
            }
        }

        [ConfigurationProperty("SourceSQL", IsRequired = true)]
        public string SourceSQL
        {
            get
            {
                return (string)this["SourceSQL"];
            }
            set
            {
                this["SourceSQL"] = value;
            }
        }

        [ConfigurationProperty("SourceUnique", IsRequired = false)]
        public string SourceUnique
        {
            get
            {
                return (string)this["SourceUnique"];
            }
            set
            {
                this["SourceUnique"] = value;
            }
        }

        [ConfigurationProperty("TargetTable", IsRequired = true)]
        public string TargetTable
        {
            get
            {
                return (string)this["TargetTable"];
            }
            set
            {
                this["TargetTable"] = value;
            }
        }

        [ConfigurationProperty("Truncate", IsRequired = false, DefaultValue = true)]
        public bool Truncate
        {
            get
            {
                return (bool)this["Truncate"];
            }
            set
            {
                this["Truncate"] = value;
            }
        }

        [ConfigurationProperty("Switch", IsRequired = false, DefaultValue = false)]
        public bool Switch
        {
            get
            {
                return (bool)this["Switch"];
            }
            set
            {
                this["Switch"] = value;
            }
        }

        [ConfigurationProperty("TargetAutoCreate", IsRequired = false, DefaultValue = false)]
        public bool TargetAutoCreate
        {
            get
            {
                return (bool)this["TargetAutoCreate"];
            }
            set
            {
                this["TargetAutoCreate"] = value;
            }
        }

        [ConfigurationProperty("TargetSQLBefore", IsRequired = false)]
        public string TargetSQLBefore
        {
            get
            {
                return (string)this["TargetSQLBefore"];
            }
            set
            {
                this["TargetSQLBefore"] = value;
            }
        }

        [ConfigurationProperty("TargetSQLAfter", IsRequired = false)]
        public string TargetSQLAfter
        {
            get
            {
                return (string)this["TargetSQLAfter"];
            }
            set
            {
                this["TargetSQLAfter"] = value;
            }
        }

        [ConfigurationProperty("SourceSQLBefore", IsRequired = false)]
        public string SourceSQLBefore
        {
            get
            {
                return (string)this["SourceSQLBefore"];
            }
            set
            {
                this["SourceSQLBefore"] = value;
            }
        }

        [ConfigurationProperty("SourceSQLAfter", IsRequired = false)]
        public string SourceSQLAfter
        {
            get
            {
                return (string)this["SourceSQLAfter"];
            }
            set
            {
                this["SourceSQLAfter"] = value;
            }
        }

        [ConfigurationProperty("Mapping", IsRequired = false)]
        public Mapping Mapping
        {
            get
            {
                return (Mapping)this["Mapping"];
            }
            set
            {
                this["Mapping"] = value;
            }
        }
    }

}
