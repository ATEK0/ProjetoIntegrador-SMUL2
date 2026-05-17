from rest_framework.renderers import JSONRenderer

class ApiResponseRenderer(JSONRenderer):
    """
    Padroniza todas as respostas da API de forma limpa para consumo pelo Portal C#:
    {
        "success": true/false,
        "data": {...} ou [...],
        "errors": [...] ou null
    }
    """
    def render(self, data, accepted_media_type=None, renderer_context=None):
        status_code = renderer_context['response'].status_code
        success = 200 <= status_code < 300

        if isinstance(data, dict) and ('success' in data or 'errors' in data):
            response_data = data
        else:
            if success:
                response_data = {
                    "success": True,
                    "data": data,
                    "errors": None
                }
            else:
                errors_list = data if isinstance(data, list) else [data]
                response_data = {
                    "success": False,
                    "data": None,
                    "errors": errors_list
                }

        return super().render(response_data, accepted_media_type, renderer_context)