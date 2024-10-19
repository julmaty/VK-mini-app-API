using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VK_mini_app.Models;

namespace VK_mini_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public RatingController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: /Rating/GetRating?vkId=12345
        [HttpGet]
        [Route("GetRating/{vkId}")]
        public async Task<ActionResult<RatingViewModel>> GetRating(int vkId)
        {
            // Ищем пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkId);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            // Получаем всех пользователей и их очки
            var users = await _context.Users
                .OrderByDescending(u => u.Score) // Сортируем по очкам
                .ToListAsync();

            // Берём 3 лучших пользователя
            var topUsers = users.Take(3).ToList();

            // Формируем рейтинг
            var ratingList = topUsers.Select((u, index) => new Rating
            {
                Place = index + 1, // Место (индекс + 1)
                Name = u.Name,
                Score = u.Score
            }).ToList();

            // Проверяем, входит ли текущий пользователь в топ-3
            Rating meRating = null;
            if (!topUsers.Any(u => u.Id == user.Id)) // Поиск по Id
            {
                meRating = new Rating
                {
                    Place = users.FindIndex(u => u.Id == user.Id) + 1, // Место текущего пользователя
                    Name = user.Name,
                    Score = user.Score
                };
            }

            // Формируем RatingViewModel
            var ratingViewModel = new RatingViewModel
            {
                Rating = ratingList,
                Me = meRating // Если текущий пользователь в топ-3, то Me будет null
            };

            return Ok(ratingViewModel); // Возвращаем RatingViewModel с рейтингом
        }
    }
}
