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

        public async Task<ChallengeDetailsViewModel> GetChallengeDetailsAsync(int id)
        {
            var challengeQuery = _context.Challenges
                .Include(c => c.Class)
                .Include(c => c.Teacher)
                .AsQueryable();

            // Include questions if it's a quiz to show in details
            challengeQuery = challengeQuery.Include(c => c.QuizQuestions).ThenInclude(q => q.Options);

            var challenge = await challengeQuery.FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null) return null;

            var scenarios = new List<Scenario>();
            var quizSubmissions = new List<QuizSubmission>();

            if (challenge.Type == Models.Enums.ChallengeType.Standard || challenge.Type == Models.Enums.ChallengeType.ScenarioIntegrated)
            {
                scenarios = await _context.Scenarios
                    .Include(s => s.Student)
                    .Where(s => s.ChallengeId == id)
                    .ToListAsync();
            }
            else if (challenge.Type == Models.Enums.ChallengeType.Quiz)
            {
                quizSubmissions = await _context.QuizSubmissions
                    .Include(qs => qs.Student)
                    .Where(qs => qs.ChallengeId == id)
                    .OrderByDescending(qs => qs.Score)
                    .ToListAsync();
            }

            var availableClasses = await _context.Classes.ToListAsync();

            var participants = new List<ParticipantViewModel>();
            
            // Get all potential participants
            var enrolledUsers = new List<User>();
            if (challenge.ClassId.HasValue)
            {
                enrolledUsers = await _context.ClassEnrollments
                    .Include(ce => ce.Student)
                    .Where(ce => ce.ClassId == challenge.ClassId.Value)
                    .Select(ce => ce.Student)
                    .ToListAsync();
            }
            else
            {
                enrolledUsers = await _context.StudentChallenges
                    .Include(sc => sc.Student)
                    .Where(sc => sc.ChallengeId == challenge.Id)
                    .Select(sc => sc.Student)
                    .ToListAsync();
            }

            foreach (var student in enrolledUsers)
            {
                if (challenge.Type == Models.Enums.ChallengeType.Quiz)
                {
                    var submission = quizSubmissions.FirstOrDefault(qs => qs.StudentId == student.Id);
                    if (submission != null)
                    {
                        participants.Add(new ParticipantViewModel
                        {
                            Name = student.Name,
                            Status = "Respondeu",
                            Detail = ""
                        });
                    }
                    else
                    {
                        participants.Add(new ParticipantViewModel
                        {
                            Name = student.Name,
                            Status = "Não Respondeu",
                            Detail = ""
                        });
                    }
                }
                else
                {
                    var scenario = scenarios.FirstOrDefault(s => s.StudentId == student.Id);
                    if (scenario != null)
                    {
                        participants.Add(new ParticipantViewModel
                        {
                            Name = student.Name,
                            Status = "Respondeu",
                            Detail = ""
                        });
                    }
                    else
                    {
                        participants.Add(new ParticipantViewModel
                        {
                            Name = student.Name,
                            Status = "Não Respondeu",
                            Detail = ""
                        });
                    }
                }
            }

            return new ChallengeDetailsViewModel
            {
                Challenge = challenge,
                Scenarios = scenarios,
                QuizSubmissions = quizSubmissions,
                AvailableClasses = availableClasses,
                Participants = participants
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
            return null;
        }

        public async Task<List<SchoolClass>> GetTeacherClassesAsync(int teacherId)
        {
            return await _context.Classes.Where(c => c.TeacherId == teacherId).ToListAsync();
        }

        public async Task<int?> CreateQuizAsync(int teacherId, CreateQuizViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var code = await CodeGenerator.GenerateUniqueAsync(c =>
                    _context.Challenges.AnyAsync(x => x.AccessLinkCode == c));

                var challenge = new Challenge(teacherId, model.Title, code, Models.Enums.ChallengeType.Quiz)
                {
                    Description = model.Description?.Trim() ?? "",
                    ClassId = model.ClassId
                };

                _context.Challenges.Add(challenge);
                await _context.SaveChangesAsync();

                foreach (var qViewModel in model.Questions.OrderBy(q => q.Order))
                {
                    var question = new QuizQuestion(challenge.Id, qViewModel.Text, qViewModel.Order);
                    _context.QuizQuestions.Add(question);
                    await _context.SaveChangesAsync();

                    foreach (var optViewModel in qViewModel.Options)
                    {
                        var option = new QuizOption(question.Id, optViewModel.Text, optViewModel.IsCorrect);
                        _context.QuizOptions.Add(option);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Quiz '{Title}' criado com sucesso.", model.Title);
                return challenge.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar Quiz");
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<SolveQuizViewModel?> GetSolveQuizViewModelAsync(int challengeId)
        {
            var challenge = await _context.Challenges
                .Include(c => c.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c => c.Id == challengeId && c.Type == Models.Enums.ChallengeType.Quiz);

            if (challenge == null) return null;

            return new SolveQuizViewModel
            {
                ChallengeId = challenge.Id,
                Title = challenge.Title,
                Description = challenge.Description,
                Questions = challenge.QuizQuestions.OrderBy(q => q.Order).Select(q => new SolveQuizQuestionViewModel
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Order = q.Order,
                    Options = q.Options.Select(o => new SolveQuizOptionViewModel
                    {
                        OptionId = o.Id,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<int?> SubmitQuizAsync(int studentId, SubmitQuizViewModel model)
        {
            var challenge = await _context.Challenges
                .Include(c => c.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c => c.Id == model.ChallengeId && c.Type == Models.Enums.ChallengeType.Quiz);

            if (challenge == null) return null;

            int score = 0;
            var submission = new QuizSubmission(challenge.Id, studentId, 0);
            _context.QuizSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            foreach (var answerModel in model.Answers)
            {
                var question = challenge.QuizQuestions.FirstOrDefault(q => q.Id == answerModel.QuestionId);
                if (question != null)
                {
                    var selectedOption = question.Options.FirstOrDefault(o => o.Id == answerModel.SelectedOptionId);
                    if (selectedOption != null && selectedOption.IsCorrect)
                    {
                        score++;
                    }

                    _context.QuizAnswers.Add(new QuizAnswer(submission.Id, question.Id, answerModel.SelectedOptionId));
                }
            }

            submission.Score = score;
            await _context.SaveChangesAsync();

            return score;
        }

        public async Task<Challenge?> GetChallengeByCodeAsync(string accessCode)
        {
            return await _context.Challenges.FirstOrDefaultAsync(c => c.AccessLinkCode == accessCode);
        }

        public async Task<bool> HasStudentCompletedChallengeAsync(int studentId, int challengeId)
        {
            var hasScenario = await _context.Scenarios.AnyAsync(s => s.StudentId == studentId && s.ChallengeId == challengeId);
            var hasQuiz = await _context.QuizSubmissions.AnyAsync(q => q.StudentId == studentId && q.ChallengeId == challengeId);
            
            return hasScenario || hasQuiz;
        }

        public async Task<bool> IsStudentEnrolledInChallengeAsync(int studentId, int challengeId)
        {
            return await _context.StudentChallenges.AnyAsync(sc => sc.StudentId == studentId && sc.ChallengeId == challengeId);
        }

        public async Task EnrollStudentInChallengeAsync(int studentId, int challengeId)
        {
            var enrollment = new StudentChallenge(studentId, challengeId);
            _context.StudentChallenges.Add(enrollment);
            await _context.SaveChangesAsync();
        }

    }
}
