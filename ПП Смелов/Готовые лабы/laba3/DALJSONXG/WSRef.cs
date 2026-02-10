using REPO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALJSONXG
{
    public class WSRef 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string Url { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; } = string.Empty;

        public int Plus { get; set; }
        public int Minus { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
