"""
Testes de integracao para os endpoints da API.
Usa o APIClient do DRF para testar os requests HTTP.
"""

import pytest
from rest_framework.test import APIClient


@pytest.fixture
def client():
    return APIClient()


# --- Health Check ---

@pytest.mark.django_db
class TestHealthCheck:

    def test_retorna_200(self, client):
        resp = client.get("/api/v1/health/")
        assert resp.status_code == 200

    def test_info_do_servico(self, client):
        resp = client.get("/api/v1/health/")
        data = resp.json()
        assert data["service"] == "MS-CAFIN"
        assert data["status"] == "online"
        assert "version" in data


# --- Endpoint de Juros ---

@pytest.mark.django_db
class TestInterestEndpoint:

    def test_juros_simples_200(self, client, simple_interest_payload):
        resp = client.post("/api/v1/simulate/interest/", data=simple_interest_payload, format="json")
        assert resp.status_code == 200
        data = resp.json()
        assert "interest" in data
        assert "total_amount" in data

    def test_calculo_correto(self, client):
        # 10000 * 0.1 * 5 = 5000
        payload = {"principal": 10000, "rate": 0.1, "time": 5, "type": "simple"}
        resp = client.post("/api/v1/simulate/interest/", data=payload, format="json")
        assert resp.status_code == 200
        data = resp.json()
        assert data["interest"] == 5000.0
        assert data["total_amount"] == 15000.0

    def test_juros_compostos_200(self, client, compound_interest_payload):
        resp = client.post("/api/v1/simulate/interest/", data=compound_interest_payload, format="json")
        assert resp.status_code == 200
        assert resp.json()["interest"] > 0

    def test_com_tiers_200(self, client, interest_with_tiers_payload):
        resp = client.post("/api/v1/simulate/interest/", data=interest_with_tiers_payload, format="json")
        assert resp.status_code == 200
        assert resp.json()["breakdown"] is not None

    def test_campos_em_falta_400(self, client):
        resp = client.post("/api/v1/simulate/interest/", data={}, format="json")
        assert resp.status_code == 400

    def test_type_invalido_400(self, client):
        payload = {"principal": 10000, "rate": 0.05, "time": 3, "type": "exponential"}
        resp = client.post("/api/v1/simulate/interest/", data=payload, format="json")
        assert resp.status_code == 400

    def test_sem_rate_sem_tiers_400(self, client):
        payload = {"principal": 10000, "time": 3, "type": "simple"}
        resp = client.post("/api/v1/simulate/interest/", data=payload, format="json")
        assert resp.status_code == 400

    def test_get_nao_permitido(self, client):
        resp = client.get("/api/v1/simulate/interest/")
        assert resp.status_code == 405

    def test_principal_negativo_400(self, client):
        payload = {"principal": -1000, "rate": 0.05, "time": 3, "type": "simple"}
        resp = client.post("/api/v1/simulate/interest/", data=payload, format="json")
        assert resp.status_code == 400


# --- Endpoint de Amortizacao ---

@pytest.mark.django_db
class TestAmortizationEndpoint:

    def test_french_200(self, client, french_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=french_amortization_payload, format="json")
        assert resp.status_code == 200
        data = resp.json()
        assert "schedule" in data
        assert "taeg" in data
        assert "is_inicial" in data

    def test_sac_200(self, client, sac_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=sac_amortization_payload, format="json")
        assert resp.status_code == 200

    def test_american_200(self, client, american_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=american_amortization_payload, format="json")
        assert resp.status_code == 200

    def test_schedule_nao_vazio(self, client, french_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=french_amortization_payload, format="json")
        assert len(resp.json()["schedule"]) > 0

    def test_balance_final_zero(self, client, french_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=french_amortization_payload, format="json")
        assert resp.json()["schedule"][-1]["balance"] == 0.0

    def test_campos_em_falta_400(self, client):
        resp = client.post("/api/v1/simulate/amortization/", data={}, format="json")
        assert resp.status_code == 400

    def test_type_invalido_400(self, client):
        payload = {
            "principal": 100000, "rate": 0.05, "years": 5,
            "periodicity": "monthly", "commission": 0.0, "type": "linear",
        }
        resp = client.post("/api/v1/simulate/amortization/", data=payload, format="json")
        assert resp.status_code == 400

    def test_get_nao_permitido(self, client):
        resp = client.get("/api/v1/simulate/amortization/")
        assert resp.status_code == 405

    def test_taeg_positiva(self, client, french_amortization_payload):
        resp = client.post("/api/v1/simulate/amortization/", data=french_amortization_payload, format="json")
        assert resp.json()["taeg"] > 0

    def test_com_comissao(self, client):
        payload = {
            "principal": 100000, "rate": 0.05, "years": 1,
            "periodicity": "monthly", "commission": 25.0, "type": "french",
        }
        resp = client.post("/api/v1/simulate/amortization/", data=payload, format="json")
        assert resp.status_code == 200
        for row in resp.json()["schedule"]:
            if row["period"] > 0:
                assert row["comissao"] == 25.0
