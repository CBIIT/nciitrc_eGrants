# NCI ITRC eGrants

Source repository for the eGrants application — a grants management system built with a Next.js frontend and FastAPI backend.

## Project Structure

```
├── frontend/          # Next.js 16 / React 19 / TypeScript / Tailwind CSS 4
│   └── src/
│       ├── app/       # Next.js App Router pages
│       ├── components/
│       ├── contexts/
│       ├── hooks/
│       └── lib/
├── backend/           # FastAPI / SQLAlchemy / SQL Server (pyodbc)
│   ├── app/
│   │   ├── auth/
│   │   ├── models/
│   │   ├── routers/
│   │   ├── schemas/
│   │   └── services/
│   └── tests/
```

## Prerequisites

- **Node.js** (v18+) and **npm**
- **Python** 3.11+
- **ODBC Driver 18 for SQL Server**
- Access to a SQL Server database instance

## Getting Started

### Backend

```bash
cd backend

# Create and activate a virtual environment
python -m venv .venv
source .venv/bin/activate        # macOS / Linux
# .venv\Scripts\activate         # Windows

# Install dependencies
pip install -r requirements.txt

# Configure environment variables
cp .env.example .env
# Edit .env with your database connection string and other settings

# Run the development server
uvicorn app.main:app --reload --port 8000
```

The API will be available at `http://localhost:8000`. Interactive docs are at `http://localhost:8000/docs`.

### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Run the development server
npm run dev
```

The frontend will be available at `http://localhost:3000`. API requests to `/api/*` are proxied to the backend at `localhost:8000`.

### Running Both Together

Open two terminals and start each server:

| Terminal | Command |
|----------|---------|
| 1 | `cd backend && source .venv/bin/activate && uvicorn app.main:app --reload --port 8000` |
| 2 | `cd frontend && npm run dev` |

Then open `http://localhost:3000` in your browser.

## Environment Variables

Copy `backend/.env.example` to `backend/.env` and configure:

| Variable | Description |
|----------|-------------|
| `DATABASE_ODBC` | Full ODBC connection string for SQL Server |
| `ENVIRONMENT` | `dev`, `test`, or `prod` |
| `APP_VERSION` | Application version |
| `CORS_ORIGINS` | Allowed CORS origins (JSON array) |
| `IMAGE_SERVER_URL` | Base URL for document/image serving |
| `WEB_GRANT_URL` | Local file path for grant documents |
| `SMTP_HOST` | SMTP server hostname |
| `SMTP_FROM` | Sender email address |
| `SEND_MAIL` | `true` / `false` — enable email sending |
| `MAIL_OVERRIDE_ADDRESS` | Override all outgoing mail to this address (for testing) |
| `SM_USER_BLACKLIST` | Comma-separated SiteMinder service accounts to ignore |

## Testing

```bash
cd backend
source .venv/bin/activate
pytest
```