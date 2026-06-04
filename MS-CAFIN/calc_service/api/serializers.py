from rest_framework import serializers


class InterestRateTierSerializer(serializers.Serializer):
    from_period = serializers.IntegerField(min_value=1, help_text="Período inicial (1 = primeiro ano/mês)")
    rate = serializers.FloatField(min_value=0.0, help_text="Taxa decimal a partir deste período (ex: 0.05)")


class InterestSimulationSerializer(serializers.Serializer):
    INTEREST_CHOICES = (
        ('simple', 'Simple Interest'),
        ('compound', 'Compound Interest'),
    )

    principal = serializers.FloatField(min_value=0.01, help_text="Capital inicial (Principal)")
    rate = serializers.FloatField(
        min_value=0.0,
        required=False,
        help_text="Taxa base por período (usada se não houver faixas ou como fallback)",
    )
    time = serializers.FloatField(min_value=0.1, help_text="Tempo da aplicação/empréstimo")
    type = serializers.ChoiceField(choices=INTEREST_CHOICES, help_text="Tipo de juros (simple ou compound)")
    rate_tiers = InterestRateTierSerializer(many=True, required=False, allow_empty=True, allow_null=True)

    def validate(self, attrs):
        tiers = attrs.get('rate_tiers') or []
        if tiers is None:
            tiers = []
        rate = attrs.get('rate')

        if tiers:
            tiers = sorted(tiers, key=lambda t: t['from_period'])
            if tiers[0]['from_period'] != 1:
                raise serializers.ValidationError({
                    'rate_tiers': 'A primeira faixa deve começar no período 1.'
                })
            attrs['rate_tiers'] = tiers
            if rate is None:
                attrs['rate'] = tiers[0]['rate']
        elif rate is None:
            raise serializers.ValidationError({
                'rate': 'Indique a taxa de juro ou configure faixas variáveis.'
            })

        return attrs


class AmortizationSimulationSerializer(serializers.Serializer):
    AMORTIZATION_CHOICES = (
        ('french', 'French (Price)'),
        ('sac', 'SAC (Constant Amortization)'),
        ('american', 'American'),
    )
    
    principal = serializers.FloatField(min_value=0.01, help_text="Valor financiado (Principal)")
    rate = serializers.FloatField(min_value=0.0, help_text="Taxa de juro por período (ex: 0.05 para 5%)")
    periods = serializers.IntegerField(min_value=1, help_text="Número de períodos/meses")
    type = serializers.ChoiceField(choices=AMORTIZATION_CHOICES, help_text="Tipo de amortização (french, sac ou american)")
