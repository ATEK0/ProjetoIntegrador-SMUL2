from rest_framework.exceptions import ValidationError

from api.domain.facades import SimulatorFacade
from api.domain.rate_tiers import normalize_rate_tiers, validate_rate_tiers


class SimulationService:
    @staticmethod
    def run_interest(validated_data: dict) -> dict:
        tiers = normalize_rate_tiers(validated_data.get("rate_tiers"))
        time = validated_data["time"]
        rate = validated_data.get("rate")

        if tiers:
            try:
                validate_rate_tiers(tiers, time)
            except ValueError as e:
                raise ValidationError({"rate_tiers": str(e)}) from e
            if rate is None:
                rate = tiers[0]["rate"]
        elif rate is None:
            raise ValidationError(
                {"rate": "Indique a taxa de juro ou configure faixas variáveis."}
            )

        try:
            return SimulatorFacade.simulate_interest(
                principal=validated_data["principal"],
                rate=rate,
                time=time,
                interest_type=validated_data["type"],
                rate_tiers=tiers or None,
            )
        except ValueError as e:
            raise ValidationError({"detail": str(e)}) from e

    @staticmethod
    def run_amortization(validated_data: dict) -> list:
        try:
            return SimulatorFacade.simulate_amortization(
                principal=validated_data["principal"],
                rate=validated_data["rate"],
                periods=validated_data["periods"],
                amortization_type=validated_data["type"],
            )
        except ValueError as e:
            raise ValidationError({"detail": str(e)}) from e
