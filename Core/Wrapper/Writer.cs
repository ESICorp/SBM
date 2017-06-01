using System;
using System.IO.Pipes;
using System.Text;

namespace SBM.Wrapper
{
    public class Writer : Pipe, IDisposable
    {

        public Writer() :
            base("from_wrapper_response", "from_wrapper_cancel", PipeDirection.Out)
        {
        }

        public void WriteChannelResponse(string value)
        {
            try
            {
                Log.Debug("SBM.Wrapper [Write.WriteChannelResponse] Write on channel dispatcher_from_wrapper_response : " + value);

                Write(value, 0);
            }
            catch (Exception e)
            {
                Log.Write("SBM.Wrapper [Writer.WriteChannelResponse] Couldn't write on channel dispatcher_from_wrapper_response", e);
            }
        }

        public void WriteChannelCancel(string value)
        {
            try
            {
                Log.Debug("SBM.Wrapper [Write.WriteChannelCancel] Write on channel dispatcher_from_wrapper_cancel : " + value);

                Write(value, 1);
            }
            catch (Exception e)
            {
                Log.Write("SBM.Wrapper [Writer.WriteChannelCancel] Couldn't write on channel dispatcher_from_wrapper_cancel", e);
            }
        }

        private void Write(string value, int channel)
        {
            byte[] aux = UTF8Encoding.UTF8.GetBytes("<<START>>");
            pipes[channel].Write(aux, 0, aux.Length);

            aux = UTF8Encoding.UTF8.GetBytes(value);
            pipes[channel].Write(aux, 0, aux.Length);

            aux = UTF8Encoding.UTF8.GetBytes("<<END>>");
            pipes[channel].Write(aux, 0, aux.Length);
        }

        public void Dispose()
        {
            base.Close();
        }
    }
}
