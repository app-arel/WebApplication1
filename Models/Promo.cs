using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Promo
    {
        public Guid Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public bool IsRunning { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
