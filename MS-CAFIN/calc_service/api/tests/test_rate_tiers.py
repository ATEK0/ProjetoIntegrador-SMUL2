"""
Testes para as funcoes de rate_tiers.
"""

import pytest
from api.domain.rate_tiers import (
    normalize_rate_tiers,
    rate_for_period,
    validate_rate_tiers,
)

# --- normalize_rate_tiers ---


class TestNormalizeRateTiers:

    def test_lista_vazia(self):
        assert normalize_rate_tiers([]) == []

    def test_none(self):
        assert normalize_rate_tiers(None) == []

    def test_um_tier(self):
        result = normalize_rate_tiers([{"from_period": 1, "rate": 0.05}])
        assert len(result) == 1
        assert result[0] == {"from_period": 1, "rate": 0.05}

    def test_ordena_por_from_period(self):
        tiers = [
            {"from_period": 3, "rate": 0.08},
            {"from_period": 1, "rate": 0.03},
            {"from_period": 2, "rate": 0.05},
        ]
        result = normalize_rate_tiers(tiers)
        assert result[0]["from_period"] == 1
        assert result[1]["from_period"] == 2
        assert result[2]["from_period"] == 3

    def test_converte_tipos(self):
        tiers = [{"from_period": "2", "rate": "0.05"}]
        result = normalize_rate_tiers(tiers)
        assert result[0]["from_period"] == 2
        assert isinstance(result[0]["from_period"], int)
        assert result[0]["rate"] == 0.05
        assert isinstance(result[0]["rate"], float)


# --- rate_for_period ---


class TestRateForPeriod:

    def test_sem_tiers_retorna_default(self):
        assert rate_for_period(1, 0.05, []) == 0.05

    def test_com_um_tier(self):
        tiers = [{"from_period": 1, "rate": 0.03}]
        assert rate_for_period(1, 0.05, tiers) == 0.03
        assert rate_for_period(5, 0.05, tiers) == 0.03

    def test_multiplos_tiers(self):
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 3, "rate": 0.05},
            {"from_period": 6, "rate": 0.08},
        ]
        assert rate_for_period(1, 0.01, tiers) == 0.03
        assert rate_for_period(2, 0.01, tiers) == 0.03
        assert rate_for_period(3, 0.01, tiers) == 0.05
        assert rate_for_period(5, 0.01, tiers) == 0.05
        assert rate_for_period(6, 0.01, tiers) == 0.08
        assert rate_for_period(10, 0.01, tiers) == 0.08

    def test_periodo_antes_do_primeiro_tier(self):
        tiers = [{"from_period": 3, "rate": 0.05}]
        assert rate_for_period(1, 0.01, tiers) == 0.01
        assert rate_for_period(2, 0.01, tiers) == 0.01


# --- validate_rate_tiers ---


class TestValidateRateTiers:

    def test_tiers_validos(self):
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 3, "rate": 0.05},
        ]
        validate_rate_tiers(tiers, time=5)  # nao deve dar erro

    def test_lista_vazia_ok(self):
        validate_rate_tiers([], time=5)

    def test_none_ok(self):
        validate_rate_tiers(None, time=5)

    def test_primeiro_tier_tem_de_comecar_em_1(self):
        tiers = [{"from_period": 2, "rate": 0.05}]
        with pytest.raises(ValueError, match="período 1"):
            validate_rate_tiers(tiers, time=5)

    def test_periodos_duplicados(self):
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 1, "rate": 0.05},
        ]
        with pytest.raises(ValueError, match="único"):
            validate_rate_tiers(tiers, time=5)

    def test_periodo_maior_que_total(self):
        tiers = [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 10, "rate": 0.05},
        ]
        with pytest.raises(ValueError, match="superior"):
            validate_rate_tiers(tiers, time=5)
