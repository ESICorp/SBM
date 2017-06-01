using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_DONE_STATUS")]
    public partial class SBM_DONE_STATUS : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte ID_DONE_STATUS { get; set; }
        public string DESCRIPTION { get; set; }
        public string ALIAS_ID { get; set; }

        //public virtual ICollection<SBM_DONE> SBM_DONE { get; set; } = new HashSet<SBM_DONE>();
    }
}
