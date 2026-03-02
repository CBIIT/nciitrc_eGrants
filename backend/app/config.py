from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    database_odbc: str = (
        "DRIVER={ODBC Driver 18 for SQL Server};"
        "SERVER=localhost;"
        "DATABASE=EIM;"
        "UID=egrantsuser;"
        "PWD=password;"
        "TrustServerCertificate=yes;"
    )
    environment: str = "dev"
    app_version: str = "2.0.0"
    app_build: str = ""
    cors_origins: list[str] = ["http://localhost:3000"]
    image_server_url: str = "https://egrants-dev.nci.nih.gov/docs/egrants"
    web_grant_url: str = "/local/content/web/egrants/wwwroot/docs/egrants"
    smtp_host: str = "mailfwd.nih.gov"
    smtp_from: str = "egrants-noreply@mail.nih.gov"
    send_mail: bool = False
    mail_override_address: str = ""
    sm_user_blacklist: str = "nciservermon,ncicbiitappscan,ncicbiitsecure"

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}


settings = Settings()
