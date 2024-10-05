namespace Assignment3.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string ReleasedBy { get; set; }
        public ICollection<Actor> Actors { get; } = new List<Actor>();
    }
}
