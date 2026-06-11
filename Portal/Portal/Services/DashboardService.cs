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
        private readonly IAuditService _audit;

        public DashboardService(ApplicationDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<AdminInfraViewModel> GetAdminInfraAsync()
        {
            bool dbOnline = false;
            try
            {
                dbOnline = await _context.Database.CanConnectAsync();
            }
            catch
            {
                dbOnline = false;
            }

            int auditCount = 0;
            if (dbOnline)
            {
                try { auditCount = await _context.AuditLogs.CountAsync(); }
                catch { auditCount = 0; }
            }

            return new AdminInfraViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalChallenges = await _context.Challenges.CountAsync(),
                TotalAuditLogs = auditCount,
                DatabaseOnline = dbOnline
            };
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync() => new()
        {
            Classes = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Challenges)
                .ToListAsync(),
            Challenges = await _context.Challenges.Include(c => c.Teacher).ToListAsync(),
            Users = await _context.Users.ToListAsync()
        };

        public async Task<List<AuditLogItemViewModel>> GetRecentAuditLogsAsync(int count = 10)
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
            return logs.Select(MapAuditLog).ToList();
        }

        public async Task<AdminAuditLogsViewModel> GetAuditLogsAsync(int page = 1, string? filterAction = null, string? entityType = null, int pageSize = 25)
        {
            if (page < 1) page = 1;

            try
            {
                var query = _context.AuditLogs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(filterAction))
                    query = query.Where(a => a.ActionName == filterAction);

                if (!string.IsNullOrWhiteSpace(entityType))
                    query = query.Where(a => a.EntityType == entityType);

                var totalCount = await query.CountAsync();
                var totalPages = totalCount == 0 ? 1 : (int)System.Math.Ceiling(totalCount / (double)pageSize);

                var entities = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new AdminAuditLogsViewModel
                {
                    Logs = entities.Select(MapAuditLog).ToList(),
                    Page = page,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    FilterAction = filterAction,
                    FilterEntityType = entityType,
                    AvailableActions = await _context.AuditLogs.AsNoTracking().Select(a => a.ActionName).Distinct().OrderBy(a => a).ToListAsync(),
                    AvailableEntityTypes = await _context.AuditLogs.AsNoTracking().Select(a => a.EntityType).Distinct().OrderBy(a => a).ToListAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new AdminAuditLogsViewModel
                {
                    Logs = new List<AuditLogItemViewModel>(),
                    Page = page,
                    TotalPages = 1,
                    TotalCount = 0,
                    FilterAction = filterAction,
                    FilterEntityType = entityType,
                    LoadError = ex.Message
                };
            }
        }

        private static AuditLogItemViewModel MapAuditLog(AuditLog a) => new()
        {
            Id = a.Id,
            Timestamp = a.Timestamp,
            UserId = a.UserId,
            UserEmail = a.UserEmail,
            Action = a.ActionName,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
        };

        public async Task<AdminDashboardViewModel> GetAdminClassesAsync() => new()
        {
            Classes = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Challenges)
                .ToListAsync(),
            Challenges = new List<Challenge>()
        };

        public async Task<AdminDashboardViewModel> GetAdminChallengesAsync() => new()
        {
            Classes = await _context.Classes.Include(c => c.Teacher).ToListAsync(),
            Challenges = await _context.Challenges.Include(c => c.Class).Include(c => c.Teacher).ToListAsync()
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

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return "Utilizador não encontrado.";

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == newRoleName);
            if (role == null) return "Função/Role não existe na base de dados.";

            user.RoleId = role.Id;
            await _context.SaveChangesAsync();
            await _audit.LogAsync(AuditAction.Update, "User", userId.ToString());
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
