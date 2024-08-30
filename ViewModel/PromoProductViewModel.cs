using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class PromoProductViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public Product Product { get; set; }
        public PromoViewModel Promo { get; set; }
    }
}
