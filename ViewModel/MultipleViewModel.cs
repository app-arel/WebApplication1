using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class MultipleViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
