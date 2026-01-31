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
            var _todoItems = _db.TodoItems.ToList();
            return View(_todoItems);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ListItemModel newItem)
        {
            if (ModelState.IsValid)
            {
                _db.TodoItems.Add(newItem); 
                _db.SaveChanges(); 
                return RedirectToAction("Index");
            }
            return View(newItem);
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
