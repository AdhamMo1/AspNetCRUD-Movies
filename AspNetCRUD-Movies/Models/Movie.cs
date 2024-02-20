using System.ComponentModel.DataAnnotations;

namespace AspNetCRUD_Movies.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required , MaxLength(250)]
        public string Title { get; set; }
        [Required]
        public double Rate { get; set; }
        [Required,MaxLength(2500)]
        public string StoryLine { get; set; }
        [Required]
        public byte[] Poster { get; set; }
        public byte CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
