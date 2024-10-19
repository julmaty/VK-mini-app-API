namespace VK_mini_app.Models
{
    public class UserToCategoryScore
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int Level { get; set; }
    }
}
