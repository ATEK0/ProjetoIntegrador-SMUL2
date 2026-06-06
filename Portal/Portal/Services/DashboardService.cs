using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync() => new()
        {
            Classes = await _context.Classes.ToListAsync(),
            Challenges = await _context.Challenges.ToListAsync(),
            Users = await _context.Users.ToListAsync()
        };

        public async Task<AdminDashboardViewModel> GetAdminClassesAsync() => new()
        {
            Classes = await _context.Classes.ToListAsync(),
            Challenges = Enumerable.Empty<Challenge>()
        };

        public async Task<AdminDashboardViewModel> GetAdminChallengesAsync() => new()
        {
            Classes = await _context.Classes.ToListAsync(),
            Challenges = await _context.Challenges.Include(c => c.Class).ToListAsync()
        };

        public async Task<AdminUsersViewModel> GetAdminUsersAsync()
        {
            var enrollments = (await _context.ClassEnrollments
                .Include(ce => ce.Class)
                .ToListAsync())
                .GroupBy(ce => ce.StudentId)
                .ToDictionary(g => g.Key, g => g.Select(ce => new UserClassEnrollmentDetail
                {
                    ClassId = ce.ClassId,
                    ClassName = ce.Class.Name
                }).ToList());

            var users = await _context.Users.Include(u => u.Role).ToListAsync();

            return new AdminUsersViewModel
            {
                Users = users.Select(u => new AdminUserDetailViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    Enrollments = enrollments.GetValueOrDefault(u.Id) ?? new List<UserClassEnrollmentDetail>()
                }).ToList(),
                Classes = await _context.Classes.ToListAsync()
            };
        }

        public async Task<string?> ChangeUserRoleAsync(int currentUserId, int userId, string newRoleName)
        {
            if (userId == currentUserId)
                return "Não pode alterar o seu próprio papel de administrador.";

            if (newRoleName is not ("Professor" or "Aluno" or "Admin"))
                return "Função/Role inválida.";

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return "Utilizador não encontrado.";

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == newRoleName);
            if (role == null) return "Função/Role não existe na base de dados.";

            user.RoleId = role.Id;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(int teacherId)
        {
            var teacherClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();

            var classIds = teacherClasses.Select(c => c.Id).ToList();

            var enrollmentCounts = await _context.ClassEnrollments
                .Where(ce => classIds.Contains(ce.ClassId))
                .GroupBy(ce => ce.ClassId)
                .Select(g => new { ClassId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClassId, x => x.Count);

            return new TeacherDashboardViewModel
            {
                TotalStudents = enrollmentCounts.Values.Sum(),
                TotalChallenges = await _context.Challenges.CountAsync(c => c.TeacherId == teacherId),
                Classes = teacherClasses.Select(c => new TeacherClassDetailViewModel
                {
                    ClassId = c.Id,
                    ClassName = c.Name,
                    MembershipCode = c.MembershipCode,
                    StudentCount = enrollmentCounts.GetValueOrDefault(c.Id)
                }).ToList(),
                Challenges = await _context.Challenges.Include(c => c.Class)
                    .Where(c => c.TeacherId == teacherId)
                    .ToListAsync()
            };
        }
        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(int studentId, string studentName)
        {
            var scenarios = await _context.Scenarios
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var enrollments = await _context.ClassEnrollments
                .Include(ce => ce.Class)
                .Where(ce => ce.StudentId == studentId)
                .Select(ce => new UserClassEnrollmentDetail
                {
                    ClassId = ce.ClassId,
                    ClassName = ce.Class.Name
                })
                .ToListAsync();

            var classIds = enrollments.Select(e => e.ClassId).ToList();

            var pendingChallenges = await _context.Challenges
                .Include(c => c.Class)
                .Where(c => classIds.Contains(c.ClassId.Value))
                .ToListAsync();

            return new StudentDashboardViewModel
            {
                StudentName = studentName,
                Scenarios = scenarios,
                EnrolledClasses = enrollments,
                PendingChallenges = pendingChallenges
            };
        }

        public async Task<UserDetailsViewModel> GetUserDetailsAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            var roles = await _context.Roles.ToListAsync();

            var taughtClasses = await _context.Classes
                .Where(c => c.TeacherId == id)
                .ToListAsync();

            var enrolledClasses = await _context.ClassEnrollments
                .Include(ce => ce.Class)
                .Where(ce => ce.StudentId == id)
                .Select(ce => ce.Class)
                .ToListAsync();

            var createdChallenges = await _context.Challenges
                .Where(c => c.TeacherId == id)
                .ToListAsync();

            var participatedChallenges = await _context.Scenarios
                .Include(s => s.Challenge)
                .Where(s => s.StudentId == id && s.ChallengeId != null)
                .Select(s => s.Challenge)
                .Distinct()
                .ToListAsync();

            return new UserDetailsViewModel
            {
                User = user,
                AvailableRoles = roles,
                TaughtClasses = taughtClasses,
                EnrolledClasses = enrolledClasses,
                CreatedChallenges = createdChallenges,
                ParticipatedChallenges = participatedChallenges
            };
        }
    }
}
