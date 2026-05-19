using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Linq;

namespace Portal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes.ToList(),
                Challenges = _context.Challenges.ToList()
            };
            return View("Admin", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminClasses()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes.ToList(),
                Challenges = System.Linq.Enumerable.Empty<Challenge>()
            };
            return View("AdminClasses", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminChallenges()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes.ToList(),
                Challenges = _context.Challenges.Include(c => c.Class).ToList()
            };
            return View("AdminChallenges", viewModel);
        }

        [Authorize(Roles = "Aluno,Admin")]
        public IActionResult Aluno()
        {
            return View();
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        [Authorize(Roles = "Professor,Admin")]
        public IActionResult Professor()
        {
            int teacherId = GetUserId();

            var teacherClasses = _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .ToList();

            var classIds = teacherClasses.Select(c => c.Id).ToList();

            var classDetails = teacherClasses.Select(c => new TeacherClassDetailViewModel
            {
                ClassId = c.Id,
                ClassName = c.Name,
                MembershipCode = c.MembershipCode,
                StudentCount = _context.ClassEnrollments.Count(ce => ce.ClassId == c.Id)
            }).ToList();

            var teacherChallenges = _context.Challenges
                .Include(c => c.Class)
                .Where(c => c.TeacherId == teacherId)
                .ToList();

            var totalStudents = _context.ClassEnrollments
                .Count(ce => classIds.Contains(ce.ClassId));

            var viewModel = new TeacherDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalChallenges = teacherChallenges.Count,
                Classes = classDetails,
                Challenges = teacherChallenges
            };

            return View(viewModel);
        }
    }
}
