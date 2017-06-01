using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_REMOTING")]
    public partial class SBM_REMOTING : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_REMOTING { get; set; }
        public short ID_SERVICE { get; set; }
        public string TARGET_SERVER { get; set; }
        public string SERVER_ALIAS { get; set; }
        public short MAX_RESPONSE_TIME { get; set; }
        public bool ENABLED { get; set; }

        //public virtual ICollection<SBM_DONE> SBM_DONE { get; set; } = new HashSet<SBM_DONE>();

        //[ForeignKey("ID_SERVICE")]
        //public virtual SBM_SERVICE SBM_SERVICE { get; set; }
    }
}
