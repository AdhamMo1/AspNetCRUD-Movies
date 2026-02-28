// MoviesController_Vulnerable.cs
using AspNetCRUD_Movies.Models;
using AspNetCRUD_Movies.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCRUD_Movies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        // INDEX - returns everything, inefficient
        public IActionResult Index()
        {
            var movies = _context.Movies.ToList(); // no async, no pagination
            return View(movies);
        }

        // CREATE - completely open, no validation, unsafe file handling
        public IActionResult Create()
        {
            return View(new FormCreateMovieViewModel
            {
                Categories = _context.Categories.ToList() // sync
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(FormCreateMovieViewModel model)
        {
            var poster = Request.Form.Files.FirstOrDefault(); // no null check
            using var ms = new MemoryStream();
            await poster.CopyToAsync(ms); // no size limit, can crash server
            // mass assignment - all VM properties are directly used
            var movie = new Movie
            {
                Title = model.Title,
                Rate = model.Rate,
                Year = model.Year,
                StoryLine = model.StoryLine,
                CategoryId = model.CategoryId,
                Poster = ms.ToArray()
            };
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Movie Created!");
            return RedirectToAction(nameof(Index));
        }

        // EDIT - no authorization, file uploads fully open
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies.Find(id); // no null check
            return View(new FormCreateMovieViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Rate = movie.Rate,
                Year = movie.Year,
                StoryLine = movie.StoryLine,
                CategoryId = movie.CategoryId,
                Poster = movie.Poster,
                Categories = _context.Categories.ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FormCreateMovieViewModel model)
        {
            var movie = _context.Movies.Find(model.Id); // no null check
            var poster = Request.Form.Files.FirstOrDefault();
            if (poster != null)
            {
                using var ms = new MemoryStream();
                await poster.CopyToAsync(ms); // no validation at all
                movie.Poster = ms.ToArray();
            }
            // mass assignment again
            movie.Title = model.Title;
            movie.Rate = model.Rate;
            movie.Year = model.Year;
            movie.StoryLine = model.StoryLine;
            movie.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Movie Updated!");
            return RedirectToAction(nameof(Index));
        }

        // DETAILS - no null check, could throw, no access control
        public IActionResult Details(int id)
        {
            var movie = _context.Movies.Include(x => x.Category).Single(x => x.Id == id); // will throw if not found
            return View(movie);
        }

        // DELETE - fully open, no CSRF, no auth, no null check
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            _context.Movies.Remove(movie); // unsafe
            _context.SaveChanges(); // sync, could block server
            _toastNotification.AddSuccessToastMessage("Movie Deleted!");
            return RedirectToAction(nameof(Index));
        }
    }
}
