using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ETickets.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        [MinLength(3)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string CinemaLogo { get; set; }
        public string Address { get; set; }
        [ValidateNever]
        public ICollection<Movie> Movies { get; set; }
    }
}
