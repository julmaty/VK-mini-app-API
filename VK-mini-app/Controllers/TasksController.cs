using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_mini_app.Models;

namespace VK_mini_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public TasksController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: /Tasks/GetTasks
        [HttpGet]
        [Route("GetTasks/{vkid}")]
        public async Task<ActionResult<List<TaskViewModel>>> GetTasks(int vkid)
        {
            // Получаем пользователя по vkid
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkid);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            // Находим категории, в которых у пользователя Level > 0
            var categoryScores = await _context.UserToCategoryScores
                .Where(ucs => ucs.UserId == user.Id && ucs.Level > 0) // Уровень > 0
                .Select(ucs => ucs.CategoryId) // Получаем CategoryId
                .Distinct() // Убираем дубликаты
                .ToListAsync();

            // Получаем все задачи из базы данных
            var tasks = await _context.Tasks.ToListAsync();

            // Загружаем все категории, чтобы избежать дополнительных запросов
            var categories = await _context.Categories.ToListAsync();

            // Загружаем все связи UserToTasks для данного пользователя
            var userTasks = await _context.UserToTasks
                .Where(ut => ut.UserId == user.VkId)
                .ToListAsync();

            // Преобразуем задачи в TaskViewModel
            var taskViewModels = tasks.Select(task => new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Points = task.Points,
                CategoryId = task.Category,
                CategoryName = categories.FirstOrDefault(c => c.Id == task.Category)?.Name ?? "Unknown",
                Status = userTasks.FirstOrDefault(ut => ut.TaskId == task.Id)?.Status ?? 0,
                Recommended = categoryScores.Contains(task.Category)
            }).ToList();

            return Ok(taskViewModels);
        }


        // POST: /Tasks/ChangeStatus
        [HttpPost]
        [Route("ChangeStatus/{userId}/{taskId}/{status}")]
        public async Task<IActionResult> ChangeStatus(int userId, int taskId, int status)
        {
            if (status == 1)
            {
                var userToTask = new UserToTasks { UserId = userId, TaskId = taskId, Status = status };
                _context.UserToTasks.Add(userToTask);
                await _context.SaveChangesAsync();
            }

            else if (status == 3)
            {
                // Ищем запись в таблице UserToTasks
                var userTask = await _context.UserToTasks
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TaskId == taskId);

                if (userTask == null)
                {
                    return NotFound("Запись с указанными UserId и TaskId не найдена.");
                }

                // Обновляем статус задачи для пользователя
                userTask.Status = status;
                _context.UserToTasks.Update(userTask);

                // Если новый статус равен 3, увеличиваем очки у пользователя

                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == userId);

                if (task != null && user != null)
                {
                    user.Score += task.Points;
                    _context.Users.Update(user);
                }
             

                // Сохраняем изменения в базе данных
                await _context.SaveChangesAsync();
            }
            else if (status == 0)
            {

                // Ищем запись в таблице UserToTasks
                var userTask = await _context.UserToTasks
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TaskId == taskId);

                if (userTask == null)
                {
                    return NotFound("Запись с указанными UserId и TaskId не найдена.");
                }

                _context.UserToTasks.Remove(userTask);


                // Сохраняем изменения в базе данных
                await _context.SaveChangesAsync();
            }

            
            return Ok("Статус задачи успешно обновлен.");
        }

        // GET: /Tasks/GetRecommendedTasks?vkId=12345
        [HttpGet]
        [Route("GetRecommendedTasks/{vkId}")]
        public async Task<ActionResult<List<TaskViewModel>>> GetRecommendedTasks(int vkId)
        {
            // Находим пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkId);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            // Находим категории, в которых у пользователя Level > 0
            var categoryScores = await _context.UserToCategoryScores
                .Where(ucs => ucs.UserId == user.Id && ucs.Level > 0) // Уровень > 0
                .Select(ucs => ucs.CategoryId) // Получаем CategoryId
                .Distinct() // Убираем дубликаты
                .ToListAsync();

            // Загружаем задачи, соответствующие этим категориям
            var tasks = await _context.Tasks
                .Where(t => categoryScores.Contains(t.Category)) // Фильтруем по категориям
                .ToListAsync();

            // Загружаем все категории для сопоставления имен
            var categories = await _context.Categories.ToListAsync();

            // Загружаем связи UserToTasks для текущего пользователя
            var userTasks = await _context.UserToTasks
                .Where(ut => ut.UserId == user.VkId)
                .ToListAsync();

            // Преобразуем задачи в TaskViewModel
            var recommendedTaskViewModels = tasks.Select(task => new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Points = task.Points,
                CategoryId = task.Category,
                CategoryName = categories.FirstOrDefault(c => c.Id == task.Category)?.Name ?? "Unknown",
                Status = userTasks.FirstOrDefault(ut => ut.TaskId == task.Id)?.Status ?? 0
            }).ToList();

            return Ok(recommendedTaskViewModels); // Возвращаем список рекомендованных задач в виде TaskViewModel
        }

    }
}
