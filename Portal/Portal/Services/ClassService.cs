using System;
using System.Collections.Generic;
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
    public class ClassService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassService> _logger;
        private readonly IAuditService _audit;

        public ClassService(ApplicationDbContext context, ILogger<ClassService> logger, IAuditService audit)
        {
            _context = context;
            _logger = logger;
            _audit = audit;
        }

        public async Task<List<TeacherClassViewModel>> GetTeacherClassesAsync(int teacherId)
        {
            var classes = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();

            var classIds = classes.Select(c => c.Id).ToList();

            var studentsByClass = (await _context.ClassEnrollments
                .Where(ce => classIds.Contains(ce.ClassId))
                .Include(ce => ce.Student)
                .ToListAsync())
                .GroupBy(ce => ce.ClassId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Student.Name).ToList());

            return classes.Select(c => new TeacherClassViewModel
            {
                ClassId = c.Id,
                ClassName = c.Name,
                MembershipCode = c.MembershipCode,
                StudentNames = studentsByClass.GetValueOrDefault(c.Id) ?? new List<string>()
            }).ToList();
        }

        public async Task CreateClassAsync(int teacherId, string name)
        {
            var code = await CodeGenerator.GenerateUniqueAsync(code =>
                _context.Classes.AnyAsync(c => c.MembershipCode == code));

            var schoolClass = new SchoolClass(teacherId, name, code);
            _context.Classes.Add(schoolClass);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Turma '{Name}' criada com código {Code}.", name, code);
            await _audit.LogAsync(AuditAction.Create, "SchoolClass", schoolClass.Id.ToString());
        }

        public async Task<string?> JoinClassAsync(int studentId, string membershipCode)
        {
            if (string.IsNullOrWhiteSpace(membershipCode))
                return "O código de adesão é obrigatório.";

            var targetClass = await _context.Classes
                .FirstOrDefaultAsync(c => c.MembershipCode == membershipCode.Trim().ToUpper());

            if (targetClass == null)
                return "Código inválido. Não foi encontrada nenhuma turma com este código.";

            if (await _context.ClassEnrollments.AnyAsync(ce => ce.ClassId == targetClass.Id && ce.StudentId == studentId))
                return "Já estás inscrito nesta turma!";

            var enrollment = new ClassEnrollment(targetClass.Id, studentId);
            _context.ClassEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Create, "ClassEnrollment", enrollment.Id.ToString(), userId: studentId);
            return null;
        }

        public async Task<string?> EnrollUserAsync(int userId, int classId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return "Utilizador não encontrado.";

            var schoolClass = await _context.Classes.FindAsync(classId);
            if (schoolClass == null) return "Turma não encontrada.";

            if (await _context.ClassEnrollments.AnyAsync(ce => ce.ClassId == classId && ce.StudentId == userId))
                return "O utilizador já está inscrito nesta turma.";

            var enrollment = new ClassEnrollment(classId, userId);
            _context.ClassEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Create, "ClassEnrollment", enrollment.Id.ToString());
            return null;
        }

        public async Task<string?> RemoveUserFromClassAsync(int userId, int classId)
        {
            var enrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == classId && ce.StudentId == userId);

            if (enrollment == null) return "Inscrição não encontrada.";

            enrollment.MarkAsDeleted();
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Delete, "ClassEnrollment", enrollment.Id.ToString());
            return null;
        }

        public async Task<ClassDetailsViewModel> GetClassDetailsAsync(int id)
        {
            var schoolClass = await _context.Classes
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (schoolClass == null) return null;

            var enrolledStudents = await _context.ClassEnrollments
                .Where(ce => ce.ClassId == id)
                .Select(ce => ce.Student)
                .ToListAsync();

            var availableTeachers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Professor" || u.Role.RoleName == "Admin")
                .ToListAsync();

            var challenges = await _context.Challenges
                .Where(ch => ch.ClassId == id)
                .ToListAsync();

            var challengeIds = challenges.Select(ch => ch.Id).ToList();

            var submissions = await _context.ChallengeSubmissions
                .Where(cs => challengeIds.Contains(cs.ChallengeId))
                .ToListAsync();

            var studentChallengeStatuses = new Dictionary<int, List<StudentChallengeStatusViewModel>>();
            foreach (var student in enrolledStudents)
            {
                var statuses = new List<StudentChallengeStatusViewModel>();
                foreach (var challenge in challenges)
                {
                    var sub = submissions.FirstOrDefault(s => s.StudentId == student.Id && s.ChallengeId == challenge.Id);
                    statuses.Add(new StudentChallengeStatusViewModel
                    {
                        ChallengeId = challenge.Id,
                        ChallengeTitle = challenge.Title,
                        IsSubmitted = sub != null,
                        IsGraded = sub?.GradedAt != null,
                        SubmissionId = sub?.Id,
                        SubmittedAt = sub?.CreatedAt
                    });
                }
                studentChallengeStatuses[student.Id] = statuses;
            }

            return new ClassDetailsViewModel
            {
                Id = schoolClass.Id,
                Name = schoolClass.Name,
                MembershipCode = schoolClass.MembershipCode,
                TeacherId = schoolClass.TeacherId,
                Teacher = schoolClass.Teacher,
                EnrolledStudents = enrolledStudents,
                AvailableTeachers = availableTeachers,
                Challenges = challenges,
                StudentChallengeStatuses = studentChallengeStatuses
            };
        }

        public async Task<string?> EditClassAsync(int id, string name, int teacherId)
        {
            var schoolClass = await _context.Classes.FindAsync(id);
            if (schoolClass == null) return "Turma não encontrada.";
            if (string.IsNullOrWhiteSpace(name)) return "O nome da turma não pode estar vazio.";

            var teacher = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == teacherId && (u.Role.RoleName == "Professor" || u.Role.RoleName == "Admin"));

            if (teacher == null) return "Professor inválido ou sem permissões.";

            schoolClass.Name = name;
            schoolClass.TeacherId = teacherId;
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "SchoolClass", id.ToString());
            return null;
        }

        public async Task<string?> DeleteClassAsync(int id)
        {
            var schoolClass = await _context.Classes.FindAsync(id);
            if (schoolClass == null) return "Turma não encontrada.";

            // Soft-delete all enrollments in the class
            var enrollments = await _context.ClassEnrollments.Where(ce => ce.ClassId == id).ToListAsync();
            foreach (var enrollment in enrollments)
            {
                enrollment.MarkAsDeleted();
            }

            // Unlink any challenges from this class
            var challenges = await _context.Challenges.Where(ch => ch.ClassId == id).ToListAsync();
            foreach (var challenge in challenges)
            {
                challenge.ClassId = null;
            }

            // Soft-delete the class itself
            schoolClass.MarkAsDeleted();

            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Delete, "SchoolClass", id.ToString());
            return null;
        }
    }
}
