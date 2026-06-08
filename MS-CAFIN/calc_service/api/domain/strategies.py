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


def calculate_irr(
    cash_flows: list[float], guess=0.1, max_iter=1000, tol=1e-6
) -> float | None:
    rate = guess
    for _ in range(max_iter):
        npv = sum(cf / (1 + rate) ** t for t, cf in enumerate(cash_flows))
        d_npv = sum(-t * cf / (1 + rate) ** (t + 1) for t, cf in enumerate(cash_flows))
        if abs(d_npv) < 1e-12:
            return None
        new_rate = rate - npv / d_npv
        if abs(new_rate - rate) < tol:
            return new_rate
        rate = new_rate
    return None


class AmortizationStrategy(ABC):
    def calculate(
        self,
        principal: float,
        tan: float,
        years: float,
        periodicity: str,
        commission: float,
    ) -> dict:
        periods_per_year = {
            "monthly": 12,
            "quarterly": 4,
            "semiannual": 2,
            "annual": 1,
        }.get(periodicity, 12)
        n = int(years * periods_per_year)
        i = tan / periods_per_year

        if years < 1:
            taxa_is_inicial = 0.0004 * (years * 12)
        elif years < 5:
            taxa_is_inicial = 0.0050
        else:
            taxa_is_inicial = 0.0060

        is_inicial = principal * taxa_is_inicial
        capital_liquido = principal - is_inicial

        schedule = self._generate_schedule(principal, n, i, commission)

        cash_flows = [capital_liquido]
        for row in schedule:
            if row["period"] > 0:
                cash_flows.append(-row["total_pago"])

        taxa_periodica_taeg = calculate_irr(cash_flows)
        if taxa_periodica_taeg is not None:
            taeg = ((1 + taxa_periodica_taeg) ** periods_per_year - 1) * 100
        else:
            taeg = 0.0

        return {
            "schedule": schedule,
            "taeg": round(taeg, 4),
            "is_inicial": round(is_inicial, 2),
        }

    @abstractmethod
    def _generate_schedule(
        self, principal: float, n: int, i: float, commission: float
    ) -> list:
        pass


class FrenchAmortization(AmortizationStrategy):
    def _generate_schedule(
        self, principal: float, n: int, i: float, commission: float
    ) -> list:
        schedule = []
        balance = principal

        if i == 0:
            prestacao_base = principal / n
        else:
            prestacao_base = principal * (i * (1 + i) ** n) / ((1 + i) ** n - 1)

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "imposto_selo": 0.0,
                "comissao": 0.0,
                "total_pago": 0.0,
                "balance": round(balance, 2),
            }
        )

        for t in range(1, n + 1):
            juros = balance * i
            amortizacao = prestacao_base - juros
            balance -= amortizacao

            if t == n:
                balance = 0.0

            is_sobre_juros = juros * 0.04
            total_pago = prestacao_base + is_sobre_juros + commission

            schedule.append(
                {
                    "period": t,
                    "installment": round(prestacao_base, 2),
                    "interest": round(juros, 2),
                    "amortization": round(amortizacao, 2),
                    "imposto_selo": round(is_sobre_juros, 2),
                    "comissao": round(commission, 2),
                    "total_pago": round(total_pago, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule


class SACAmortization(AmortizationStrategy):
    def _generate_schedule(
        self, principal: float, n: int, i: float, commission: float
    ) -> list:
        schedule = []
        balance = principal
        amortizacao = principal / n

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "imposto_selo": 0.0,
                "comissao": 0.0,
                "total_pago": 0.0,
                "balance": round(balance, 2),
            }
        )

        for t in range(1, n + 1):
            juros = balance * i
            prestacao_base = amortizacao + juros
            balance -= amortizacao

            if t == n:
                balance = 0.0

            is_sobre_juros = juros * 0.04
            total_pago = prestacao_base + is_sobre_juros + commission

            schedule.append(
                {
                    "period": t,
                    "installment": round(prestacao_base, 2),
                    "interest": round(juros, 2),
                    "amortization": round(amortizacao, 2),
                    "imposto_selo": round(is_sobre_juros, 2),
                    "comissao": round(commission, 2),
                    "total_pago": round(total_pago, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule


class AmericanAmortization(AmortizationStrategy):
    def _generate_schedule(
        self, principal: float, n: int, i: float, commission: float
    ) -> list:
        schedule = []
        balance = principal

        schedule.append(
            {
                "period": 0,
                "installment": 0.0,
                "interest": 0.0,
                "amortization": 0.0,
                "imposto_selo": 0.0,
                "comissao": 0.0,
                "total_pago": 0.0,
                "balance": round(balance, 2),
            }
        )

        for t in range(1, n + 1):
            juros = balance * i
            if t == n:
                amortizacao = principal
            else:
                amortizacao = 0.0

            prestacao_base = juros + amortizacao
            balance -= amortizacao

            is_sobre_juros = juros * 0.04
            total_pago = prestacao_base + is_sobre_juros + commission

            schedule.append(
                {
                    "period": t,
                    "installment": round(prestacao_base, 2),
                    "interest": round(juros, 2),
                    "amortization": round(amortizacao, 2),
                    "imposto_selo": round(is_sobre_juros, 2),
                    "comissao": round(commission, 2),
                    "total_pago": round(total_pago, 2),
                    "balance": round(balance, 2),
                }
            )

        return schedule
