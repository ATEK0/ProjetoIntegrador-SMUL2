using System;

namespace Portal.Models.ViewModels
{
    public class AuditLogItemViewModel
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string? EntityId { get; set; }

        public string EntityTypeLabel => EntityType switch
        {
            "SchoolClass" => "Turma",
            "ClassEnrollment" => "Inscrição",
            "Challenge" => "Desafio",
            "Scenario" => "Cenário",
            "Entry" => "Registo Financeiro",
            "ScenarioMember" => "Membro do Cenário",
            "User" => "Utilizador",
            _ => EntityType
        };

        public string ActionLabel => Action switch
        {
            "Create" => "Criação",
            "Update" => "Atualização",
            "Delete" => "Eliminação",
            _ => Action
        };
    }
}
