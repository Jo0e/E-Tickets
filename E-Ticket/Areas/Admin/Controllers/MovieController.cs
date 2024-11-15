using E_Ticket.Models;
using E_Ticket.Repository;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class MovieController : Controller
    {
        private readonly IMovieRepository movieRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly ICinemaRepository cinemaRepository;
        private readonly IRepository<CinemaMovie> cinemaMovieRepository;
        private readonly IRepository<ActorMovie> actorMovieRepository;
        private readonly IActorRepository actorRepository;
        private readonly ILogger<MovieController> logger;

        public MovieController(IMovieRepository movieRepository, ICategoryRepository categoryRepository,
            ICinemaRepository cinemaRepository, IRepository<CinemaMovie> CinemaMovieRepository,
            IRepository<ActorMovie> actorMovieRepository, IActorRepository actorRepository, ILogger<MovieController> logger)
        {
            this.movieRepository = movieRepository;
            this.categoryRepository = categoryRepository;
            this.cinemaRepository = cinemaRepository;
            cinemaMovieRepository = CinemaMovieRepository;
            this.actorMovieRepository = actorMovieRepository;
            this.actorRepository = actorRepository;
            this.logger = logger;
        }


        public IActionResult Index(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Movie> movies;
            if (Name == null)
                movies = movieRepository.Get(include: [c => c.Category]);
            else
                movies = movieRepository.Get(include: [c => c.Category], where: e => e.Name.Contains(Name));

            if (!movies.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the Movie try again";
                return RedirectToAction("MovieCRUD");
            }

            int itemsNum = 8;
            int totalMovies = movies.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);

            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(movies.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(categoryRepository.Get(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(cinemaRepository.Get(), "Id", "Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie, List<int> CinemasList, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            ModelState.Remove(nameof(CinemasList));
            if (ModelState.IsValid)
            {
                if (movie.MovieStatus == MovieStatus.Expired)
                {
                    movie.TotalTickets = 0;
                    movie.TicketsSold = 0;
                }
                movieRepository.CreateWithImage(movie, PhotoUrl, "movies", "ImgUrl");
                Log(nameof(Create), nameof(movie));

                foreach (var item in CinemasList)
                {
                    cinemaMovieRepository.Create(new CinemaMovie { CinemaId = item, MovieId = movie.Id });
                    cinemaMovieRepository.Commit();
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }


        public IActionResult Edit(int movieId)
        {
            ViewBag.Categories = new SelectList(categoryRepository.Get(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(cinemaRepository.Get(), "Id", "Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));
            var movie = movieRepository.GetDetails(movieId);



            var selectedCinemas = movie.CinemaMovies.Select(cm => cm.CinemaId).ToList();
            ViewBag.SelectedCinemas = selectedCinemas;

            return View(movie);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Movie movie, List<int> CinemasList, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            ModelState.Remove(nameof(CinemasList));
            if (ModelState.IsValid)
            {
                var oldMovie = movieRepository.GetOne(where: e => e.Id == movie.Id);
                if (movie.MovieStatus == MovieStatus.Expired)
                {
                    movie.TotalTickets = 0;
                    movie.TicketsSold = 0;

                }
                movieRepository.UpdateImage(movie, PhotoUrl, oldMovie.ImgUrl, "movies", "ImgUrl");
                Log(nameof(Edit), nameof(movie));

                var toDelete = cinemaMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
                if (toDelete != null)
                {
                    foreach (var item in toDelete)
                    {
                        cinemaMovieRepository.Delete(item);
                    }
                }

                foreach (var cinemaId in CinemasList)
                {
                    cinemaMovieRepository.Create(new CinemaMovie { CinemaId = cinemaId, MovieId = movie.Id });
                }
                cinemaMovieRepository.Commit();
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }

        public IActionResult Delete(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);
            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Movie movie)
        {
            var cinemaToDelete = cinemaMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (cinemaToDelete != null)
            {
                foreach (var cinema in cinemaToDelete)
                {
                    cinemaMovieRepository.Delete(cinema);
                }
            }

            var actorToDelete = actorMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (actorToDelete != null)
            {
                foreach (var actor in actorToDelete)
                {
                    actorMovieRepository.Delete(actor);
                }
            }

            Log(nameof(Delete), nameof(movie));

            movieRepository.Delete(movie);
            movieRepository.Commit();
            return RedirectToAction("Index");
        }



        public IActionResult AssignActor(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);

            var allActors = actorRepository.Get().ToList();
            var assignedActors = movie.ActorMovies.Select(am => am.ActorId).ToList();

            ViewBag.AllActors = new SelectList(allActors, "Id", "FullName");
            ViewBag.AssignedActors = assignedActors;

            return View(movie);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignActor(int Id, List<int> AssignedActors)
        {
            var movie = movieRepository.GetOne(where: w => w.Id == Id);

            if (movie == null)
            {
                return NotFound();
            }
            var toDelete = actorMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (toDelete != null)
            {
                foreach (var item in toDelete)
                {
                    actorMovieRepository.Delete(item);
                }
            }

            foreach (var actorId in AssignedActors)
            {
                actorMovieRepository.Create(new ActorMovie { ActorId = actorId, MovieId = movie.Id });
            }

            Log(nameof(AssignActor), nameof(movie));
            actorMovieRepository.Commit();

            return RedirectToAction("Index");
        }




        public byte[] ExportMoviesToExcel(IEnumerable<Movie> movies)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Movies");

                // Add headers
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Description";
                worksheet.Cells[1, 3].Value = "Price";
                worksheet.Cells[1, 4].Value = "Img Url";
                worksheet.Cells[1, 5].Value = "TrailerUrl";
                worksheet.Cells[1, 6].Value = "StartDate";
                worksheet.Cells[1, 7].Value = "EndDate";
                worksheet.Cells[1, 8].Value = "MovieStatus";
                worksheet.Cells[1, 9].Value = "TotalTickets";
                worksheet.Cells[1, 10].Value = "TicketsSold";
                worksheet.Cells[1, 11].Value = "CategoryId";
                worksheet.Cells[1, 12].Value = "CategoryName";
                worksheet.Cells[1, 13].Value = "ActorsId";
                worksheet.Cells[1, 14].Value = "ActorsNames";
                worksheet.Cells[1, 15].Value = "CinemasId";
                worksheet.Cells[1, 16].Value = "CinemasNames";

                int row = 2;
                foreach (var movie in movies)
                {
                    worksheet.Cells[row, 1].Value = movie.Name;
                    worksheet.Cells[row, 2].Value = movie.Description;
                    worksheet.Cells[row, 3].Value = movie.Price;
                    worksheet.Cells[row, 4].Value = movie.ImgUrl;
                    worksheet.Cells[row, 5].Value = movie.TrailerUrl;
                    worksheet.Cells[row, 6].Value = movie.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 7].Value = movie.EndDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 8].Value = movie.MovieStatus;
                    worksheet.Cells[row, 9].Value = movie.TotalTickets;
                    worksheet.Cells[row, 10].Value = movie.TicketsSold;
                    worksheet.Cells[row, 11].Value = movie.CategoryId;
                    worksheet.Cells[row, 12].Value = movie.Category.Name;
                    worksheet.Cells[row, 13].Value = string.Join(", ", movie.ActorMovies.Select(i => i.ActorId));
                    worksheet.Cells[row, 14].Value = string.Join(", ", movie.ActorMovies.Select(p => p.Actor.FullName));
                    worksheet.Cells[row, 15].Value = string.Join(", ", movie.CinemaMovies.Select(i => i.CinemaId));
                    worksheet.Cells[row, 16].Value = string.Join(", ", movie.CinemaMovies.Select(p => p.Cinema.Name));
                    row++;
                }

                return package.GetAsByteArray();
            }
        }


        public IActionResult DownloadMoviesAsExcel()
        {
            var movies = movieRepository.GetAllMoviesDetails();
            var fileContents = ExportMoviesToExcel(movies);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Movies.xlsx");
        }



        public IActionResult UploadExcel()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);

                // Ensure the uploads directory exists
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var allData = ReadMoviesFromExcel(filePath);
                
                // Save movies to database
                //movieRepository.AddRange(allData.Movies);
                //movieRepository.Commit();
                actorMovieRepository.AddRange(allData.ActorMovies);
                movieRepository.Commit();
                cinemaMovieRepository.AddRange(allData.CinemaMovies);
                movieRepository.Commit();
                Log("Add Data By Excel File" , "Movies With all Data");
                // Delete the file after processing
                System.IO.File.Delete(filePath);

                return RedirectToAction("Index");
            }

            return View("SomethingWrong", "Errors");
        }

        public MovieData ReadMoviesFromExcel(string filePath)
        {
            var movieData = new MovieData 
            {
                Movies = new List<Movie>(), 
                ActorMovies = new List<ActorMovie>() 
            };
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Assuming the first row is headers
                {
                    var movie = new Movie
                    {
                        Name = worksheet.Cells[row, 1].Text,
                        Description = worksheet.Cells[row, 2].Text,
                        Price = double.Parse(worksheet.Cells[row, 3].Text),
                        ImgUrl = string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? "default.jpg" : worksheet.Cells[row, 4].Text,
                        TrailerUrl = worksheet.Cells[row, 5].Text,
                        StartDate = DateTime.Parse(worksheet.Cells[row, 6].Text),
                        EndDate = DateTime.Parse(worksheet.Cells[row, 7].Text),
                        MovieStatus = Enum.TryParse(worksheet.Cells[row, 8].Text, out MovieStatus status) ? status : default,
                        TotalTickets = int.Parse(worksheet.Cells[row, 9].Text),
                        TicketsSold = int.Parse(worksheet.Cells[row, 10].Text),
                        CategoryId = int.Parse(worksheet.Cells[row, 11].Text)
                    };
                    movieData.Movies.Add(movie);
                    movieRepository.Create(movie);
                    movieRepository.Commit();
                    var actorIds = worksheet.Cells[row, 13].Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in actorIds)
                    {
                        if (int.TryParse(item, out int actorId))
                        {
                            var actorMovie = new ActorMovie
                            {
                                MovieId = movie.Id,
                                ActorId = actorId,
                            };
                            movieData.ActorMovies.Add(actorMovie);
                        }
                    }

                    var cinemaIds = worksheet.Cells[row, 15].Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in cinemaIds)
                    {
                        if (int.TryParse(item, out int cinemaId))
                        {
                            var cinemaMovie = new CinemaMovie
                            {
                                MovieId= movie.Id,
                                CinemaId = cinemaId,
                            };
                            movieData.CinemaMovies.Add(cinemaMovie);
                        }
                    }

                }
            }
            
            return movieData;
        }



        public void Log(string action, string entity)
        {
            var admin = User.Identity.Name;
            LoggerHelper.LogAdminAction(logger, admin, action, entity);
        }



    }
}
