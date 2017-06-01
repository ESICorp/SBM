using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_DISPATCHER")]
    public partial class SBM_DISPATCHER : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_DISPATCHER { get; set; }
        public short ID_SERVICE { get; set; }
        public short ID_OWNER { get; set; }
        public string ID_PRIVATE { get; set; }
        public string PARAMETERS { get; set; }
        public DateTimeOffset? REQUESTED { get; set; }
        public Guid? HANDLE { get; set; }

        [ForeignKey("ID_OWNER")]
        public virtual SBM_OWNER SBM_OWNER { get; set; }

        [ForeignKey("ID_SERVICE")]
        public virtual SBM_SERVICE SBM_SERVICE { get; set; }
    }
}
