using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_OWNER")]
    public partial class SBM_OWNER : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short ID_OWNER { get; set; }
        public string DESCRIPTION { get; set; }
        public string TOKEN { get; set; }
        public bool ENABLED { get; set; }

        //public virtual ICollection<SBM_DISPATCHER> SBM_DISPATCHER { get; set; } = new HashSet<SBM_DISPATCHER>();
        //public virtual ICollection<SBM_DONE> SBM_DONE { get; set; } = new HashSet<SBM_DONE>();
        //public virtual ICollection<SBM_SERVICE_OWNER> SBM_SERVICE_OWNER { get; set; } = new HashSet<SBM_SERVICE_OWNER>();
        //public virtual ICollection<SBM_SERVICE_TIMER> SBM_SERVICE_TIMER { get; set; } = new HashSet<SBM_SERVICE_TIMER>();

        public virtual ICollection<SBM_SERVICE> SBM_SERVICE { get; set; } = new HashSet<SBM_SERVICE>();
    }
}
