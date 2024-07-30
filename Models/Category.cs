using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        [StringLength (100)]
        public string Name { get; set; }
        [StringLength (300)]
        public string Description { get; set; }
        public Guid? ParentId { get; set; }
        [ForeignKey ("ParentId")]
        public Category? Parent { get; set; }
    }
}
