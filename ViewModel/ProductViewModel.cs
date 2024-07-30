using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class ProductViewModel
    {
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public Decimal Price { get; set; }
        [StringLength(100)]
        public string ImageFile { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
        [StringLength(300)]
        public string Description { get; set; }
    }
}
