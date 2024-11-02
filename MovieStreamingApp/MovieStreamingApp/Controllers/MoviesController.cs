using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MovieStreamingApp.Models;
using MovieStreamingApp.Services;
using Microsoft.Extensions.Logging;

namespace MovieStreamingApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly S3Service _s3Service;
        private readonly DynamoDbService _dynamoDbService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(S3Service s3Service, DynamoDbService dynamoDbService, ILogger<MoviesController> logger)
        {
            _s3Service = s3Service;
            _dynamoDbService = dynamoDbService;
            _logger = logger;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index method invoked: Fetching all movies from DynamoDB.");
            var movieDocuments = await _dynamoDbService.GetAllMoviesAsync();

            var movies = movieDocuments.Select(doc => new Movie
            {
                MovieID = doc["MovieID"],
                Title = doc["Title"],
                Genre = doc["Genre"],
                Director = doc["Director"],
                ReleaseTime = doc.ContainsKey("ReleaseTime") ? doc["ReleaseTime"].AsString() : "",
                Rating = doc.ContainsKey("Rating") ? Convert.ToInt32(doc["Rating"]) : 0,
                FileUrl = doc.ContainsKey("FileUrl") ? doc["FileUrl"].AsString() : "",
                Comments = doc.ContainsKey("Comments") ? doc["Comments"].AsString() : "",
                OwnerId = doc.ContainsKey("OwnerId") ? Convert.ToInt32(doc["OwnerId"]) : 0
            }).ToList();

            return View(movies);
        }

        // GET: Movies/Download/5
        public async Task<IActionResult> Download(string id)
        {
            var movie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (movie == null || string.IsNullOrEmpty(movie["FileUrl"]))
            {
                return NotFound();
            }

            // Retrieve the file URL from DynamoDB
            var fileUrl = movie["FileUrl"].AsString();

            // Return a redirect result to the file URL, making it downloadable
            return Redirect(fileUrl);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Create GET method invoked: Displaying the create movie form.");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? movieFile)
        {
            _logger.LogInformation("Create POST method invoked.");
            movie.MovieID = string.IsNullOrEmpty(movie.MovieID) ? Guid.NewGuid().ToString() : movie.MovieID;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for Create method.");
                return View(movie);
            }

            try
            {
                movie.OwnerId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
                movie.Comments ??= ""; // Initialize Comments

                if (movieFile != null && movieFile.Length > 0)
                {
                    using var stream = movieFile.OpenReadStream();
                    movie.FileUrl = await _s3Service.UploadFileAsync(stream, movieFile.FileName);
                }
                else
                {
                    movie.FileUrl ??= "No file uploaded";
                }

                await _dynamoDbService.SaveMovieAsync(
                    movie.MovieID,
                    movie.Title,
                    movie.Genre,
                    movie.Director,
                    movie.ReleaseTime,
                    movie.Rating,
                    movie.FileUrl,
                    movie.Comments,
                    movie.OwnerId
                );

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie.");
                return View(movie);
            }
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var movie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (Convert.ToInt32(movie["OwnerId"]) != currentUserId)
            {
                return Forbid();
            }

            return View(new Movie
            {
                MovieID = movie["MovieID"],
                Title = movie["Title"],
                Genre = movie["Genre"],
                Director = movie["Director"],
                ReleaseTime = movie.ContainsKey("ReleaseTime") ? movie["ReleaseTime"].AsString() : "",
                Rating = movie.ContainsKey("Rating") ? Convert.ToInt32(movie["Rating"]) : 0,
                FileUrl = movie.ContainsKey("FileUrl") ? movie["FileUrl"].AsString() : "",
                Comments = movie.ContainsKey("Comments") ? movie["Comments"].AsString() : "",
                OwnerId = Convert.ToInt32(movie["OwnerId"])
            });
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Movie updatedMovie)
        {
            var existingMovie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (existingMovie == null || Convert.ToInt32(existingMovie["OwnerId"]) != HttpContext.Session.GetInt32("UserId"))
            {
                return Forbid();
            }

            try
            {
                updatedMovie.MovieID = id;
                updatedMovie.OwnerId = Convert.ToInt32(existingMovie["OwnerId"]); // Ensure OwnerId is preserved
                await _dynamoDbService.UpdateMovieAsync(updatedMovie);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie.");
                return View(updatedMovie);
            }
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var movie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (movie == null || Convert.ToInt32(movie["OwnerId"]) != HttpContext.Session.GetInt32("UserId"))
            {
                return Forbid();
            }

            return View(new Movie
            {
                MovieID = movie["MovieID"],
                Title = movie["Title"],
                Genre = movie["Genre"],
                Director = movie["Director"],
                ReleaseTime = movie.ContainsKey("ReleaseTime") ? movie["ReleaseTime"].AsString() : "",
                Rating = movie.ContainsKey("Rating") ? Convert.ToInt32(movie["Rating"]) : 0,
                FileUrl = movie.ContainsKey("FileUrl") ? movie["FileUrl"].AsString() : "",
                Comments = movie.ContainsKey("Comments") ? movie["Comments"].AsString() : "",
                OwnerId = Convert.ToInt32(movie["OwnerId"])
            });
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var movie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(new Movie
            {
                MovieID = movie["MovieID"],
                Title = movie["Title"],
                Genre = movie["Genre"],
                Director = movie["Director"],
                ReleaseTime = movie.ContainsKey("ReleaseTime") ? movie["ReleaseTime"].AsString() : "",
                Rating = movie.ContainsKey("Rating") ? Convert.ToInt32(movie["Rating"]) : 0,
                FileUrl = movie.ContainsKey("FileUrl") ? movie["FileUrl"].AsString() : "",
                Comments = movie.ContainsKey("Comments") ? movie["Comments"].AsString() : "",
                OwnerId = Convert.ToInt32(movie["OwnerId"])
            });
        }


        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var movie = await _dynamoDbService.GetMovieByIdAsync(id);
            if (movie == null || Convert.ToInt32(movie["OwnerId"]) != HttpContext.Session.GetInt32("UserId"))
            {
                return Forbid();
            }

            await _dynamoDbService.DeleteMovieAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
