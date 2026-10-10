using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppWeb
{
    public class Product
    {
        public long Id { get; set; }
        [Required]
        [StringLength(40)]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "money")]
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}