namespace VK_mini_app.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Status { get; set; }

        public bool Recommended { get; set; }
    }
}
