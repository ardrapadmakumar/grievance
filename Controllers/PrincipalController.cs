using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grievance.Data;
using grievance.Models;

namespace grievance.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly AppDbContext _db;
        public PrincipalController(AppDbContext db) { _db = db; }

        // ── Auth helper ───────────────────────────────────────────────────────
        private async Task<User?> GetCurrentUser()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            if (id == null || HttpContext.Session.GetString("UserRole") != "Principal")
                return null;
            return await _db.Users.FindAsync(id.Value);
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var grievances = await _db.Grievances
                .Include(g => g.Student)
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            // Per-department breakdown
            var deptStats = Enum.GetValues<Department>()
                .Select(d => new
                {
                    Dept = d.ToString(),
                    Total = grievances.Count(g => g.Department == d),
                    Pending = grievances.Count(g => g.Department == d && g.Status == GrievanceStatus.Pending),
                    Solved = grievances.Count(g => g.Department == d &&
                                    (g.Status == GrievanceStatus.Solved || g.Status == GrievanceStatus.Resolved)),
                    Forwarded = grievances.Count(g => g.Department == d && g.ForwardedToPrincipal)
                })
                .Where(d => d.Total > 0)
                .ToList();

            ViewBag.User = user;
            ViewBag.Grievances = grievances;
            ViewBag.DeptStats = deptStats;
            ViewBag.Total = grievances.Count;
            ViewBag.Pending = grievances.Count(g => g.Status == GrievanceStatus.Pending);
            ViewBag.Solved = grievances.Count(g => g.Status == GrievanceStatus.Solved || g.Status == GrievanceStatus.Resolved);
            ViewBag.Forwarded = grievances.Count(g => g.ForwardedToPrincipal);
            ViewBag.UnderReview = grievances.Count(g => g.Status == GrievanceStatus.UnderReview);

            return View();
        }

        // ── All Grievances (with filter) ──────────────────────────────────────
        public async Task<IActionResult> Grievances(string? dept, string? status)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _db.Grievances
                .Include(g => g.Student)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dept) && Enum.TryParse<Department>(dept, out var deptEnum))
                query = query.Where(g => g.Department == deptEnum);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<GrievanceStatus>(status, out var statusEnum))
                query = query.Where(g => g.Status == statusEnum);

            var grievances = await query
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.SelectedDept = dept;
            ViewBag.SelectedStatus = status;

            return View(grievances);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrievance(int id, GrievanceStatus status, string remarks)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var g = await _db.Grievances.FindAsync(id);
            if (g == null)
            {
                TempData["Error"] = "Grievance not found.";
                return RedirectToAction("Grievances");
            }

            g.Status = status;
            g.PrincipalRemarks = remarks ?? "";
            g.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Grievance status updated.";
            return RedirectToAction("Grievances");
        }

        // ── Students overview ─────────────────────────────────────────────────
        public async Task<IActionResult> Students()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var students = await _db.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.Department)
                .ThenBy(u => u.RollNumber)
                .ToListAsync();

            // Grievance count per student
            var grievanceCounts = await _db.Grievances
                .GroupBy(g => g.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentId, x => x.Count);

            ViewBag.User = user;
            ViewBag.GrievanceCounts = grievanceCounts;

            return View(students);
        }


        // ── All Marks (add this method inside PrincipalController class) ──────────
        public async Task<IActionResult> Marks(string? dept, string? grade)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _db.InternalMarks
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .Include(m => m.RevaluationRequest)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dept) && Enum.TryParse<Department>(dept, out var deptEnum))
                query = query.Where(m => m.Department == deptEnum);

            if (!string.IsNullOrEmpty(grade))
                query = query.Where(m => m.Grade == grade);

            var marks = await query
                .OrderBy(m => m.Department)
                .ThenBy(m => m.Student.RollNumber)
                .ThenBy(m => m.SubjectCode)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.SelectedDept = dept ?? "";
            ViewBag.SelectedGrade = grade ?? "";
            ViewBag.TotalStudents = marks.Select(m => m.StudentId).Distinct().Count();
            ViewBag.TotalSubjects = marks.Count;
            ViewBag.Failing = marks.Count(m => m.Grade == "F");

            return View(marks);
        }
    }


}