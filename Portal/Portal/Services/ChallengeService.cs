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

            if (model.Questions != null && model.Questions.Count > 0)
            {
                foreach (var qModel in model.Questions)
                {
                    if (string.IsNullOrWhiteSpace(qModel.QuestionText)) continue;

                    var question = new ChallengeQuestion(challenge.Id, qModel.QuestionText, qModel.QuestionType)
                    {
                        OptionA = qModel.OptionA,
                        OptionB = qModel.OptionB,
                        OptionC = qModel.OptionC,
                        OptionD = qModel.OptionD,
                        CorrectAnswer = qModel.CorrectAnswer
                    };
                    _context.ChallengeQuestions.Add(question);
                }
                await _context.SaveChangesAsync();
            }

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

            if (challenge == null) return null!;

            var availableClasses = await _context.Classes.ToListAsync();

            var questions = await _context.ChallengeQuestions
                .Where(q => q.ChallengeId == id)
                .OrderBy(q => q.Id)
                .ToListAsync();

            var enrolledStudents = new List<User>();
            if (challenge.ClassId != null)
            {
                enrolledStudents = await _context.ClassEnrollments
                    .Include(ce => ce.Student)
                    .Where(ce => ce.ClassId == challenge.ClassId.Value)
                    .Select(ce => ce.Student)
                    .ToListAsync();
            }

            var submissions = await _context.ChallengeSubmissions
                .Where(cs => cs.ChallengeId == id)
                .ToDictionaryAsync(cs => cs.StudentId);

            var studentStatuses = enrolledStudents.Select(student => new StudentSubmissionStatusViewModel
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentEmail = student.Email,
                IsSubmitted = submissions.ContainsKey(student.Id),
                SubmittedAt = submissions.TryGetValue(student.Id, out var sub) ? sub.CreatedAt : null,
                IsGraded = submissions.TryGetValue(student.Id, out sub) && sub.GradedAt != null,
                GradedAt = submissions.TryGetValue(student.Id, out sub) ? sub.GradedAt : null,
                SubmissionId = submissions.TryGetValue(student.Id, out sub) ? sub.Id : null
            }).ToList();

            // Adicionar alunos que submeteram mas não estão na turma (ex: removidos da turma ou desafio geral)
            foreach (var subKvp in submissions)
            {
                if (studentStatuses.All(s => s.StudentId != subKvp.Key))
                {
                    var student = await _context.Users.FindAsync(subKvp.Key);
                    if (student != null)
                    {
                        studentStatuses.Add(new StudentSubmissionStatusViewModel
                        {
                            StudentId = student.Id,
                            StudentName = student.Name,
                            StudentEmail = student.Email,
                            IsSubmitted = true,
                            SubmittedAt = subKvp.Value.CreatedAt,
                            IsGraded = subKvp.Value.GradedAt != null,
                            GradedAt = subKvp.Value.GradedAt,
                            SubmissionId = subKvp.Value.Id
                        });
                    }
                }
            }

            return new ChallengeDetailsViewModel
            {
                Challenge = challenge,
                Questions = questions,
                StudentStatuses = studentStatuses,
                AvailableClasses = availableClasses
            };
        }

        public async Task<string?> EditChallengeAsync(int id, string title, string description, int? classId)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return "Desafio não encontrado.";
            if (string.IsNullOrWhiteSpace(title)) return "O título é obrigatório.";

            challenge.Title = title.Trim();
            challenge.Description = string.IsNullOrWhiteSpace(description) ? "" : description.Trim();
            challenge.ClassId = classId;
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "Challenge", id.ToString());
            return null;
        }

        public async Task<Challenge?> GetChallengeByIdAsync(int id)
        {
            return await _context.Challenges
                .Include(c => c.Teacher)
                .Include(c => c.Class)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ChallengeSubmission?> GetSubmissionAsync(int challengeId, int studentId)
        {
            return await _context.ChallengeSubmissions
                .Include(cs => cs.Challenge)
                .Include(cs => cs.Answers)
                .ThenInclude(a => a.ChallengeQuestion)
                .FirstOrDefaultAsync(cs => cs.ChallengeId == challengeId && cs.StudentId == studentId);
        }

        public async Task<ChallengeSubmission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.ChallengeSubmissions
                .Include(cs => cs.Challenge)
                .Include(cs => cs.Student)
                .Include(cs => cs.Answers)
                .ThenInclude(a => a.ChallengeQuestion)
                .FirstOrDefaultAsync(cs => cs.Id == submissionId);
        }

        public async Task<List<ChallengeQuestion>> GetQuestionsByChallengeIdAsync(int challengeId)
        {
            return await _context.ChallengeQuestions
                .Where(q => q.ChallengeId == challengeId)
                .OrderBy(q => q.Id)
                .ToListAsync();
        }

        public async Task AddQuestionAsync(int challengeId, ChallengeQuestion question)
        {
            question.ChallengeId = challengeId;
            _context.ChallengeQuestions.Add(question);
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Create, "ChallengeQuestion", question.Id.ToString());
        }

        public async Task<string?> DeleteQuestionAsync(int questionId)
        {
            var question = await _context.ChallengeQuestions.FindAsync(questionId);
            if (question == null) return "Pergunta não encontrada.";

            question.MarkAsDeleted();
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Delete, "ChallengeQuestion", questionId.ToString());
            return null;
        }

        public async Task SubmitAnswersAsync(int studentId, int challengeId, Dictionary<int, string> answers)
        {
            var exists = await _context.ChallengeSubmissions
                .AnyAsync(cs => cs.ChallengeId == challengeId && cs.StudentId == studentId);

            if (exists)
            {
                throw new InvalidOperationException("Este desafio já foi respondido.");
            }

            var submission = new ChallengeSubmission(studentId, challengeId);
            _context.ChallengeSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            foreach (var kvp in answers)
            {
                var questionId = kvp.Key;
                var answerText = kvp.Value ?? string.Empty;

                var answer = new ChallengeAnswer(submission.Id, questionId, answerText);
                _context.ChallengeAnswers.Add(answer);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Aluno {StudentId} submeteu respostas ao desafio {ChallengeId}.", studentId, challengeId);
            await _audit.LogAsync(AuditAction.Create, "ChallengeSubmission", submission.Id.ToString());
        }

        public async Task<string?> GradeSubmissionAsync(int submissionId, Dictionary<int, bool> grades, string? feedback)
        {
            var submission = await _context.ChallengeSubmissions
                .Include(cs => cs.Answers)
                .FirstOrDefaultAsync(cs => cs.Id == submissionId);

            if (submission == null) return "Submissão não encontrada.";

            submission.Feedback = feedback;
            submission.GradedAt = DateTime.UtcNow;

            foreach (var answer in submission.Answers)
            {
                if (grades.TryGetValue(answer.ChallengeQuestionId, out bool isCorrect))
                {
                    answer.IsCorrect = isCorrect;
                }
            }

            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "ChallengeSubmission", submissionId.ToString());
            return null;
        }

        public async Task<Challenge?> GetChallengeByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var cleanCode = code.Trim().ToUpper();
            return await _context.Challenges
                .FirstOrDefaultAsync(c => c.AccessLinkCode == cleanCode);
        }

        public async Task<List<Scenario>> GetStudentScenariosAsync(int studentId)
        {
            return await _context.Scenarios
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
