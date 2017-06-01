using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_SERVICE_TYPE")]
    public partial class SBM_SERVICE_TYPE : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short ID_SERVICE_TYPE { get; set; }
        public string DESCRIPTION { get; set; }
        public string ALIAS_ID { get; set; }

        //public virtual ICollection<SBM_SERVICE> SBM_SERVICE { get; set; } = new HashSet<SBM_SERVICE>();
    }
}
