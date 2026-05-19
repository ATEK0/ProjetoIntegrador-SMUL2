from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from drf_spectacular.utils import extend_schema


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
