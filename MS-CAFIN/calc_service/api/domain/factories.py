from .strategies import (
    InterestStrategy,
    SimpleInterest,
    CompoundInterest,
    AmortizationStrategy,
    FrenchAmortization,
    SACAmortization,
    AmericanAmortization
)

class InterestFactory:
    @staticmethod
    def create_strategy(strategy_type: str) -> InterestStrategy:
        strategy_type = strategy_type.lower()
        if strategy_type == 'simple':
            return SimpleInterest()
        elif strategy_type == 'compound':
            return CompoundInterest()
        else:
            raise ValueError(f"Unknown interest strategy type: {strategy_type}")


class AmortizationFactory:
    @staticmethod
    def create_strategy(strategy_type: str) -> AmortizationStrategy:
        strategy_type = strategy_type.lower()
        if strategy_type == 'french':
            return FrenchAmortization()
        elif strategy_type == 'sac':
            return SACAmortization()
        elif strategy_type == 'american':
            return AmericanAmortization()
        else:
            raise ValueError(f"Unknown amortization strategy type: {strategy_type}")
