using Portal.Models;
using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class ChallengeDetailsViewModel
    {
        public Challenge Challenge { get; set; }
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();
        public List<QuizSubmission> QuizSubmissions { get; set; } = new List<QuizSubmission>();
        public List<SchoolClass> AvailableClasses { get; set; } = new List<SchoolClass>();
        public List<ParticipantViewModel> Participants { get; set; } = new List<ParticipantViewModel>();
    }

    public class ParticipantViewModel
    {
        public string Name { get; set; }
        public string Status { get; set; } // e.g., "Não Iniciado", "Em Progresso", "Concluído"
        public string Detail { get; set; } // e.g., "Pontuação: 5", "Saldo: 1000€"
    }
}
