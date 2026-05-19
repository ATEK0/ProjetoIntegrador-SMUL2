using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System;
using System.Linq;

namespace Portal.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChallengesController(ApplicationDbContext context)
        {
            _context = context;
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
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                 ?? "Dados inválidos para criação do desafio.";
                TempData["Error"] = firstError;
                return RedirectBasedOnRole();
            }

            try
            {
                string code = GenerateAccessCode();
                int teacherId = GetUserId();

                var challenge = new Challenge(teacherId, model.Title, code)
                {
                    Description = model.Description?.Trim() ?? "",
                    ClassId = model.ClassId
                };

                _context.Challenges.Add(challenge);
                _context.SaveChanges();

                TempData["Success"] = "Desafio criado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao criar desafio: {ex.Message}";
                return RedirectBasedOnRole();
            }
        }

        [HttpPost]
        public IActionResult AssociateClass(int challengeId, int? classId)
        {
            try
            {
                var challenge = _context.Challenges.FirstOrDefault(c => c.Id == challengeId);
                if (challenge == null)
                {
                    TempData["Error"] = "Desafio não encontrado.";
                    return RedirectBasedOnRole();
                }

                challenge.ClassId = classId;
                _context.SaveChanges();

                TempData["Success"] = "Desafio associado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao associar desafio: {ex.Message}";
                return RedirectBasedOnRole();
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var challenge = _context.Challenges.FirstOrDefault(c => c.Id == id);
                if (challenge == null)
                {
                    TempData["Error"] = "Desafio não encontrado.";
                    return RedirectBasedOnRole();
                }

                _context.Challenges.Remove(challenge);
                _context.SaveChanges();

                TempData["Success"] = "Desafio eliminado com sucesso!";
                return RedirectBasedOnRole();
            }
            catch (Exception ex)
            {
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
