using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_EVENT_LOG")]
    public partial class SBM_EVENT_LOG : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_EVENT_LOG { get; set; }
        public byte ID_EVENT { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTimeOffset? TIME_STAMP { get; set; }

        [ForeignKey("ID_EVENT")]
        public virtual SBM_EVENT SBM_EVENT { get; set; }
    }
}
