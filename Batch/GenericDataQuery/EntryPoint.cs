using SBM.GenericDataQuery.Reader;
using SBM.GenericDataQuery.Writer;
using SBM.Common;
using System;

namespace SBM.GenericDataQuery
{
    public class EntryPoint : Batch
    {
#if FALSE
        public static int Main(string[] args)
        {
            string xml = string.Format("<Parameters><Source Type='{0}'{1}><Provider>{2}</Provider><Input>{3}</Input></Source><Target Type='{4}'><Provider>{5}</Provider><Output Append='false'>{6}</Output></Target></Parameters>",
                //
                // INPUT
                //

                //"mssql", 
                //"",
                //@"Password=password;User ID=sa;Initial Catalog=dele;Data Source=WARFS01\INSTANCIA2",
                //"select 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA' as titulo, 'BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB' as autor, 2016 as ano, 120.1 as precio, 'CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC' as categoria",
                //@"SELECT top 10 P.[Notes] AS ServiceNumber,SS.SaleServiceStatus,P.[Surname],P.[Name],P.[Sex],P.[PersonId],IDT.[Abbreviation],P.[IdentityNumber],ISNull(P.[Birthday], '') AS [Birthday],ISNull(P.[FiscalIdentityCode], '') AS [FiscalIdentityCode],PR.ProductName AS [PRD_Name],PR.ProductDescription AS [PRD_Desc],ISNull(SPR.ProductName, '') AS [Sub_PRD_Name],ISNull(SPR.ProductDescription, '') AS [Sub_PRD_Desc],SS.RENOVADO,SS.ALTA FROM [dbo].[Persons] AS P with(nolock) INNER JOIN [dbo].[SaleServices] AS SS with(nolock) ON SS.SaleServiceNumber=P.Notes INNER JOIN [dbo].[IdentityTypes] AS IDT with(nolock) ON IDT.IdentityTypeId=P.IdentityTypeId INNER JOIN [dbo].[Products] AS PR with(nolock) ON PR.ProductId = SS.ProductId LEFT JOIN [dbo].[Products] AS SPR with(nolock) ON SPR.ProductId=SS.SubProductId",
                //"SELECT RUN_INTERVAL, NEXT_TIME_RUN, [ENABLED], CRONTAB FROM DSP_SERVICE_TIMER",
                //"SELECT top 1000[tarjeta],[producto],[subprod],[nro_serv],[tiene_asist],[es_titular],[nombre],[paterno],[materno],[fec_nacim],[sexo],[tipo_doc],[nro_doc],[vig_dde],[vig_hta],[fec_viaje],[fec_alta],[hora_alta],[id_usuar],[obra_social],[plan_pag],[plan],[credencial],[importe],[estado],[cuota],[ult_debaut],[travel],[observ],[factura],[tipo_fac],[cliente],[fec_fact],[mot_com],[mot_can],[empresa],[amex_int],[welk_pack],[prox_planp],[renovado],[inout],[controlreg],[tipo_solic],[precio],[cambio],[cod_parent],[coef_iva],[f_vto_tar]FROM[dbo].[servicios]",
                //"select * from dele",

                //"TXT",
                //"",
                //@"c:\temp\tab.txt",
                //"{texto}{*numero1}{numero2}",

                //"TXT",
                //" Delimiter=';'",
                //@"c:\temp\csv.txt",
                //"{texto}{*numero1}{numero2}",

                "TXT",
                " Delimiter=';'",
                @"c:\temp\andres\final_road.csv",
                //"{numero:4}{texto:7}{*filler:1}",
                "{POLIZA}{NOMBRE}{VIGENCIA_DDE}{VIGENCIA_HTA}{DOCUMENTO}{DIRECCION}{CIUDAD}{PROVINCIA}{PAIS}{CPOSTAL}{TELEFONO1}{TELEFONO2}{TIPO}{CHAPA}{MARCA}{MODELO}{ANIO}{COLOR}{GRUPO}",

                //"xBase",
                //"",
                //@"C:\temp\Sicae\Sicae\sicaedb.dbc",
                //"SELECT * FROM Precios",

                //"XML",
                //"",
                //@"C:\temp\dele\sample",
                //"{author, autor}{title, titulo}{year, ano}{price, precio}{category, categoria}",

                //"xls",
                //"",
                //@"C:\temp\andres\SW033994.xml",
                //"{Nº Contrato,Contrato}{Apellido}{Nombre}{Cedula}{FN,FechaNacimiento}{Clase}{Plan}{Opcional}{FE,FechaEfectividadPoliza}{FR,FechaRenovacionPoliza}",

                //"XML",
                //"",
                //"",
                //@"<book category='children'><title>Harry Potter</title><author>J K. Rowling</author><year>2005</year><price>29.99</price></book><book category='web'><title>Learning XML</title><author>Erik T. Ray</author><year>2003</year><price>39.95</price></book>",

                //
                // OUTPUT
                //

                //"XML",
                //@"C:\temp\andres",
                //"sample[YYYY]-[MM]-[DD]_[###].xml"
                //"dele[#].xml"

                //"XML",
                //@"",
                //""

                //"DB",
                //@"Password=password;User ID=sa;Initial Catalog=dele;Data Source=WARFS01\INSTANCIA2",
                //"SW033994"

                //"TXT|;",
                //@"c:\temp\andres",
                //"dele.txt"

                "XLS",
                @"c:\temp\andres",
                "dele.xml"
                );

            //xml = @"<Parameters><Source Type='txt'><Provider>C:\Users\Andres.Castiglia\Downloads\test.txt</Provider><Input>{CODE:15}{NAME:22}{fechadd:8}{fechafin:8}</Input></Source><Target Type='mssql'><Provider>Password=AXAsql24191;Persist Security Info=True;User ID=sa;Initial Catalog=invoice;Data Source=10.146.137.106</Provider><Output>test</Output></Target></Parameters>";

            //xml = @"<Parameters><Source Type='mssql'><Provider>Password=AXA255SQL17vo!;Persist Security Info=True;User ID=sa;Initial Catalog=AXADATOS;Data Source=10.146.137.120</Provider><Input>SELECT * FROM VW_PORTALRP_ASISTENCIAS</Input></Source><Target Type='mssql'><Provider>Password=AXA255SQL17vo!;Persist Security Info=True;User ID=sa;Initial Catalog=RepositorioPortal;Data Source=10.146.137.120</Provider><Output>SIA_ASISTENCIAS</Output></Target></Parameters>";

            Console.WriteLine(new EntryPoint().Submit(xml));

            //<Provider>c:\temp\</Provider><Output>[ID_SERVICE].txt</Output>

            return 0;
        }
#endif

        private FactoryReader Reader { get; set; }
        private FactoryWriter Writer { get; set; }

        public override void Init()
        {
            Parameter.Parse(Context);

            TraceLog.Configure();

            this.Reader = FactoryReader.GetInstance();
            var headers = this.Reader.Header();

            this.Writer = FactoryWriter.GetInstance();
            this.Writer.Header(headers);
        }

        public override object Read()
        {
            return this.Reader.Next();
        }

        public override void Write(object @object)
        {
            try
            {
                var values = @object as string[];

                this.Writer.Write(values);
            }
            catch (Exception e)
            {
                TraceLog.AddError("Couldn't write", e);
            }
        }

        public override void Destroy()
        {
            try
            {
                if (TraceLog.Count > 0)
                {
                    throw new Exception(string.Format("Read {0}, written {1} and {2} exceptions", this.Reader.RowNum, this.Writer.RowNum, TraceLog.Count), TraceLog.Exceptions);
                }

                this.RESPONSE = this.Writer.GetResult() ?? string.Format("Read {0} and written {1}", this.Reader.RowNum, this.Writer.RowNum);
            }
            finally
            {
                if (this.Reader != null) this.Reader.Dispose();
                if (this.Writer != null) this.Writer.Dispose();

                TraceLog.Dispose();
            }
        }
    }
}
