from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from rest_framework.exceptions import ValidationError
from drf_spectacular.utils import extend_schema
from .serializers import InterestSimulationSerializer, AmortizationSimulationSerializer
from .services.simulation_service import SimulationService


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
        if not serializer.is_valid():
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        try:
            result = SimulationService.run_interest(serializer.validated_data)
        except ValidationError as e:
            return Response(e.detail, status=status.HTTP_400_BAD_REQUEST)
        return Response(result, status=status.HTTP_200_OK)


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
        if not serializer.is_valid():
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        try:
            result = SimulationService.run_amortization(serializer.validated_data)
        except ValidationError as e:
            return Response(e.detail, status=status.HTTP_400_BAD_REQUEST)
        return Response(result, status=status.HTTP_200_OK)
