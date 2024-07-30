using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public Decimal Price { get; set; }
        [StringLength(100)]
        public string ImageFile { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
        [StringLength(300)]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
