using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grievance.Data;
using grievance.Models;

namespace grievance.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;
        public StudentController(AppDbContext db) { _db = db; }

        // ── Auth helper ───────────────────────────────────────────────────────
        private async Task<User?> GetCurrentUser()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            if (id == null || HttpContext.Session.GetString("UserRole") != "Student")
                return null;
            return await _db.Users.FindAsync(id.Value);
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var grievances = await _db.Grievances
                .Where(g => g.StudentId == user.Id)
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            var marks = await _db.InternalMarks
                .Include(m => m.RevaluationRequest)
                .Where(m => m.StudentId == user.Id)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Grievances = grievances;
            ViewBag.Marks = marks;
            ViewBag.Total = grievances.Count;
            ViewBag.Pending = grievances.Count(g => g.Status == GrievanceStatus.Pending);
            ViewBag.Solved = grievances.Count(g => g.Status == GrievanceStatus.Solved || g.Status == GrievanceStatus.Resolved);
            ViewBag.Forwarded = grievances.Count(g => g.ForwardedToPrincipal);

            return View();
        }

        // ── Submit Grievance ──────────────────────────────────────────────────
        public async Task<IActionResult> SubmitGrievance()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitGrievance(GrievanceFormViewModel vm)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields.";
                return View(vm);
            }

            var g = new Grievance
            {
                Title = vm.Title,
                Description = vm.Description,
                Category = vm.Category,
                Status = GrievanceStatus.Pending,
                StudentId = user.Id,
                Department = user.Department,
                SubmittedAt = DateTime.Now
            };

            _db.Grievances.Add(g);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Grievance submitted successfully!";
            return RedirectToAction("Index");
        }

        // ── My Marks ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Marks()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var marks = await _db.InternalMarks
                .Include(m => m.RevaluationRequest)
                .Where(m => m.StudentId == user.Id)
                .OrderBy(m => m.SubjectCode)
                .ToListAsync();

            ViewBag.User = user;
            return View(marks);
        }

        // ── Request Revaluation ───────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RequestRevaluation(int markId, string reason)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            // Check mark belongs to this student
            var mark = await _db.InternalMarks.FindAsync(markId);
            if (mark == null || mark.StudentId != user.Id)
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction("Marks");
            }

            // Only one request per mark
            bool exists = await _db.RevaluationRequests.AnyAsync(r => r.MarkId == markId);
            if (exists)
            {
                TempData["Error"] = "A revaluation request already exists for this subject.";
                return RedirectToAction("Marks");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Please provide a reason for revaluation.";
                return RedirectToAction("Marks");
            }

            var req = new RevaluationRequest
            {
                MarkId = markId,
                StudentId = user.Id,
                Reason = reason,
                Status = RevaluationStatus.Requested,
                RequestedAt = DateTime.Now
            };

            _db.RevaluationRequests.Add(req);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Revaluation request submitted!";
            return RedirectToAction("Marks");
        }

        // ── Grievance Detail ──────────────────────────────────────────────────
        public async Task<IActionResult> GrievanceDetail(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var g = await _db.Grievances.FindAsync(id);
            if (g == null || g.StudentId != user.Id) return NotFound();

            ViewBag.User = user;
            return View(g);
        }
    }
}