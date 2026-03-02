import logging
from abc import ABC, abstractmethod

from starlette.requests import Request

logger = logging.getLogger(__name__)


class AuthProvider(ABC):
    @abstractmethod
    def authenticate(self, request: Request) -> tuple[str | None, str]:
        """Extract user identity and IC from request.
        Returns (userid, ic). userid is None if not authenticated.
        """
        ...


class SiteMinderProvider(AuthProvider):
    """Extracts user identity from SiteMinder headers.

    Matches legacy Global.asax.cs exactly:
      userid = Request.ServerVariables["HEADER_SM_USER"]
      ic     = Request.ServerVariables["HEADER_USER_SUB_ORG"]  (default "NCI")
    """

    def __init__(self, blacklist: list[str] | None = None):
        self.blacklist = {b.lower() for b in (blacklist or [])}

    def authenticate(self, request: Request) -> tuple[str | None, str]:
        # IIS HEADER_SM_USER maps to HTTP header SM-USER (hyphen).
        # Also accept SM_USER (underscore) for tools like ModHeader.
        user = request.headers.get("SM-USER") or request.headers.get("SM_USER")
        if not user:
            return None, "NCI"

        if user.lower() in self.blacklist:
            return None, "NCI"

        # IIS HEADER_USER_SUB_ORG maps to HTTP header USER-SUB-ORG
        ic = (
            request.headers.get("USER-SUB-ORG")
            or request.headers.get("USER_SUB_ORG")
            or "NCI"
        )

        logger.debug("Authenticated via SM-USER: %s, IC: %s", user, ic)
        return user, ic


class EntraOIDCProvider(AuthProvider):
    """Placeholder for future MS Entra OIDC authentication."""

    def authenticate(self, request: Request) -> tuple[str | None, str]:
        raise NotImplementedError("Entra OIDC authentication is not yet implemented")
