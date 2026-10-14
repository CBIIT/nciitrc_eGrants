from dataclasses import dataclass, field
from typing import List


@dataclass(slots=True)
class IncomingEmail:
    subject: str
    body: str
    sender: str = ""


@dataclass(slots=True)
class OutgoingEmail:
    subject: str
    body: str
    recipients: List[str] = field(default_factory=list)
    # Source explicitly resets the Change of Institution forward to its original subject.
    preserve_subject: bool = False


@dataclass(slots=True)
class RouteResult:
    matched: bool
    route_name: str
    outgoing_messages: List[OutgoingEmail] = field(default_factory=list)
