using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    [Index ("Email", IsUnique = true)]
    [Index ("UserName", IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; }
        [StringLength (50)] 
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
        [StringLength(255)]
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
