from fastapi import HTTPException, Request

from ..schemas.auth import UserInfo


def get_current_user(request: Request) -> UserInfo:
    return request.state.user_info


def require_authorized(request: Request) -> UserInfo:
    user = get_current_user(request)
    if not user.authorized:
        raise HTTPException(status_code=403, detail="Not authorized")
    return user


def require_admin(request: Request) -> UserInfo:
    user = require_authorized(request)
    if not user.can_admin:
        raise HTTPException(status_code=403, detail="Admin access required")
    return user
