using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_SERVICE_TIMER")]
    public partial class SBM_SERVICE_TIMER : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_SERVICE_TIMER { get; set; }

        public short ID_SERVICE { get; set; }
        public short ID_OWNER { get; set; }
        public string ID_PRIVATE { get; set; }
        public string PARAMETERS { get; set; }
        public short RUN_INTERVAL { get; set; }
        public DateTimeOffset? LAST_TIME_RUN { get; set; }
        public DateTimeOffset? NEXT_TIME_RUN { get; set; }
        public bool ENABLED { get; set; }
        public string CRONTAB { get; set; }
        public string DESCRIPTION { get; set; }

        [ForeignKey("ID_OWNER")]
        public virtual SBM_OWNER SBM_OWNER { get; set; }

        [ForeignKey("ID_SERVICE")]
        public virtual SBM_SERVICE SBM_SERVICE { get; set; }
    }
}
