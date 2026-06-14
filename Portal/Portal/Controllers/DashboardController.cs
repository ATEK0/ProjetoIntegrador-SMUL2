using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Linq;
using Portal.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;
        private readonly SimulationService _simulationService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(DashboardService dashboardService, SimulationService simulationService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _simulationService = simulationService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            _logger.LogInformation("Dashboard Admin acedido por {User}", User.Identity?.Name);
            return View("Admin", await _dashboardService.GetAdminInfraAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> MsHealth()
        {
            var status = await _simulationService.CheckHealthAsync();
            return Json(status);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminClasses() =>
            View("AdminClasses", await _dashboardService.GetAdminClassesAsync());

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChallenges() =>
            View("AdminChallenges", await _dashboardService.GetAdminChallengesAsync());

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUsers() =>
            View("AdminUsers", await _dashboardService.GetAdminUsersAsync());

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAuditLogs(int page = 1, string? filterAction = null, string? entityType = null) =>
            View("AdminAuditLogs", await _dashboardService.GetAuditLogsAsync(page, filterAction, entityType));

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserRole(int userId, string newRoleName, string returnUrl = null)
        {
            var currentId = GetUserId();
            if (currentId == null) return Unauthorized();

            var error = await _dashboardService.ChangeUserRoleAsync(currentId.Value, userId, newRoleName);
            TempData[error == null ? "Success" : "Error"] = error ?? "Papel alterado com sucesso!";
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("AdminUsers");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserDetails(int id)
        {
            var viewModel = await _dashboardService.GetUserDetailsAsync(id);
            if (viewModel == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToAction("AdminUsers");
            }
            return View("UserDetails", viewModel);
        }

        [Authorize(Roles = "Aluno,Admin")]
        public async Task<IActionResult> Aluno()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard do Aluno.", User.Identity?.Name ?? "Anónimo");
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetStudentDashboardAsync(id.Value, User.Identity?.Name ?? "Aluno"));
        }

        [Authorize(Roles = "Aluno,Admin")]
        public async Task<IActionResult> AlunoClasses()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetStudentClassesAsync(id.Value, User.Identity?.Name ?? "Aluno"));
        }

        [Authorize(Roles = "Aluno,Admin")]
        public async Task<IActionResult> AlunoChallenges()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetStudentChallengesAsync(id.Value, User.Identity?.Name ?? "Aluno"));
        }

        [Authorize(Roles = "Aluno,Admin")]
        public async Task<IActionResult> AlunoScenarios()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetStudentScenariosAsync(id.Value, User.Identity?.Name ?? "Aluno"));
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }
            return null;
        }

        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> Professor()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetTeacherDashboardAsync(id.Value));
        }

        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> ProfessorClasses()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetTeacherClassesAsync(id.Value));
        }

        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> ProfessorChallenges()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetTeacherChallengesAsync(id.Value));
        }


    }
}
