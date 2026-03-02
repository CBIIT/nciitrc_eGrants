"""Application constants ported from ASP.NET EgrantsCommon and related models."""

# Supported file types for document upload
ALLOWED_FILE_TYPES = {
    ".pdf", ".doc", ".docx", ".msg", ".rtf",
    ".jpg", ".png", ".gif", ".tif",
    ".html", ".htm", ".log", ".dat", ".txt",
}

# File types that require QC review
QC_REQUIRED_FILE_TYPES = {
    ".doc", ".docx", ".msg", ".rtf", ".jpg",
    ".png", ".gif", ".tif", ".html", ".htm",
    ".log", ".dat", ".txt",
}

# Grant flag types
FLAG_TYPES = {
    "ARRA": "American Recovery and Reinvestment Act",
    "FDA": "Food and Drug Administration",
    "TOBACCO": "Tobacco Funding Restriction",
    "STOP_NOTICE": "Stop Notice",
    "MS": "MS Flag",
    "OD": "OD Flag",
    "DS": "DS Flag",
    "STP": "STP Flag",
}

# Application type codes
APPL_TYPE_CODES = {
    1: "New",
    2: "Competing Renewal",
    3: "Non-Competing Continuation",
    4: "Competing Supplement",
    5: "Non-Competing Supplement",
    7: "Change of Grantee Institution",
    9: "Administrative Supplement",
}

# Email routing categories (from eMailRouter.vbs)
EMAIL_CATEGORIES = {
    "ERA_NOTIFICATION": "eRANotification",
    "RPPR": "RPPR",
    "JIT": "JIT Info",
    "CLOSEOUT": "Closeout",
    "PUBLIC_ACCESS": "PublicAccess",
    "FUNDING": "Funding",
    "SUPPLEMENT": "Supplement",
    "CORRESPONDENCE": "Correspondence",
    "FRAM": "FRAM",
    "PRAM": "PRAM",
    "FFR": "FFR",
    "IRPPR": "IRPPR",
}

# Dashboard widget types
WIDGET_TYPES = {
    "GRANTS_TOGO": "Grants To Go",
    "EXPEDITED": "Expedited Grants",
    "LATE": "Late Grants",
    "NEW_GRANTS": "New Grants",
    "AVG_TIME": "Average Time",
}
