using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class SimulationHistory : BaseEntity
    {
        private int _userId;
        private string _simulationMode;
        private double _principal;
        private double _ratePercentage;
        private double _time;
        private string _type;
        private string _periodicity;
        private double? _monthlyCommission;
        private string _parametersJson;
        private string _resultJson;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        [MaxLength(50)]
        public string SimulationMode
        {
            get { return _simulationMode; }
            set { _simulationMode = value; UpdateTimestamp(); }
        }

        public double Principal
        {
            get { return _principal; }
            set { _principal = value; UpdateTimestamp(); }
        }

        public double RatePercentage
        {
            get { return _ratePercentage; }
            set { _ratePercentage = value; UpdateTimestamp(); }
        }

        public double Time
        {
            get { return _time; }
            set { _time = value; UpdateTimestamp(); }
        }

        [Required]
        [MaxLength(50)]
        public string Type
        {
            get { return _type; }
            set { _type = value; UpdateTimestamp(); }
        }

        [MaxLength(50)]
        public string Periodicity
        {
            get { return _periodicity; }
            set { _periodicity = value; UpdateTimestamp(); }
        }

        public double? MonthlyCommission
        {
            get { return _monthlyCommission; }
            set { _monthlyCommission = value; UpdateTimestamp(); }
        }

        [Required]
        public string ParametersJson
        {
            get { return _parametersJson; }
            set { _parametersJson = value; UpdateTimestamp(); }
        }

        [Required]
        public string ResultJson
        {
            get { return _resultJson; }
            set { _resultJson = value; UpdateTimestamp(); }
        }

        public SimulationHistory(int userId, string simulationMode, double principal, double ratePercentage, double time, string type, string parametersJson, string resultJson)
        {
            UserId = userId;
            SimulationMode = simulationMode;
            Principal = principal;
            RatePercentage = ratePercentage;
            Time = time;
            Type = type;
            ParametersJson = parametersJson;
            ResultJson = resultJson;
        }

        protected SimulationHistory() { }
    }
}
