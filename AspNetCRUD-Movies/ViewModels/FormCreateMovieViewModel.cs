using AspNetCRUD_Movies.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCRUD_Movies.ViewModels
{
    public class FormCreateMovieViewModel
    {
        [Required, StringLength(250)]
        public string Title { get; set; }
        [Required,Range(1,10)]
        public double Rate { get; set; }
        [Required, StringLength(2500)]
        public string StoryLine { get; set; }
        public byte[] Poster { get; set; }
        public byte CategoryId { get; set; }
        public IEnumerable<Category> Categories {  get; set; }
    }
}
