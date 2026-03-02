"""Authentication middleware — replicates legacy Global.asax.cs exactly.

Legacy flow (Global.asax.cs):
  Application_AuthorizeRequest:
    1. userid = Request.ServerVariables["HEADER_SM_USER"]
    2. ic = Request.ServerVariables["HEADER_USER_SUB_ORG"]  (default "NCI")
    3. If ic not NCI → sp_web_egrants_user_exception(@operator) → if 1, ic="nci"
    4. sp_web_egrants_user_validation(@ic, @operator) → if count=0, reject

  Session_Start:
    5. sp_web_egrants_user_type_check(@ic, @Operator) → OUTPUT @user_application_type
    6. If empty/NULL, reject
    7. sp_web_egrants_user_profile(@ic, @Operator, @type) → name/value pairs:
       VALIDATION, USERID, IC, PERSONID, POSITIONID,
       USERNAME, USEREMAIL, MENULIST
    8. If VALIDATION != "OK", reject
    9. UpdateUsersLastLoginDate(userid)

  MENULIST format: ",Management|M,Admin|A,Dashboard|D"
    - Leading comma, entries are DisplayName|Code
    - Menu display names map to permission flags
"""

import logging

from sqlalchemy import text
from starlette.middleware.base import BaseHTTPMiddleware, RequestResponseEndpoint
from starlette.requests import Request
from starlette.responses import JSONResponse, Response

from ..auth.providers import AuthProvider
from ..config import settings
from ..database import SessionLocal
from ..schemas.auth import UserInfo

logger = logging.getLogger(__name__)

SKIP_AUTH_PATHS = {"/api/health", "/docs", "/openapi.json", "/redoc"}

# Map MENULIST display names to permission flags
# MENULIST entries: "DisplayName|Code" e.g. "Management|M", "Admin|A"
MENU_TO_PERMISSION = {
    "egrants": "can_egrants",
    "management": "can_mgt",
    "admin": "can_admin",
    "docman": "can_docman",
    "cft": "can_cft",
    "dashboard": "can_dashboard",
    "iccoord": "can_iccoord",
    "ic coordinator": "can_iccoord",
}


class AuthMiddleware(BaseHTTPMiddleware):
    def __init__(self, app, auth_provider: AuthProvider):
        super().__init__(app)
        self.auth_provider = auth_provider

    async def dispatch(
        self, request: Request, call_next: RequestResponseEndpoint
    ) -> Response:
        if request.url.path in SKIP_AUTH_PATHS:
            return await call_next(request)

        userid, ic = self.auth_provider.authenticate(request)
        if not userid:
            return JSONResponse(
                status_code=401,
                content={"detail": "Authentication required"},
            )

        db = SessionLocal()
        try:
            user_info = _authenticate_user(db, userid, ic)
            request.state.user_info = user_info
            logger.info(
                "Session for user %s (authorized=%s)", userid, user_info.authorized
            )
        finally:
            db.close()

        return await call_next(request)


def _authenticate_user(db, userid: str, ic: str) -> UserInfo:
    """Replicate legacy Global.asax.cs auth flow using the same stored procedures."""

    base_info = UserInfo(
        userid=userid,
        ic=ic,
        authorized=False,
        environment=settings.environment,
        version=settings.app_version,
        build=settings.app_build,
    )

    # Step 3: If ic is not NCI, check user exception
    # Legacy: if (this.ic != "nci" && this.ic != "NCI") { CheckUsersException(userid) }
    if ic.lower() != "nci":
        row = db.execute(
            text("SET NOCOUNT ON; EXEC sp_web_egrants_user_exception @operator = :operator"),
            {"operator": userid},
        ).first()
        if row and int(row[0]) == 1:
            ic = "nci"

    # Step 4: sp_web_egrants_user_validation(@ic, @operator) → count
    # Legacy: if (userValidation == 0) redirect to error page
    row = db.execute(
        text(
            "SET NOCOUNT ON; "
            "EXEC sp_web_egrants_user_validation @ic = :ic, @operator = :operator"
        ),
        {"ic": ic, "operator": userid},
    ).first()
    if not row or int(row[0]) == 0:
        logger.warning("User %s failed validation (ic=%s)", userid, ic)
        return base_info

    # Step 5: sp_web_egrants_user_type_check(@ic, @Operator) → OUTPUT @user_application_type
    # Legacy: if (string.IsNullOrEmpty(usertype) || usertype == "NULL") redirect
    result = db.execute(
        text(
            "SET NOCOUNT ON; "
            "DECLARE @user_application_type VARCHAR(2); "
            "EXEC sp_web_egrants_user_type_check "
            "@ic = :ic, @Operator = :operator, "
            "@user_application_type = @user_application_type OUTPUT; "
            "SELECT @user_application_type;"
        ),
        {"ic": ic, "operator": userid},
    ).first()
    usertype = result[0] if result else None
    if not usertype or usertype == "NULL":
        logger.warning("User %s has no application type (ic=%s)", userid, ic)
        return base_info

    # Step 7: sp_web_egrants_user_profile(@ic, @Operator, @type) → name/value rows
    # SP returns multiple result sets; the actual data (nam/val) is in the last one.
    # Use raw connection + pyodbc cursor to iterate result sets.
    raw_conn = db.connection().connection.dbapi_connection
    cursor = raw_conn.cursor()
    cursor.execute(
        "EXEC sp_web_egrants_user_profile @ic=?, @Operator=?, @type=?",
        (ic, userid, usertype),
    )

    # Skip to the result set that has columns (nam, val)
    profile = {}
    while True:
        if cursor.description:
            for row in cursor.fetchall():
                profile[row[0]] = row[1]
        if not cursor.nextset():
            break
    cursor.close()

    # Step 8: If VALIDATION != "OK", reject
    if profile.get("VALIDATION") != "OK":
        logger.warning(
            "User %s validation not OK: %s", userid, profile.get("VALIDATION")
        )
        return base_info

    # Build authorized UserInfo from SP results (matching legacy Session_Start)
    person_id = int(profile["PERSONID"]) if profile.get("PERSONID") else None
    position_id = int(profile["POSITIONID"]) if profile.get("POSITIONID") else None
    person_name = profile.get("USERNAME", "")
    user_ic = profile.get("IC", ic)
    menulist = profile.get("MENULIST", "")

    # Parse MENULIST — format: ",Management|M,Admin|A,Dashboard|D"
    # Each entry is "DisplayName|Code", separated by commas with a leading comma
    perms = _parse_menulist(menulist)

    # Update last_login_date (legacy: EgrantsCommon.UpdateUsersLastLoginDate)
    db.execute(
        text("UPDATE people SET last_login_date = GETDATE() WHERE userid = :userid"),
        {"userid": userid},
    )
    db.commit()

    return UserInfo(
        person_id=person_id,
        userid=profile.get("USERID", userid),
        first_name="",
        last_name="",
        full_name=person_name,
        email=profile.get("USEREMAIL", ""),
        ic=user_ic,
        position_id=position_id,
        position_name="",
        is_coordinator=False,
        coordinator_id=None,
        menulist=menulist,
        **perms,
        authorized=True,
        environment=settings.environment,
        version=settings.app_version,
        build=settings.app_build,
    )


def _parse_menulist(menulist: str) -> dict:
    """Parse legacy MENULIST format into permission flags.

    Format: ",Management|M,Admin|A,Dashboard|D"
    Each entry: "DisplayName|Code"
    """
    perms = {
        "can_egrants": False,
        "can_mgt": False,
        "can_admin": False,
        "can_docman": False,
        "can_cft": False,
        "can_dashboard": False,
        "can_iccoord": False,
    }

    for entry in menulist.split(","):
        entry = entry.strip()
        if not entry:
            continue
        # Split "DisplayName|Code" — use display name for mapping
        display_name = entry.split("|")[0].strip().lower()
        perm_key = MENU_TO_PERMISSION.get(display_name)
        if perm_key:
            perms[perm_key] = True

    return perms
