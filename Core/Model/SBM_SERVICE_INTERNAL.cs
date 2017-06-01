using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_SERVICE_INTERNAL")]
    public partial class SBM_SERVICE_INTERNAL : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short ID_SERVICE_INTERNAL { get; set; }
        public string DESCRIPTION { get; set; }
        public string VERSION { get; set; }
        public string ASSEMBLY_FILE { get; set; }
        public string CONFIG { get; set; }
        public short MAX_TIME_RUN { get; set; }
        public bool SINGLE_EXEC { get; set; }
        public bool IS_PUBLIC { get; set; }
        public bool ENABLED { get; set; }
    }
}