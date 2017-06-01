using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SBM.Model
{
    [Serializable]
    [Table("SBM_OBJ_POOL")]
    public partial class SBM_OBJ_POOL : SBM_TABLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID_DISPATCHER { get; set; }
        public short ID_SERVICE { get; set; }
        public string PID { get; set; }
        public DateTimeOffset STARTED { get; set; }
        public short? MAX_TIME_RUN { get; set; }

        [ForeignKey("ID_SERVICE")]
        public virtual SBM_SERVICE SBM_SERVICE { get; set; }
    }
}
