using AspNetCRUD_Movies.Models;
using AspNetCRUD_Movies.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCRUD_Movies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }
        public IActionResult Create()
        {
            var viewModel = new FormCreateMovieViewModel
            {
                Categories = _context.Categories.OrderBy(x=>x.Name).ToList()
            };
            return View(viewModel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormCreateMovieViewModel model)
        {
            if(!ModelState.IsValid)
            {
                model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                return View(model);
            }
            var files = Request.Form.Files;
            var poster = files.FirstOrDefault();
            if (poster == null)
            {
                ModelState.AddModelError("Poster", "Please select poster..");
                model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                return View(model);
            }
            var extentions = new List<string> { ".jpg", ".png" };
            if (!extentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                ModelState.AddModelError("Poster", "Please select poster with - .png  or .jpg");
                model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                return View(model);
            }
            if(poster.Length > 1048576)
            {

                ModelState.AddModelError("Poster", "poster can't be more than 1MB");
                model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                return View(model);
            }
            using var fileData = new MemoryStream();
            await poster.CopyToAsync(fileData);
            var movie = new Movie
            {
                Title = model.Title,
                Rate = model.Rate,
                Year = model.Year,
                StoryLine = model.StoryLine,
                CategoryId = model.CategoryId,
                Poster = fileData.ToArray()
            };
            await _context.AddAsync(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
