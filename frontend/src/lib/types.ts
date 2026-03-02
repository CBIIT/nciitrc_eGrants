// User & Auth
export interface UserInfo {
  person_id: number | null;
  userid: string;
  first_name: string;
  last_name: string;
  full_name: string;
  email: string;
  ic: string;
  position_id: number | null;
  position_name: string;
  is_coordinator: boolean;
  coordinator_id: number | null;
  can_egrants: boolean;
  can_mgt: boolean;
  can_admin: boolean;
  can_docman: boolean;
  can_cft: boolean;
  can_dashboard: boolean;
  can_iccoord: boolean;
  menulist: string;
  authorized: boolean;
  environment: string;
  version: string;
  build: string;
}

// Search
export interface GrantResult {
  grant_id: number;
  serial_num: string | null;
  admin_phs_org_code: string | null;
  current_pi_name: string | null;
  current_pi_email_address: string | null;
}

export interface ApplicationResult {
  appl_id: number;
  grant_id: number | null;
  full_grant_num: string | null;
  support_year: string | null;
  project_title: string | null;
  first_name: string | null;
  last_name: string | null;
  org_name: string | null;
  label: string | null;
  appl_type_code: number | null;
  deleted_by_impac: string | null;
}

export interface SearchResult {
  grants: GrantResult[];
  applications: ApplicationResult[];
  doc_counts: Record<string, unknown>[];
  total_count: number;
  page_num: number;
  message: string;
}

// Documents
export interface DocumentOut {
  document_id: number;
  appl_id: number | null;
  category_id: number | null;
  category_name: string | null;
  sub_category_name: string | null;
  document_date: string | null;
  document_name: string | null;
  url: string | null;
  created_by: string | null;
  created_date: string | null;
  modified_by: string | null;
  modified_date: string | null;
  page_count: number | null;
  qc_date: string | null;
  problem_msg: string | null;
}

export interface DocumentGridResponse {
  documents: DocumentOut[];
  categories: Record<string, unknown>[];
  sub_categories: Record<string, unknown>[];
  flags: Record<string, unknown>[];
  grant_info: Record<string, unknown>;
  appl_info: Record<string, unknown>;
  years: Record<string, unknown>[];
  message: string;
}

// Dashboard
export interface WidgetData {
  widget_id: number;
  widget_title: string;
  data: Record<string, unknown>[];
}

export interface DashboardResponse {
  widgets: WidgetData[];
  links: Record<string, unknown>[];
  message: string;
}

// Admin
export interface PersonOut {
  person_id: number;
  userid: string | null;
  first_name: string | null;
  last_name: string | null;
  email: string | null;
  position_name: string | null;
  active: number | null;
  ic: string | null;
  can_egrants: boolean;
  can_mgt: boolean;
  can_admin: boolean;
  can_docman: boolean;
  can_cft: boolean;
  can_dashboard: boolean;
  can_iccoord: boolean;
}

// Lookups
export interface Category {
  category_id: number;
  category_name: string;
  package: string | null;
}

export interface SubCategory {
  parent_category_id: number;
  sub_category_name: string;
}

export interface FundingCategory {
  category_id: number;
  category_name: string | null;
  level_id: number | null;
  parent_id: number | null;
  category_fy: string | null;
  child_count: number;
  doc_count: number;
}

// Institutional
export interface OrgOut {
  org_id: number | null;
  org_name: string | null;
  doc_count: number;
}

export interface InstitutionalDoc {
  document_id: number;
  org_id: number | null;
  category_name: string | null;
  start_date: string | null;
  end_date: string | null;
  comments: string | null;
  disabled: boolean;
}
