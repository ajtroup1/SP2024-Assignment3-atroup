namespace Assignment3.Models
{
    public class ActorDetails
    {
        public Actor Actor { get; set; }
        public List<SentimentResult> SentimentResults { get; set; }
        public double OverallSentiment { get; set; }
        public string SentimentString { get; set; }
    }
}
