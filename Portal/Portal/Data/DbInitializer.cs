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
        }
    }
}
