using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grievance.Data;
using grievance.Models;

namespace grievance.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public IActionResult Login()
        {
            // Already logged in → redirect to dashboard
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToDashboard();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (user == null || !AppDbContext.VerifyPassword(vm.Password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View(vm);
            }

            // Store session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserDept", user.Department.ToString());

            return RedirectToDashboard(user.Role);
        }

        // ── Register (Students only) ──────────────────────────────────────────
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            bool emailExists = await _db.Users.AnyAsync(u => u.Email == vm.Email);
            if (emailExists)
            {
                ViewBag.Error = "An account with this email already exists.";
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(vm.RollNumber))
            {
                ViewBag.Error = "Roll number is required for students.";
                return View(vm);
            }

            var user = new User
            {
                Name = vm.Name,
                Email = vm.Email,
                PasswordHash = AppDbContext.HashPassword(vm.Password),
                Role = "Student",
                Department = vm.Department,
                RollNumber = vm.RollNumber
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserDept", user.Department.ToString());

            return RedirectToAction("Index", "Student");
        }

        // ── Logout ────────────────────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private IActionResult RedirectToDashboard(string? role = null)
        {
            role ??= HttpContext.Session.GetString("UserRole");
            return role switch
            {
                "Principal" => RedirectToAction("Index", "Principal"),
                "Teacher" => RedirectToAction("Index", "Teacher"),
                _ => RedirectToAction("Index", "Student")
            };
        }
    }
}