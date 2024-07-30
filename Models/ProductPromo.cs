using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
 
    public class ProductPromo
    {
        public Guid Id { get; set; }
        public Guid PromoId { get; set; }
        [ForeignKey("PromoId")]
        public Promo? Promo { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
