using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(ApplicationDbContext context, ILogger<ChallengesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        private IActionResult RedirectBasedOnRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminChallenges", "Dashboard");
            }
            return RedirectToAction("Professor", "Dashboard");
        }

        [HttpPost]
        public IActionResult Create(CreateChallengeViewModel model)
        {
            int teacherId = GetUserId();

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                 ?? "Dados inválidos para criação do desafio.";
                _logger.LogWarning("Professor ID {TeacherId} tentou criar desafio com dados inválidos. Erro: {Error}", teacherId, firstError);
                TempData["Error"] = firstError;
                return RedirectBasedOnRole();
            }

            _logger.LogInformation("Professor ID {TeacherId} está a tentar criar o desafio '{Title}'", teacherId, model.Title);

            try
            {
                string code = GenerateAccessCode();

                var challenge = new Challenge(teacherId, model.Title, code)
                {
                    Description = model.Description?.Trim() ?? "",
                    ClassId = model.ClassId
                };

                _context.Challenges.Add(challenge);
                _context.SaveChanges();

                _logger.LogInformation("Desafio '{Title}' criado com sucesso por Professor ID {TeacherId}. Código gerado: {Code}", model.Title, teacherId, code);

                TempData["Success"] = "Desafio criado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao criar desafio '{Title}' por Professor ID {TeacherId}", model.Title, teacherId);
                TempData["Error"] = $"Erro ao criar desafio: {ex.Message}";
                return RedirectBasedOnRole();
            }
        }

        [HttpPost]
        public IActionResult AssociateClass(int challengeId, int? classId)
        {
            int userId = GetUserId();
            _logger.LogInformation("Utilizador ID {UserId} a associar desafio ID {ChallengeId} à turma ID {ClassId}", userId, challengeId, classId);

            try
            {
                var challenge = _context.Challenges.FirstOrDefault(c => c.Id == challengeId);
                if (challenge == null)
                {
                    _logger.LogWarning("Associação falhou: desafio ID {ChallengeId} não encontrado.", challengeId);
                    TempData["Error"] = "Desafio não encontrado.";
                    return RedirectBasedOnRole();
                }

                challenge.ClassId = classId;
                _context.SaveChanges();

                _logger.LogInformation("Desafio '{Title}' (ID: {ChallengeId}) associado com sucesso à turma ID {ClassId}", challenge.Title, challengeId, classId);

                TempData["Success"] = "Desafio associado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao associar desafio ID {ChallengeId} à turma ID {ClassId}", challengeId, classId);
                TempData["Error"] = $"Erro ao associar desafio: {ex.Message}";
                return RedirectBasedOnRole();
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            int userId = GetUserId();
            _logger.LogInformation("Utilizador ID {UserId} a tentar eliminar o desafio ID {ChallengeId}", userId, id);

            try
            {
                var challenge = _context.Challenges.FirstOrDefault(c => c.Id == id);
                if (challenge == null)
                {
                    _logger.LogWarning("Remoção falhou: desafio ID {ChallengeId} não encontrado.", id);
                    TempData["Error"] = "Desafio não encontrado.";
                    return RedirectBasedOnRole();
                }

                _context.Challenges.Remove(challenge);
                _context.SaveChanges();

                _logger.LogInformation("Desafio '{Title}' (ID: {ChallengeId}) eliminado com sucesso por utilizador ID {UserId}.", challenge.Title, id, userId);

                TempData["Success"] = "Desafio eliminado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao eliminar desafio ID {ChallengeId} por utilizador ID {UserId}", id, userId);
                TempData["Error"] = $"Erro ao eliminar desafio: {ex.Message}";
                return RedirectBasedOnRole();
            }
        }

        private string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (_context.Challenges.Any(c => c.AccessLinkCode == code));

            return code;
        }
    }
}
