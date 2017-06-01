using SBM.Common;

namespace SBM.Dummy
{
    public class Sample : Batch
    {
        private int? Index { get; set; }

        public override void Init()
        {
            Index = 0;
        }

        public override object Read()
        {
            return ++Index > 3 ? null : Index;
        }

        public override object Process(object source)
        {
            return source;
        }

        public override void Write(object @object)
        {
            //do nothing
        }

        public override void Destroy()
        {
            this.RESPONSE = "done";
        }
    }
}
