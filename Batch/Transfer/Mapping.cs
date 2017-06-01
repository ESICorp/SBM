using System.Configuration;

namespace SBM.Transfer
{
    [ConfigurationCollection(typeof(Field), AddItemName="Field")]
    public class Mapping : ConfigurationElementCollection
    {
        public Field this[int index]
        {
            get
            {
                return base.BaseGet(index) as Field;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new Field this[string source]
        {
            get
            {
                return base.BaseGet(source) as Field;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Field();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Field)element).Source;
        }
    }
}
