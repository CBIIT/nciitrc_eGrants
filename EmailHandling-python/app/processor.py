from app.config import RouterConfig
from app.models import IncomingEmail, RouteResult
from app.routing_rules import GrantLookup, route_email


class Processor:
    """Route an incoming email without side effects; IO is handled by adapters."""

    def __init__(self, config: RouterConfig, lookup: GrantLookup | None = None) -> None:
        self._config = config
        self._lookup = lookup

    def handle_single_email(self, email: IncomingEmail, debug: str = "y") -> RouteResult:
        return route_email(email=email, config=self._config, debug=debug, lookup=self._lookup)
