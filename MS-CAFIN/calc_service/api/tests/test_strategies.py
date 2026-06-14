"""
Testes para as estrategias de calculo (juros e amortizacao).
"""

import pytest
from api.domain.strategies import (
    SimpleInterest,
    CompoundInterest,
    FrenchAmortization,
    SACAmortization,
    AmericanAmortization,
    calculate_irr,
)

# --- Juros Simples ---


class TestSimpleInterest:

    def setup_method(self):
        self.strategy = SimpleInterest()

    def test_calculo_basico(self):
        # J = 10000 * 0.05 * 3 = 1500
        result = self.strategy.calculate(principal=10000, rate=0.05, time=3)
        assert result["interest"] == 1500.0
        assert result["total_amount"] == 11500.0
        assert result["breakdown"] is None

    def test_taxa_zero(self):
        result = self.strategy.calculate(principal=10000, rate=0.0, time=5)
        assert result["interest"] == 0.0
        assert result["total_amount"] == 10000.0

    def test_tempo_fracionario(self):
        result = self.strategy.calculate(principal=10000, rate=0.1, time=2.5)
        assert result["interest"] == 2500.0
        assert result["total_amount"] == 12500.0

    def test_valores_grandes(self):
        result = self.strategy.calculate(principal=1_000_000, rate=0.12, time=30)
        assert result["interest"] == 3_600_000.0
        assert result["total_amount"] == 4_600_000.0

    def test_taxa_pequena(self):
        result = self.strategy.calculate(principal=1000, rate=0.001, time=1)
        assert result["interest"] == 1.0
        assert result["total_amount"] == 1001.0


# --- Juros Compostos ---


class TestCompoundInterest:

    def setup_method(self):
        self.strategy = CompoundInterest()

    def test_calculo_basico(self):
        # M = 10000 * (1.05)^3 - 10000
        result = self.strategy.calculate(principal=10000, rate=0.05, time=3)
        assert result["interest"] == pytest.approx(1576.25, abs=0.01)
        assert result["total_amount"] == pytest.approx(11576.25, abs=0.01)
        assert result["breakdown"] is None

    def test_taxa_zero(self):
        result = self.strategy.calculate(principal=10000, rate=0.0, time=10)
        assert result["interest"] == 0.0
        assert result["total_amount"] == 10000.0

    def test_um_periodo(self):
        # com 1 periodo, deve ser igual a juros simples
        result = self.strategy.calculate(principal=10000, rate=0.05, time=1)
        assert result["interest"] == pytest.approx(500.0, abs=0.01)
        assert result["total_amount"] == pytest.approx(10500.0, abs=0.01)

    def test_composto_maior_que_simples(self):
        # para t > 1, composto tem de dar mais que simples
        compound = CompoundInterest().calculate(principal=10000, rate=0.1, time=5)
        simple = SimpleInterest().calculate(principal=10000, rate=0.1, time=5)
        assert compound["interest"] > simple["interest"]

    def test_tempo_fracionario(self):
        result = self.strategy.calculate(principal=10000, rate=0.1, time=2.5)
        expected_total = 10000 * (1.1**2.5)
        assert result["total_amount"] == pytest.approx(expected_total, rel=1e-4)


# --- Juros com Tiers ---


class TestInterestWithTiers:

    def test_simples_com_tiers(self):
        strategy = SimpleInterest()
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 3, "rate": 0.05},
        ]
        result = strategy.calculate_with_tiers(
            principal=10000, default_rate=0.03, time=4, tiers=tiers
        )

        assert result["breakdown"] is not None
        assert len(result["breakdown"]) == 4

        # periodos 1-2 com 3%, periodos 3-4 com 5%
        assert result["breakdown"][0]["rate"] == 0.03
        assert result["breakdown"][1]["rate"] == 0.03
        assert result["breakdown"][2]["rate"] == 0.05
        assert result["breakdown"][3]["rate"] == 0.05

        assert result["breakdown"][0]["interest"] == pytest.approx(300.0, abs=0.01)
        assert result["breakdown"][2]["interest"] == pytest.approx(500.0, abs=0.01)

    def test_composto_com_tiers(self):
        strategy = CompoundInterest()
        tiers = [
            {"from_period": 1, "rate": 0.05},
            {"from_period": 3, "rate": 0.08},
        ]
        result = strategy.calculate_with_tiers(
            principal=10000, default_rate=0.05, time=4, tiers=tiers
        )

        assert result["breakdown"] is not None
        assert len(result["breakdown"]) == 4
        assert result["breakdown"][0]["amount"] == pytest.approx(10500.0, abs=0.01)

    def test_periodo_fracionario_com_tiers(self):
        strategy = SimpleInterest()
        tiers = [{"from_period": 1, "rate": 0.1}]
        result = strategy.calculate_with_tiers(
            principal=10000, default_rate=0.1, time=2.5, tiers=tiers
        )

        assert len(result["breakdown"]) == 3  # 2 inteiros + 1 parcial
        assert result["breakdown"][2].get("partial") is True

    def test_tier_unico(self):
        strategy = CompoundInterest()
        tiers = [{"from_period": 1, "rate": 0.05}]
        result = strategy.calculate_with_tiers(
            principal=10000, default_rate=0.05, time=3, tiers=tiers
        )
        assert len(result["breakdown"]) == 3
        assert all(row["rate"] == 0.05 for row in result["breakdown"])


# --- Amortizacao Francesa ---


class TestFrenchAmortization:

    def setup_method(self):
        self.strategy = FrenchAmortization()

    def test_tamanho_schedule(self):
        # deve ter n+1 linhas (periodo 0 + n periodos)
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=2, periodicity="monthly", commission=0.0
        )
        assert len(result["schedule"]) == 25  # 24 meses + periodo 0

    def test_balance_inicial(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=1, periodicity="monthly", commission=0.0
        )
        assert result["schedule"][0]["balance"] == 100000.0
        assert result["schedule"][0]["period"] == 0

    def test_balance_final_zero(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=2, periodicity="monthly", commission=0.0
        )
        assert result["schedule"][-1]["balance"] == 0.0

    def test_prestacao_constante(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.06, years=5, periodicity="monthly", commission=0.0
        )
        installments = [
            row["installment"] for row in result["schedule"] if row["period"] > 0
        ]
        assert all(i == installments[0] for i in installments)

    def test_imposto_selo(self):
        # IS = 4% sobre os juros
        result = self.strategy.calculate(
            principal=100000, tan=0.12, years=1, periodicity="monthly", commission=0.0
        )
        first = result["schedule"][1]
        expected_is = round(first["interest"] * 0.04, 2)
        assert first["imposto_selo"] == expected_is

    def test_com_comissao(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=1, periodicity="monthly", commission=10.0
        )
        first = result["schedule"][1]
        assert first["comissao"] == 10.0
        expected_total = first["installment"] + first["imposto_selo"] + 10.0
        assert first["total_pago"] == pytest.approx(expected_total, abs=0.02)

    def test_taeg_positiva(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=5, periodicity="monthly", commission=0.0
        )
        assert result["taeg"] > 0

    def test_is_inicial(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=5, periodicity="monthly", commission=0.0
        )
        # prazo >= 5 anos -> taxa 0.006
        assert result["is_inicial"] == pytest.approx(100000 * 0.006, abs=0.01)

    def test_taxa_zero(self):
        # sem juros, prestacao = principal / n
        result = self.strategy.calculate(
            principal=12000, tan=0.0, years=1, periodicity="monthly", commission=0.0
        )
        for row in result["schedule"]:
            if row["period"] > 0:
                assert row["installment"] == pytest.approx(1000.0, abs=0.01)

    def test_periodicidade_trimestral(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.08, years=2, periodicity="quarterly", commission=0.0
        )
        assert len(result["schedule"]) == 9  # 2*4 + periodo 0


# --- Amortizacao SAC ---


class TestSACAmortization:

    def setup_method(self):
        self.strategy = SACAmortization()

    def test_amortizacao_constante(self):
        result = self.strategy.calculate(
            principal=120000, tan=0.06, years=1, periodicity="monthly", commission=0.0
        )
        expected = 120000 / 12
        for row in result["schedule"]:
            if row["period"] > 0:
                assert row["amortization"] == pytest.approx(expected, abs=0.01)

    def test_juros_decrescentes(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.12, years=2, periodicity="monthly", commission=0.0
        )
        interests = [row["interest"] for row in result["schedule"] if row["period"] > 0]
        for j in range(len(interests) - 1):
            assert interests[j] >= interests[j + 1]

    def test_balance_final_zero(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=3, periodicity="monthly", commission=0.0
        )
        assert result["schedule"][-1]["balance"] == 0.0

    def test_tamanho_schedule(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=2, periodicity="monthly", commission=0.0
        )
        assert len(result["schedule"]) == 25


# --- Amortizacao Americana ---


class TestAmericanAmortization:

    def setup_method(self):
        self.strategy = AmericanAmortization()

    def test_so_juros_ate_ao_final(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.06, years=1, periodicity="monthly", commission=0.0
        )
        for row in result["schedule"]:
            if 0 < row["period"] < 12:
                assert row["amortization"] == 0.0

    def test_principal_todo_no_fim(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.06, years=1, periodicity="monthly", commission=0.0
        )
        last = result["schedule"][-1]
        assert last["amortization"] == 100000.0
        assert last["balance"] == 0.0

    def test_juros_constantes(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.12, years=2, periodicity="monthly", commission=0.0
        )
        interests = [row["interest"] for row in result["schedule"] if row["period"] > 0]
        assert all(i == interests[0] for i in interests)

    def test_balance_final_zero(self):
        result = self.strategy.calculate(
            principal=100000, tan=0.05, years=5, periodicity="annual", commission=0.0
        )
        assert result["schedule"][-1]["balance"] == 0.0


# --- IRR ---


class TestCalculateIRR:

    def test_irr_conhecida(self):
        # investimento de 1000, retorno 1100 -> IRR = 10%
        result = calculate_irr([-1000, 1100])
        assert result is not None
        assert result == pytest.approx(0.1, abs=0.001)

    def test_npv_zero(self):
        result = calculate_irr([-1000, 500, 500])
        assert result is not None
        assert result == pytest.approx(0.0, abs=0.01)

    def test_multiplos_periodos(self):
        result = calculate_irr([-10000, 3000, 4000, 5000])
        assert result is not None
        assert result > 0

    def test_nao_convergencia(self):
        # com max_iter=1 provavelmente nao converge
        result = calculate_irr([-1000, 200, 300, 400, 500], max_iter=1)
        assert result is None or isinstance(result, float)
