import type {
  UserInfo,
  SearchResult,
  DocumentGridResponse,
  DashboardResponse,
  Category,
  SubCategory,
  FundingCategory,
  OrgOut,
  InstitutionalDoc,
  PersonOut,
} from "./types";

const API_BASE = "/api";

async function fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, options);
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `HTTP ${res.status}`);
  }
  return res.json();
}

async function postJson<T>(url: string, body: unknown): Promise<T> {
  return fetchJson<T>(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
}

// ---- Users ----
export function getCurrentUser(): Promise<UserInfo> {
  return fetchJson(`${API_BASE}/users/me`);
}

// ---- Search ----
export function searchByString(q: string, pkg?: string): Promise<SearchResult> {
  const params = new URLSearchParams({ q });
  if (pkg) params.set("package", pkg);
  return fetchJson(`${API_BASE}/search/by-string?${params}`);
}

export function searchByGrant(grantId: number): Promise<SearchResult> {
  return fetchJson(`${API_BASE}/search/by-grant/${grantId}`);
}

export function searchByFilters(
  fy: string,
  mechanism: string,
  adminCode: string,
  serialNum: string,
  pageNum: number,
): Promise<SearchResult> {
  const params = new URLSearchParams({
    fy,
    mechanism,
    admin_code: adminCode,
    serial_num: serialNum,
    page_num: String(pageNum),
  });
  return fetchJson(`${API_BASE}/search/by-filters?${params}`);
}

export function searchByApplId(
  applId: number,
  searchType?: string,
  categoryList?: string,
): Promise<SearchResult> {
  const params = new URLSearchParams();
  if (searchType) params.set("search_type", searchType);
  if (categoryList) params.set("category_list", categoryList);
  return fetchJson(`${API_BASE}/search/by-appl/${applId}?${params}`);
}

export function getSupplement(
  grantId: number,
  act: string = "to_view",
): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams({ grant_id: String(grantId), act });
  return fetchJson(`${API_BASE}/search/supplement?${params}`);
}

export function renameLabel(applId: number, label: string): Promise<{ ok: boolean; label: string }> {
  return postJson(`${API_BASE}/search/rename-label`, { appl_id: applId, label });
}

export function getAllApplsList(
  adminCode: string,
  serialNum: string,
): Promise<{ full_grant_num: string; appl_id: number }[]> {
  const params = new URLSearchParams({ admin_code: adminCode, serial_num: serialNum });
  return fetchJson(`${API_BASE}/search/appls-list?${params}`);
}

export function createGrantYear(data: {
  grant_id: number;
  appl_type_code: number;
  activity_code: string;
  admin_code: string;
  serial_num: string;
  support_year: string;
  suffix_code?: string;
}): Promise<{ appl_id: number; full_grant_num: string }> {
  return postJson(`${API_BASE}/search/create-grant-year`, data);
}

export function getStopNotice(grantId: number): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/search/stop-notice/${grantId}`);
}

export function getDataYears(
  fy: string,
  mechanism: string,
  adminCode: string,
  serialNum: string,
): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams({ fy, mechanism, admin_code: adminCode, serial_num: serialNum });
  return fetchJson(`${API_BASE}/search/data-years?${params}`);
}

export function autocompleteFy(term: string): Promise<string[]> {
  return fetchJson(`${API_BASE}/search/autocomplete/fy?term=${encodeURIComponent(term)}`);
}

export function autocompleteMechanism(term: string): Promise<string[]> {
  return fetchJson(`${API_BASE}/search/autocomplete/mechanism?term=${encodeURIComponent(term)}`);
}

export function autocompleteSerialNum(term: string): Promise<string[]> {
  return fetchJson(`${API_BASE}/search/autocomplete/serial-num?term=${encodeURIComponent(term)}`);
}

// ---- Documents ----
export function getDocumentGrid(
  applId: number,
  searchType?: string,
  categoryList?: string,
): Promise<DocumentGridResponse> {
  const params = new URLSearchParams();
  if (searchType) params.set("search_type", searchType);
  if (categoryList) params.set("category_list", categoryList);
  const qs = params.toString();
  return fetchJson(`${API_BASE}/documents/grid/${applId}${qs ? `?${qs}` : ""}`);
}

export function createDocument(data: {
  appl_id: number;
  category_id: number;
  sub_category?: string;
  document_date?: string;
  file_type?: string;
}): Promise<{ document_id: number }> {
  return postJson(`${API_BASE}/documents/create`, data);
}

export async function uploadDocumentFile(
  documentId: number,
  file: File,
): Promise<{ document_id: number; filename: string }> {
  const formData = new FormData();
  formData.append("file", file);
  const res = await fetch(`${API_BASE}/documents/upload/${documentId}`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) throw new Error("Failed to upload file");
  return res.json();
}

export async function uploadDocumentFileAsPdf(
  documentId: number,
  file: File,
): Promise<{ document_id: number; filename: string }> {
  const formData = new FormData();
  formData.append("file", file);
  const res = await fetch(`${API_BASE}/documents/upload-as-pdf/${documentId}`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || "Failed to convert and upload file");
  }
  return res.json();
}

export function docQcAction(act: string, docids: string): Promise<{ ok: boolean }> {
  return postJson(`${API_BASE}/documents/qc-action`, { act, docids });
}

export function getCategories(grantId: number, years?: string): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams();
  if (years) params.set("years", years);
  return fetchJson(`${API_BASE}/documents/categories/${grantId}?${params}`);
}

// ---- Dashboard ----
export function getDashboard(): Promise<DashboardResponse> {
  return fetchJson(`${API_BASE}/dashboard`);
}

export function getAuditReport(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/dashboard/audit-report`);
}

// ---- Admin ----
export function getAccessControl(): Promise<PersonOut[]> {
  return fetchJson(`${API_BASE}/admin/access`);
}

export function updateAccessControl(data: Record<string, unknown>): Promise<PersonOut[]> {
  return postJson(`${API_BASE}/admin/access`, data);
}

export function getFlags(flagType?: string): Promise<Record<string, unknown>[]> {
  const params = flagType ? `?flag_type=${encodeURIComponent(flagType)}` : "";
  return fetchJson(`${API_BASE}/admin/flags${params}`);
}

export function updateFlags(data: Record<string, unknown>): Promise<Record<string, unknown>[]> {
  return postJson(`${API_BASE}/admin/flags`, data);
}

export function getAdminCategories(): Promise<Record<string, unknown>> {
  return fetchJson(`${API_BASE}/admin/categories`);
}

export function updateCategory(data: Record<string, unknown>): Promise<Record<string, unknown>> {
  return postJson(`${API_BASE}/admin/categories`, data);
}

export function getPositions(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/admin/positions`);
}

export function getAdminCodes(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/admin/admin-codes`);
}

// ---- Funding ----
export function getFundingDocs(serialNum: string, fy?: string): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams({ serial_num: serialNum });
  if (fy) params.set("fy", fy);
  return fetchJson(`${API_BASE}/funding?${params}`);
}

export function createFundingDoc(data: Record<string, unknown>): Promise<{ document_id: number }> {
  return postJson(`${API_BASE}/funding/create`, data);
}

// ---- Institutional ----
export function getInstitutionalOrgs(): Promise<OrgOut[]> {
  return fetchJson(`${API_BASE}/institutional/orgs`);
}

export function findInstitutionalOrg(orgId: number): Promise<OrgOut[]> {
  return fetchJson(`${API_BASE}/institutional/orgs/${orgId}`);
}

export function searchInstitutionalOrgs(q: string): Promise<OrgOut[]> {
  return fetchJson(`${API_BASE}/institutional/orgs/search?q=${encodeURIComponent(q)}`);
}

export function getInstitutionalDocs(orgId: number): Promise<InstitutionalDoc[]> {
  return fetchJson(`${API_BASE}/institutional/docs/${orgId}`);
}

export function createInstitutionalFile(data: Record<string, unknown>): Promise<{ document_id: number }> {
  return postJson(`${API_BASE}/institutional/docs/create`, data);
}

// ---- Management ----
export function getQcQueue(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/qc`);
}

export function getDocTransactions(
  startDate: string,
  endDate: string,
): Promise<Record<string, unknown>[]> {
  return fetchJson(
    `${API_BASE}/management/doc-transactions?start_date=${encodeURIComponent(startDate)}&end_date=${encodeURIComponent(endDate)}`,
  );
}

export function getQcReasons(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/qc-reasons`);
}

export function getSpecialists(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/specialists`);
}

export function getQcPersons(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/qc-persons`);
}

export function getQcReport(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/qc-report`);
}

export function qcAssign(data: {
  act: string;
  person_id?: number;
  qc_person_id?: number;
  qc_reason?: string;
  percent?: number;
}): Promise<{ ok: boolean }> {
  return postJson(`${API_BASE}/management/qc-assign`, data);
}

export function getDocTransactionReport(
  transactionType: string,
  personId: number,
  opts?: { startDate?: string; endDate?: string; dateRange?: string },
): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams({
    transaction_type: transactionType,
    person_id: String(personId),
  });
  if (opts?.startDate) params.set("start_date", opts.startDate);
  if (opts?.endDate) params.set("end_date", opts.endDate);
  if (opts?.dateRange) params.set("date_range", opts.dateRange);
  return fetchJson(`${API_BASE}/management/doc-transaction-report?${params}`);
}

export function getAccessions(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/management/accessions`);
}

export function getSystemReport(
  act: string,
  searchNumber: number,
): Promise<Record<string, unknown>[]> {
  const params = new URLSearchParams({
    act,
    search_number: String(searchNumber),
  });
  return fetchJson(`${API_BASE}/management/system-report?${params}`);
}

// ---- Lookups ----
export function getLookupCategories(): Promise<Category[]> {
  return fetchJson(`${API_BASE}/lookups/categories`);
}

export function getSubCategories(categoryId: number): Promise<SubCategory[]> {
  return fetchJson(`${API_BASE}/lookups/categories/${categoryId}/sub-categories`);
}

export function getFundingCategories(fy?: string): Promise<FundingCategory[]> {
  const params = fy ? `?fy=${encodeURIComponent(fy)}` : "";
  return fetchJson(`${API_BASE}/lookups/funding-categories${params}`);
}

export function getFlagTypes(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/lookups/flag-types`);
}

// ---- Files ----
export function getDownloadUrl(documentId: number): string {
  return `${API_BASE}/files/download/${documentId}`;
}

// ---- Reminders ----
export function getDeactivationReminders(): Promise<Record<string, unknown>[]> {
  return fetchJson(`${API_BASE}/reminders/deactivation`);
}
