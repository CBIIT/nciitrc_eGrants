"""Read-only Graph batch planning for ExchangeFixed and AddSuppProd.

No document registrations, file writes, forwarding, sends or moves are exposed.
"""

from collections.abc import Mapping
from dataclasses import dataclass
from typing import Literal, Protocol, cast

from app.add_supp_prod import SupplementLookup, SupplementPlan, plan_supplement
from app.exchange_fixed import ApplicationLookup, DocumentPlan, plan_documents
from app.graph import GraphAttachment, GraphFolder, GraphMail


class GraphReader(Protocol):
    def connect(self) -> None: ...

    def folder(self, name: str) -> GraphFolder: ...

    def emails(self, folder: GraphFolder) -> list[GraphMail]: ...

    def attachments(self, mail: GraphMail) -> list[GraphAttachment]: ...


@dataclass(frozen=True)
class PlannedMessage:
    message_id: str
    actions: tuple[DocumentPlan, ...] | SupplementPlan


def preview_project(
    service: GraphReader,
    project: Literal["exchange-fixed", "add-supp-prod"],
    graph_folder: str,
    lookup: ApplicationLookup | SupplementLookup,
    *,
    trusted_aliases: Mapping[str, str] | None = None,
    max_items: int = 30,
) -> list[PlannedMessage]:
    """Read a *configured Graph mailbox folder*; never infer Outlook public folders.

    Previews must use a separately validated test mailbox. `GraphEmailService`
    is injected so synthetic tests can prove no write methods are called.
    """
    if project not in {"exchange-fixed", "add-supp-prod"}:
        raise ValueError("Unsupported project")
    if not graph_folder.strip() or "public folders -" in graph_folder.casefold():
        raise ValueError(
            "Supply a reviewed Graph mailbox folder, not an Outlook public-folder path"
        )
    if not 1 <= max_items <= 30:
        raise ValueError("max_items must be between 1 and 30")
    service.connect()
    folder = service.folder(graph_folder)
    plans: list[PlannedMessage] = []
    for mail in service.emails(folder)[:max_items]:
        files = service.attachments(mail)
        actions: tuple[DocumentPlan, ...] | SupplementPlan
        if project == "exchange-fixed":
            actions = tuple(plan_documents(mail, files, lookup))
        else:
            actions = plan_supplement(
                mail, files, cast(SupplementLookup, lookup), trusted_aliases=trusted_aliases
            )
        plans.append(PlannedMessage(mail.id, actions))
    return plans
