using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(DashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            _logger.LogInformation("Dashboard Admin acedido por {User}", User.Identity?.Name);
            return View("Admin", await _dashboardService.GetAdminDashboardAsync());
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

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserRole(int userId, string newRoleName)
        {
            var currentId = GetUserId();
            if (currentId == null) return Unauthorized();

            var error = await _dashboardService.ChangeUserRoleAsync(currentId.Value, userId, newRoleName);
            TempData[error == null ? "Success" : "Error"] = error ?? "Papel alterado com sucesso!";
            return RedirectToAction("AdminUsers");
        }

        [Authorize(Roles = "Aluno,Admin")]
        public IActionResult Aluno() => View();

        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> Professor()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _dashboardService.GetTeacherDashboardAsync(id.Value));
        }


    }
}
