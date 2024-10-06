namespace Assignment3.Models
{
    public class MovieDetails
    {
        public Movie Movie { get; set; }
        public List<SentimentResult> SentimentResults { get; set; }
        public double OverallSentiment { get; set; }
        public string SentimentString { get; set; }
    }

}
