using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Web;
using VK_mini_app.Models;
using VK_mini_app.Models.Response;

namespace VK_mini_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration Configuration;

        public UserController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        // GET: /User/getScore?vkId=12345
        [HttpGet]
        [Route("getScore/{vkId}")]
        public async Task<ActionResult<int>> GetScore(int vkId)
        {
            // Ищем пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkId);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            return Ok(user.Score); // Возвращаем score пользователя
        }

        // GET: /User/getBestCategories?vkId=12345
        [HttpGet]
        [Route("getBestCategories/{vkId}")]
        public async Task<ActionResult<List<CategoryUserViewModel>>> GetBestCategories(int vkId)
        {
            // Ищем пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkId);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            // Ищем записи в UserToCategoryScore для данного пользователя и сортируем по Level
            var bestCategories = await _context.UserToCategoryScores
                .Where(ucs => ucs.UserId == user.Id)
                .OrderByDescending(ucs => ucs.Level)
                .Take(3) // Берём топ-3 категории по уровню
                .Join(
                    _context.Categories,
                    ucs => ucs.CategoryId,
                    category => category.Id,
                    (ucs, category) => new CategoryUserViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Level = ucs.Level
                    }
                )
                .ToListAsync();

            return Ok(bestCategories); // Возвращаем список CategoryUserViewModel
        }

        // GET: /User/getAvatar?vkId=12345
        [HttpGet]
        [Route("getAvatar/{vkId}")]
        public async Task<ActionResult<string>> GetAvatar(int vkId)
        {
            // Ищем пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkId);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            return Ok(user.AvatarUrl); // Возвращаем AvatarUrl пользователя
        }

        private async Task<string> GPTAvatar(List<string> bestCategories, string sex)
        {
            var apiSettings = Configuration.GetSection("GPT");
            // Токен из личного кабинета
            string apiKey = apiSettings["Key"];
            // Адрес API для взаимодействия с DALL-E
            string endpoint = "https://api.openai.com/v1/images/generations";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // Формируем описание для генерации аватара на основе интересов пользователя
                string contentforGPT = $"Create a user avatar in a skeuomorphic style with a blue color theme inspired by the VK.com app. " +
                                       $"The avatar should reflect the user's interests in the following categories: {string.Join(", ", bestCategories)}. " +
                                       $"User sex is {sex}" +
                                       "The design should be modern, visually appealing, and highlight the user's personality with elements that relate to their top interests.";

                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = contentforGPT,
                    n = 1, // количество изображений для генерации
                    size = "1024x1024" // размер изображения
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                var imageUrl = responseObject.data[0].url.ToString();

                return imageUrl;
                // Теперь можно использовать imageUrl, например, сохранить его в базу данных или отобразить пользователю
            }
        }

        private async Task<string> GPT(UserInfo userInfo, BaseResponse<List<GetUsersResponse>> vkUser)
        {
            var apiSettings = Configuration.GetSection("GPT");
            // токен из личного кабинета
            string apiKey = apiSettings["Key"];

            // Чтение и парсинг categories.json
            List<Category> categories;
            using (StreamReader r = new StreamReader("categories.json"))
            {
                string jsonText = await r.ReadToEndAsync();
                var jsonData = JsonConvert.DeserializeObject<CategoriesResponse>(jsonText);
                categories = jsonData.Res;
            }
            var vkUserData = vkUser.Response.First();

            // Формируем запрос для GPT
            string contentForGPT = $"Учитывая следующую информацию о пользователе:\n" +
                                    $"- Департамент: {userInfo.Dep}\n" +
                                    $"- Спорт: {userInfo.Sport}\n" +
                                    $"- Клуб: {userInfo.Club}\n" +
                                    $"- Мероприятия: {userInfo.Mer}\n" +
                                    $"Дополнительная информация из профиля VK:\n" +
                        $"- Интересы: {vkUserData.interests}\n" +
                        $"- О книгах: {vkUserData.books}\n" +
                        $"- Игры: {vkUserData.games}\n" +
                        $"- Музыка: {vkUserData.music}\n" +
                        $"- Фильмы: {vkUserData.movies}\n";

            // адрес api для взаимодействия с чат-ботом
            string endpoint = "https://api.openai.com/v1/chat/completions";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                    new { role = "system", content =  "Выбери 3 наиболее актуальные категории для данного пользователя из списка и верни их ID через запятую:\n" +
                                    string.Join(", ", categories.Select(c => $"{c.Id}: {c.Name}")) + "\nОтвет обязательно должен быть в виде строки из 3 чисел через запятую (названия не нужны, только числа и запятые между ними)."
                                    +  $"{contentForGPT}"}
                },
                    temperature = 0.5
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                return responseObject.choices[0].message.content.ToString();
            }
        }

        private async Task<BaseResponse<List<GetUsersResponse>>> GetUser(int vkid)
        {
            var apiSettings = Configuration.GetSection("VkApi");
            string url = $"{apiSettings["Api_url"]}users.get";
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["user_ids"] = vkid.ToString();
            queryString["v"] = "5.199";
            queryString["access_token"] = apiSettings["Server_key"];
            queryString["fields"] = "activities, about, books, education, sex, games, interests, movies, music, quotes";
            var postValues = new FormUrlEncodedContent(queryString.AllKeys.ToDictionary(k => k, k => queryString[k]));
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient
                .PostAsync($"{url}?{queryString}", postValues)
                .ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var userResponse = JsonConvert.DeserializeObject<BaseResponse<List<GetUsersResponse>>>(json);

                return userResponse;
            }
        }

        [HttpPost]
        [Route("getInitialCategories/{vkid}")]
        public async Task<ActionResult<List<CategoryUserViewModel>>> getInitialCategories(int vkid)
        {
            // Шаг 1: Получение информации о пользователе по vkid
            var vkUserResponse = await GetUser(vkid);
            if (vkUserResponse == null || vkUserResponse.Response == null || !vkUserResponse.Response.Any())
            {
                throw new Exception("Не удалось получить информацию о пользователе из VK.");
            }

            // Шаг 2: Генерация случайного числа и получение данных из Isu.json
            var random = new Random();
            int number = random.Next(2, 10001); // Генерируем случайное число от 1 до 10000
            UserInfo item = _context.UserInfos.Skip(number - 1).Take(1).FirstOrDefault();

            // Находим UserInfo с соответствующим номером
            if (item == null)
            {
                throw new Exception("Не удалось найти соответствующую запись UserInfo.");
            }

            // Шаг 3: Вызов метода GPT для получения трех категорий
            string gptResponse = await GPT(item, vkUserResponse);
            var categoryIds = gptResponse.Split(',').Select(int.Parse).ToList();

            // Шаг 4: Добавление нового пользователя в базу данных
            var newUser = new User
            {
                Name = $"{vkUserResponse.Response[0].first_name} {vkUserResponse.Response[0].last_name}",
                VkId = vkid,
                Score = 0,
                AvatarUrl = "", // пустая строка для AvatarUrl
                UserInfoId = number
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Шаг 5: Создание записей UserToCategoryScore для полученных категорий
            List<CategoryUserViewModel> categoryViewModels = new List<CategoryUserViewModel>();
            List<string> bestcategories = new List<string>();
            foreach (int categoryId in categoryIds)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    var userToCategoryScore = new UserToCategoryScore
                    {
                        UserId = newUser.Id,
                        CategoryId = categoryId,
                        Level = 1
                    };
                    _context.UserToCategoryScores.Add(userToCategoryScore);

                    categoryViewModels.Add(new CategoryUserViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Level = 1
                    });
                    bestcategories.Add(category.Name);
                }
            }
            await _context.SaveChangesAsync();

            return Ok(categoryViewModels);
        }

        [HttpPost]
        [Route("getAvatarInitial/{vkid}/{sex}")]
        public async Task<ActionResult<string>> getAvatarInitial(int vkid, int sexint)
        {
            // Ищем пользователя по VkId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VkId == vkid);

            if (user == null)
            {
                return NotFound("Пользователь с указанным VkId не найден.");
            }

            // Ищем записи в UserToCategoryScore для данного пользователя и сортируем по Level
            var bestCategories = await _context.UserToCategoryScores
                .Where(ucs => ucs.UserId == user.Id)
                .OrderByDescending(ucs => ucs.Level)
                .Take(3) // Берём топ-3 категории по уровню
                .Join(
                    _context.Categories,
                    ucs => ucs.CategoryId,
                    category => category.Id,
                    (ucs, category) => new CategoryUserViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Level = ucs.Level
                    }
                )
                .ToListAsync();

            List<string> bestcategories = new();

            foreach (var item in bestCategories)
            {
                bestcategories.Add(item.Name);
            }


            string sex;
            if (sexint == 1)
            {
                sex = "female";
            }
            else
            {
                sex = "male";
            }

            string avatar = await GPTAvatar(bestcategories, sex);
            user.AvatarUrl = avatar;
            await _context.SaveChangesAsync();

            return Ok(avatar);
        }
    }

    // Вспомогательные классы для парсинга JSON
    public class CategoriesResponse
    {
        [JsonProperty("res")]
        public List<Category> Res { get; set; }
    }
}
