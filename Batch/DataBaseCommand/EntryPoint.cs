using SBM.Common;
using System;
using System.Collections;
using System.Data.SqlClient;

namespace SBM.DataBaseCommand
{
    public class EntryPoint : Batch
    {
//#if DEBUG
//        public static int Main(string[] args)
//        {
//            string xml = string.Format("<Parameters><Source Type='{0}'><Provider>{1}</Provider><Command>{2}</Command></Source><Log>{3}</Log></Parameters>",
//                @"mssql",
//                @"Password=password;Persist Security Info=True;User ID=sa;Initial Catalog=dele;Data Source=WARFS01\INSTANCIA2",
//                @"insert into libro values('X','X',2016,1,'X'); delete from libro where titulo1='X';",
//                @"c:\temp\dbc_log.txt");

//            return 0;
//        }
//#endif

        private IEnumerator Sentences { get; set; }
        private int CountExecuted { get; set; }
        private int AcumRowsAffected { get; set; }

        private SqlConnection Connection { get; set; }

        public override void Init()
        {
            Parameter.Parse(base.PARAMETERS);

            TraceLog.Configure();

            this.Sentences = Parameter.Source.Command.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();

            this.Connection = new SqlConnection(Parameter.Source.Provider);
            this.Connection.Open();
        }

        public override object Read()
        {
            return this.Sentences.MoveNext() ? this.Sentences.Current : null;
        }

        public override void Write(object @object)
        {
            var sentence = @object as string;

            try
            {
                using (var command = new SqlCommand(sentence, this.Connection))
                {
                    this.AcumRowsAffected += command.ExecuteNonQuery();
                }

                this.CountExecuted++;
            }
            catch (Exception e)
            {
                TraceLog.AddError(string.Format("Couldn't execute {0}", sentence), e);
            }
        }

        public override void Destroy()
        {
            try
            {
                if (this.Connection != null)
                {
                    this.Connection.Dispose();
                }

                if (TraceLog.Count > 0)
                {
                    throw new Exception(string.Format("Executed {0}, rows affected {1} and {2} exceptions", this.CountExecuted, this.AcumRowsAffected, TraceLog.Count), TraceLog.Exceptions);
                }

                this.RESPONSE = string.Format("Executed {0} and rows affected {1}", this.CountExecuted, this.AcumRowsAffected);
            }
            finally
            {
                TraceLog.Dispose();
            }
        }
    }
}
