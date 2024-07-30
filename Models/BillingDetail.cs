using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class BillingDetail
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [StringLength(150)]
        public string CompanyName { get; set; }
        [StringLength(30)]
        public string Phone { get; set; }
        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(50)]
        public string City { get; set; }
        public Guid StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }
        public bool IsActive { get; set; }

    }
}
