from fastapi import APIRouter, Depends

from ..auth.dependencies import get_current_user
from ..schemas.auth import UserInfo

router = APIRouter(prefix="/api/users", tags=["users"])


@router.get("/me")
def get_me(user: UserInfo = Depends(get_current_user)):
    return user
