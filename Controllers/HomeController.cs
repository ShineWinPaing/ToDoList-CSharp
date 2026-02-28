using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var rememberMeCookie = Request.Cookies["RememberMeCookie"];

            if (userId == null && !string.IsNullOrEmpty(rememberMeCookie))
            {
                if (int.TryParse(rememberMeCookie,out int id))
                {
                    HttpContext.Session.SetInt32("UserId",id);
                    userId = id;
                }
                if (userId==null)
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            var list = _db.TodoItems.Where(item => item.UserId == userId).ToList();
            return View(list);
        }

        [HttpGet]
        public JsonResult GetCalendarTasks()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { Error = "User not logged in" });
            }

            var tasks = _db.TodoItems
                .Where(item => item.UserId == userId)
                .Select(item => new
                {
                    id = item.Id,
                    title = item.Title,
                    start = item.DueDate.ToString("yyyy-MM-dd"),
                    color = item.IsCompleted ? "#28a745" : "#dc3545" 
                }).ToList();

            return Json(tasks);
        }

        public IActionResult Create(string? date)
        {
            var model = new ListItemModel();
            if (!string.IsNullOrEmpty(date))
            {
                model.DueDate = DateTime.Parse(date);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(ListItemModel newItem)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                newItem.UserId = userId.Value;

                if (newItem.Description==null)
                {
                    newItem.Description = "";
                }

                _db.TodoItems.Add(newItem);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Edit(int id)
        {
            var item = _db.TodoItems.Find(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(ListItemModel updateItem)
        {
            if (ModelState.IsValid)
            {
                var existingItem = _db.TodoItems.Find(updateItem.Id);

                if (existingItem != null)
                {
                    existingItem.Title = updateItem.Title;
                    existingItem.Description = updateItem.Description;
                    existingItem.IsCompleted = updateItem.IsCompleted;

                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return View(updateItem);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _db.TodoItems.Find(id);
            if (item != null)
            {
                _db.TodoItems.Remove(item);
                _db.SaveChanges();
            }


            return RedirectToAction("Index");
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
