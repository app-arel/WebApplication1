using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class BillingDetailsUserViewModel
    {
        public User User { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public Product Product { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public Category category { get; set; }
        public IEnumerable<Cart> Carts { get; set; }
        public Cart Cart { get; set; }
        public BillingDetailsViewModel Billing { get; set; }
    }
}
