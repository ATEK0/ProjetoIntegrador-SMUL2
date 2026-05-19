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
        [Route("/admin")]
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
        [Route("/admin/classes")]
        public IActionResult AdminClasses()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao painel de Administração de Turmas.", User.Identity?.Name ?? "Anónimo");
            var viewModel = new AdminDashboardViewModel
            {
                Classes = _context.Classes.ToList(),
                Challenges = System.Linq.Enumerable.Empty<Challenge>()
            };
            return View("AdminClasses", viewModel);
        }

        [Authorize(Roles = "Admin")]
        [Route("/admin/challenges")]
        public IActionResult AdminChallenges()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao painel de Administração de Desafios.", User.Identity?.Name ?? "Anónimo");
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
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard do Aluno.", User.Identity?.Name ?? "Anónimo");
            return View();
        }

        [Authorize(Roles = "Professor,Admin")]
        public IActionResult Professor()
        {
            _logger.LogInformation("Utilizador '{UserName}' acedeu ao Dashboard do Professor.", User.Identity?.Name ?? "Anónimo");
            return View();
        }
    }
}
