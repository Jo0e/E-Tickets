
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace E_Ticket.Models
{
    public class Category
    {
        public int Id { get; set; }
        [MinLength(3)]
        public string Name { get; set; }

        [ValidateNever]
        public ICollection<Movie> Movies { get; set; }
    }
}
