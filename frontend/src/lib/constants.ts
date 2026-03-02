export const APP_TITLE = "eGrants";
export const APP_FULL_TITLE = "eGrants - NCI Grant Document Management";

export const NAV_TABS = [
  { label: "Dashboard", href: "/dashboard", permission: "can_dashboard" },
  { label: "eGrants", href: "/search", permission: "can_egrants" },
  { label: "Funding", href: "/funding", permission: "can_cft" },
  { label: "Institutional", href: "/institutional", permission: "can_egrants" },
  { label: "QC", href: "/qc", permission: "can_mgt" },
  { label: "Management", href: "/management", permission: "can_mgt" },
  { label: "Admin", href: "/admin", permission: "can_admin" },
] as const;
