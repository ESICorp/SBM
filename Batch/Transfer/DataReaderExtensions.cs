using System;
using System.Collections.Generic;
using System.Data;

namespace SBM.Transfer
{
    public static class DataReaderExtensions
    {
        public static string GetSqlType(this IDataReader dataReader, string field)
        {
            return dataReader.GetSqlType(dataReader.GetOrdinal(field));
        }

        public static string GetSqlType(this IDataReader dataReader, int field)
        {
            int idxSize = dataReader.GetSchemaTable().Columns["ColumnSize"].Ordinal;
            //int idxDataType = dataReader.GetSchemaTable().Columns["DataType"].Ordinal;
            //int idxColumnName = dataReader.GetSchemaTable().Columns["ColumnName"].Ordinal;
            int idxNumericPrecision = dataReader.GetSchemaTable().Columns["NumericPrecision"].Ordinal;
            int idxNumericScale = dataReader.GetSchemaTable().Columns["NumericScale"].Ordinal;


            long size = Convert.ToInt64(dataReader.GetSchemaTable().Rows[field][idxSize]);
            //object dataType = dataReader.GetSchemaTable().Rows[field][idxDataType];
            //string columnName = (string)dataReader.GetSchemaTable().Rows[field][idxColumnName];
            long numericPrecision = Convert.ToInt64(dataReader.GetSchemaTable().Rows[field][idxNumericPrecision]);
            long numericScale = Convert.ToInt64(dataReader.GetSchemaTable().Rows[field][idxNumericScale]);

            var type = dataReader.GetFieldType(field);
            
            if (type == typeof(Boolean))
            {
                return ("bit");
            }
            else if (type == typeof(Byte))
            {
                return ("tinyint");
            }
            else if (type == typeof(Byte[]))
            {
                return ("varbinary(max)");
            }
            else if (type == typeof(Char))
            {
                return ("char");
            }
            else if (type == typeof(Char[]))
            {
                return string.Format("char({0})", size);
            }
            else if (type == typeof(DateTime))
            {
                return ("datetime");
            }
            else if (type == typeof(DateTimeOffset))
            {
                return ("datetimeoffset");
            }
            else if (type == typeof(Decimal))
            {
                return string.Format("decimal({0},{1})", numericPrecision, numericScale);
            }
            else if (type == typeof(Double))
            {
                return ("float");
            }
            else if (type == typeof(Int16))
            {
                return ("tinyint");
            }
            else if (type == typeof(Int32))
            {
                return ("smallint");
            }
            else if (type == typeof(Int64))
            {
                return ("int");
            }
            else if (type == typeof(Single))
            {
                return ("real");
            }
            //else if (type == typeof(String) && column.MaxLength > 0)
            //{
            //    return string.Format("varchar({0})", column.MaxLength);
            //}
            else if (type == typeof(String))
            {
                return size > 8000 ? "text" : string.Format("varchar({0})", size);
            }
            else
            {
                throw new NotImplementedException(type.ToString() + " not implemented on DataReaderExtensions");
            }
        }

        private static IDictionary<Type,DbType> typeMap = null;

        public static DbType GetSqlDbType(this IDataReader dataReader, string field)
        {
            return dataReader.GetSqlDbType(dataReader.GetOrdinal(field));
        }

        public static DbType GetSqlDbType(this IDataReader dataReader, int field)
        {
            var type = dataReader.GetFieldType(field);

            if (typeMap == null)
            {
                typeMap = new Dictionary<Type, DbType>();
                typeMap[typeof(byte)] = DbType.Byte;
                typeMap[typeof(sbyte)] = DbType.SByte;
                typeMap[typeof(short)] = DbType.Int16;
                typeMap[typeof(ushort)] = DbType.UInt16;
                typeMap[typeof(int)] = DbType.Int32;
                typeMap[typeof(uint)] = DbType.UInt32;
                typeMap[typeof(long)] = DbType.Int64;
                typeMap[typeof(ulong)] = DbType.UInt64;
                typeMap[typeof(float)] = DbType.Single;
                typeMap[typeof(double)] = DbType.Double;
                typeMap[typeof(decimal)] = DbType.Decimal;
                typeMap[typeof(bool)] = DbType.Boolean;
                typeMap[typeof(string)] = DbType.String;
                typeMap[typeof(char)] = DbType.StringFixedLength;
                typeMap[typeof(Guid)] = DbType.Guid;
                typeMap[typeof(DateTime)] = DbType.DateTime; //DbType.DateTimeOffset
                typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
                typeMap[typeof(byte[])] = DbType.Binary;
                typeMap[typeof(byte?)] = DbType.Byte;
                typeMap[typeof(sbyte?)] = DbType.SByte;
                typeMap[typeof(short?)] = DbType.Int16;
                typeMap[typeof(ushort?)] = DbType.UInt16;
                typeMap[typeof(int?)] = DbType.Int32;
                typeMap[typeof(uint?)] = DbType.UInt32;
                typeMap[typeof(long?)] = DbType.Int64;
                typeMap[typeof(ulong?)] = DbType.UInt64;
                typeMap[typeof(float?)] = DbType.Single;
                typeMap[typeof(double?)] = DbType.Double;
                typeMap[typeof(decimal?)] = DbType.Decimal;
                typeMap[typeof(bool?)] = DbType.Boolean;
                typeMap[typeof(char?)] = DbType.StringFixedLength;
                typeMap[typeof(Guid?)] = DbType.Guid;
                typeMap[typeof(DateTime?)] = DbType.DateTime;
                typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
                //typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
            }
            return typeMap[type];
        }
    }
}
