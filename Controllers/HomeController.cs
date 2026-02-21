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
            var rememeberEmail = Request.Cookies["RememberMeCookie"];
            var userEmail = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(rememeberEmail))
            {
                HttpContext.Session.SetString("UserSession", rememeberEmail);
                userEmail = rememeberEmail;
            }
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var list = _db.TodoItems.Where(item => item.UserEmail == userEmail).ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
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
        public IActionResult Create(ListItemModel newItem)
        {
            var userEmail = HttpContext.Session.GetString("UserSession");
            if (userEmail != null)
            {
                newItem.UserEmail = userEmail;

                if (newItem.Description == null)
                {
                    newItem.Description = "";
                }

                _db.TodoItems.Add(newItem);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", "Account");
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
