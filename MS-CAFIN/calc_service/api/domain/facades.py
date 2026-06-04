from .factories import InterestFactory, AmortizationFactory

class SimulatorFacade:
    """
    Facade to simplify interaction with the complex subsystem of simulators
    (factories and strategies).
    """

    @staticmethod
    def simulate_interest(
        principal: float,
        rate: float,
        time: float,
        interest_type: str,
        rate_tiers: list[dict] | None = None,
    ) -> dict:
        """
        Simulates interest based on the provided type ('simple' or 'compound').
        Optional rate_tiers: [{'from_period': 1, 'rate': 0.03}, {'from_period': 3, 'rate': 0.05}]
        """
        strategy = InterestFactory.create_strategy(interest_type)
        tiers = rate_tiers or []
        if tiers:
            return strategy.calculate_with_tiers(principal, rate, time, tiers)
        return strategy.calculate(principal, rate, time)

    @staticmethod
    def simulate_amortization(principal: float, rate: float, periods: int, amortization_type: str) -> list:
        """
        Simulates loan amortization based on the type ('french', 'sac', 'american').
        """
        strategy = AmortizationFactory.create_strategy(amortization_type)
        return strategy.calculate(principal, rate, periods)
