using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Models.ViewModels;
using Portal.Services;
using Portal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class ChallengesController : BaseController
    {
        private readonly ChallengeService _challengeService;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(ChallengeService challengeService, ILogger<ChallengesController> logger)
        {
            _challengeService = challengeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateChallengeViewModel model)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                ToastMessages.SetErrors(this);
                return RedirectByRole();
            }

            try
            {
                await _challengeService.CreateAsync(id.Value, model);
                TempData["Success"] = "Desafio criado com sucesso!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar desafio");
                TempData["Error"] = $"Erro ao criar desafio: {ex.Message}";
            }

            return RedirectByRole();
        }

        [HttpPost]
        public async Task<IActionResult> AssociateClass(int challengeId, int? classId)
        {
            var error = await _challengeService.AssociateClassAsync(challengeId, classId);
            TempData[error == null ? "Success" : "Error"] = error ?? "Desafio associado com sucesso!";
            return RedirectByRole();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _challengeService.DeleteAsync(id);
            TempData[error == null ? "Success" : "Error"] = error ?? "Desafio eliminado com sucesso!";
            return RedirectByRole();
        }



        private IActionResult RedirectByRole() =>
            User.IsInRole("Admin")
                ? RedirectToAction("AdminChallenges", "Dashboard")
                : RedirectToAction("Professor", "Dashboard");
    }
}
