"""
Testes para as factories de estrategias.
"""

import pytest
from api.domain.factories import InterestFactory, AmortizationFactory
from api.domain.strategies import (
    SimpleInterest,
    CompoundInterest,
    FrenchAmortization,
    SACAmortization,
    AmericanAmortization,
)


class TestInterestFactory:

    def test_criar_simples(self):
        strategy = InterestFactory.create_strategy("simple")
        assert isinstance(strategy, SimpleInterest)

    def test_criar_composto(self):
        strategy = InterestFactory.create_strategy("compound")
        assert isinstance(strategy, CompoundInterest)

    def test_case_insensitive(self):
        assert isinstance(InterestFactory.create_strategy("Simple"), SimpleInterest)
        assert isinstance(InterestFactory.create_strategy("COMPOUND"), CompoundInterest)

    def test_tipo_invalido(self):
        with pytest.raises(ValueError, match="Unknown interest strategy"):
            InterestFactory.create_strategy("exponential")


class TestAmortizationFactory:

    def test_criar_french(self):
        strategy = AmortizationFactory.create_strategy("french")
        assert isinstance(strategy, FrenchAmortization)

    def test_criar_sac(self):
        strategy = AmortizationFactory.create_strategy("sac")
        assert isinstance(strategy, SACAmortization)

    def test_criar_american(self):
        strategy = AmortizationFactory.create_strategy("american")
        assert isinstance(strategy, AmericanAmortization)

    def test_case_insensitive(self):
        assert isinstance(
            AmortizationFactory.create_strategy("French"), FrenchAmortization
        )
        assert isinstance(AmortizationFactory.create_strategy("SAC"), SACAmortization)
        assert isinstance(
            AmortizationFactory.create_strategy("AMERICAN"), AmericanAmortization
        )

    def test_tipo_invalido(self):
        with pytest.raises(ValueError, match="Unknown amortization strategy"):
            AmortizationFactory.create_strategy("linear")
