import pytest

# -- Fixtures para juros --


@pytest.fixture
def simple_interest_payload():
    return {
        "principal": 10000.0,
        "rate": 0.05,
        "time": 3.0,
        "type": "simple",
    }


@pytest.fixture
def compound_interest_payload():
    return {
        "principal": 10000.0,
        "rate": 0.05,
        "time": 3.0,
        "type": "compound",
    }


@pytest.fixture
def interest_with_tiers_payload():
    return {
        "principal": 10000.0,
        "rate": 0.03,
        "time": 4.0,
        "type": "compound",
        "rate_tiers": [
            {"from_period": 1, "rate": 0.03},
            {"from_period": 3, "rate": 0.05},
        ],
    }


# -- Fixtures para amortizacao --


@pytest.fixture
def french_amortization_payload():
    return {
        "principal": 100000.0,
        "rate": 0.05,
        "years": 2.0,
        "periodicity": "monthly",
        "commission": 0.0,
        "type": "french",
    }


@pytest.fixture
def sac_amortization_payload():
    return {
        "principal": 100000.0,
        "rate": 0.05,
        "years": 2.0,
        "periodicity": "monthly",
        "commission": 0.0,
        "type": "sac",
    }


@pytest.fixture
def american_amortization_payload():
    return {
        "principal": 100000.0,
        "rate": 0.05,
        "years": 2.0,
        "periodicity": "monthly",
        "commission": 0.0,
        "type": "american",
    }
