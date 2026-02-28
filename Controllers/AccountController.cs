using Microsoft.AspNetCore.Mvc;
using ToDoList.Data;
using ToDoList.Models;
using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace ToDoList.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<UserModel> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext db,IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
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

                string otpCode = new Random().Next(100000, 999999).ToString();
                newUser.OTP = otpCode;

                newUser.IsVerified = false;

                try
                {
                    SendEmail(newUser.Email, otpCode);
                }catch (Exception ex)
                {
                    ViewBag.Error = $"Email Error: {ex.Message}";
                    return View(newUser);
                }

                _db.Users.Add(newUser);
                _db.SaveChanges();

                return RedirectToAction("Verify", new { email = newUser.Email });   
            }
            return View(newUser);
        }

        public IActionResult Login()
        {
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            var rememberMeCookie = Request.Cookies["RememberMeCookie"];

            if (userIdSession != null ||!string.IsNullOrEmpty(rememberMeCookie))
            {
                if (userIdSession == null && int.TryParse(rememberMeCookie, out int id))
                {
                    HttpContext.Session.SetInt32("UserId",id);

                    var user = _db.Users.Find(id);
                    if (user != null) HttpContext.Session.SetString("UserSession",user.Email);
                }
                return RedirectToAction("Index", "Home");
            }


            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password, bool rememberMe)
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
                    HttpContext.Session.SetInt32("UserId", user.Id);

                    if(rememberMe)
                    {
                        var options = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            IsEssential = true,
                            Path = "/"
                        };
                        Response.Cookies.Append("RememberMeCookie", user.Id.ToString(), options);                    }

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        private void SendEmail(string receiverEmail, string otpCode)
        {
            var senderEmail = _configuration["EmailSettings:Email"];
            var appPassword = _configuration["EmailSettings:Password"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword))
            {
                throw new Exception("Email configuration is missing from appsettings.json!");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ToDo List support", senderEmail));
            message.To.Add(new MailboxAddress("",receiverEmail));

            message.Subject = $"ToDo App Verification Code";

            var bodyBuilder = new BodyBuilder();

            bodyBuilder.TextBody = $"Welcome to ToDo App! Your Verification Code : {otpCode}";

            bodyBuilder.HtmlBody = $@"
            <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee;'>
            <h2 style='color: #2c3e50;'>Verify Your Account</h2>
            <p>Thank you for joining ToDo App. Please use the code below to complete your registration:</p>
            <div style='font-size: 24px; font-weight: bold; color: #e74c3c; padding: 10px; background: #f9f9f9; display: inline-block;'>
                {otpCode}
            </div>
            <p style='font-size: 12px; color: #7f8c8d; margin-top: 20px;'>If you did not request this, please ignore this email.</p>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client=new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                client.Authenticate(senderEmail,appPassword );

                client.Send(message);
                client.Disconnect(true);
            }

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMeCookie");
            return RedirectToAction("Login");
        }
    }
}