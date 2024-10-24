using Microsoft.AspNetCore.Identity;

namespace ETickets.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
    }
}
