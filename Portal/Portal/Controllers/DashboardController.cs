using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard de Administração Geral.", User.Identity?.Name ?? "Anónimo");
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
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao painel de Administração de Turmas.", User.Identity?.Name ?? "Anónimo");
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes
                            .Include(c => c.Teacher)
                            .Include(c => c.Enrollments)
                            .Include(c => c.Challenges)
                            .ToList(),
                Challenges = System.Linq.Enumerable.Empty<Challenge>()
            };
            return View("AdminClasses", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminChallenges()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao painel de Administração de Desafios.", User.Identity?.Name ?? "Anónimo");
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes.Include(c => c.Teacher).ToList(),
                Challenges = _context.Challenges
                                .Include(c => c.Class)
                                .Include(c => c.Teacher)
                                .Include(c => c.Scenarios)
                                    .ThenInclude(s => s.Entries)
                                .ToList()
            };
            return View("AdminChallenges", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminUsers()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao painel de Administração de Utilizadores.", User.Identity?.Name ?? "Anónimo");

            var users = _context.Users
                .Include(u => u.Role)
                .Select(u => new AdminUserDetailViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    Enrollments = _context.ClassEnrollments
                        .Where(ce => ce.StudentId == u.Id)
                        .Select(ce => new UserClassEnrollmentDetail
                        {
                            ClassId = ce.ClassId,
                            ClassName = ce.Class.Name
                        })
                        .ToList()
                })
                .ToList();

            var viewModel = new AdminUsersViewModel
            {
                Users = users,
                Classes = _context.Classes.ToList()
            };

            return View("AdminUsers", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeUserRole(int userId, string newRoleName, string returnUrl = null)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilizador não encontrado.";
                    if(!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("AdminUsers");
                }

                int currentUserId = GetUserId();
                if (userId == currentUserId)
                {
                    TempData["Error"] = "Não pode alterar o seu próprio papel de administrador.";
                    if(!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("AdminUsers");
                }

                if (newRoleName != "Professor" && newRoleName != "Aluno" && newRoleName != "Admin")
                {
                    TempData["Error"] = "Função/Role inválida.";
                    if(!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("AdminUsers");
                }

                var role = _context.Roles.FirstOrDefault(r => r.RoleName == newRoleName);
                if (role == null)
                {
                    TempData["Error"] = "Função/Role não existe na base de dados.";
                    if(!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("AdminUsers");
                }

                user.RoleId = role.Id;
                _context.SaveChanges();

                TempData["Success"] = $"Papel do utilizador {user.Name} alterado para {newRoleName} com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao alterar papel do utilizador: {ex.Message}";
            }

            if(!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("AdminUsers");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult UserDetails(int id)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToAction("AdminUsers");
            }

            var roles = _context.Roles.ToList();
            
            var taughtClasses = _context.Classes
                .Where(c => c.TeacherId == id)
                .ToList();

            var enrolledClasses = _context.ClassEnrollments
                .Include(ce => ce.Class)
                .Where(ce => ce.StudentId == id)
                .Select(ce => ce.Class)
                .ToList();

            var createdChallenges = _context.Challenges
                .Where(c => c.TeacherId == id)
                .ToList();

            var participatedChallenges = _context.Scenarios
                .Include(s => s.Challenge)
                .Where(s => s.StudentId == id && s.ChallengeId != null)
                .Select(s => s.Challenge)
                .Distinct()
                .ToList();

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                AvailableRoles = roles,
                TaughtClasses = taughtClasses,
                EnrolledClasses = enrolledClasses,
                CreatedChallenges = createdChallenges,
                ParticipatedChallenges = participatedChallenges
            };

            return View("UserDetails", viewModel);
        }

        [Authorize(Roles = "Aluno,Admin")]
        public IActionResult Aluno()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard do Aluno.", User.Identity?.Name ?? "Anónimo");
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
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard do Professor.", User.Identity?.Name ?? "Anónimo");

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
