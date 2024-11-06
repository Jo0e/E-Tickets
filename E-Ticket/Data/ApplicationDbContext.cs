using E_Ticket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_Ticket.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<ActorMovie> ActorMovies { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<CinemaMovie> CinemaMovies { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<OrderList> OrderLists { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder);
                var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var connection = builder.GetConnectionString("DefualtConnection");
                optionsBuilder.UseSqlServer(connection);

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>().ToTable(t => t.HasTrigger("TR_UpdateMovieStatusOnEndDate"));

            modelBuilder.Entity<ActorMovie>()
                .HasKey(e => new
                {
                    e.ActorId,
                    e.MovieId
                });

            modelBuilder.Entity<CinemaMovie>()
                .HasKey(m => new 
                {
                    m.CinemaId, 
                    m.MovieId 
                });

            modelBuilder.Entity<ActorMovie>()
                .HasOne(am => am.Actor)
                .WithMany(a => a.ActorMovies)
                .HasForeignKey(am => am.ActorId);

            modelBuilder.Entity<ActorMovie>()
                .HasOne(am => am.Movie)
                .WithMany(m => m.ActorMovies)
                .HasForeignKey(am => am.MovieId);


            modelBuilder.Entity<Wishlist>()
            .HasKey(w => w.WishlistId);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Movie)
                .WithMany()
                .HasForeignKey(w => w.MovieId);



            modelBuilder.Entity<Movie>()
                .Property(m => m.TicketsSold)
                .HasDefaultValue(0);

            modelBuilder.Entity<Movie>()
                .Property(m => m.TotalTickets)
                .HasDefaultValue(0);



            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Tickets)
                .WithOne(t => t.Cart)
                .HasForeignKey(t => t.CartId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Movie)
                .WithMany(m => m.Tickets)
                .HasForeignKey(t => t.MovieId);

        }
    }




}
//public DbSet<ETickets.ViewModel.ApplicationUserVM> ApplicationUserVM { get; set; } = default!;
//public DbSet<ETickets.ViewModel.LoginVM> LoginVM { get; set; } = default!;
//    }
//}
