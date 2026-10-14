import argparse
import logging
from dataclasses import replace
from pathlib import Path

from app.config import SOURCE_ROUTER_CONFIG, load_config
from app.email_service import ConsoleEmailService
from app.models import IncomingEmail
from app.processor import Processor


def main() -> int:
    parser = argparse.ArgumentParser(description="EmailHandling Python router")
    parser.add_argument("--subject", help="Incoming email subject for offline preview")
    parser.add_argument("--body", default="", help="Incoming email body")
    parser.add_argument("--sender", default="", help="Incoming email sender")
    parser.add_argument("--debug", choices=["y", "n"], help="Override debug mode (y/n)")
    parser.add_argument("--live", action="store_true", help="Connect to Graph and SQL Server")
    parser.add_argument(
        "--graph-check",
        action="store_true",
        help="Read-only Graph authentication, folder and message check (no SQL)",
    )
    parser.add_argument("--send", action="store_true", help="With --live, forward and archive mail")
    parser.add_argument(
        "--test-recipient", help="Force all test forwards and notifications to one address"
    )
    parser.add_argument(
        "--config-dir", type=Path, help="Directory containing upgraded Router appsettings files"
    )
    args = parser.parse_args()

    if args.send and not args.live:
        parser.error("--send requires --live")
    if args.graph_check and args.send:
        parser.error("--graph-check cannot send or archive mail")
    if not args.live and not args.graph_check and not args.subject:
        parser.error("--subject is required for offline preview")
    if args.test_recipient and (
        args.test_recipient.count("@") != 1
        or any(character in args.test_recipient for character in ";,\r\n ")
    ):
        parser.error("--test-recipient must be a single email address")
    config = load_config(
        args.config_dir or (SOURCE_ROUTER_CONFIG if args.live or args.graph_check else None),
        resolve_connection=not args.graph_check,
    )
    if args.test_recipient:
        config = replace(
            config,
            test_recipient=args.test_recipient,
            debug_email=args.test_recipient,
            egrants_dev_email=args.test_recipient,
            error_recipients=args.test_recipient,
            legacy_error_recipient=args.test_recipient,
        )
    debug = "y" if args.test_recipient else args.debug or config.debug
    if args.graph_check:
        from app.graph import GraphEmailService

        try:
            service = GraphEmailService(config)
            service.connect()
            folder = service.folder(config.folder_path)
            service.subfolder(folder, "Old emails")
            count = len(service.emails(folder))
        except Exception as error:
            print(f"Graph check failed ({type(error).__name__}). No mail sent or moved.")
            return 1
        print(f"Graph folder check succeeded: {count} message(s). No mail sent or moved.")
        return 0
    if args.live:
        from app.database import OdbcDriverError, SqlConnectionError, SqlGrantLookup
        from app.graph import GraphEmailService, process_graph_folder
        from app.notifications import send_global_error

        logging.basicConfig(level=logging.INFO, format="%(levelname)s %(message)s")
        try:
            with SqlGrantLookup(config.connection_string) as lookup:
                count = process_graph_folder(
                    GraphEmailService(config),
                    Processor(config, lookup),
                    config,
                    debug,
                    dry_run=not args.send,
                )
        except Exception as error:
            # Graph/SQL exceptions can include response bodies or connection data.
            if isinstance(error, (OdbcDriverError, SqlConnectionError)):
                logging.error("Router failed: %s", error)
            else:
                logging.error("Router failed (%s)", type(error).__name__)
            if args.send:
                try:
                    send_global_error(config, f"Router failed: {type(error).__name__}")
                except Exception:
                    logging.error("Global SMTP error notification also failed")
            return 1
        print(f"Inspected {count} mail items. Sending {'enabled' if args.send else 'disabled'}.")
        return 0
    processor = Processor(config)
    incoming = IncomingEmail(subject=args.subject, body=args.body, sender=args.sender)

    result = processor.handle_single_email(incoming, debug=debug)

    if not result.matched:
        print("No routing rule matched.")
        return 0

    sender = ConsoleEmailService()
    for message in result.outgoing_messages:
        sender.send(message)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
