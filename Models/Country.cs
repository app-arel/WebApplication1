using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Country
    {
        public Guid Id { get; set; }
        [StringLength (100)]
        public string Name { get; set; }
    }
}
