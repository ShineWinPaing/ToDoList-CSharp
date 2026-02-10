using Microsoft.AspNetCore.Mvc;
using ToDoList.Data;
using ToDoList.Models;
using Microsoft.AspNetCore.Identity;

namespace ToDoList.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<UserModel> _passwordHasher;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
            _passwordHasher = new PasswordHasher<UserModel>();
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
                if (_db.Users.Any(u => u.Email ==newUser.Email))
                {
                    ViewBag.Error = "Email already registered.";
                    return View(newUser);
                }
                newUser.Password = _passwordHasher.HashPassword(newUser, newUser.Password);

                newUser.OTP = new Random().Next(100000,999999).ToString();

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
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    if (!user.IsVerified)
                    {
                        return RedirectToAction("Verify", new { email = user.Email });
                    }

                    HttpContext.Session.SetString("UserSession", user.Email);
                    return RedirectToAction("Index", "Home");
                }
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