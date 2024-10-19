namespace VK_mini_app.Models
{
    public class UserToTasks
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public int Status { get; set; }
    }
}
