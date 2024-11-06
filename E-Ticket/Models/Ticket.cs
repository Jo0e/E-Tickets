
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace E_Ticket.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string MovieName { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public int CartId { get; set; }
        [ValidateNever]
        public Cart Cart { get; set; }

        public int MovieId { get; set; }
        [ValidateNever]
        public Movie Movie { get; set; }
        
    }
}
