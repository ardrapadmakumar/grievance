using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace grievance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grievances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeacherRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PrincipalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ForwardedToPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grievances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grievances_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalMarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mark = table.Column<int>(type: "int", nullable: false),
                    MaxMark = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalMarks_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalMarks_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevaluationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevaluationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevaluationRequests_InternalMarks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "InternalMarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "Email", "EmployeeId", "Name", "PasswordHash", "Role", "RollNumber" },
                values: new object[,]
                {
                    { 1, 0, "principal@college.edu", "", "Dr. S. Krishnamurthy", "IZDj5oIcw1aQaFyKgfSdJgN56OkGpb9+P91APezA9Vs=", "Principal", "" },
                    { 2, 0, "ravi@college.edu", "T001", "Prof. Ravi Kumar", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 3, 1, "meena@college.edu", "T002", "Prof. Meena Sharma", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 4, 2, "suresh@college.edu", "T003", "Prof. Suresh Nair", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 5, 4, "anjali@college.edu", "T004", "Prof. Anjali Pillai", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 6, 3, "deepak@college.edu", "T005", "Prof. Deepak Menon", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 7, 5, "latha@college.edu", "T006", "Prof. Latha Rao", "XIEzXxRWVJmnVwvxlDClAwryKSgCyyHdBpN9rME6WWM=", "Teacher", "" },
                    { 8, 0, "arjun@student.edu", "", "Arjun Dev", "J8MWMBT7tpZyQsqEXAy9pdVczg8Ay/pARXfdIqJelXg=", "Student", "CSE001" },
                    { 9, 0, "priya@student.edu", "", "Priya Menon", "J8MWMBT7tpZyQsqEXAy9pdVczg8Ay/pARXfdIqJelXg=", "Student", "CSE002" },
                    { 10, 1, "rahul@student.edu", "", "Rahul Krishnan", "J8MWMBT7tpZyQsqEXAy9pdVczg8Ay/pARXfdIqJelXg=", "Student", "AIML001" },
                    { 11, 2, "sneha@student.edu", "", "Sneha Thomas", "J8MWMBT7tpZyQsqEXAy9pdVczg8Ay/pARXfdIqJelXg=", "Student", "MECH001" }
                });

            migrationBuilder.InsertData(
                table: "Grievances",
                columns: new[] { "Id", "Category", "Department", "Description", "ForwardedToPrincipal", "PrincipalRemarks", "Status", "StudentId", "SubmittedAt", "TeacherRemarks", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 0, "Computers in Lab 3 crash constantly during practicals.", false, "", 0, 8, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "Lab computers outdated", null },
                    { 2, 0, 0, "Two exams scheduled the same day with no break.", false, "", 1, 9, new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "Exam schedule conflict", null },
                    { 3, 1, 1, "No AC in Room 204, causing discomfort during lectures.", true, "", 3, 10, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "Classroom ventilation issue", null }
                });

            migrationBuilder.InsertData(
                table: "InternalMarks",
                columns: new[] { "Id", "Department", "Grade", "Mark", "MaxMark", "StudentId", "SubjectCode", "SubjectName", "TeacherId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0, "B+", 38, 50, 8, "CS301", "Data Structures", 2, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 0, "A", 45, 50, 8, "CS302", "Operating Systems", 2, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 0, "A", 42, 50, 9, "CS301", "Data Structures", 2, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 1, "B+", 40, 50, 10, "AI201", "Machine Learning", 3, new DateTime(2025, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_StudentId",
                table: "Grievances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMarks_StudentId",
                table: "InternalMarks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMarks_TeacherId",
                table: "InternalMarks",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_RevaluationRequests_MarkId",
                table: "RevaluationRequests",
                column: "MarkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grievances");

            migrationBuilder.DropTable(
                name: "RevaluationRequests");

            migrationBuilder.DropTable(
                name: "InternalMarks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
