using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class CompositeViewModel
    {
        public ProductViewModel Product { get; set; }
        public IEnumerable<Category> Category { get; set; }
    }
}
