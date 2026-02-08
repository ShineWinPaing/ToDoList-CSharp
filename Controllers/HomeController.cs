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
            var userEmail = HttpContext.Session.GetString("UserSession");

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
                _db.TodoItems.Update(updateItem);
                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(updateItem);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            ListItemModel itemToRemove = null;
            foreach(var item in _db.TodoItems)
            {
                if (item.Id == id)
                {
                    itemToRemove = item;
                    break;
                }
            }
            if (itemToRemove != null)
            {
                _db.TodoItems.Remove(itemToRemove);
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
