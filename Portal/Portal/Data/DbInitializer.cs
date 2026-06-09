using Microsoft.EntityFrameworkCore;
using Portal.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Portal.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Certificar que a base de dados existe e aplicar todas as migrações pendentes
            context.Database.Migrate();

            // Inicializar as Roles se não existirem
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role("Admin"),
                    new Role("Professor"),
                    new Role("Aluno")
                );
                context.SaveChanges();
            }

            // Inicializar os UserStatuses se não existirem
            if (!context.UserStatuses.Any())
            {
                context.UserStatuses.AddRange(
                    new UserStatus("Ativo"),
                    new UserStatus("Pendente"),
                    new UserStatus("Inativo")
                );
                context.SaveChanges();
            }

            // Criar um utilizador Administrador padrão se não existir nenhum utilizador
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                var activeStatus = context.UserStatuses.FirstOrDefault(s => s.StatusName == "Ativo");

                if (adminRole != null && activeStatus != null)
                {
                    var adminUser = new User("Administrador do Sistema", activeStatus.Id, adminRole.Id)
                    {
                        Email = "admin@dec.pt"
                    };

                    var hasher = new PasswordHasher<User>();
                    adminUser.PasswordHash = hasher.HashPassword(adminUser, "admin123");

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }

            // Seeding Convincing Data
            if (context.Users.Count() <= 1)
            {
                var profRole = context.Roles.FirstOrDefault(r => r.RoleName == "Professor");
                var alunoRole = context.Roles.FirstOrDefault(r => r.RoleName == "Aluno");
                var activeStatus = context.UserStatuses.FirstOrDefault(s => s.StatusName == "Ativo");

                if (profRole != null && alunoRole != null && activeStatus != null)
                {
                    var hasher = new PasswordHasher<User>();

                    // 1. Create Teachers
                    var prof1 = new User("Ana Martins", activeStatus.Id, profRole.Id) { Email = "ana.martins@escola.pt" };
                    prof1.PasswordHash = hasher.HashPassword(prof1, "prof123");

                    var prof2 = new User("Carlos Ribeiro", activeStatus.Id, profRole.Id) { Email = "carlos.ribeiro@escola.pt" };
                    prof2.PasswordHash = hasher.HashPassword(prof2, "prof123");

                    // 2. Create Students
                    var aluno1 = new User("João Santos", activeStatus.Id, alunoRole.Id) { Email = "joao.santos@aluno.pt" };
                    aluno1.PasswordHash = hasher.HashPassword(aluno1, "aluno123");

                    var aluno2 = new User("Maria Fernandes", activeStatus.Id, alunoRole.Id) { Email = "maria.fernandes@aluno.pt" };
                    aluno2.PasswordHash = hasher.HashPassword(aluno2, "aluno123");

                    var aluno3 = new User("Pedro Silva", activeStatus.Id, alunoRole.Id) { Email = "pedro.silva@aluno.pt" };
                    aluno3.PasswordHash = hasher.HashPassword(aluno3, "aluno123");

                    context.Users.AddRange(prof1, prof2, aluno1, aluno2, aluno3);
                    context.SaveChanges();

                    // 3. Create Classes
                    var turma1 = new SchoolClass(prof1.Id, "Educação Financeira 10ºA", "10A2026");
                    var turma2 = new SchoolClass(prof2.Id, "Economia 11ºB", "11B2026");

                    context.Classes.AddRange(turma1, turma2);
                    context.SaveChanges();

                    // 4. Enroll Students
                    context.ClassEnrollments.AddRange(
                        new ClassEnrollment(turma1.Id, aluno1.Id),
                        new ClassEnrollment(turma1.Id, aluno2.Id),
                        new ClassEnrollment(turma2.Id, aluno2.Id),
                        new ClassEnrollment(turma2.Id, aluno3.Id)
                    );
                    context.SaveChanges();

                    // 5. Create Challenges
                    var challenge1 = new Challenge(prof1.Id, "Gestão do Primeiro Salário", "SALARIO1", Models.Enums.ChallengeType.Standard)
                    {
                        ClassId = turma1.Id,
                        Description = "Imagina que começaste a trabalhar e recebeste o teu primeiro salário de 1000€. Tens de pagar renda, alimentação e queres poupar para um carro."
                    };

                    var challenge2 = new Challenge(prof1.Id, "Quiz: Literacia Financeira Básica", "QUIZFIN1", Models.Enums.ChallengeType.Quiz)
                    {
                        Description = "Testa os teus conhecimentos sobre poupança, crédito e investimento."
                    };

                    context.Challenges.AddRange(challenge1, challenge2);
                    context.SaveChanges();

                    // 6. Create Quiz Questions and Options
                    var q1 = new QuizQuestion(challenge2.Id, "O que é um Fundo de Emergência?", 1);
                    var q2 = new QuizQuestion(challenge2.Id, "Qual é a regra de ouro da poupança?", 2);
                    context.QuizQuestions.AddRange(q1, q2);
                    context.SaveChanges();

                    context.QuizOptions.AddRange(
                        new QuizOption(q1.Id, "Um fundo para gastar nas férias", false),
                        new QuizOption(q1.Id, "Dinheiro guardado para despesas imprevistas (ex: saúde, desemprego)", true),
                        new QuizOption(q1.Id, "Um tipo de investimento em ações", false),

                        new QuizOption(q2.Id, "Gastar primeiro e poupar o que sobrar", false),
                        new QuizOption(q2.Id, "Pagar-se a si próprio primeiro (poupar logo que recebe)", true),
                        new QuizOption(q2.Id, "Colocar todo o dinheiro debaixo do colchão", false)
                    );
                    context.SaveChanges();

                    // 7. Create a Scenario for Student 1 in Challenge 1
                    var scenario1 = new Scenario(aluno1.Id, "Vida Independente", 1000m)
                    {
                        ChallengeId = challenge1.Id
                    };
                    context.Scenarios.Add(scenario1);
                    context.SaveChanges();

                    context.Entries.AddRange(
                        new Entry(scenario1.Id, Models.EntryType.Expense, "Renda da Casa", 400m, 1, Models.RecurrenceType.Monthly),
                        new Entry(scenario1.Id, Models.EntryType.Expense, "Supermercado", 200m, 1, Models.RecurrenceType.Monthly),
                        new Entry(scenario1.Id, Models.EntryType.Expense, "Poupança Carro", 150m, 1, Models.RecurrenceType.Monthly)
                    );
                    context.SaveChanges();

                    // 8. Enroll Student 3 in Independent Challenge (Challenge 2)
                    context.StudentChallenges.Add(new StudentChallenge(aluno3.Id, challenge2.Id));
                    context.SaveChanges();
                }
            }
        }
    }
}
