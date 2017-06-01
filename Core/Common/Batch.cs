using System;

namespace SBM.Common
{
    public abstract class Batch : MarshalByRefObject
    {
        public BatchContext Context { get; set; }

        public string PARAMETERS
        {
            get { return Context.PARAMETERS; }
        }

        public string RESPONSE
        {
            get { return Context.RESPONSE; }
            set { Context.RESPONSE = value; }
        }

        private bool _trick_read_ = false;

        public virtual void Init() { }

        public virtual object Read()
        {
            if (this._trick_read_)
            {
                return null;
            }
            else
            {
                this._trick_read_ = true;

                return new object();
            }
        }
         
        public virtual object Process(object source)
        {
            return source;
        }

        public virtual void Write(object @object) { }

        public virtual void Destroy() { }
    }
}
