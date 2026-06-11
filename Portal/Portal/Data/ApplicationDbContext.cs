using Microsoft.EntityFrameworkCore;
using Portal.Models;

namespace Portal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        public DbSet<ScenarioMember> ScenarioMembers { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<SimulationHistory> SimulationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Soft Delete globalmente
            modelBuilder.Entity<Role>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<UserStatus>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<User>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<SchoolClass>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<ClassEnrollment>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Challenge>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Scenario>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<ScenarioMember>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Entry>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Objective>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<SimulationHistory>().HasQueryFilter(x => x.DeletedAt == null);

            // Traduzir os Enums para String no MySQL
            modelBuilder.Entity<Entry>().Property(e => e.EntryType).HasConversion<string>();
            modelBuilder.Entity<Entry>().Property(e => e.Recurrence).HasConversion<string>();


            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            // Aplicar os Índices do ficheiro SQL
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => new { u.RoleId, u.UserStatusId, u.DeletedAt });

            modelBuilder.Entity<SchoolClass>().HasIndex(c => c.MembershipCode).IsUnique();
            modelBuilder.Entity<SchoolClass>().HasIndex(c => new { c.TeacherId, c.DeletedAt });

            modelBuilder.Entity<ClassEnrollment>().HasIndex(ce => new { ce.ClassId, ce.StudentId, ce.DeletedAt });

            modelBuilder.Entity<Challenge>().HasIndex(c => c.AccessLinkCode).IsUnique();
            modelBuilder.Entity<Challenge>().HasIndex(c => new { c.TeacherId, c.ClassId, c.DeletedAt });

            modelBuilder.Entity<Scenario>().HasIndex(s => new { s.StudentId, s.ChallengeId, s.DeletedAt });
            modelBuilder.Entity<ScenarioMember>().HasIndex(sm => new { sm.ScenarioId, sm.DeletedAt });
            modelBuilder.Entity<Entry>().HasIndex(e => new { e.ScenarioId, e.DeletedAt });
            modelBuilder.Entity<Objective>().HasIndex(o => new { o.ScenarioId, o.DeletedAt });
            modelBuilder.Entity<SimulationHistory>().HasIndex(sh => new { sh.UserId, sh.DeletedAt });
        }
    }
}