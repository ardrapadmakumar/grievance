using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grievance.Data;
using grievance.Models;

namespace grievance.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;
        public TeacherController(AppDbContext db) { _db = db; }

        // ── Auth helper ───────────────────────────────────────────────────────
        private async Task<User?> GetCurrentUser()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            if (id == null || HttpContext.Session.GetString("UserRole") != "Teacher")
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
                .Where(g => g.Department == user.Department)
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            var marks = await _db.InternalMarks
                .Include(m => m.Student)
                .Include(m => m.RevaluationRequest)
                .Where(m => m.Department == user.Department)
                .ToListAsync();

            var pendingRevals = marks
                .Where(m => m.RevaluationRequest != null &&
                            m.RevaluationRequest.Status == RevaluationStatus.Requested)
                .Count();

            ViewBag.User = user;
            ViewBag.Grievances = grievances;
            ViewBag.Marks = marks;
            ViewBag.Total = grievances.Count;
            ViewBag.Pending = grievances.Count(g => g.Status == GrievanceStatus.Pending);
            ViewBag.Solved = grievances.Count(g => g.Status == GrievanceStatus.Solved);
            ViewBag.Forwarded = grievances.Count(g => g.ForwardedToPrincipal);
            ViewBag.PendingRevals = pendingRevals;

            return View();
        }

        // ── Grievances ────────────────────────────────────────────────────────
        public async Task<IActionResult> Grievances()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var grievances = await _db.Grievances
                .Include(g => g.Student)
                .Where(g => g.Department == user.Department)
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            ViewBag.User = user;
            return View(grievances);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrievance(int id, GrievanceStatus status, string remarks)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var g = await _db.Grievances.FindAsync(id);
            if (g == null || g.Department != user.Department)
            {
                TempData["Error"] = "Grievance not found.";
                return RedirectToAction("Grievances");
            }

            g.Status = status;
            g.TeacherRemarks = remarks ?? "";
            g.UpdatedAt = DateTime.Now;

            if (status == GrievanceStatus.Forwarded)
                g.ForwardedToPrincipal = true;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Grievance status updated.";
            return RedirectToAction("Grievances");
        }

        // ── Marks ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Marks()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var marks = await _db.InternalMarks
                .Include(m => m.Student)
                .Include(m => m.RevaluationRequest)
                .Where(m => m.Department == user.Department)
                .OrderBy(m => m.Student.RollNumber)
                .ThenBy(m => m.SubjectCode)
                .ToListAsync();

            var students = await _db.Users
                .Where(u => u.Role == "Student" && u.Department == user.Department)
                .OrderBy(u => u.RollNumber)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Students = students;
            return View(marks);
        }

        [HttpPost]
        public async Task<IActionResult> AddMark(MarkFormViewModel vm)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _db.Users.FindAsync(vm.StudentId);
            if (student == null || student.Department != user.Department)
            {
                TempData["Error"] = "Invalid student.";
                return RedirectToAction("Marks");
            }

            // Check duplicate subject for same student
            bool exists = await _db.InternalMarks.AnyAsync(m =>
                m.StudentId == vm.StudentId && m.SubjectCode == vm.SubjectCode);
            if (exists)
            {
                TempData["Error"] = $"Marks for {vm.SubjectCode} already exist for this student. Use edit instead.";
                return RedirectToAction("Marks");
            }

            var mark = new InternalMark
            {
                StudentId = vm.StudentId,
                TeacherId = user.Id,
                Department = user.Department,
                SubjectCode = vm.SubjectCode,
                SubjectName = vm.SubjectName,
                Mark = vm.Mark,
                MaxMark = vm.MaxMark,
                Grade = AppDbContext.CalculateGrade(vm.Mark, vm.MaxMark),
                UpdatedAt = DateTime.Now
            };

            _db.InternalMarks.Add(mark);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Mark added successfully.";
            return RedirectToAction("Marks");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMark(int id, int mark, int maxMark, string subjectName)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var m = await _db.InternalMarks.FindAsync(id);
            if (m == null || m.Department != user.Department)
            {
                TempData["Error"] = "Mark not found.";
                return RedirectToAction("Marks");
            }

            m.Mark = mark;
            m.MaxMark = maxMark;
            m.SubjectName = subjectName;
            m.Grade = AppDbContext.CalculateGrade(mark, maxMark);
            m.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Mark updated.";
            return RedirectToAction("Marks");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMark(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var m = await _db.InternalMarks
                .Include(x => x.RevaluationRequest)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null || m.Department != user.Department)
            {
                TempData["Error"] = "Mark not found.";
                return RedirectToAction("Marks");
            }

            _db.InternalMarks.Remove(m);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Mark deleted.";
            return RedirectToAction("Marks");
        }

        // ── Revaluations ──────────────────────────────────────────────────────
        public async Task<IActionResult> Revaluations()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var revals = await _db.RevaluationRequests
                .Include(r => r.Mark)
                    .ThenInclude(m => m.Student)
                .Where(r => r.Mark.Department == user.Department)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            ViewBag.User = user;
            return View(revals);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRevaluation(int id, RevaluationStatus status,
                                                            string feedback, int? newMark)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var reval = await _db.RevaluationRequests
                .Include(r => r.Mark)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reval == null || reval.Mark.Department != user.Department)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Revaluations");
            }

            reval.Status = status;
            reval.TeacherFeedback = feedback ?? "";
            reval.UpdatedAt = DateTime.Now;

            // If grade changed, update the actual mark
            if (status == RevaluationStatus.GradeChanged && newMark.HasValue)
            {
                reval.Mark.Mark = newMark.Value;
                reval.Mark.Grade = AppDbContext.CalculateGrade(newMark.Value, reval.Mark.MaxMark);
                reval.Mark.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Revaluation updated.";
            return RedirectToAction("Revaluations");
        }
    }
}