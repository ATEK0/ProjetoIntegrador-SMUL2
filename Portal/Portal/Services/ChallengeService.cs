using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using Portal.Helpers;

namespace Portal.Services
{
    public class ChallengeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChallengeService> _logger;
        private readonly IAuditService _audit;

        public ChallengeService(ApplicationDbContext context, ILogger<ChallengeService> logger, IAuditService audit)
        {
            _context = context;
            _logger = logger;
            _audit = audit;
        }

        public async Task CreateAsync(int teacherId, CreateChallengeViewModel model)
        {
            var code = await CodeGenerator.GenerateUniqueAsync(c =>
                _context.Challenges.AnyAsync(x => x.AccessLinkCode == c));
            var challenge = new Challenge(teacherId, model.Title, code)
            {
                Description = model.Description?.Trim() ?? "",
                ClassId = model.ClassId
            };
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Desafio '{Title}' criado.", model.Title);
            await _audit.LogAsync(AuditAction.Create, "Challenge", challenge.Id.ToString());
        }

        public async Task<string?> AssociateClassAsync(int challengeId, int? classId)
        {
            var challenge = await _context.Challenges.FindAsync(challengeId);
            if (challenge == null) return "Desafio não encontrado.";

            challenge.ClassId = classId;
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "Challenge", challengeId.ToString());
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return "Desafio não encontrado.";

            challenge.MarkAsDeleted();
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Delete, "Challenge", id.ToString());
            return null;
        }

        public async Task<ChallengeDetailsViewModel> GetChallengeDetailsAsync(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Class)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null) return null;

            var scenarios = await _context.Scenarios
                .Include(s => s.Student)
                .Where(s => s.ChallengeId == id)
                .ToListAsync();

            var availableClasses = await _context.Classes.ToListAsync();

            return new ChallengeDetailsViewModel
            {
                Challenge = challenge,
                Scenarios = scenarios,
                AvailableClasses = availableClasses
            };
        }

        public async Task<string?> EditChallengeAsync(int id, string title, string description)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return "Desafio não encontrado.";
            if (string.IsNullOrWhiteSpace(title)) return "O título é obrigatório.";

            challenge.Title = title.Trim();
            challenge.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "Challenge", id.ToString());
            return null;
        }

    }
}
