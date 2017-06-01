using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_DONE")]
    public partial class SBM_DONE : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID_DISPATCHER { get; set; }
        public short ID_SERVICE { get; set; }
        public short ID_OWNER { get; set; }
        public string ID_PRIVATE { get; set; }
        public string PARAMETERS { get; set; }
        public DateTimeOffset? REQUESTED { get; set; }
        public DateTimeOffset? STARTED { get; set; }
        public DateTimeOffset? ENDED { get; set; }
        public byte ID_DONE_STATUS { get; set; }
        public string RESULT { get; set; }
        public int ID_REMOTING { get; set; }
        public Guid? HANDLE { get; set; }

        [ForeignKey("ID_DONE_STATUS")]
        public virtual SBM_DONE_STATUS SBM_DONE_STATUS { get; set; }

        [ForeignKey("ID_OWNER")]
        public virtual SBM_OWNER SBM_OWNER { get; set; }

        [ForeignKey("ID_SERVICE")]
        public virtual SBM_SERVICE SBM_SERVICE { get; set; }

        [ForeignKey("ID_REMOTING")]
        public virtual SBM_REMOTING SBM_REMOTING { get; set; }
    }
}
