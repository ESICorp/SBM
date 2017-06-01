using System.Collections;
using System.Configuration;

namespace SBM.Transfer
{
    public class Config : ConfigurationSection
    {
        private static Config instance = null;

        public Config()
        {
        }

        internal static Source Source
        {
            get
            {
                if (instance == null)
                {
                    instance = (Config)ConfigurationManager.GetSection("TransferSection");
                }
                return instance.sourceElement;
            }
        }

        internal static Target Target
        {
            get
            {
                if (instance == null)
                {
                    instance = (Config)ConfigurationManager.GetSection("TransferSection");
                }
                return instance.targetElement;
            }
        }

        [ConfigurationProperty("Source", IsRequired = true)]
        public Source sourceElement
        {
            get
            {
                return (Source)this["Source"];
            }
            set
            {
                this["Source"] = value;
            }
        }

        [ConfigurationProperty("Target", IsRequired = true)]
        public Target targetElement
        {
            get
            {
                return (Target)this["Target"];
            }
            set
            {
                this["Target"] = value;
            }
        }

        internal static IEnumerator Items
        {
            get
            {
                if (instance == null)
                {
                    instance = (Config)ConfigurationManager.GetSection("Items");
                }

                return instance.itemsElement.GetEnumerator();
            }
        }

        [ConfigurationProperty("Items", IsRequired = true)]
        public Items itemsElement
        {
            get
            {
                return (Items)this["Items"];
            }
            set
            {
                this["Items"] = value;
            }
        }


        public static int Parallel
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["PARALLEL"]);
            }
        }

        public static int CommandTimeout
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["COMMAND_TIMEOUT"]);
            }
        }

        public static int BatchSize
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["BATCH_SIZE"]);
            }
        }
    }
}
