"""Tests for the health endpoint."""

from unittest.mock import patch, MagicMock

import pytest
from fastapi.testclient import TestClient


@pytest.fixture
def client():
    """Create a test client with mocked auth and database."""
    with patch("app.auth.middleware.SessionLocal") as mock_session:
        mock_db = MagicMock()
        mock_session.return_value = mock_db
        mock_db.execute.return_value.first.return_value = None

        from app.main import app
        yield TestClient(app)


def test_health_endpoint(client):
    """Health endpoint should return without auth."""
    response = client.get("/api/health")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "ok"
    assert "version" in data
    assert "environment" in data


def test_health_returns_version(client):
    response = client.get("/api/health")
    data = response.json()
    assert data["version"] == "2.0.0"
    assert data["environment"] == "dev"
