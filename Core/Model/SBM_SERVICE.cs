using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_SERVICE")]
    public partial class SBM_SERVICE : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short ID_SERVICE { get; set; }
        public string DESCRIPTION { get; set; }
        public string VERSION { get; set; }
        public DateTimeOffset? PUBLISHED { get; set; }
        public short ID_SERVICE_TYPE { get; set; }
        public string ASSEMBLY_FILE { get; set; }
        public string ASSEMBLY_PATH { get; set; }
        public short MAX_TIME_RUN { get; set; }
        public bool SINGLE_EXEC { get; set; }
        public bool ENABLED { get; set; }
        public string DOMAIN { get; set; }
        public string USER { get; set; }
        public byte[] PASSWORD { get; set; }
        public bool x86 { get; set; }
        public short? ID_PARENT_SERVICE { get; set; }

        //public virtual ICollection<SBM_DISPATCHER> SBM_DISPATCHER { get; set; } = new HashSet<SBM_DISPATCHER>();
        //public virtual ICollection<SBM_DONE> SBM_DONE { get; set; } = new HashSet<SBM_DONE>();
        //public virtual ICollection<SBM_OBJ_POOL> SBM_OBJ_POOL { get; set; } = new HashSet<SBM_OBJ_POOL>();
        //public virtual ICollection<SBM_SERVICE_TIMER> SBM_SERVICE_TIMER { get; set; } = new HashSet<SBM_SERVICE_TIMER>();
        //public virtual ICollection<SBM_SERVICE_OWNER> SBM_SERVICE_OWNER { get; set; } = new HashSet<SBM_SERVICE_OWNER>();

        [ForeignKey("ID_SERVICE_TYPE")]
        public virtual SBM_SERVICE_TYPE SBM_SERVICE_TYPE { get; set; }

        [ForeignKey("ID_PARENT_SERVICE")]
        public virtual SBM_SERVICE SBM_SERVICE_PARENT { get; set; }

        public virtual ICollection<SBM_REMOTING> SBM_REMOTING { get; set; } = new HashSet<SBM_REMOTING>();

        public virtual ICollection<SBM_OWNER> SBM_OWNER { get; set; } = new HashSet<SBM_OWNER>();
    }
}
