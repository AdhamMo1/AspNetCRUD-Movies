using AspNetCRUD_Movies.Models;
using AspNetCRUD_Movies.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
