"use client";

import { Suspense, useEffect, useState, useMemo, useCallback } from "react";
import { useSearchParams } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import {
  getInstitutionalOrgs,
  searchInstitutionalOrgs,
  findInstitutionalOrg,
  getInstitutionalDocs,
  getDownloadUrl,
} from "@/lib/api";
import type { OrgOut, InstitutionalDoc } from "@/lib/types";

export default function InstitutionalPage() {
  return (
    <Suspense fallback={<div className="flex min-h-screen items-center justify-center"><p className="text-gray-500">Loading...</p></div>}>
      <InstitutionalContent />
    </Suspense>
  );
}

/* ── Helpers ── */

function formatDate(dateStr: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}/${d.getFullYear()}`;
}

/* ── Page number builder ── */
function buildPageNumbers(current: number, total: number): (number | "...")[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "...")[] = [1];
  if (current > 3) pages.push("...");
  for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) pages.push(i);
  if (current < total - 2) pages.push("...");
  pages.push(total);
  return pages;
}

const PAGE_SIZE = 25;

/* ── Main content ── */

function InstitutionalContent() {
  const { user, loading } = useAuth();
  const searchParams = useSearchParams();

  const [orgs, setOrgs] = useState<OrgOut[]>([]);
  const [selectedOrg, setSelectedOrg] = useState<OrgOut | null>(null);
  const [docs, setDocs] = useState<InstitutionalDoc[]>([]);
  const [docsLoading, setDocsLoading] = useState(false);
  const [searchStr, setSearchStr] = useState("");
  const [filter, setFilter] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  const orgIdParam = searchParams.get("org_id");

  // Load orgs on mount; if org_id param, find and select that org
  useEffect(() => {
    if (!user) return;

    if (orgIdParam) {
      // Navigate from grant icon — find the org and load its docs
      findInstitutionalOrg(Number(orgIdParam))
        .then((result) => {
          if (result.length > 0) {
            const org = result[0] as OrgOut;
            setSelectedOrg(org);
            setOrgs([org]);
            if (org.org_id) {
              setDocsLoading(true);
              getInstitutionalDocs(org.org_id)
                .then(setDocs)
                .catch(console.error)
                .finally(() => setDocsLoading(false));
            }
          }
        })
        .catch(console.error);
    } else {
      getInstitutionalOrgs().then(setOrgs).catch(console.error);
    }
  }, [user, orgIdParam]);

  const handleSearch = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchStr.trim()) {
      getInstitutionalOrgs().then(setOrgs).catch(console.error);
      return;
    }
    const results = await searchInstitutionalOrgs(searchStr);
    setOrgs(results);
    setSelectedOrg(null);
    setDocs([]);
  }, [searchStr]);

  const handleSelectOrg = useCallback(async (org: OrgOut) => {
    setSelectedOrg(org);
    setFilter("");
    setCurrentPage(1);
    if (org.org_id) {
      setDocsLoading(true);
      try {
        const data = await getInstitutionalDocs(org.org_id);
        setDocs(data);
      } catch (err) {
        console.error(err);
      } finally {
        setDocsLoading(false);
      }
    }
  }, []);

  /* ── Filter ── */
  const filteredDocs = useMemo(() => {
    if (!filter.trim()) return docs;
    const lc = filter.toLowerCase();
    return docs.filter((doc) => {
      const cat = (doc.category_name || "").toLowerCase();
      const comments = (doc.comments || "").toLowerCase();
      const start = formatDate(doc.start_date).toLowerCase();
      const end = formatDate(doc.end_date).toLowerCase();
      return cat.includes(lc) || comments.includes(lc) || start.includes(lc) || end.includes(lc);
    });
  }, [docs, filter]);

  /* ── Pagination ── */
  const totalFiltered = filteredDocs.length;
  const totalPages = Math.ceil(totalFiltered / PAGE_SIZE);
  const visibleDocs = filteredDocs.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  const handleFilterChange = (val: string) => { setFilter(val); setCurrentPage(1); };

  if (loading || !user) return null;

  return (
    <AppShell user={user}>
      <div className="space-y-3">
        {/* ── Search bar ── */}
        <div className="rounded-xl border border-border bg-white shadow-sm">
          <div className="flex items-center gap-2 bg-[#f8fafc] px-4 py-2 border-b border-border-light">
            <svg className="h-4 w-4 text-primary/60 shrink-0" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 21h16.5M4.5 3h15M5.25 3v18m13.5-18v18M9 6.75h1.5m-1.5 3h1.5m-1.5 3h1.5m3-6H15m-1.5 3H15m-1.5 3H15M9 21v-3.375c0-.621.504-1.125 1.125-1.125h3.75c.621 0 1.125.504 1.125 1.125V21" />
            </svg>
            <span className="font-semibold text-sm text-primary">Institutional Files</span>
          </div>

          <form onSubmit={handleSearch} className="px-4 py-3 flex items-center gap-2">
            <div className="relative flex-1 max-w-md">
              <svg className="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-text-muted pointer-events-none" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
              </svg>
              <input
                type="text"
                value={searchStr}
                onChange={(e) => setSearchStr(e.target.value)}
                placeholder="Search organizations..."
                className="w-full rounded-lg border border-border pl-8 pr-3 py-1.5 text-sm focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
              />
            </div>
            <button type="submit" className="px-3 py-1.5 rounded-lg text-xs font-semibold bg-primary text-white hover:bg-primary-dark transition-colors">
              Search
            </button>
            {orgIdParam && (
              <button
                type="button"
                onClick={() => {
                  setSearchStr("");
                  getInstitutionalOrgs().then(setOrgs).catch(console.error);
                }}
                className="px-3 py-1.5 rounded-lg text-xs font-semibold bg-gray-100 text-text-secondary hover:bg-gray-200 transition-colors"
              >
                Show All
              </button>
            )}
          </form>
        </div>

        <div className="grid gap-3 lg:grid-cols-[300px_1fr]">
          {/* ── Organization list ── */}
          <div className="rounded-xl border border-border bg-white shadow-sm">
            <div className="px-4 py-2 border-b border-border-light bg-gray-50/50">
              <span className="text-xs font-semibold text-text-secondary">
                Organizations ({orgs.length})
              </span>
            </div>
            <div className="max-h-[calc(100vh-280px)] overflow-y-auto">
              {orgs.length === 0 && (
                <div className="px-4 py-4 text-sm text-text-muted text-center">No organizations found.</div>
              )}
              {orgs.map((org) => (
                <button
                  key={org.org_id}
                  type="button"
                  onClick={() => handleSelectOrg(org)}
                  className={`w-full border-b border-border-light/50 px-3 py-2 text-left text-sm hover:bg-blue-50/60 transition-colors ${
                    selectedOrg?.org_id === org.org_id ? "bg-blue-50 border-l-2 border-l-primary" : ""
                  }`}
                >
                  <span className={`${selectedOrg?.org_id === org.org_id ? "font-semibold text-primary" : "text-text-primary"}`}>
                    {org.org_name}
                  </span>
                  <span className="ml-2 text-[10px] text-text-muted">
                    ({org.doc_count})
                  </span>
                </button>
              ))}
            </div>
          </div>

          {/* ── Document table ── */}
          <div className="rounded-xl border border-border bg-white shadow-sm">
            <div className="flex items-center gap-2 bg-[#f8fafc] px-4 py-2 border-b border-border-light">
              <span className="font-semibold text-sm text-text-primary">
                {selectedOrg ? selectedOrg.org_name : "Select an organization"}
              </span>

              <div className="flex-1" />

              {/* Filter */}
              {selectedOrg && docs.length > 0 && (
                <div className="relative">
                  <svg className="absolute left-2 top-1/2 -translate-y-1/2 h-3 w-3 text-text-muted pointer-events-none" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
                  </svg>
                  <input
                    type="text"
                    value={filter}
                    onChange={(e) => handleFilterChange(e.target.value)}
                    placeholder="Filter..."
                    className="w-32 rounded border border-border pl-6 pr-2 py-0.5 text-[11px] focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
                  />
                  {filter && (
                    <button type="button" onClick={() => handleFilterChange("")} className="absolute right-1.5 top-1/2 -translate-y-1/2 text-text-muted hover:text-text-primary">
                      <svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
                    </button>
                  )}
                </div>
              )}
            </div>

            <div className="overflow-x-auto">
              {!selectedOrg && (
                <div className="px-4 py-8 text-sm text-text-muted text-center">
                  Select an organization to view its documents.
                </div>
              )}

              {selectedOrg && docsLoading && (
                <div className="px-4 py-6 flex items-center justify-center gap-2 text-sm text-text-muted">
                  <svg className="animate-spin h-4 w-4 text-primary" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Loading documents...
                </div>
              )}

              {selectedOrg && !docsLoading && docs.length === 0 && (
                <div className="px-4 py-4 text-sm text-text-muted text-center">No documents found.</div>
              )}

              {selectedOrg && !docsLoading && totalFiltered === 0 && docs.length > 0 && (
                <div className="px-4 py-4 text-sm text-text-muted text-center">
                  No documents match &ldquo;{filter}&rdquo;.
                </div>
              )}

              {selectedOrg && !docsLoading && totalFiltered > 0 && (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border-light bg-gray-50/50">
                      <th className="px-4 py-2 text-left font-semibold text-text-secondary" style={{ width: "40%" }}>Document Name</th>
                      <th className="px-3 py-2 text-left font-semibold text-text-secondary">Created</th>
                      <th className="px-3 py-2 text-left font-semibold text-text-secondary">Flag Start</th>
                      <th className="px-3 py-2 text-left font-semibold text-text-secondary">Flag End</th>
                      <th className="px-3 py-2 text-left font-semibold text-text-secondary">Comments</th>
                    </tr>
                  </thead>
                  <tbody>
                    {visibleDocs.map((doc, idx) => {
                      const rowBg = idx % 2 === 0 ? "bg-white" : "bg-blue-50/40";
                      const docR = doc as unknown as Record<string, unknown>;
                      const url = String(docR.url ?? "");
                      const hasUrl = url && url !== "null";
                      const docName = [doc.category_name, doc.comments].filter(Boolean).join(" - ") || `Document ${doc.document_id}`;
                      const createdDate = formatDate(String(docR.created_date ?? ""));

                      return (
                        <tr key={doc.document_id} className={`${rowBg} border-b border-border-light/50 hover:bg-blue-50/60 transition-colors`}>
                          <td className="px-4 py-1.5">
                            {hasUrl ? (
                              <a
                                href={getDownloadUrl(doc.document_id)}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-primary hover:underline"
                              >
                                {docName}
                              </a>
                            ) : (
                              <span className="text-text-primary">{docName}</span>
                            )}
                          </td>
                          <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">{createdDate}</td>
                          <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">{formatDate(doc.start_date)}</td>
                          <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">{formatDate(doc.end_date)}</td>
                          <td className="px-3 py-1.5 text-text-secondary text-xs">{doc.comments}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              )}
            </div>

            {/* ── Footer: pagination + count ── */}
            {selectedOrg && !docsLoading && totalFiltered > 0 && (
              <div className="px-4 py-1.5 border-t border-border-light flex items-center gap-2 text-[11px] text-text-muted flex-wrap">
                <span>
                  {totalPages <= 1
                    ? `${totalFiltered} document${totalFiltered !== 1 ? "s" : ""}`
                    : `Showing ${(currentPage - 1) * PAGE_SIZE + 1}–${Math.min(currentPage * PAGE_SIZE, totalFiltered)} of ${totalFiltered}`}
                  {filter && ` (${docs.length} total)`}
                </span>

                <div className="flex-1" />

                {totalPages > 1 && (
                  <div className="flex items-center gap-0.5">
                    <button type="button" disabled={currentPage === 1} onClick={() => setCurrentPage((p) => p - 1)} className="px-1.5 py-0.5 rounded text-primary hover:bg-blue-50 disabled:text-gray-300 disabled:hover:bg-transparent transition-colors font-medium">&laquo;</button>
                    {buildPageNumbers(currentPage, totalPages).map((p, i) =>
                      p === "..." ? (
                        <span key={`ellipsis-${i}`} className="px-1 text-text-muted">...</span>
                      ) : (
                        <button key={p} type="button" onClick={() => setCurrentPage(p as number)} className={`min-w-[22px] px-1 py-0.5 rounded text-center font-medium transition-colors ${currentPage === p ? "bg-primary text-white" : "text-primary hover:bg-blue-50"}`}>{p}</button>
                      ),
                    )}
                    <button type="button" disabled={currentPage === totalPages} onClick={() => setCurrentPage((p) => p + 1)} className="px-1.5 py-0.5 rounded text-primary hover:bg-blue-50 disabled:text-gray-300 disabled:hover:bg-transparent transition-colors font-medium">&raquo;</button>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </AppShell>
  );
}
