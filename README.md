# College Grievance Management System

A role-based web portal built with ASP.NET Core 9 MVC and SQL Server. Students can submit grievances and view internal marks, teachers manage department grievances and marks, and the principal monitors everything college-wide.

## Tech Stack
- ASP.NET Core 9 MVC
- Entity Framework Core 9
- Microsoft SQL Server
- Razor Views

## Roles
- **Student** — Submit grievances, track status, view marks, request revaluation
- **Teacher** — Manage department grievances, add/edit marks, handle revaluations
- **Principal** — College-wide grievance and marks overview, take final decisions

## Setup Instructions

1. Clone or download this repo
2. Open `grievance.csproj` in Visual Studio 2022
3. Update the connection string in `appsettings.json` to match your SQL Server
4. Open Package Manager Console and run:
   Update-Database

5. 5. Press F5 to run

## Demo Credentials
| Role | Email | Password |
|------|-------|----------|
| Student | arjun@student.edu | student123 |
| Teacher | ravi@college.edu | teacher123 |
| Principal | principal@college.edu | principal123 |
