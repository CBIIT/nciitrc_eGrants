from app.models import OutgoingEmail


class ConsoleEmailService:
    """Local dev sender for slice testing (no real send)."""

    def send(self, message: OutgoingEmail) -> None:
        print("=== Simulated Email Send ===")
        print(f"Subject: {message.subject}")
        print(f"Recipients: {', '.join(message.recipients)}")
        print(f"Body: {message.body}")
