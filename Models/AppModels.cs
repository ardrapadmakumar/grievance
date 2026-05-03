using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace grievance.Models
{
    // ─── Enums ───────────────────────────────────────────────────────────────
    public enum Department { CSE, AIML, MECH, CIVIL, ECE, EEE }

    public enum GrievanceCategory { Academic, Infrastructure, Administration, Faculty, Other }

    public enum GrievanceStatus
    {
        Pending,
        InProgress,
        Solved,
        Forwarded,      // Teacher → Principal
        Resolved,       // Principal final decision
        Rejected,
        UnderReview
    }

    public enum RevaluationStatus
    {
        Requested,
        OngoingEvaluation,
        NoChange,
        GradeChanged
    }

    // ─── Entities ─────────────────────────────────────────────────────────────

    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        // "Student" | "Teacher" | "Principal"
        [Required, MaxLength(20)]
        public string Role { get; set; } = "";

        public Department Department { get; set; }

        [MaxLength(20)]
        public string RollNumber { get; set; } = "";   // students only

        [MaxLength(20)]
        public string EmployeeId { get; set; } = "";   // teachers only

        // Navigation
        public ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
        public ICollection<InternalMark> Marks { get; set; } = new List<InternalMark>();
    }

    public class Grievance
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        public GrievanceCategory Category { get; set; }
        public GrievanceStatus Status { get; set; } = GrievanceStatus.Pending;

        // FK to student
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public Department Department { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(500)]
        public string TeacherRemarks { get; set; } = "";

        [MaxLength(500)]
        public string PrincipalRemarks { get; set; } = "";

        public bool ForwardedToPrincipal { get; set; } = false;
    }

    public class InternalMark
    {
        public int Id { get; set; }

        // FK to student
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        // FK to teacher who entered the mark
        public int TeacherId { get; set; }
        public User Teacher { get; set; } = null!;

        public Department Department { get; set; }

        [Required, MaxLength(20)]
        public string SubjectCode { get; set; } = "";

        [Required, MaxLength(100)]
        public string SubjectName { get; set; } = "";

        public int Mark { get; set; }
        public int MaxMark { get; set; } = 50;

        [MaxLength(5)]
        public string Grade { get; set; } = "";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public RevaluationRequest? RevaluationRequest { get; set; }
    }

    public class RevaluationRequest
    {
        public int Id { get; set; }

        // FK to InternalMark (one-to-one)
        public int MarkId { get; set; }
        public InternalMark Mark { get; set; } = null!;

        public int StudentId { get; set; }

        [Required]
        public string Reason { get; set; } = "";

        public RevaluationStatus Status { get; set; } = RevaluationStatus.Requested;

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(500)]
        public string TeacherFeedback { get; set; } = "";
    }

    // ─── ViewModels ───────────────────────────────────────────────────────────

    public class LoginViewModel
    {
        [Required] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required] public string Name { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        public Department Department { get; set; }
        public string RollNumber { get; set; } = "";
    }

    public class GrievanceFormViewModel
    {
        [Required] public string Title { get; set; } = "";
        [Required] public string Description { get; set; } = "";
        public GrievanceCategory Category { get; set; }
    }

    public class MarkFormViewModel
    {
        public int StudentId { get; set; }
        [Required] public string SubjectCode { get; set; } = "";
        [Required] public string SubjectName { get; set; } = "";
        [Range(0, 100)] public int Mark { get; set; }
        public int MaxMark { get; set; } = 50;
    }
}