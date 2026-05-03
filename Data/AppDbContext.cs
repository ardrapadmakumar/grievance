using Microsoft.EntityFrameworkCore;
using grievance.Models;

namespace grievance.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Grievance> Grievances => Set<Grievance>();
        public DbSet<InternalMark> InternalMarks => Set<InternalMark>();
        public DbSet<RevaluationRequest> RevaluationRequests => Set<RevaluationRequest>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── Indexes ────────────────────────────────────────────
            mb.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // ── Relationships ──────────────────────────────────────

            // Grievance → Student (restrict to avoid cascade cycles)
            mb.Entity<Grievance>()
                .HasOne(g => g.Student)
                .WithMany(u => u.Grievances)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // InternalMark → Student
            mb.Entity<InternalMark>()
                .HasOne(m => m.Student)
                .WithMany(u => u.Marks)
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // InternalMark → Teacher (no cascade)
            mb.Entity<InternalMark>()
                .HasOne(m => m.Teacher)
                .WithMany()
                .HasForeignKey(m => m.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // RevaluationRequest → InternalMark (one-to-one)
            mb.Entity<RevaluationRequest>()
                .HasOne(r => r.Mark)
                .WithOne(m => m.RevaluationRequest)
                .HasForeignKey<RevaluationRequest>(r => r.MarkId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Seed Data ──────────────────────────────────────────
            // Password hash for "password123" — simple SHA256-based approach
            // In production use ASP.NET Identity; this is for demo purposes
            string teacherHash = HashPassword("teacher123");
            string studentHash = HashPassword("student123");
            string principalHash = HashPassword("principal123");

            mb.Entity<User>().HasData(
                // Principal
                new User { Id = 1, Name = "Dr. S. Krishnamurthy", Email = "principal@college.edu", PasswordHash = principalHash, Role = "Principal", Department = Department.CSE },

                // Teachers
                new User { Id = 2, Name = "Prof. Ravi Kumar", Email = "ravi@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.CSE, EmployeeId = "T001" },
                new User { Id = 3, Name = "Prof. Meena Sharma", Email = "meena@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.AIML, EmployeeId = "T002" },
                new User { Id = 4, Name = "Prof. Suresh Nair", Email = "suresh@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.MECH, EmployeeId = "T003" },
                new User { Id = 5, Name = "Prof. Anjali Pillai", Email = "anjali@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.ECE, EmployeeId = "T004" },
                new User { Id = 6, Name = "Prof. Deepak Menon", Email = "deepak@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.CIVIL, EmployeeId = "T005" },
                new User { Id = 7, Name = "Prof. Latha Rao", Email = "latha@college.edu", PasswordHash = teacherHash, Role = "Teacher", Department = Department.EEE, EmployeeId = "T006" },

                // Students – CSE
                new User { Id = 8, Name = "Arjun Dev", Email = "arjun@student.edu", PasswordHash = studentHash, Role = "Student", Department = Department.CSE, RollNumber = "CSE001" },
                new User { Id = 9, Name = "Priya Menon", Email = "priya@student.edu", PasswordHash = studentHash, Role = "Student", Department = Department.CSE, RollNumber = "CSE002" },
                // Students – AIML
                new User { Id = 10, Name = "Rahul Krishnan", Email = "rahul@student.edu", PasswordHash = studentHash, Role = "Student", Department = Department.AIML, RollNumber = "AIML001" },
                // Students – MECH
                new User { Id = 11, Name = "Sneha Thomas", Email = "sneha@student.edu", PasswordHash = studentHash, Role = "Student", Department = Department.MECH, RollNumber = "MECH001" }
            );

            mb.Entity<InternalMark>().HasData(
                new InternalMark { Id = 1, StudentId = 8, TeacherId = 2, Department = Department.CSE, SubjectCode = "CS301", SubjectName = "Data Structures", Mark = 38, MaxMark = 50, Grade = "B+", UpdatedAt = new DateTime(2025, 3, 10) },
                new InternalMark { Id = 2, StudentId = 8, TeacherId = 2, Department = Department.CSE, SubjectCode = "CS302", SubjectName = "Operating Systems", Mark = 45, MaxMark = 50, Grade = "A", UpdatedAt = new DateTime(2025, 3, 10) },
                new InternalMark { Id = 3, StudentId = 9, TeacherId = 2, Department = Department.CSE, SubjectCode = "CS301", SubjectName = "Data Structures", Mark = 42, MaxMark = 50, Grade = "A", UpdatedAt = new DateTime(2025, 3, 10) },
                new InternalMark { Id = 4, StudentId = 10, TeacherId = 3, Department = Department.AIML, SubjectCode = "AI201", SubjectName = "Machine Learning", Mark = 40, MaxMark = 50, Grade = "B+", UpdatedAt = new DateTime(2025, 3, 11) }
            );

            mb.Entity<Grievance>().HasData(
                new Grievance { Id = 1, Title = "Lab computers outdated", Description = "Computers in Lab 3 crash constantly during practicals.", Category = GrievanceCategory.Infrastructure, Status = GrievanceStatus.Pending, StudentId = 8, Department = Department.CSE, SubmittedAt = new DateTime(2025, 4, 1) },
                new Grievance { Id = 2, Title = "Exam schedule conflict", Description = "Two exams scheduled the same day with no break.", Category = GrievanceCategory.Academic, Status = GrievanceStatus.InProgress, StudentId = 9, Department = Department.CSE, SubmittedAt = new DateTime(2025, 4, 3) },
                new Grievance { Id = 3, Title = "Classroom ventilation issue", Description = "No AC in Room 204, causing discomfort during lectures.", Category = GrievanceCategory.Infrastructure, Status = GrievanceStatus.Forwarded, StudentId = 10, Department = Department.AIML, SubmittedAt = new DateTime(2025, 4, 5), ForwardedToPrincipal = true }
            );
        }

        // Simple deterministic hash — replace with BCrypt in production
        public static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "_gms_salt_2025");
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        public static bool VerifyPassword(string password, string hash) =>
            HashPassword(password) == hash;

        public static string CalculateGrade(int mark, int max)
        {
            double pct = (double)mark / max * 100;
            return pct >= 90 ? "A+" : pct >= 80 ? "A" : pct >= 70 ? "B+" : pct >= 60 ? "B" : pct >= 50 ? "C" : "F";
        }
    }
}