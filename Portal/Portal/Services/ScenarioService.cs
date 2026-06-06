using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Services
{
    public class ScenarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ScenarioService> _logger;

        public ScenarioService(ApplicationDbContext context, ILogger<ScenarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> CreateScenarioAsync(int studentId, string familyName, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(familyName))
                return "O nome de família é obrigatório.";

            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                return $"Erro: O estudante com ID {studentId} não existe na base de dados.";

            var scenario = new Scenario(studentId, familyName, initialBalance);
            _context.Scenarios.Add(scenario);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cenário '{FamilyName}' criado pelo Aluno ID {StudentId}.", familyName, studentId);
            return null;
        }

        public async Task<ScenarioDetailsViewModel?> GetScenarioDetailsAsync(int scenarioId, int studentId, int? month)
        {
            var scenario = await _context.Scenarios
                .Include(s => s.Challenge)
                .FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);

            if (scenario == null) return null;

            int targetMonth = month ?? DateTime.Now.Month;

            var query = _context.Entries.Where(e => e.ScenarioId == scenarioId);
            
            query = query.Where(e => 
                (e.Recurrence == RecurrenceType.Monthly && targetMonth >= e.EntryMonth) || 
                e.EntryMonth == targetMonth
            );

            var entries = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();

            var incomes = entries.Where(e => e.EntryType == EntryType.Income).ToList();
            var expenses = entries.Where(e => e.EntryType == EntryType.Expense).ToList();
            
            var members = await _context.ScenarioMembers.Where(sm => sm.ScenarioId == scenarioId).ToListAsync();
            
            decimal totalIncome = incomes.Sum(i => i.Amount) + members.Sum(m => m.MonthlyIncome);
            decimal totalExpense = expenses.Sum(e => e.Amount);
            decimal balance = totalIncome - totalExpense;
            decimal savingsRate = totalIncome > 0 ? (balance / totalIncome) * 100 : 0;

            decimal accumulatedIncome = members.Sum(m => m.MonthlyIncome) * targetMonth;
            decimal accumulatedExpense = 0;

            var allIncomes = await _context.Entries.Where(e => e.ScenarioId == scenarioId && e.EntryType == EntryType.Income).ToListAsync();
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

            var allExpenses = await _context.Entries.Where(e => e.ScenarioId == scenarioId && e.EntryType == EntryType.Expense).ToListAsync();
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

            foreach (var member in members)
            {
                incomes.Add(new Entry(scenarioId, EntryType.Income, $"Salário - {member.Name}", member.MonthlyIncome, 1, RecurrenceType.Monthly));
            }
            incomes = incomes.OrderByDescending(i => i.Amount).ToList();

            return new ScenarioDetailsViewModel
            {
                Scenario = scenario,
                Incomes = incomes,
                Expenses = expenses,
                SelectedMonth = targetMonth,
                Members = members,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance,
                SavingsRate = savingsRate,
                FinalBankBalance = currentBankBalance
            };
        }

        public async Task<string?> EditScenarioAsync(int scenarioId, int studentId, string familyName)
        {
            if (string.IsNullOrWhiteSpace(familyName)) return "O nome não pode estar vazio.";
            
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);
            if (scenario == null) return "Cenário não encontrado.";

            scenario.FamilyName = familyName;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteScenarioAsync(int scenarioId, int studentId)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);
            if (scenario == null) return "Cenário não encontrado.";

            var entries = _context.Entries.Where(e => e.ScenarioId == scenarioId);
            _context.Entries.RemoveRange(entries);
            _context.Scenarios.Remove(scenario);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<(string? Error, int? ScenarioId)> EditEntryAsync(int entryId, int studentId, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            var entry = await _context.Entries.Include(e => e.Scenario).FirstOrDefaultAsync(e => e.Id == entryId && e.Scenario.StudentId == studentId);
            if (entry == null) return ("Registo não encontrado.", null);
            if (amount <= 0) return ("O valor deve ser positivo.", entry.ScenarioId);

            if (entry.EntryType == EntryType.Income)
                entry.Scenario.InitialBalance -= entry.Amount;
            else
                entry.Scenario.InitialBalance += entry.Amount;
            
            entry.Category = category;
            entry.Amount = amount;
            entry.EntryMonth = entryMonth;
            entry.Recurrence = recurrence;

            if (entry.EntryType == EntryType.Income)
                entry.Scenario.InitialBalance += amount;
            else
                entry.Scenario.InitialBalance -= amount;
            
            await _context.SaveChangesAsync();
            return (null, entry.ScenarioId);
        }

        public async Task<(string? Error, int? ScenarioId)> DeleteEntryAsync(int entryId, int studentId)
        {
            var entry = await _context.Entries.Include(e => e.Scenario).FirstOrDefaultAsync(e => e.Id == entryId && e.Scenario.StudentId == studentId);
            if (entry == null) return ("Registo não encontrado.", null);

            int scenarioId = entry.ScenarioId;
            
            if (entry.EntryType == EntryType.Income)
                entry.Scenario.InitialBalance -= entry.Amount;
            else
                entry.Scenario.InitialBalance += entry.Amount;
            
            _context.Entries.Remove(entry);
            await _context.SaveChangesAsync();
            return (null, scenarioId);
        }

        public async Task<string?> RegisterIncomeAsync(int scenarioId, int studentId, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);
            if (scenario == null) return "Cenário não encontrado.";
            if (string.IsNullOrWhiteSpace(category) || amount <= 0) return "Categoria obrigatória e valor positivo.";

            var entry = new Entry(scenarioId, EntryType.Income, category, amount, entryMonth, recurrence);
            _context.Entries.Add(entry);
            scenario.InitialBalance += amount;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> RegisterExpenseAsync(int scenarioId, int studentId, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);
            if (scenario == null) return "Cenário não encontrado.";
            if (string.IsNullOrWhiteSpace(category) || amount <= 0) return "Categoria obrigatória e valor positivo.";

            var entry = new Entry(scenarioId, EntryType.Expense, category, amount, entryMonth, recurrence);
            _context.Entries.Add(entry);
            scenario.InitialBalance -= amount;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> AddMemberAsync(int scenarioId, int studentId, string name, decimal monthlyIncome)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.StudentId == studentId);
            if (scenario == null) return "Cenário não encontrado.";
            if (string.IsNullOrWhiteSpace(name) || monthlyIncome < 0) return "Nome obrigatório e rendimento >= 0.";

            var member = new ScenarioMember(scenarioId, name, monthlyIncome);
            _context.ScenarioMembers.Add(member);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<(string? Error, int? ScenarioId)> DeleteMemberAsync(int memberId, int studentId)
        {
            var member = await _context.ScenarioMembers.Include(sm => sm.Scenario).FirstOrDefaultAsync(sm => sm.Id == memberId);
            if (member == null || member.Scenario.StudentId != studentId) return ("Membro não encontrado.", null);

            int scenarioId = member.ScenarioId;
            member.MarkAsDeleted();
            await _context.SaveChangesAsync();
            return (null, scenarioId);
        }
    }
}
