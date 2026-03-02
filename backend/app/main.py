"""FastAPI application assembly -- eGrants backend."""

import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from .auth.middleware import AuthMiddleware
from .auth.providers import SiteMinderProvider
from .config import settings
from .database import engine
from .routers import (
    admin,
    dashboard,
    documents,
    files,
    funding,
    institutional,
    lookups,
    management,
    reminders,
    search,
    supplements,
    users,
)

logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan -- dispose of the DB engine on shutdown."""
    logger.info("eGrants backend starting up (env=%s)", settings.environment)
    yield
    logger.info("eGrants backend shutting down -- disposing DB engine")
    engine.dispose()


app = FastAPI(
    title="eGrants API",
    version=settings.app_version,
    lifespan=lifespan,
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.cors_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# SiteMinder authentication
blacklist = [b.strip() for b in settings.sm_user_blacklist.split(",") if b.strip()]
app.add_middleware(
    AuthMiddleware,
    auth_provider=SiteMinderProvider(blacklist=blacklist),
)

# Routers
app.include_router(users.router)
app.include_router(search.router)
app.include_router(documents.router)
app.include_router(dashboard.router)
app.include_router(admin.router)
app.include_router(funding.router)
app.include_router(institutional.router)
app.include_router(management.router)
app.include_router(supplements.router)
app.include_router(lookups.router)
app.include_router(reminders.router)
app.include_router(files.router)


@app.get("/api/health")
def health_check():
    """Simple health check endpoint."""
    return {
        "status": "ok",
        "version": settings.app_version,
        "build": settings.app_build,
        "environment": settings.environment,
    }
