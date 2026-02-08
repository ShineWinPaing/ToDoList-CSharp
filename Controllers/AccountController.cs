using Microsoft.AspNetCore.Mvc;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Verify(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult Verify(string email, string otp)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email && u.OTP == otp);

            if (user != null)
            {
                user.IsVerified = true;
                user.OTP = null;
                _db.SaveChanges();

                return RedirectToAction("Login");
            }

            ViewBag.Email = email;
            ViewBag.Error = "Invalid OTP. Please try again.";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserModel newUser)
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.Email == newUser.Email))
                {
                    ViewBag.Error = "This email is already registered.";
                    return View(newUser);
                }

                string otpCode = new Random().Next(100000, 999999).ToString();
                newUser.OTP = otpCode;
                newUser.IsVerified = false;

                _db.Users.Add(newUser);
                _db.SaveChanges();

                return RedirectToAction("Verify", new { email = newUser.Email });
            }
            return View(newUser);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                if (!user.IsVerified)
                {
                    ViewBag.Error = "Please verify your email before logging in.";
                    return RedirectToAction("Verify", new { email = user.Email });
                }

                HttpContext.Session.SetString("UserSession", user.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}