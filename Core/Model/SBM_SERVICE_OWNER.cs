using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_SERVICE_OWNER")]
    public partial class SBM_SERVICE_OWNER : SBM_TABLE
    {
        [Key]
        [Column("ID_SERVICE", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ID_SERVICE { get; set; }

        [Key]
        [Column("ID_OWNER", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ID_OWNER { get; set; }

        [Column("SECURITY_LEVEL")]
        public byte SECURITY_LEVEL { get; set; }

        //[ForeignKey("ID_SERVICE")]
        //public virtual SBM_SERVICE SBM_SERVICE { get; set; }

        //[ForeignKey("ID_OWNER")]
        //public virtual SBM_OWNER SBM_OWNER { get; set; }
    }
}
