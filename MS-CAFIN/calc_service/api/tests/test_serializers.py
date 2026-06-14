"""
Testes para os serializers.
"""

from api.serializers import (
    InterestSimulationSerializer,
    AmortizationSimulationSerializer,
)


class TestInterestSerializer:

    def test_dados_validos_simples(self):
        data = {"principal": 10000, "rate": 0.05, "time": 3, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_dados_validos_composto(self):
        data = {"principal": 10000, "rate": 0.05, "time": 3, "type": "compound"}
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_com_rate_tiers(self):
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
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_falta_principal(self):
        data = {"rate": 0.05, "time": 3, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "principal" in s.errors

    def test_falta_time(self):
        data = {"principal": 10000, "rate": 0.05, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "time" in s.errors

    def test_falta_type(self):
        data = {"principal": 10000, "rate": 0.05, "time": 3}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "type" in s.errors

    def test_type_invalido(self):
        data = {"principal": 10000, "rate": 0.05, "time": 3, "type": "exponential"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "type" in s.errors

    def test_principal_abaixo_minimo(self):
        data = {"principal": 0.0, "rate": 0.05, "time": 3, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "principal" in s.errors

    def test_time_abaixo_minimo(self):
        data = {"principal": 10000, "rate": 0.05, "time": 0.0, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "time" in s.errors

    def test_rate_opcional(self):
        # rate pode nao vir se houver tiers
        data = {
            "principal": 10000,
            "time": 3,
            "type": "compound",
            "rate_tiers": [{"from_period": 1, "rate": 0.05}],
        }
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_rate_tiers_vazio(self):
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "simple",
            "rate_tiers": [],
        }
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_rate_tiers_null(self):
        data = {
            "principal": 10000,
            "rate": 0.05,
            "time": 3,
            "type": "simple",
            "rate_tiers": None,
        }
        s = InterestSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_rate_negativa(self):
        data = {"principal": 10000, "rate": -0.05, "time": 3, "type": "simple"}
        s = InterestSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "rate" in s.errors


class TestAmortizationSerializer:

    def test_french_valido(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_sac_valido(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "sac",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_american_valido(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "american",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert s.is_valid(), s.errors

    def test_todas_periodicidades(self):
        for p in ["monthly", "quarterly", "semiannual", "annual"]:
            data = {
                "principal": 100000,
                "rate": 0.05,
                "years": 5,
                "periodicity": p,
                "commission": 0.0,
                "type": "french",
            }
            s = AmortizationSimulationSerializer(data=data)
            assert s.is_valid(), f"{p}: {s.errors}"

    def test_periodicidade_invalida(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "weekly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "periodicity" in s.errors

    def test_type_invalido(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "linear",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "type" in s.errors

    def test_falta_principal(self):
        data = {
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "principal" in s.errors

    def test_falta_rate(self):
        data = {
            "principal": 100000,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "rate" in s.errors

    def test_falta_years(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "years" in s.errors

    def test_principal_abaixo_minimo(self):
        data = {
            "principal": 0.0,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "principal" in s.errors

    def test_years_abaixo_minimo(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 0.0,
            "periodicity": "monthly",
            "commission": 0.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "years" in s.errors

    def test_comissao_negativa(self):
        data = {
            "principal": 100000,
            "rate": 0.05,
            "years": 5,
            "periodicity": "monthly",
            "commission": -10.0,
            "type": "french",
        }
        s = AmortizationSimulationSerializer(data=data)
        assert not s.is_valid()
        assert "commission" in s.errors
