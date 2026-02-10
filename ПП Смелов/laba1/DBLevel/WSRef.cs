using System.ComponentModel.DataAnnotations;

namespace DBLevel
{
    public class WSRef
    {
        [Key]
        public int? Id { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public int? Plus { get; set; }
        public int? Minus { get; set; }
        List<Comment>? Comments { get; set; }
    }
}
