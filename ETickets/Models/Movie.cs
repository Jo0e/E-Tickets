using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ETickets.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(1, 10000)]
        public double Price { get; set; }
        [ValidateNever]
        public string ImgUrl { get; set; }
        public string TrailerUrl { get; set; }

        public DateTime StartDate { get; set; }
        [CustomValidation(typeof(Movie), "ValidateStartEndDate")]
        public DateTime EndDate { get; set; }

        [Required]
        public MovieStatus MovieStatus { get; set; }

        public int CinemaId { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever]
        public Cinema Cinema { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public ICollection<ActorMovie> ActorMovies { get; set; }
        [Range(0, int.MaxValue)]
        public int TotalTickets { get; set; }
        [Range(0, int.MaxValue)]
        public int TicketsSold { get; set; }
        public int TicketsRemaining => TotalTickets - TicketsSold;
        [ValidateNever]
        public ICollection<Ticket> Tickets { get; set; }

        public static ValidationResult ValidateStartEndDate(DateTime endDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as Movie;
            if (instance == null || instance.StartDate <= endDate)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("End Date must be after Start Date.");
        }

    }
    public enum MovieStatus
    {
        Upcoming,
        Available,
        Expired,
    }
}