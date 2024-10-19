namespace VK_mini_app.Models
{
    public class RatingViewModel
    {
        public List<Rating> Rating { get; set; }
        public Rating Me { get; set; }

    }

    public class Rating
    {
        public int Place { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
