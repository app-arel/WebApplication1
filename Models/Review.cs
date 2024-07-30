using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Index("Email", IsUnique = true)]
    public class Review
    {
        public Guid Id { get; set; }
        [StringLength(100)]
        public string Name { get; set;}
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(300)]
        public string Content { get; set; }
        public int Rating { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
