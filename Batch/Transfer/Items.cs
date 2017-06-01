using System.Configuration;

namespace SBM.Transfer
{
    [ConfigurationCollection(typeof(Item), AddItemName = "Item")]
    public class Items : ConfigurationElementCollection
    {
        public Item this[int index]
        {
            get
            {
                return base.BaseGet(index) as Item;
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

        public new Item this[string name]
        {
            get
            {
                return base.BaseGet(name) as Item;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Item();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Item)element).Name;
        }
    }
}
