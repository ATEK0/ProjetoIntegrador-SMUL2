from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from drf_spectacular.utils import extend_schema
from .serializers import InterestSimulationSerializer, AmortizationSimulationSerializer
from .domain.facades import SimulatorFacade


class HealthCheckView(APIView):
    """
    Endpoint de verificação de estado (Health-Check) do microserviço financeiro.
    """

    @extend_schema(
        summary="Health Check do Microserviço",
        description="Retorna o status atual do serviço Python/Django para monitorização.",
        responses={200: dict},
    )
    def get(self, request):
        data = {"service": "MS-CAFIN", "status": "online", "version": "1.0.0"}
        return Response(data, status=status.HTTP_200_OK)


class InterestSimulationView(APIView):
    """
    Endpoint para simulação de Juros Simples e Compostos.
    """

    @extend_schema(
        summary="Simulação de Juros",
        description="Calcula juros simples ou compostos e retorna o valor de juros e o montante total.",
        request=InterestSimulationSerializer,
        responses={200: dict, 400: dict},
    )
    def post(self, request):
        serializer = InterestSimulationSerializer(data=request.data)
        if serializer.is_valid():
            try:
                result = SimulatorFacade.simulate_interest(
                    principal=serializer.validated_data['principal'],
                    rate=serializer.validated_data['rate'],
                    time=serializer.validated_data['time'],
                    interest_type=serializer.validated_data['type'],
                    rate_tiers=serializer.validated_data.get('rate_tiers'),
                )
                return Response(result, status=status.HTTP_200_OK)
            except ValueError as e:
                return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class AmortizationSimulationView(APIView):
    """
    Endpoint para simulação de quadros de amortização (Francês, SAC, Americano).
    """

    @extend_schema(
        summary="Simulação de Amortização de Empréstimo",
        description="Gera o quadro de amortização baseado no regime escolhido (French, SAC, American).",
        request=AmortizationSimulationSerializer,
        responses={200: list, 400: dict},
    )
    def post(self, request):
        serializer = AmortizationSimulationSerializer(data=request.data)
        if serializer.is_valid():
            try:
                result = SimulatorFacade.simulate_amortization(
                    principal=serializer.validated_data['principal'],
                    rate=serializer.validated_data['rate'],
                    periods=serializer.validated_data['periods'],
                    amortization_type=serializer.validated_data['type'],
                )
                return Response(result, status=status.HTTP_200_OK)
            except ValueError as e:
                return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
