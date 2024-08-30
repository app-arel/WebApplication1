using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class UserCategoriesViewModel
    {
        public UserViewModel User { get; set; }
        public IEnumerable<Category> Categories { get; set; }

    }
}
