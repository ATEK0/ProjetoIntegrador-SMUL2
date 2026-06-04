using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Linq;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Controllers
{
    [Authorize]
    public class ScenariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScenariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string familyName, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(familyName))
            {
                return Content("Erro de validação: O nome de família é obrigatório.");
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int studentId;
                if (int.TryParse(userIdClaim, out int parsedId))
                {
                    studentId = parsedId;
                }
                else
                {
                    studentId = 1; // Fallback
                }

                // Verificar se o estudante existe de forma síncrona
                var student = _context.Users.Find(studentId);
                if (student == null)
                {
                    return Content($"Erro: O estudante com ID {studentId} não existe na base de dados. Por favor, verifique se a conta está ativa e se o ID é válido.");
                }

                var scenario = new Scenario(studentId, familyName, initialBalance);
                
                _context.Add(scenario);
                _context.SaveChanges();

                return RedirectToAction("Aluno", "Dashboard");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $"\nDetalhe Interno: {ex.InnerException.Message}" : "";
                return Content($"Erro interno no servidor ao criar o cenário: {ex.Message}{innerMsg}");
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var studentId = GetUserId();
            var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == id && s.StudentId == studentId);
            
            if (scenario == null)
            {
                TempData["Error"] = "Cenário não encontrado.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            var entries = _context.Entries.Where(e => e.ScenarioId == id).ToList();

            var vm = new ScenarioDetailsViewModel
            {
                Scenario = scenario,
                Incomes = entries.Where(e => e.EntryType == EntryType.Income).ToList(),
                Expenses = entries.Where(e => e.EntryType == EntryType.Expense).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string familyName)
        {
            var studentId = GetUserId();
            var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == id && s.StudentId == studentId);
            
            if (scenario != null && !string.IsNullOrWhiteSpace(familyName))
            {
                scenario.FamilyName = familyName;
                _context.SaveChanges();
                TempData["Success"] = "Cenário atualizado com sucesso!";
            }
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var studentId = GetUserId();
            var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == id && s.StudentId == studentId);
            
            if (scenario != null)
            {
                var entries = _context.Entries.Where(e => e.ScenarioId == id);
                _context.Entries.RemoveRange(entries);
                _context.Scenarios.Remove(scenario);
                _context.SaveChanges();
                TempData["Success"] = "Cenário removido com sucesso!";
            }
            return RedirectToAction("Aluno", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEntry(int id, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            var studentId = GetUserId();
            var entry = _context.Entries.Include(e => e.Scenario).FirstOrDefault(e => e.Id == id && e.Scenario.StudentId == studentId);
            
            if (entry != null)
            {
                if (amount <= 0)
                {
                    TempData["Error"] = "O valor deve ser positivo.";
                    return RedirectToAction("Details", new { id = entry.ScenarioId });
                }

                // Reverter o impacto do valor antigo no saldo do cenário
                if (entry.EntryType == EntryType.Income)
                    entry.Scenario.InitialBalance -= entry.Amount;
                else
                    entry.Scenario.InitialBalance += entry.Amount;
                
                entry.Category = category;
                entry.Amount = amount;
                entry.EntryMonth = entryMonth;
                entry.Recurrence = recurrence;

                // Aplicar o novo impacto no saldo
                if (entry.EntryType == EntryType.Income)
                    entry.Scenario.InitialBalance += amount;
                else
                    entry.Scenario.InitialBalance -= amount;
                
                _context.SaveChanges();
                TempData["Success"] = "Registo atualizado com sucesso!";
                return RedirectToAction("Details", new { id = entry.ScenarioId });
            }
            TempData["Error"] = "Registo não encontrado.";
            return RedirectToAction("Aluno", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEntry(int id)
        {
            var studentId = GetUserId();
            var entry = _context.Entries.Include(e => e.Scenario).FirstOrDefault(e => e.Id == id && e.Scenario.StudentId == studentId);
            
            if (entry != null)
            {
                int scenarioId = entry.ScenarioId;
                
                // Reverter o impacto do valor antigo no saldo do cenário
                if (entry.EntryType == EntryType.Income)
                    entry.Scenario.InitialBalance -= entry.Amount;
                else
                    entry.Scenario.InitialBalance += entry.Amount;
                
                _context.Entries.Remove(entry);
                _context.SaveChanges();
                TempData["Success"] = "Registo apagado com sucesso!";
                return RedirectToAction("Details", new { id = scenarioId });
            }
            TempData["Error"] = "Registo não encontrado.";
            return RedirectToAction("Aluno", "Dashboard");
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }
    }
}