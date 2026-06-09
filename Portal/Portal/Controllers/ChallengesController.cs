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



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _challengeService.GetChallengeDetailsAsync(id);
            if (viewModel == null)
            {
                TempData["Error"] = "Desafio não encontrado.";
                return RedirectByRole();
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string description)
        {
            var error = await _challengeService.EditChallengeAsync(id, title, description);
            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "Desafio atualizado com sucesso!";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuiz()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            ViewBag.Classes = await _challengeService.GetTeacherClassesAsync(id.Value);
            
            return View(new CreateQuizViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizViewModel model)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var challengeId = await _challengeService.CreateQuizAsync(id.Value, model);
                if (challengeId != null)
                {
                    return Ok(new { success = true, message = "Quiz criado com sucesso!" });
                }
                return BadRequest(new { success = false, message = "Erro ao criar quiz." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar Quiz");
                return StatusCode(500, new { success = false, message = $"Erro interno: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SolveQuiz(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var viewModel = await _challengeService.GetSolveQuizViewModelAsync(id);
            if (viewModel == null)
            {
                TempData["Error"] = "Quiz não encontrado.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var score = await _challengeService.SubmitQuizAsync(userId.Value, model);
                if (score != null)
                {
                    return Ok(new { success = true, score = score });
                }
                return BadRequest(new { success = false, message = "Erro ao submeter quiz." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao submeter Quiz");
                return StatusCode(500, new { success = false, message = $"Erro interno: {ex.Message}" });
            }
        }

        private IActionResult RedirectByRole() =>
            User.IsInRole("Admin")
                ? RedirectToAction("AdminChallenges", "Dashboard")
                : RedirectToAction("Professor", "Dashboard");
    }
}
