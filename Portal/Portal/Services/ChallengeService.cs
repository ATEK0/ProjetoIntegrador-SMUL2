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

        public ChallengeService(ApplicationDbContext context, ILogger<ChallengeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(int teacherId, CreateChallengeViewModel model)
        {
            var code = await CodeGenerator.GenerateUniqueAsync(c =>
                _context.Challenges.AnyAsync(x => x.AccessLinkCode == c));
            _context.Challenges.Add(new Challenge(teacherId, model.Title, code)
            {
                Description = model.Description?.Trim() ?? "",
                ClassId = model.ClassId
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("Desafio '{Title}' criado.", model.Title);
        }

        public async Task<string?> AssociateClassAsync(int challengeId, int? classId)
        {
            var challenge = await _context.Challenges.FindAsync(challengeId);
            if (challenge == null) return "Desafio não encontrado.";

            challenge.ClassId = classId;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return "Desafio não encontrado.";

            challenge.MarkAsDeleted();
            await _context.SaveChangesAsync();
            return null;
        }

    }
}
