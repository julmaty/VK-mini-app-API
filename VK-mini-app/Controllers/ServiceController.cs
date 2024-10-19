using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using VK_mini_app.Models;
using VK_mini_app.Models.Request;
using VK_mini_app.Models.Response;
using static System.Net.WebRequestMethods;

namespace VK_mini_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration Configuration;
        private readonly ApplicationContext _context;

        private readonly ILogger<ServiceController> _logger;

        public ServiceController(ILogger<ServiceController> logger, IConfiguration configuration, ApplicationContext context)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            Configuration = configuration;
            _context = context;
        }

        [HttpGet]
        [Route("GetUser")]
        public async Task<BaseResponse<List<GetUsersResponse>>> GetUser()
        {
            var apiSettings = Configuration.GetSection("VkApi");
            string url = $"{apiSettings["Api_url"]}users.get";
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["user_ids"] = "12287614";
            queryString["v"] = "5.199";
            queryString["access_token"] = apiSettings["Server_key"];
            queryString["fields"] = "activities, about, books, education, sex, games, interests, movies, music, quotes";
            var postValues = new FormUrlEncodedContent(queryString.AllKeys.ToDictionary(k => k, k => queryString[k]));
            var response = await _httpClient
                .PostAsync($"{url}?{queryString}", postValues)
                .ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var userResponse = JsonConvert.DeserializeObject<BaseResponse<List<GetUsersResponse>>>(json);

            return userResponse;
        }

        [HttpGet]
        [Route("GetInfo")]
        public async Task<JsonType<UserInfo>> GetInfo()
        {
            JsonType<UserInfo> items;
            using (StreamReader r = new StreamReader("Isu.json"))
            {
                string jsonText = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<JsonType<UserInfo>>(jsonText);
            }

            foreach (var item in items.res)
            {
                _context.UserInfos.Add(item);
            }

            await _context.SaveChangesAsync();

            return items;
        }

        [HttpGet]
        [Route("GetCategories")]
        public async Task<JsonType<Category>> GetCategories()
        {
            JsonType<Category> items;
            using (StreamReader r = new StreamReader("categories.json"))
            {
                string jsonText = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<JsonType<Category>>(jsonText);
            }

            foreach (var item in items.res)
            {
                _context.Categories.Add(item);
            }

            await _context.SaveChangesAsync();
            return items;
        }

        [HttpGet]
        [Route("GetCategoriesRus")]
        public async Task<JsonType<CategoryRus>> GetCategoriesRus()
        {
            JsonType<CategoryRus> items;
            using (StreamReader r = new StreamReader("categories_rus.json"))
            {
                string jsonText = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<JsonType<CategoryRus>>(jsonText);
            }

            foreach (var item in items.res)
            {
                _context.CategoriesRus.Add(item);
            }

            await _context.SaveChangesAsync();
            return items;
        }

        [HttpGet]
        [Route("GetTasks")]
        public async Task<JsonType<Models.Task>> GetTasks()
        {
            JsonType<Models.Task> items;
            using (StreamReader r = new StreamReader("tasks.json"))
            {
                string jsonText = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<JsonType<Models.Task>>(jsonText);
            }

            foreach (var item in items.res)
            {
                _context.Tasks.Add(item);
            }

            await _context.SaveChangesAsync();
            return items;
        }

        [HttpGet]
        [Route("GPT")]
        public async Task<string> GPT()
        {
            var apiSettings = Configuration.GetSection("GPT");
            // токен из личного кабинета
            string apiKey = apiSettings["Key"];
            // адрес api для взаимодействия с чат-ботом
            string endpoint = "https://api.openai.com/v1/chat/completions";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = "Напиши идеи для проекта в сфере образования" }
                },
                    max_tokens = 100,
                    temperature = 0.7
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                return responseObject.choices[0].message.content.ToString();
            }
        }

        [HttpGet]
        [Route("GPTAvatar")]
        public async Task<string> GPTAvatar()
        {
            var apiSettings = Configuration.GetSection("GPT");
            // токен из личного кабинета
            string apiKey = apiSettings["Key"];
            // адрес api для взаимодействия с чат-ботом
            string endpoint = "https://api.openai.com/v1/images/generations";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = "A cat sitting on a beach",
                    n = 1, // количество изображений для генерации
                    size = "1024x1024" // размер изображения
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                return responseObject.data[0].url.ToString();
            }
        }
    }
}
