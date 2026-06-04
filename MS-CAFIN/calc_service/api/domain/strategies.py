from abc import ABC, abstractmethod
from .rate_tiers import normalize_rate_tiers, rate_for_period, validate_rate_tiers


class InterestStrategy(ABC):
    @abstractmethod
    def calculate(self, principal: float, rate: float, time: float) -> dict:
        """
        Calculates interest and total amount.
        Returns a dict: {'interest': float, 'total_amount': float, 'breakdown': list|None}
        """
        pass

    def calculate_with_tiers(
        self,
        principal: float,
        default_rate: float,
        time: float,
        tiers: list[dict],
    ) -> dict:
        tiers = normalize_rate_tiers(tiers)
        validate_rate_tiers(tiers, time)

        whole_periods = int(time)
        fractional = round(time - whole_periods, 6)
        breakdown = []
        total_interest = 0.0
        amount = principal

        for period in range(1, whole_periods + 1):
            r = rate_for_period(period, default_rate, tiers)
            period_interest = self._period_interest(amount, principal, r)
            total_interest += period_interest
            amount = self._amount_after_period(amount, principal, r, period_interest)
            breakdown.append(
                {
                    "period": period,
                    "rate": round(r, 6),
                    "interest": round(period_interest, 2),
                    "amount": round(amount, 2),
                }
            )

        if fractional > 0:
            r = rate_for_period(whole_periods + 1, default_rate, tiers)
            period_interest = self._period_interest_fractional(
                amount, principal, r, fractional
            )
            total_interest += period_interest
            amount = self._amount_after_period(amount, principal, r, period_interest)
            breakdown.append(
                {
                    "period": whole_periods + 1,
                    "rate": round(r, 6),
                    "interest": round(period_interest, 2),
                    "amount": round(amount, 2),
                    "partial": True,
                }
            )

        return {
            "interest": round(total_interest, 2),
            "total_amount": round(amount, 2),
            "breakdown": breakdown,
        }

    def _period_interest(self, amount: float, principal: float, rate: float) -> float:
        raise NotImplementedError

    def _period_interest_fractional(
        self, amount: float, principal: float, rate: float, fraction: float
    ) -> float:
        return self._period_interest(amount, principal, rate) * fraction

    def _amount_after_period(
        self, amount: float, principal: float, rate: float, period_interest: float
    ) -> float:
        raise NotImplementedError


class SimpleInterest(InterestStrategy):
    def calculate(self, principal: float, rate: float, time: float) -> dict:
        interest = principal * rate * time
        total = principal + interest
        return {
            "interest": round(interest, 2),
            "total_amount": round(total, 2),
            "breakdown": None,
        }

    def _period_interest(self, amount: float, principal: float, rate: float) -> float:
        return principal * rate

    def _amount_after_period(
        self, amount: float, principal: float, rate: float, period_interest: float
    ) -> float:
        return amount + period_interest


class CompoundInterest(InterestStrategy):
    def calculate(self, principal: float, rate: float, time: float) -> dict:
        total = principal * ((1 + rate) ** time)
        interest = total - principal
        return {
            "interest": round(interest, 2),
            "total_amount": round(total, 2),
            "breakdown": None,
        }

    def _period_interest(self, amount: float, principal: float, rate: float) -> float:
        return amount * rate

    def _amount_after_period(
        self, amount: float, principal: float, rate: float, period_interest: float
    ) -> float:
        return amount + period_interest


class AmortizationStrategy(ABC):
    @abstractmethod
    def calculate(self, principal: float, rate: float, periods: int) -> list:
        """
        Calculates the amortization schedule.
        Returns a list of dicts:
        [{'period': int, 'installment': float, 'interest': float, 'amortization': float, 'balance': float}]
        """
        pass


class FrenchAmortization(AmortizationStrategy):
    def calculate(self, principal: float, rate: float, periods: int) -> list:
        schedule = []
        balance = principal

        # Handling edge case if rate is 0
        if rate == 0:
            installment = principal / periods
        else:
            installment = (
                principal
                * (rate * ((1 + rate) ** periods))
                / (((1 + rate) ** periods) - 1)
            )

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "balance": round(balance, 2),
            }
        )

        for i in range(1, periods + 1):
            interest = balance * rate
            amortization = installment - interest
            balance -= amortization

            # Avoid floating point issues at the end
            if i == periods:
                balance = 0.0

            schedule.append(
                {
                    "period": i,
                    "installment": round(installment, 2),
                    "interest": round(interest, 2),
                    "amortization": round(amortization, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule


class SACAmortization(AmortizationStrategy):
    def calculate(self, principal: float, rate: float, periods: int) -> list:
        schedule = []
        balance = principal
        amortization = principal / periods

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "balance": round(balance, 2),
            }
        )

        for i in range(1, periods + 1):
            interest = balance * rate
            installment = amortization + interest
            balance -= amortization

            if i == periods:
                balance = 0.0

            schedule.append(
                {
                    "period": i,
                    "installment": round(installment, 2),
                    "interest": round(interest, 2),
                    "amortization": round(amortization, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule


class AmericanAmortization(AmortizationStrategy):
    def calculate(self, principal: float, rate: float, periods: int) -> list:
        schedule = []
        balance = principal

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "balance": round(balance, 2),
            }
        )

        for i in range(1, periods + 1):
            interest = balance * rate

            if i == periods:
                amortization = principal
            else:
                amortization = 0.0

            installment = interest + amortization
            balance -= amortization

            schedule.append(
                {
                    "period": i,
                    "installment": round(installment, 2),
                    "interest": round(interest, 2),
                    "amortization": round(amortization, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule
