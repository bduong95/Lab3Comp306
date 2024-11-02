using Microsoft.AspNetCore.Mvc;
using MovieStreamingApp.Models;
using System.Linq;

namespace MovieStreamingApp.Controllers
{
    public class AccountController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("", "Username already exists.");
                    return View();
                }

                // Add user to database and save changes
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View();
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(User user)
        {
            var loginUser = _context.Users
                .FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);

            if (loginUser != null)
            {
                // Store the user ID in the session
                HttpContext.Session.SetInt32("UserId", loginUser.Id);
                return RedirectToAction("Index", "Movies");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Login");
        }
    }
}
