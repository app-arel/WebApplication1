using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModel
{
    public class UserViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
