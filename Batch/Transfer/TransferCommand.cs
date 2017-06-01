namespace SBM.Transfer
{
    public partial class Transfer
    {
        private void Copy(ThreadParameter param)
        {
            param.BulkCopy.DestinationTableName = param.ItemConfig.TargetTable;

            if (param.ItemConfig.Mapping != null && param.ItemConfig.Mapping.Count > 0)
            {
                param.Step = param.ItemConfig.Name + ": mapping";

                param.BulkCopy.ColumnMappings.Clear();
                foreach (Field field in param.ItemConfig.Mapping)
                {
                    param.BulkCopy.ColumnMappings.Add(field.Source, field.Target);
                }
            }
            else
            {
                for (int i = 0; i < param.SourceDataReader.FieldCount; i++)
                {
                    param.BulkCopy.ColumnMappings.Add(
                        param.SourceDataReader.GetName(i).Trim(), 
                        param.SourceDataReader.GetName(i).Trim());
                }
            }

            param.Step = param.ItemConfig.Name + ": coping";

            Log.Write("SBM.Transfer [TransferCommand.Copy] " + param.Step);
            
            param.BulkCopy.WriteToServer(param.SourceDataReader);
        }
    }
}
