using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_EVENT")]
    public partial class SBM_EVENT : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte ID_EVENT { get; set; }
        public string DESCRIPTION { get; set; }

        //public virtual ICollection<SBM_EVENT_LOG> SBM_EVENT_LOG { get; set; } = new HashSet<SBM_EVENT_LOG>();
    }
}
