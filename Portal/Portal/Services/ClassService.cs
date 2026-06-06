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

        public ClassService(ApplicationDbContext context, ILogger<ClassService> logger)
        {
            _context = context;
            _logger = logger;
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

            _context.Classes.Add(new SchoolClass(teacherId, name, code));
            await _context.SaveChangesAsync();
            _logger.LogInformation("Turma '{Name}' criada com código {Code}.", name, code);
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

            _context.ClassEnrollments.Add(new ClassEnrollment(targetClass.Id, studentId));
            await _context.SaveChangesAsync();
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

            _context.ClassEnrollments.Add(new ClassEnrollment(classId, userId));
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> RemoveUserFromClassAsync(int userId, int classId)
        {
            var enrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == classId && ce.StudentId == userId);

            if (enrollment == null) return "Inscrição não encontrada.";

            enrollment.MarkAsDeleted();
            await _context.SaveChangesAsync();
            return null;
        }

    }
}
