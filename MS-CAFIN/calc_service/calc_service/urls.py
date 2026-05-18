from django.urls import path
from drf_spectacular.views import SpectacularAPIView, SpectacularSwaggerView
from api.views import HealthCheckView

urlpatterns = [

    path('api/v1/health/', HealthCheckView.as_view(), name='health_check'),

    # OpenAPI 3 / Swagger Documentation Endpoints
    path('api/schema/', SpectacularAPIView.as_view(), name='schema'),
    path('api/docs/', SpectacularSwaggerView.as_view(url_name='schema'), name='swagger-ui'),
]