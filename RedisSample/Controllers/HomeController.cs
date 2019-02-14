using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisSample.Models;

namespace RedisSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public HomeController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            Person person = new Person();
            person.Name = "Emrah";
            person.Surname = "Ozguner";
            person.Age = 30;
            person.isMail = false;

            var data = JsonConvert.SerializeObject(person);
            var dataByte = Encoding.UTF8.GetBytes(data);
            _distributedCache.Set("Person", dataByte);
            _distributedCache.SetString("Person2", data);
        }

        public async Task<IActionResult> Index()
        {
            var cacheKey = "Time";
            var existingTime = _distributedCache.GetString(cacheKey);
            if (string.IsNullOrEmpty(existingTime))
            {
                existingTime = DateTime.UtcNow.ToString();
                var option = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(5));
                option.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                string name = await _distributedCache.GetStringAsync("Name");
                await _distributedCache.SetStringAsync(cacheKey, $"{name}: {existingTime}", option);
            }
            ViewBag.Time = await _distributedCache.GetStringAsync(cacheKey);
            /* //Person
            var personByte=await _distributedCache.GetAsync("Person");
            var personString=Encoding.UTF8.GetString(personByte);
            var person=JsonConvert.DeserializeObject<Person>(personString);
            return View(person); */
            //Person2
            var personString = await _distributedCache.GetStringAsync("Person");
            var person = JsonConvert.DeserializeObject<Person>(personString);
            return View(person);
        }

        public IActionResult Detail()
        {
            string value = _distributedCache.GetString("CacheTime");

            if (value == null)
            {
                value = DateTime.Now.ToString();
                var options = new DistributedCacheEntryOptions();
                options.SetSlidingExpiration(TimeSpan.FromMinutes(1));
                _distributedCache.SetString("CacheTime", value, options);
            }

            ViewData["CacheTime"] = value;
            ViewData["CurrentTime"] = DateTime.Now.ToString();

            return View();
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}