import argparse
import logging
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
    parser.add_argument("--send", action="store_true", help="With --live, forward and archive mail")
    parser.add_argument(
        "--config-dir", type=Path, help="Directory containing upgraded Router appsettings files"
    )
    args = parser.parse_args()

    if args.send and not args.live:
        parser.error("--send requires --live")
    if not args.live and not args.subject:
        parser.error("--subject is required for offline preview")
    config = load_config(args.config_dir or (SOURCE_ROUTER_CONFIG if args.live else None))
    debug = args.debug or config.debug
    if args.live:
        from app.database import SqlGrantLookup
        from app.graph import GraphEmailService, process_graph_folder
        from app.notifications import send_global_error

        logging.basicConfig(level=logging.INFO, format="%(levelname)s %(message)s")
        try:
            with SqlGrantLookup(config.connection_string) as lookup:
                count = process_graph_folder(
                    GraphEmailService(config), Processor(config, lookup), config, debug,
                    dry_run=not args.send,
                )
        except Exception as error:
            logging.exception("Router failed")
            if args.send:
                try:
                    send_global_error(config, f"Router failed: {type(error).__name__}")
                except Exception:
                    logging.exception("Global SMTP error notification also failed")
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
