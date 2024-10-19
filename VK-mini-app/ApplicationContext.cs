namespace VK_mini_app
{
    using Microsoft.EntityFrameworkCore;
    using VK_mini_app.Models;

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryRus> CategoriesRus { get; set; }
        public DbSet<UserToCategoryScore> UserToCategoryScores { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<UserToTasks> UserToTasks {get; set;}
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
