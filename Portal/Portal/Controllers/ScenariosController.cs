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

        [Authorize(Roles = "Aluno,Admin")]
        public IActionResult Details(int id, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var scenario = _context.Scenarios
                    .Include(s => s.Challenge)
                    .FirstOrDefault(s => s.Id == id && s.StudentId == studentId);

                if (scenario == null)
                {
                    TempData["Error"] = "Cenário não encontrado.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                // Default to current month if no month is provided
                if (!month.HasValue) month = DateTime.Now.Month;

                var query = _context.Entries.Where(e => e.ScenarioId == id);
                
                query = query.Where(e => 
                    (e.Recurrence == RecurrenceType.Monthly && month.Value >= e.EntryMonth) || 
                    e.EntryMonth == month.Value
                );

                var entries = query.OrderByDescending(e => e.CreatedAt).ToList();

                var incomes = entries.Where(e => e.EntryType == EntryType.Income).ToList();
                var expenses = entries.Where(e => e.EntryType == EntryType.Expense).ToList();
                
                var members = _context.ScenarioMembers.Where(sm => sm.ScenarioId == id).ToList();
                
                decimal totalIncome = incomes.Sum(i => i.Amount) + members.Sum(m => m.MonthlyIncome);
                decimal totalExpense = expenses.Sum(e => e.Amount);
                decimal balance = totalIncome - totalExpense;
                decimal savingsRate = totalIncome > 0 ? (balance / totalIncome) * 100 : 0;

                // Calculate accumulated balance up to the selected month
                int targetMonth = month.Value;
                decimal accumulatedIncome = members.Sum(m => m.MonthlyIncome) * targetMonth;
                decimal accumulatedExpense = 0;

                var allIncomes = _context.Entries.Where(e => e.ScenarioId == id && e.EntryType == EntryType.Income).ToList();
                foreach(var inc in allIncomes)
                {
                    if (inc.Recurrence == RecurrenceType.Monthly)
                    {
                        if (targetMonth >= inc.EntryMonth)
                            accumulatedIncome += inc.Amount * (targetMonth - inc.EntryMonth + 1);
                    }
                    else
                    {
                        if (targetMonth >= inc.EntryMonth)
                            accumulatedIncome += inc.Amount;
                    }
                }

                var allExpenses = _context.Entries.Where(e => e.ScenarioId == id && e.EntryType == EntryType.Expense).ToList();
                foreach(var exp in allExpenses)
                {
                    if (exp.Recurrence == RecurrenceType.Monthly)
                    {
                        if (targetMonth >= exp.EntryMonth)
                            accumulatedExpense += exp.Amount * (targetMonth - exp.EntryMonth + 1);
                    }
                    else
                    {
                        if (targetMonth >= exp.EntryMonth)
                            accumulatedExpense += exp.Amount;
                    }
                }

                decimal accumulatedBalance = accumulatedIncome - accumulatedExpense;
                decimal currentBankBalance = scenario.InitialBalance + accumulatedBalance;

                // Inject salaries into incomes list ONLY for UI visualization in the table
                foreach (var member in members)
                {
                    incomes.Add(new Entry(id, EntryType.Income, $"Salário - {member.Name}", member.MonthlyIncome, 1, RecurrenceType.Monthly));
                }
                incomes = incomes.OrderByDescending(i => i.Amount).ToList();

                var vm = new ScenarioDetailsViewModel
                {
                    Scenario = scenario,
                    Incomes = incomes,
                    Expenses = expenses,
                    SelectedMonth = month,
                    Members = members,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    Balance = balance,
                    SavingsRate = savingsRate,
                    FinalBankBalance = currentBankBalance
                };

                return View(vm);
            }
            catch (Exception)
            {
                return RedirectToAction("Aluno", "Dashboard");
            }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterIncome(int scenarioId, string category, decimal amount, int entryMonth, RecurrenceType recurrence, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == scenarioId && s.StudentId == studentId);

                if (scenario == null)
                {
                    TempData["Error"] = "Cenário não encontrado ou sem permissão.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                if (string.IsNullOrWhiteSpace(category) || amount <= 0)
                {
                    TempData["Error"] = "A categoria é obrigatória e o valor deve ser positivo.";
                    return RedirectToAction("Details", new { id = scenarioId, month = month });
                }

                var entry = new Entry(scenarioId, EntryType.Income, category, amount, entryMonth, recurrence);
                _context.Entries.Add(entry);
                scenario.InitialBalance += amount;
                _context.SaveChanges();

                TempData["Success"] = $"Rendimento de €{amount:N2} registado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao registar o rendimento: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterExpense(int scenarioId, string category, decimal amount, int entryMonth, RecurrenceType recurrence, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == scenarioId && s.StudentId == studentId);

                if (scenario == null)
                {
                    TempData["Error"] = "Cenário não encontrado ou sem permissão.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                if (string.IsNullOrWhiteSpace(category) || amount <= 0)
                {
                    TempData["Error"] = "A categoria é obrigatória e o valor deve ser positivo.";
                    return RedirectToAction("Details", new { id = scenarioId, month = month });
                }

                var entry = new Entry(scenarioId, EntryType.Expense, category, amount, entryMonth, recurrence);
                _context.Entries.Add(entry);
                scenario.InitialBalance -= amount;
                _context.SaveChanges();

                TempData["Success"] = $"Despesa de €{amount:N2} registada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao registar a despesa: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(int scenarioId, string name, decimal monthlyIncome, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var scenario = _context.Scenarios.FirstOrDefault(s => s.Id == scenarioId && s.StudentId == studentId);

                if (scenario == null)
                {
                    TempData["Error"] = "Cenário não encontrado ou sem permissão.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                if (string.IsNullOrWhiteSpace(name) || monthlyIncome < 0)
                {
                    TempData["Error"] = "O nome é obrigatório e o rendimento não pode ser negativo.";
                    return RedirectToAction("Details", new { id = scenarioId, month = month });
                }

                var member = new ScenarioMember(scenarioId, name, monthlyIncome);
                _context.ScenarioMembers.Add(member);
                
                _context.SaveChanges();

                TempData["Success"] = $"Membro '{name}' adicionado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao adicionar membro: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = scenarioId, month = month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMember(int id, int? month)
        {
            try
            {
                int studentId = GetUserId();
                var member = _context.ScenarioMembers.Include(sm => sm.Scenario).FirstOrDefault(sm => sm.Id == id);
                
                if (member == null || member.Scenario.StudentId != studentId)
                {
                    TempData["Error"] = "Membro não encontrado ou sem permissão.";
                    return RedirectToAction("Aluno", "Dashboard");
                }

                int scenarioId = member.ScenarioId;
                member.MarkAsDeleted();
                _context.SaveChanges();

                TempData["Success"] = "Membro removido com sucesso!";
                return RedirectToAction("Details", new { id = scenarioId, month = month });
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