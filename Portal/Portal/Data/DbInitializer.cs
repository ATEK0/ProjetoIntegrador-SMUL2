using Microsoft.EntityFrameworkCore;
using Portal.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Portal.Data
{
    public static class DbInitializer
    {
        private const string ClassMembershipCode = "DEMO26";
        private const string ChallengeAccessCode = "CHLG01";

        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            SeedRoles(context);
            SeedUserStatuses(context);

            var activeStatus = context.UserStatuses.First(s => s.StatusName == "Ativo");
            var hasher = new PasswordHasher<User>();

            var admin = SeedUser(context, hasher, activeStatus.Id, "Admin", "Administrador do Sistema", "admin@dec.pt", "admin123");
            var professor = SeedUser(context, hasher, activeStatus.Id, "Professor", "Professor Demo", "professor@dec.pt", "professor123");
            var aluno1 = SeedUser(context, hasher, activeStatus.Id, "Aluno", "Aluno Demo", "aluno@dec.pt", "aluno123");
            var aluno2 = SeedUser(context, hasher, activeStatus.Id, "Aluno", "Aluno Demo 2", "aluno2@dec.pt", "aluno123");

            if (professor == null) return;

            var schoolClass = SeedClass(context, professor.Id, "Turma Literacia Financeira", ClassMembershipCode);
            if (schoolClass == null) return;

            if (aluno1 != null) SeedEnrollment(context, schoolClass.Id, aluno1.Id);
            if (aluno2 != null) SeedEnrollment(context, schoolClass.Id, aluno2.Id);

            var challenge = SeedChallenge(context, professor.Id, schoolClass.Id,
                "Poupar para o Futuro",
                "Simulação de orçamento familiar e poupança ao longo do ano letivo.",
                ChallengeAccessCode);

            if (aluno1 != null && challenge != null)
                SeedStudentScenario(context, aluno1.Id, challenge.Id, "Família Silva", 500m);
        }

        private static void SeedRoles(ApplicationDbContext context)
        {
            if (context.Roles.Any()) return;

            context.Roles.AddRange(
                new Role("Admin"),
                new Role("Professor"),
                new Role("Aluno")
            );
            context.SaveChanges();
        }

        private static void SeedUserStatuses(ApplicationDbContext context)
        {
            if (context.UserStatuses.Any()) return;

            context.UserStatuses.AddRange(
                new UserStatus("Ativo"),
                new UserStatus("Pendente"),
                new UserStatus("Inativo")
            );
            context.SaveChanges();
        }

        private static User? SeedUser(
            ApplicationDbContext context,
            PasswordHasher<User> hasher,
            int activeStatusId,
            string roleName,
            string name,
            string email,
            string password)
        {
            var normalizedEmail = email.Trim().ToLower();
            var existing = context.Users.FirstOrDefault(u => u.Email == normalizedEmail);
            if (existing != null) return existing;

            var role = context.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null) return null;

            var user = new User(name, activeStatusId, role.Id) { Email = normalizedEmail };
            user.PasswordHash = hasher.HashPassword(user, password);
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        private static SchoolClass? SeedClass(ApplicationDbContext context, int teacherId, string name, string membershipCode)
        {
            var existing = context.Classes.FirstOrDefault(c => c.MembershipCode == membershipCode);
            if (existing != null) return existing;

            var schoolClass = new SchoolClass(teacherId, name, membershipCode);
            context.Classes.Add(schoolClass);
            context.SaveChanges();
            return schoolClass;
        }

        private static void SeedEnrollment(ApplicationDbContext context, int classId, int studentId)
        {
            if (context.ClassEnrollments.Any(ce => ce.ClassId == classId && ce.StudentId == studentId))
                return;

            context.ClassEnrollments.Add(new ClassEnrollment(classId, studentId));
            context.SaveChanges();
        }

        private static Challenge? SeedChallenge(
            ApplicationDbContext context,
            int teacherId,
            int classId,
            string title,
            string description,
            string accessLinkCode)
        {
            var existing = context.Challenges.FirstOrDefault(c => c.AccessLinkCode == accessLinkCode);
            if (existing != null) return existing;

            var challenge = new Challenge(teacherId, title, accessLinkCode)
            {
                Description = description,
                ClassId = classId
            };
            context.Challenges.Add(challenge);
            context.SaveChanges();
            return challenge;
        }

        private static void SeedStudentScenario(
            ApplicationDbContext context,
            int studentId,
            int challengeId,
            string familyName,
            decimal initialBalance)
        {
            var scenario = context.Scenarios
                .FirstOrDefault(s => s.StudentId == studentId && s.FamilyName == familyName);

            if (scenario == null)
            {
                scenario = new Scenario(studentId, familyName, initialBalance) { ChallengeId = challengeId };
                context.Scenarios.Add(scenario);
                context.SaveChanges();
            }

            if (!context.ScenarioMembers.Any(m => m.ScenarioId == scenario.Id))
            {
                context.ScenarioMembers.Add(new ScenarioMember(scenario.Id, "Maria Silva", 850m));
                context.SaveChanges();
            }

            if (!context.Entries.Any(e => e.ScenarioId == scenario.Id))
            {
                context.Entries.AddRange(
                    new Entry(scenario.Id, EntryType.Income, "Salário", 1200m, 1, RecurrenceType.Monthly),
                    new Entry(scenario.Id, EntryType.Expense, "Renda", 450m, 1, RecurrenceType.Monthly),
                    new Entry(scenario.Id, EntryType.Expense, "Supermercado", 300m, 1, RecurrenceType.Monthly),
                    new Entry(scenario.Id, EntryType.Expense, "Transportes", 40m, 1, RecurrenceType.Monthly)
                );
                context.SaveChanges();
            }
        }
    }
}
