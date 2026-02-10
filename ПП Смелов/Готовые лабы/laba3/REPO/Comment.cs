using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REPO
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(2000)")]
        public string CommText { get; set; } = string.Empty;

        public int WSRefId { get; set; }
        public DateTime? Stamp { get; set; }
        public WSRef WSRef { get; set; } = null!;
    }
}
