using AspNetCRUD_Movies.Models;
using AspNetCRUD_Movies.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Collections.Generic;
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
            _toastNotification.AddSuccessToastMessage("Movie Created Successfully!");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            var movie =await _context.Movies.FindAsync(id);
            if(movie == null)
            {
                return NotFound();
            }
            var viewModel = new FormCreateMovieViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Rate = movie.Rate,
                Year = movie.Year,
                StoryLine = movie.StoryLine,
                CategoryId = movie.CategoryId,
                Poster = movie.Poster,
                Categories = await _context.Categories.ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FormCreateMovieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var movie = await _context.Movies.FindAsync(model.Id);
            if (movie == null)
            {
                return NotFound();
            }
            var poster = Request.Form.Files.FirstOrDefault();
            if (poster != null)
            {
                using var DataStream = new MemoryStream();
                await poster.CopyToAsync(DataStream);
                var newPoster = DataStream.ToArray();
                
                    var Extention = new List<string>() { ".jpg", ".png" };
                    if (!Extention.Contains(Path.GetExtension(poster.FileName)))
                    {
                        ModelState.AddModelError("Poster", "Please select poster with - .png  or .jpg");
                        model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                        return View(model);
                    }
                    if (poster.Length > 1048576)
                    {
                        ModelState.AddModelError("Poster", "poster can't be more than 1MB");
                        model.Categories = _context.Categories.OrderBy(x => x.Name).ToList();
                        return View(model);
                    }
                    movie.Poster = newPoster;
                
            }
            movie.Title = model.Title;
            movie.StoryLine = model.StoryLine;
            movie.Rate = model.Rate;
            movie.Year = model.Year;
            movie.CategoryId = model.CategoryId;
            
            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Movie updated successfully!");
            return RedirectToAction(nameof(Index));
        }
    }
}
