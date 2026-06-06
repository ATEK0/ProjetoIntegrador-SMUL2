using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Portal.Models;
using Portal.Services;

namespace Portal.Controllers
{
    [Authorize]
    public class ScenariosController : Controller
    {
        private readonly ScenarioService _scenarioService;
        private readonly ILogger<ScenariosController> _logger;

        public ScenariosController(ScenarioService scenarioService, ILogger<ScenariosController> logger)
        {
            _scenarioService = scenarioService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string familyName, decimal initialBalance)
        {
            try
            {
                var studentId = GetUserId();
                var error = await _scenarioService.CreateScenarioAsync(studentId, familyName, initialBalance);
                
                if (error != null)
                {
                    return Content(error);
                }

                return RedirectToAction("Aluno", "Dashboard");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $"\nDetalhe Interno: {ex.InnerException.Message}" : "";
                return Content($"Erro interno no servidor ao criar o cenário: {ex.Message}{innerMsg}");
            }
        }

        [Authorize(Roles = "Aluno,Admin")]
        public async Task<IActionResult> Details(int id, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var viewModel = await _scenarioService.GetScenarioDetailsAsync(id, studentId, month);

                if (viewModel == null)
                {
                    TempData["Error"] = "Cenário não encontrado.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("Aluno", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string familyName)
        {
            var studentId = GetUserId();
            var error = await _scenarioService.EditScenarioAsync(id, studentId, familyName);
            
            if (error == null) TempData["Success"] = "Cenário atualizado com sucesso!";
            else TempData["Error"] = error;

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var studentId = GetUserId();
            var error = await _scenarioService.DeleteScenarioAsync(id, studentId);
            
            if (error == null) TempData["Success"] = "Cenário removido com sucesso!";
            else TempData["Error"] = error;

            return RedirectToAction("Aluno", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEntry(int id, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            var studentId = GetUserId();
            var result = await _scenarioService.EditEntryAsync(id, studentId, category, amount, entryMonth, recurrence);
            
            if (result.Error == null)
            {
                TempData["Success"] = "Registo atualizado com sucesso!";
                return RedirectToAction("Details", new { id = result.ScenarioId });
            }
            
            TempData["Error"] = result.Error;
            return result.ScenarioId.HasValue ? RedirectToAction("Details", new { id = result.ScenarioId }) : RedirectToAction("Aluno", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var studentId = GetUserId();
            var result = await _scenarioService.DeleteEntryAsync(id, studentId);
            
            if (result.Error == null)
            {
                TempData["Success"] = "Registo apagado com sucesso!";
                return RedirectToAction("Details", new { id = result.ScenarioId });
            }
            
            TempData["Error"] = result.Error;
            return result.ScenarioId.HasValue ? RedirectToAction("Details", new { id = result.ScenarioId }) : RedirectToAction("Aluno", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterIncome(int scenarioId, string category, decimal amount, int entryMonth, RecurrenceType recurrence, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var error = await _scenarioService.RegisterIncomeAsync(scenarioId, studentId, category, amount, entryMonth, recurrence);

                if (error == null) TempData["Success"] = $"Rendimento de €{amount:N2} registado com sucesso!";
                else TempData["Error"] = error;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao registar o rendimento: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExpense(int scenarioId, string category, decimal amount, int entryMonth, RecurrenceType recurrence, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var error = await _scenarioService.RegisterExpenseAsync(scenarioId, studentId, category, amount, entryMonth, recurrence);

                if (error == null) TempData["Success"] = $"Despesa de €{amount:N2} registada com sucesso!";
                else TempData["Error"] = error;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao registar a despesa: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int scenarioId, string name, decimal monthlyIncome, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var error = await _scenarioService.AddMemberAsync(scenarioId, studentId, name, monthlyIncome);

                if (error == null) TempData["Success"] = $"Membro '{name}' adicionado com sucesso!";
                else TempData["Error"] = error;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao adicionar membro: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(int id, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var result = await _scenarioService.DeleteMemberAsync(id, studentId);

                if (result.Error == null)
                {
                    TempData["Success"] = "Membro removido com sucesso!";
                    return RedirectToAction("Details", new { id = result.ScenarioId, month = month });
                }
                
                TempData["Error"] = result.Error;
                return result.ScenarioId.HasValue ? RedirectToAction("Details", new { id = result.ScenarioId, month = month }) : RedirectToAction("Aluno", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao remover membro: {ex.Message}";
                return RedirectToAction("Aluno", "Dashboard");
            }
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }
    }
}