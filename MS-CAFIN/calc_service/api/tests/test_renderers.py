"""
Testes para o ApiResponseRenderer.
"""

import json
import pytest
from rest_framework.response import Response
from rest_framework.test import APIRequestFactory
from api.renderers import ApiResponseRenderer


class TestApiResponseRenderer:

    def setup_method(self):
        self.renderer = ApiResponseRenderer()

    def _render(self, data, status_code):
        """Helper para renderizar com um status code."""
        response = Response(data, status=status_code)
        response.accepted_renderer = self.renderer
        response.accepted_media_type = "application/json"
        response.renderer_context = {"response": response}
        rendered = self.renderer.render(
            data, accepted_media_type="application/json",
            renderer_context={"response": response},
        )
        return json.loads(rendered)

    def test_sucesso_200(self):
        result = self._render({"interest": 1500.0}, 200)
        assert result["success"] is True
        assert result["data"] == {"interest": 1500.0}
        assert result["errors"] is None

    def test_sucesso_201(self):
        result = self._render({"created": True}, 201)
        assert result["success"] is True

    def test_erro_400(self):
        result = self._render({"principal": ["This field is required."]}, 400)
        assert result["success"] is False
        assert result["data"] is None
        assert result["errors"] is not None

    def test_erro_404(self):
        result = self._render({"detail": "Not found."}, 404)
        assert result["success"] is False
        assert result["data"] is None

    def test_erro_500(self):
        result = self._render({"detail": "Internal Server Error"}, 500)
        assert result["success"] is False

    def test_lista_erros_mantem_se(self):
        errors = [{"field": "rate", "message": "Required"}]
        result = self._render(errors, 400)
        assert result["success"] is False
        assert result["errors"] == errors

    def test_dict_erros_vira_lista(self):
        errors = {"principal": ["This field is required."]}
        result = self._render(errors, 400)
        assert result["success"] is False
        assert isinstance(result["errors"], list)

    def test_resposta_ja_formatada_passa(self):
        # se ja tem success/errors, nao deve re-embrulhar
        data = {"success": True, "data": {"test": 123}, "errors": None}
        result = self._render(data, 200)
        assert result == data

    def test_erro_ja_formatado_passa(self):
        data = {"success": False, "errors": ["Something went wrong"]}
        result = self._render(data, 400)
        assert result == data

    def test_com_lista(self):
        result = self._render([1, 2, 3], 200)
        assert result["success"] is True
        assert result["data"] == [1, 2, 3]

    def test_dict_vazio(self):
        result = self._render({}, 200)
        assert result["success"] is True
        assert result["data"] == {}
