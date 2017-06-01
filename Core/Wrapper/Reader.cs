using SBM.Model;
using System;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SBM.Wrapper
{
    public class Reader : Pipe, IDisposable
    {
        private Regex regex = new Regex("<<START>>.*?<<END>>");

        public string Text { get; private set; }

        public Reader() :
            base("to_wrapper", PipeDirection.In)
        {
        }

        public bool Next()
        {
            var buffer = new StringBuilder();

            try
            {
                byte[] read = new byte[1024];

                do
                {
                    //int len = pipe.EndRead(pipe.BeginRead(read, 0, 1024, null, null));
                    int len = pipes[0].Read(read, 0, 1024);

                    if (len < 1)
                    {
                        Thread.Sleep(Consts.ThresholdTimeout);
                    }
                    else
                    {
                        buffer.Append(UTF8Encoding.UTF8.GetString(read, 0, len));

                        Log.Debug("SBM.Wrapper [Reader.Next] Read (partial): " + UTF8Encoding.UTF8.GetString(read, 0, len));
                    }

                } while (!regex.IsMatch(buffer.ToString()));

                Log.Debug("SBM.Wrapper [Reader.Next] Read on channel dispatcher_to_wrapper : " + buffer.ToString());

                //this.Text = buffer
                //    .Replace("<<START>>", string.Empty)
                //    .Replace("<<END>>", string.Empty).ToString();

                string aux = buffer.ToString();
                int startIndex = aux.IndexOf("<<START>>") + 9;
                int endIndex = aux.IndexOf("<<END>>");

                this.Text = startIndex > endIndex ? string.Empty : aux.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception e)
            {
                Log.Write("SBM.Wrapper [Reader.Next] Coludn't read channel dispatcher_to_wrapper", e);
            }

            return this.Text != "<<SHUTDOWN>>";
        }

        public void Dispose()
        {
            base.Close();
        }
    }
}
