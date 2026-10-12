import json
from pathlib import Path

from app.config import RouterConfig
from app.models import IncomingEmail
from app.processor import Processor

FIXTURE = Path(__file__).parent / "fixtures" / "prior_approval_email.json"


def _load_fixture() -> IncomingEmail:
    data = json.loads(FIXTURE.read_text(encoding="utf-8"))
    return IncomingEmail(subject=data["subject"], body=data["body"], sender=data.get("sender", ""))


def test_prior_approval_routes_to_post_award_in_non_debug_mode() -> None:
    config = RouterConfig(
        nci_grants_post_award_email="NCIGrantsPostAward@nih.gov",
        debug_email="debug@mail.nih.gov",
        egrants_dev_email="eGrantsDev@mail.nih.gov",
    )
    processor = Processor(config)
    email = _load_fixture()

    result = processor.handle_single_email(email, debug="n")

    assert result.matched is True
    assert len(result.outgoing_messages) == 1
    assert result.outgoing_messages[0].recipients == ["NCIGrantsPostAward@nih.gov"]


def test_prior_approval_routes_to_debug_and_dev_in_debug_mode() -> None:
    config = RouterConfig(
        nci_grants_post_award_email="NCIGrantsPostAward@nih.gov",
        debug_email="debug@mail.nih.gov",
        egrants_dev_email="eGrantsDev@mail.nih.gov",
    )
    processor = Processor(config)
    email = _load_fixture()

    result = processor.handle_single_email(email, debug="y")

    assert result.matched is True
    assert len(result.outgoing_messages) == 1
    assert result.outgoing_messages[0].recipients == [
        "debug@mail.nih.gov",
        "eGrantsDev@mail.nih.gov",
    ]


def test_non_matching_subject_does_not_route() -> None:
    config = RouterConfig()
    processor = Processor(config)
    email = IncomingEmail(subject="Other Subject", body="hello")

    result = processor.handle_single_email(email, debug="n")

    assert result.matched is False
    assert result.outgoing_messages == []
