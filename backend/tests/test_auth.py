"""Tests for authentication middleware and providers."""

import base64

import pytest
from starlette.testclient import TestClient
from unittest.mock import patch, MagicMock

from app.auth.providers import SiteMinderProvider, EntraOIDCProvider


class TestSiteMinderProvider:
    def setup_method(self):
        self.provider = SiteMinderProvider()

    def _make_request(self, headers: dict):
        mock = MagicMock()
        mock.headers = headers
        return mock

    def test_sm_user_header(self):
        request = self._make_request({"SM_USER": "testuser"})
        assert self.provider.authenticate(request) == "testuser"

    def test_auth_user_header(self):
        request = self._make_request({"AUTH_USER": "testuser2"})
        assert self.provider.authenticate(request) == "testuser2"

    def test_remote_user_header(self):
        request = self._make_request({"Remote-User": "testuser3"})
        assert self.provider.authenticate(request) == "testuser3"

    def test_basic_auth_header(self):
        credentials = base64.b64encode(b"testuser4:password").decode()
        request = self._make_request({"Authorization": f"Basic {credentials}"})
        assert self.provider.authenticate(request) == "testuser4"

    def test_no_auth_header(self):
        request = self._make_request({})
        assert self.provider.authenticate(request) is None

    def test_priority_order(self):
        """SM_USER takes priority over other headers."""
        request = self._make_request({
            "SM_USER": "sm_user",
            "AUTH_USER": "auth_user",
            "Remote-User": "remote_user",
        })
        assert self.provider.authenticate(request) == "sm_user"

    def test_blacklist(self):
        provider = SiteMinderProvider(blacklist=["nciservermon", "blocked"])
        request = self._make_request({"SM_USER": "nciservermon"})
        assert provider.authenticate(request) is None

    def test_blacklist_case_insensitive(self):
        provider = SiteMinderProvider(blacklist=["NciServerMon"])
        request = self._make_request({"SM_USER": "nciservermon"})
        assert provider.authenticate(request) is None

    def test_blacklist_allows_valid_user(self):
        provider = SiteMinderProvider(blacklist=["blocked"])
        request = self._make_request({"SM_USER": "validuser"})
        assert provider.authenticate(request) == "validuser"

    def test_invalid_basic_auth(self):
        request = self._make_request({"Authorization": "Basic !!invalid!!"})
        assert self.provider.authenticate(request) is None


class TestEntraOIDCProvider:
    def test_not_implemented(self):
        provider = EntraOIDCProvider()
        request = MagicMock()
        with pytest.raises(NotImplementedError):
            provider.authenticate(request)
