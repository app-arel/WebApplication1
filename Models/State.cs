using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class State
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [StringLength (15)]
        public string Zip { get; set; }
        public Guid CountryId { get; set; }
        [ForeignKey ("CountryId")]
        public Country? Country { get; set; }
    }
}
