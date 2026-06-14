"""
Testes para o SimulationService.
"""

import pytest
from rest_framework.exceptions import ValidationError
from api.services.simulation_service import SimulationService


class TestRunInterest:

    def test_juros_simples(self):
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "simple",
            "rate_tiers": None,
        }
        result = SimulationService.run_interest(data)
        assert result["interest"] == 1500.0
        assert result["total_amount"] == 11500.0

    def test_juros_compostos(self):
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "compound",
            "rate_tiers": None,
        }
        result = SimulationService.run_interest(data)
        assert result["interest"] == pytest.approx(1576.25, abs=0.01)

    def test_sem_rate_sem_tiers_da_erro(self):
        data = {
            "principal": 10000,
            "rate": None,
            "time": 3,
            "type": "simple",
            "rate_tiers": None,
        }
        with pytest.raises(ValidationError) as exc_info:
            SimulationService.run_interest(data)
        assert "rate" in str(exc_info.value.detail)

    def test_rate_fallback_dos_tiers(self):
        # quando rate eh None mas ha tiers, usa o primeiro tier
        data = {
            "principal": 10000,
            "rate": None,
            "time": 3,
            "type": "compound",
            "rate_tiers": [
                {"from_period": 1, "rate": 0.04},
                {"from_period": 2, "rate": 0.06},
            ],
        }
        result = SimulationService.run_interest(data)
        assert result["breakdown"] is not None
        assert result["breakdown"][0]["rate"] == 0.04

    def test_tiers_invalidos_da_erro(self):
        # primeiro periodo != 1
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "compound",
            "rate_tiers": [
                {"from_period": 2, "rate": 0.03},
            ],
        }
        with pytest.raises(ValidationError) as exc_info:
            SimulationService.run_interest(data)
        assert "rate_tiers" in str(exc_info.value.detail)

    def test_com_tiers_validos(self):
        data = {
            "principal": 10000,
            "rate": 0.03,
            "time": 4,
            "type": "compound",
            "rate_tiers": [
                {"from_period": 1, "rate": 0.03},
                {"from_period": 3, "rate": 0.05},
            ],
        }
        result = SimulationService.run_interest(data)
        assert result["breakdown"] is not None
        assert len(result["breakdown"]) == 4

    def test_tier_excede_tempo(self):
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "simple",
            "rate_tiers": [
                {"from_period": 1, "rate": 0.03},
                {"from_period": 10, "rate": 0.05},
            ],
        }
        with pytest.raises(ValidationError):
            SimulationService.run_interest(data)


class TestRunAmortization:

    def test_french(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 2,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        result = SimulationService.run_amortization(data)
        assert "schedule" in result
        assert "taeg" in result
        assert "is_inicial" in result
        assert result["schedule"][-1]["balance"] == 0.0

    def test_sac(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 2,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "sac",
        }
        result = SimulationService.run_amortization(data)
        assert result["schedule"][-1]["balance"] == 0.0

    def test_american(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 2,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "american",
        }
        result = SimulationService.run_amortization(data)
        assert result["schedule"][-1]["balance"] == 0.0

    def test_com_comissao(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 1,
            "periodicity": "monthly",
            "commission": 15.0,
            "type": "french",
        }
        result = SimulationService.run_amortization(data)
        for row in result["schedule"]:
            if row["period"] > 0:
                assert row["comissao"] == 15.0

    def test_periodicidade_anual(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "annual",
            "commission": 0.0,
            "type": "sac",
        }
        result = SimulationService.run_amortization(data)
        assert len(result["schedule"]) == 6  # 5 periodos + periodo 0
