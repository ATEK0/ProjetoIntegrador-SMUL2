"""
Testes para o SimulatorFacade.
"""

import pytest
from api.domain.facades import SimulatorFacade


class TestFacadeJuros:

    def test_juros_simples(self):
        result = SimulatorFacade.simulate_interest(
            principal=10000, rate=0.05, time=3, interest_type="simple"
        )
        assert "interest" in result
        assert "total_amount" in result
        assert result["interest"] == 1500.0

    def test_juros_compostos(self):
        result = SimulatorFacade.simulate_interest(
            principal=10000, rate=0.05, time=3, interest_type="compound"
        )
        assert result["interest"] == pytest.approx(1576.25, abs=0.01)

    def test_com_rate_tiers(self):
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 3, "rate": 0.06},
        ]
        result = SimulatorFacade.simulate_interest(
            principal=10000, rate=0.03, time=4,
            interest_type="compound", rate_tiers=tiers,
        )
        assert result["breakdown"] is not None
        assert len(result["breakdown"]) == 4

    def test_sem_tiers_sem_breakdown(self):
        result = SimulatorFacade.simulate_interest(
            principal=10000, rate=0.05, time=3, interest_type="simple"
        )
        assert result["breakdown"] is None

    def test_tiers_vazios(self):
        result = SimulatorFacade.simulate_interest(
            principal=10000, rate=0.05, time=3, interest_type="simple", rate_tiers=[]
        )
        assert result["breakdown"] is None

    def test_tipo_desconhecido(self):
        with pytest.raises(ValueError):
            SimulatorFacade.simulate_interest(
                principal=10000, rate=0.05, time=3, interest_type="exponential"
            )


class TestFacadeAmortizacao:

    def test_french(self):
        result = SimulatorFacade.simulate_amortization(
            principal=100000, rate=0.05, years=2,
            periodicity="monthly", commission=0.0, amortization_type="french",
        )
        assert "schedule" in result
        assert "taeg" in result
        assert "is_inicial" in result
        assert len(result["schedule"]) == 25

    def test_sac(self):
        result = SimulatorFacade.simulate_amortization(
            principal=100000, rate=0.05, years=2,
            periodicity="monthly", commission=0.0, amortization_type="sac",
        )
        assert len(result["schedule"]) == 25

    def test_american(self):
        result = SimulatorFacade.simulate_amortization(
            principal=100000, rate=0.05, years=2,
            periodicity="monthly", commission=0.0, amortization_type="american",
        )
        assert len(result["schedule"]) == 25

    def test_tipo_desconhecido(self):
        with pytest.raises(ValueError):
            SimulatorFacade.simulate_amortization(
                principal=100000, rate=0.05, years=2,
                periodicity="monthly", commission=0.0, amortization_type="linear",
            )
