using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public int Age { get; set; }
        [Display(Name = "IMDB Hyperlink")]
        public string IMBDHyperlink { get; set; }
        [Display(Name = "Actor Photo")]
        public byte[]? ActorPhoto { get; set; }
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();

    }
}
