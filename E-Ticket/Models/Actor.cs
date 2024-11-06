using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace E_Ticket.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [MinLength(3)]
        public string FirstName { get; set; }
        [MinLength(3)]
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public string Bio { get; set; }

        [ValidateNever]
        public string ProfilePicture { get; set; }
        public string News { get; set; }
        [ValidateNever]
        public ICollection<ActorMovie> ActorMovies { get; set; }
    }
}
