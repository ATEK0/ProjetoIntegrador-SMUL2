using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Models.ViewModels;
using Portal.Models;
using Portal.Services;
using Portal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class ChallengesController : BaseController
    {
        private readonly ChallengeService _challengeService;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(ChallengeService challengeService, ILogger<ChallengesController> logger)
        {
            _challengeService = challengeService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Create(CreateChallengeViewModel model)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                ToastMessages.SetErrors(this);
                return RedirectByRole();
            }

            if (model.Questions == null || !model.Questions.Any(q => !string.IsNullOrWhiteSpace(q.QuestionText)))
            {
                TempData["Error"] = "O desafio deve conter pelo menos uma pergunta.";
                return RedirectByRole();
            }

            try
            {
                await _challengeService.CreateAsync(id.Value, model);
                TempData["Success"] = "Desafio criado com sucesso!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar desafio");
                TempData["Error"] = $"Erro ao criar desafio: {ex.Message}";
            }

            return RedirectByRole();
        }

        [HttpPost]
        public async Task<IActionResult> AssociateClass(int challengeId, int? classId)
        {
            var error = await _challengeService.AssociateClassAsync(challengeId, classId);
            TempData[error == null ? "Success" : "Error"] = error ?? "Desafio associado com sucesso!";
            return RedirectByRole();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _challengeService.DeleteAsync(id);
            TempData[error == null ? "Success" : "Error"] = error ?? "Desafio eliminado com sucesso!";
            return RedirectByRole();
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var viewModel = await _challengeService.GetChallengeDetailsAsync(id);
            if (viewModel == null)
            {
                TempData["Error"] = "Desafio não encontrado.";
                return RedirectByRole();
            }

            if (User.IsInRole("Professor") && viewModel.Challenge.TeacherId != userId.Value)
            {
                TempData["Error"] = "Não tem permissão para ver este desafio.";
                return RedirectToAction("Professor", "Dashboard");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string description)
        {
            var error = await _challengeService.EditChallengeAsync(id, title, description);
            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "Desafio atualizado com sucesso!";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var studentId = GetUserId();
            if (studentId == null) return Unauthorized();

            var challenge = await _challengeService.GetChallengeByIdAsync(id);
            if (challenge == null)
            {
                TempData["Error"] = "Desafio não encontrado.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            var questions = await _challengeService.GetQuestionsByChallengeIdAsync(id);
            if (!questions.Any())
            {
                TempData["Error"] = "Este desafio ainda não tem perguntas criadas pelo professor.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            var submission = await _challengeService.GetSubmissionAsync(id, studentId.Value);
            ViewBag.Submission = submission;
            ViewBag.Questions = questions;
            ViewBag.Scenarios = await _challengeService.GetStudentScenariosAsync(studentId.Value);

            return View(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int id, Dictionary<int, string> answers)
        {
            var studentId = GetUserId();
            if (studentId == null) return Unauthorized();

            if (answers == null || !answers.Any())
            {
                TempData["Error"] = "Deve responder a todas as perguntas do desafio.";
                return RedirectToAction("Submit", new { id = id });
            }

            try
            {
                await _challengeService.SubmitAnswersAsync(studentId.Value, id, answers);
                TempData["Success"] = "Respostas ao desafio submetidas com sucesso!";
                return RedirectToAction("Aluno", "Dashboard");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao submeter respostas ao desafio {ChallengeId}", id);
                TempData["Error"] = $"Erro ao submeter respostas: {ex.Message}";
                return RedirectToAction("Submit", new { id = id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> AddQuestion(int challengeId, string questionText, string questionType, string? optionA, string? optionB, string? optionC, string? optionD, string? correctAnswer)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                TempData["Error"] = "O texto da pergunta é obrigatório.";
                return RedirectToAction("Details", new { id = challengeId });
            }

            if (!Enum.TryParse<QuestionType>(questionType, out var type))
            {
                TempData["Error"] = "Tipo de pergunta inválido.";
                return RedirectToAction("Details", new { id = challengeId });
            }

            var question = new ChallengeQuestion(challengeId, questionText, type)
            {
                OptionA = optionA,
                OptionB = optionB,
                OptionC = optionC,
                OptionD = optionD,
                CorrectAnswer = correctAnswer
            };

            try
            {
                await _challengeService.AddQuestionAsync(challengeId, question);
                TempData["Success"] = "Pergunta adicionada com sucesso!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar pergunta ao desafio {ChallengeId}", challengeId);
                TempData["Error"] = $"Erro ao adicionar pergunta: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = challengeId });
        }

        [HttpPost]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> DeleteQuestion(int id, int challengeId)
        {
            var error = await _challengeService.DeleteQuestionAsync(id);
            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "Pergunta removida com sucesso!";

            return RedirectToAction("Details", new { id = challengeId });
        }

        [HttpGet]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> GradeSubmission(int id)
        {
            var submission = await _challengeService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                TempData["Error"] = "Submissão não encontrada.";
                return RedirectByRole();
            }

            var questions = await _challengeService.GetQuestionsByChallengeIdAsync(submission.ChallengeId);

            var viewModel = new GradeSubmissionViewModel
            {
                Submission = submission,
                Questions = questions
            };

            ViewBag.Scenarios = await _challengeService.GetStudentScenariosAsync(submission.StudentId);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> GradeSubmission(int id, Dictionary<int, bool> grades, string? feedback)
        {
            var error = await _challengeService.GradeSubmissionAsync(id, grades, feedback);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("GradeSubmission", new { id = id });
            }

            TempData["Success"] = "Avaliação guardada com sucesso!";
            return RedirectByRole();
        }

        [HttpPost]
        public async Task<IActionResult> AccessByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "O código do desafio é obrigatório.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            var challenge = await _challengeService.GetChallengeByCodeAsync(code);
            if (challenge == null)
            {
                TempData["Error"] = "Desafio não encontrado com o código fornecido.";
                return RedirectToAction("Aluno", "Dashboard");
            }

            return RedirectToAction("Submit", new { id = challenge.Id });
        }

        private IActionResult RedirectByRole() =>
            User.IsInRole("Admin")
                ? RedirectToAction("AdminChallenges", "Dashboard")
                : RedirectToAction("Professor", "Dashboard");
    }
}
