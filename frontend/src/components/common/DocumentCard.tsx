"use client";

import { useState, useEffect, useMemo, useCallback, useRef, Fragment, type ReactNode } from "react";
import { useAuth } from "@/hooks/useAuth";
import { getDocumentGrid, getDownloadUrl, docQcAction, renameLabel } from "@/lib/api";
import type { ApplicationResult } from "@/lib/types";
import {
  CompetingIcon, UmbrellaIcon, FlaskIcon, RocketIcon,
  GovernmentIcon, StopIcon, FlagBadge,
} from "./FlagIcons";

/* ── Types ── */

interface DocumentCardProps {
  application: ApplicationResult;
  searchType: string;
  categoryList: string;
}

interface DocRow {
  document_id: number;
  document_name: string;
  document_date: string;
  doc_date: string;
  page_count: number | string;
  url: string;
  category_id: number;
  category_name: string;
  sub_category_name: string;
  can_upload: string;
  can_modify_index: string;
  can_qc: string;
  can_delete: string;
  can_store: string;
  can_restore: string;
  frc_destroyed: number | string;
  fsr_count: number | string;
  attachment_count: number | string;
  created_by: string;
  created_date: string;
  modified_by: string;
  modified_date: string;
  file_modified_by: string;
  file_modified_date: string;
  problem_msg: string;
  problem_reported_by: string;
  qc_date: string;
  appl_id: number;
  [key: string]: unknown;
}

type SortField = "document_name" | "document_date";
type SortDir = "asc" | "desc";

/* ── Helpers ── */

function formatDate(dateStr: string): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}/${d.getFullYear()}`;
}

function isCompeting(a: Record<string, unknown>): boolean {
  if (String(a.competing ?? "").toLowerCase() === "yes") return true;
  const tc = Number(a.appl_type_code);
  return tc === 1 || tc === 2 || tc === 6 || tc === 9;
}

/* ── Sort icon ── */
function SortIcon({ field, sortField, sortDir }: { field: SortField; sortField: SortField; sortDir: SortDir }) {
  if (sortField !== field) {
    return (
      <svg className="inline h-3 w-3 ml-1 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 15L12 18.75 15.75 15m-7.5-6L12 5.25 15.75 9" />
      </svg>
    );
  }
  if (sortDir === "asc") {
    return (
      <svg className="inline h-3 w-3 ml-1 text-text-primary" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 13.5L12 21m0 0l-7.5-7.5M12 21V3" />
      </svg>
    );
  }
  return (
    <svg className="inline h-3 w-3 ml-1 text-text-primary" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 10.5L12 3m0 0l7.5 7.5M12 3v18" />
    </svg>
  );
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

/* ── Plus / Minus toggle icon ── */
function ToggleIcon({ open }: { open: boolean }) {
  return (
    <span className={`inline-flex items-center justify-center h-4 w-4 rounded text-[11px] font-bold leading-none transition-colors duration-150 ${open ? "bg-primary text-white" : "bg-gray-200 text-gray-600 hover:bg-primary/20 hover:text-primary"}`}>
      {open ? "−" : "+"}
    </span>
  );
}

/* ── Folder icon ── */
function FolderIcon() {
  return (
    <svg className="h-4 w-4 text-primary/60 shrink-0" fill="currentColor" viewBox="0 0 20 20">
      <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
    </svg>
  );
}

/* ── Expanded detail row ── */
function DocDetail({ doc, onAction, acting, checkedIds }: { doc: DocRow; onAction: (act: string, ids: string) => void; acting: boolean; checkedIds: Set<number> }) {
  const parts: string[] = [];
  if (doc.created_date) parts.push(`Created On ${formatDate(doc.created_date)} by ${doc.created_by || "unknown"}`);
  if (doc.file_modified_by) parts.push(`Document uploaded On ${formatDate(doc.file_modified_date)} by ${doc.file_modified_by}`);
  if (doc.modified_by) parts.push(`Updated On ${formatDate(doc.modified_date)} by ${doc.modified_by}`);

  const hasError = doc.problem_msg && doc.qc_date;
  const canStore = doc.can_store === "y";
  const canRestore = doc.can_restore === "y";
  const canDelete = doc.can_delete === "y";
  const hasActions = canStore || canRestore || canDelete;

  const handleStoreSelected = () => {
    if (checkedIds.size === 0) {
      alert("Please select documents you want to store");
      return;
    }
    onAction("to store all", Array.from(checkedIds).join(","));
  };

  return (
    <div className="px-4 py-2 text-xs text-text-secondary bg-slate-50/70">
      {/* Audit trail */}
      <p className="leading-relaxed">
        {parts.join(" · ")}
        {hasError && (
          <span className="ml-2 text-rose-600 font-semibold">
            Error Reported by {doc.problem_reported_by}: {doc.problem_msg}
          </span>
        )}
      </p>

      {/* Action links */}
      {hasActions && (
        <div className="flex items-center gap-5 mt-1.5">
          {acting && (
            <svg className="animate-spin h-3.5 w-3.5 text-primary" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
          )}
          {canStore && (
            <button
              type="button"
              disabled={acting}
              onClick={() => onAction("to store", String(doc.document_id))}
              className="text-[11px] font-medium text-primary hover:underline disabled:opacity-50"
            >
              Store
            </button>
          )}
          {canStore && (
            <button
              type="button"
              disabled={acting}
              onClick={handleStoreSelected}
              className="text-[11px] font-medium text-primary hover:underline disabled:opacity-50"
            >
              Store Selected
            </button>
          )}
          {canRestore && (
            <button
              type="button"
              disabled={acting}
              onClick={() => onAction("to restore", String(doc.document_id))}
              className="text-[11px] font-medium text-primary hover:underline disabled:opacity-50"
            >
              Restore Original
            </button>
          )}
          {canDelete && (
            <button
              type="button"
              disabled={acting}
              onClick={() => onAction("to delete", String(doc.document_id))}
              className="text-[11px] font-medium text-primary hover:underline disabled:opacity-50"
            >
              Delete
            </button>
          )}
        </div>
      )}
    </div>
  );
}

/* ── Confirm dialog ── */
function ConfirmDialog({ message, onConfirm, onCancel }: { message: string; onConfirm: () => void; onCancel: () => void }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30">
      <div className="bg-white rounded-xl shadow-xl border border-border p-5 max-w-sm mx-4">
        <p className="text-sm text-text-primary mb-4">{message}</p>
        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="px-3 py-1.5 rounded text-sm font-medium bg-gray-100 text-text-secondary hover:bg-gray-200 transition-colors"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={onConfirm}
            className="px-3 py-1.5 rounded text-sm font-medium bg-primary text-white hover:bg-primary-dark transition-colors"
          >
            Confirm
          </button>
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════
   Main component
   ══════════════════════════════════════════════════════════ */

const PAGE_SIZE = 25;

export default function DocumentCard({ application, searchType, categoryList }: DocumentCardProps) {
  const { user } = useAuth();
  const [documents, setDocuments] = useState<DocRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [sortField, setSortField] = useState<SortField>("document_date");
  const [sortDir, setSortDir] = useState<SortDir>("desc");
  const [filter, setFilter] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [visibleCount, setVisibleCount] = useState(PAGE_SIZE);

  // QC expansion state
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());
  const [checkedIds, setCheckedIds] = useState<Set<number>>(new Set());
  const [acting, setActing] = useState(false);
  const [confirmAction, setConfirmAction] = useState<{ act: string; ids: string } | null>(null);

  // Rename label state
  const [showRenameDialog, setShowRenameDialog] = useState(false);
  const [renameValue, setRenameValue] = useState("");
  const [currentLabel, setCurrentLabel] = useState("");
  const [renameSaving, setRenameSaving] = useState(false);
  const renameInputRef = useRef<HTMLInputElement>(null);

  const appl = application as unknown as Record<string, unknown>;
  const applId = application.appl_id;
  const fullGrantNum = application.full_grant_num || `#${applId}`;
  const initialLabel = String(appl.label ?? appl.request_name ?? "");
  const positionId = user?.position_id ?? 0;
  const hasQcAccess = positionId != null && positionId >= 2;

  /* ── Per-year flags ── */
  const flags: { label: string; color: string; icon: ReactNode; title: string }[] = [];
  if (isCompeting(appl))
    flags.push({ label: "C", color: "blue", icon: <CompetingIcon />, title: "Competing Year" });
  if (String(appl.appl_ds_flag ?? appl.ds_flag ?? "") === "y")
    flags.push({ label: "DS", color: "purple", icon: <UmbrellaIcon />, title: "Diversity Supplement" });
  if (String(appl.appl_fda_flag ?? appl.fda_flag ?? "") === "y")
    flags.push({ label: "FDA", color: "rose", icon: <FlaskIcon />, title: "FDA Grant" });
  if (String(appl.appl_ms_flag ?? appl.ms_flag ?? "") === "y")
    flags.push({ label: "MS", color: "amber", icon: <RocketIcon />, title: "Moonshot Funded" });
  if (String(appl.appl_od_flag ?? appl.od_flag ?? "") === "y")
    flags.push({ label: "OD", color: "emerald", icon: <GovernmentIcon />, title: "OD Funded" });
  if (application.deleted_by_impac === "y" || application.deleted_by_impac === "Y")
    flags.push({ label: "DEL", color: "rose", icon: <StopIcon />, title: "Deleted by IMPAC" });

  /* ── Permission flags ── */
  const isDeleted = application.deleted_by_impac === "y" || application.deleted_by_impac === "Y";
  const canAddDoc = hasQcAccess && String(appl.can_add_doc ?? "") === "y" && !isDeleted;
  const canAddFunding = positionId != null && positionId > 2 && String(appl.can_add_funding ?? "") === "y" && !isDeleted;
  const canRenameLabel = hasQcAccess && String(appl.can_rename_label ?? "") === "y";

  // Sync currentLabel from application data on mount
  useEffect(() => { setCurrentLabel(initialLabel); }, [initialLabel]);

  const openRenameDialog = useCallback(() => {
    setRenameValue(currentLabel);
    setShowRenameDialog(true);
    setTimeout(() => renameInputRef.current?.focus(), 0);
  }, [currentLabel]);

  const closeRenameDialog = useCallback(() => {
    setShowRenameDialog(false);
  }, []);

  const handleRenameSave = useCallback(async () => {
    setRenameSaving(true);
    try {
      const res = await renameLabel(applId, renameValue.trim());
      setCurrentLabel(res.label);
      setShowRenameDialog(false);
    } catch (err) {
      console.error(err);
    } finally {
      setRenameSaving(false);
    }
  }, [applId, renameValue]);

  const handleRenameDelete = useCallback(async () => {
    setRenameSaving(true);
    try {
      await renameLabel(applId, "");
      setCurrentLabel("");
      setShowRenameDialog(false);
    } catch (err) {
      console.error(err);
    } finally {
      setRenameSaving(false);
    }
  }, [applId]);

  /* ── Fetch documents ── */
  const fetchDocs = useCallback(() => {
    setLoading(true);
    setFilter("");
    setCurrentPage(1);
    setVisibleCount(PAGE_SIZE);
    setExpandedIds(new Set());
    setCheckedIds(new Set());
    getDocumentGrid(applId, searchType, categoryList)
      .then((res) => {
        setDocuments((res.documents || []) as unknown as DocRow[]);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [applId, searchType, categoryList]);

  useEffect(() => { fetchDocs(); }, [fetchDocs]);

  /* ── QC rows: which docs have can_qc="y" ── */
  const qcDocIds = useMemo(
    () => new Set(documents.filter((d) => d.can_qc === "y").map((d) => d.document_id)),
    [documents],
  );
  const hasAnyQc = hasQcAccess && qcDocIds.size > 0;

  /* ── Expand all / collapse all ── */
  const allExpanded = hasAnyQc && qcDocIds.size > 0 && [...qcDocIds].every((id) => expandedIds.has(id));

  const toggleExpandAll = useCallback(() => {
    if (allExpanded) {
      setExpandedIds(new Set());
      setCheckedIds(new Set());
    } else {
      setExpandedIds(new Set(qcDocIds));
    }
  }, [allExpanded, qcDocIds]);

  const toggleExpand = useCallback((docId: number) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(docId)) {
        next.delete(docId);
        setCheckedIds((c) => { const nc = new Set(c); nc.delete(docId); return nc; });
      } else {
        next.add(docId);
      }
      return next;
    });
  }, []);

  const toggleCheck = useCallback((docId: number) => {
    setCheckedIds((prev) => {
      const next = new Set(prev);
      if (next.has(docId)) next.delete(docId); else next.add(docId);
      return next;
    });
  }, []);

  /* ── QC action handler ── */
  const handleQcAction = useCallback((act: string, ids: string) => {
    setConfirmAction({ act, ids });
  }, []);

  const executeAction = useCallback(async () => {
    if (!confirmAction) return;
    setActing(true);
    try {
      await docQcAction(confirmAction.act, confirmAction.ids);
      fetchDocs();
    } catch (err) {
      console.error(err);
    } finally {
      setActing(false);
      setConfirmAction(null);
    }
  }, [confirmAction, fetchDocs]);

  /* ── Bulk action: Store Selected / Delete Selected ── */
  const checkedDocs = useMemo(
    () => documents.filter((d) => checkedIds.has(d.document_id)),
    [documents, checkedIds],
  );
  const bulkCanStore = checkedDocs.some((d) => d.can_store === "y");
  const bulkCanDelete = checkedDocs.some((d) => d.can_delete === "y");

  /* ── Filter ── */
  const filteredDocs = useMemo(() => {
    if (!filter.trim()) return documents;
    const lc = filter.toLowerCase();
    return documents.filter((doc) => {
      const name = (doc.document_name || "").toLowerCase();
      const cat = (doc.category_name || "").toLowerCase();
      const sub = (doc.sub_category_name || "").toLowerCase();
      const date = formatDate(doc.document_date || doc.doc_date).toLowerCase();
      return name.includes(lc) || cat.includes(lc) || sub.includes(lc) || date.includes(lc);
    });
  }, [documents, filter]);

  /* ── Sort ── */
  const sortedDocs = useMemo(() => {
    const docs = [...filteredDocs];
    docs.sort((a, b) => {
      let cmp = 0;
      if (sortField === "document_name") {
        cmp = (a.document_name || "").localeCompare(b.document_name || "");
      } else {
        const da = new Date(a.doc_date || a.document_date || "").getTime() || 0;
        const db = new Date(b.doc_date || b.document_date || "").getTime() || 0;
        cmp = da - db;
      }
      return sortDir === "asc" ? cmp : -cmp;
    });
    return docs;
  }, [filteredDocs, sortField, sortDir]);

  /* ── Pagination ── */
  const totalFiltered = sortedDocs.length;
  const totalPages = Math.ceil(totalFiltered / PAGE_SIZE);
  const isShowAll = visibleCount >= totalFiltered && totalFiltered > PAGE_SIZE;
  const visibleDocs = isShowAll
    ? sortedDocs
    : sortedDocs.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  const handleSort = (field: SortField) => {
    if (sortField === field) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else { setSortField(field); setSortDir("asc"); }
  };
  const handleFilterChange = (val: string) => { setFilter(val); setCurrentPage(1); setVisibleCount(PAGE_SIZE); };
  const handleShowAll = () => setVisibleCount(totalFiltered);
  const handlePaginate = () => { setVisibleCount(PAGE_SIZE); setCurrentPage(1); };

  /* ── Column count for detail row colspan ── */
  const colCount = (hasAnyQc ? 1 : 0) + 3 + (hasQcAccess ? 2 : 0);

  return (
    <div className="rounded-xl border border-border bg-white shadow-sm transition-all duration-200 hover:shadow-md">
      {/* ── Confirm dialog ── */}
      {confirmAction && (
        <ConfirmDialog
          message={`Are you sure you want ${confirmAction.act} the selected document(s)?`}
          onConfirm={executeAction}
          onCancel={() => setConfirmAction(null)}
        />
      )}

      {/* ── Header ── */}
      <div className="flex items-center gap-2 bg-[#f8fafc] px-4 py-2 border-b border-border-light flex-wrap">
        <FolderIcon />
        <span className="font-semibold text-sm text-primary">{fullGrantNum}</span>

        {flags.map((f) => (
          <FlagBadge key={f.label} label={f.label} color={f.color} icon={f.icon} title={f.title} small />
        ))}

        {currentLabel && <span className="text-xs text-text-muted italic ml-1">{currentLabel}</span>}

        <div className="flex-1" />

        {/* Filter input */}
        {!loading && documents.length > 0 && (
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

        {/* Action buttons */}
        {canRenameLabel && (
          <div className="relative">
            <button
              type="button"
              onClick={openRenameDialog}
              className="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-700 border border-slate-200 hover:bg-slate-200 transition-colors"
              title={currentLabel ? "Edit Request Name" : "Add Request Name"}
            >
              {currentLabel ? "Edit Request Name" : "Add Request Name"}
            </button>
            {showRenameDialog && (
              <div className="absolute right-0 top-full mt-1 z-30 bg-white rounded-lg shadow-lg border border-border p-3 w-56">
                <label className="block text-[11px] font-semibold text-text-secondary mb-1">Request Name (max 10 chars)</label>
                <input
                  ref={renameInputRef}
                  type="text"
                  maxLength={10}
                  value={renameValue}
                  onChange={(e) => setRenameValue(e.target.value)}
                  onKeyDown={(e) => { if (e.key === "Enter") handleRenameSave(); if (e.key === "Escape") closeRenameDialog(); }}
                  className="w-full rounded border border-border px-2 py-1 text-xs focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
                  disabled={renameSaving}
                />
                <div className="flex items-center gap-1.5 mt-2">
                  <button
                    type="button"
                    onClick={handleRenameSave}
                    disabled={renameSaving}
                    className="px-2 py-0.5 rounded text-[11px] font-semibold bg-primary text-white hover:bg-primary-dark disabled:opacity-50 transition-colors"
                  >
                    Save
                  </button>
                  {currentLabel && (
                    <button
                      type="button"
                      onClick={handleRenameDelete}
                      disabled={renameSaving}
                      className="px-2 py-0.5 rounded text-[11px] font-semibold bg-rose-50 text-rose-700 border border-rose-200 hover:bg-rose-100 disabled:opacity-50 transition-colors"
                    >
                      Delete
                    </button>
                  )}
                  <button
                    type="button"
                    onClick={closeRenameDialog}
                    disabled={renameSaving}
                    className="px-2 py-0.5 rounded text-[11px] font-semibold bg-gray-100 text-text-secondary hover:bg-gray-200 disabled:opacity-50 transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            )}
          </div>
        )}
        {canAddDoc && (
          <button type="button" className="px-2 py-0.5 rounded text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20 hover:bg-primary/20 transition-colors" title="Add Document">
            Add Document
          </button>
        )}
        {canAddFunding && (
          <button type="button" className="px-2 py-0.5 rounded text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 hover:bg-emerald-100 transition-colors" title="Add Funding Document">
            Add Funding Document
          </button>
        )}
      </div>

      {/* ── Bulk actions bar ── */}
      {checkedIds.size > 0 && (
        <div className="flex items-center gap-3 px-4 py-1.5 bg-amber-50 border-b border-amber-200 text-xs">
          <span className="font-semibold text-amber-800">{checkedIds.size} selected</span>
          {bulkCanStore && (
            <button
              type="button"
              disabled={acting}
              onClick={() => handleQcAction("to store all", Array.from(checkedIds).join(","))}
              className="px-2 py-0.5 rounded text-[11px] font-medium bg-blue-50 text-blue-700 border border-blue-200 hover:bg-blue-100 disabled:opacity-50 transition-colors"
            >
              Store Selected
            </button>
          )}
          {bulkCanDelete && (
            <button
              type="button"
              disabled={acting}
              onClick={() => handleQcAction("to delete", Array.from(checkedIds).join(","))}
              className="px-2 py-0.5 rounded text-[11px] font-medium bg-rose-50 text-rose-700 border border-rose-200 hover:bg-rose-100 disabled:opacity-50 transition-colors"
            >
              Delete Selected
            </button>
          )}
          <button
            type="button"
            onClick={() => setCheckedIds(new Set())}
            className="text-[11px] text-text-muted hover:text-text-primary ml-auto"
          >
            Clear
          </button>
        </div>
      )}

      {/* ── Document table ── */}
      <div className="overflow-x-auto">
        {loading && (
          <div className="px-4 py-6 flex items-center justify-center gap-2 text-sm text-text-muted">
            <svg className="animate-spin h-4 w-4 text-primary" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            Loading documents...
          </div>
        )}

        {!loading && documents.length === 0 && (
          <div className="px-4 py-4 text-sm text-text-muted text-center">No documents found.</div>
        )}

        {!loading && documents.length > 0 && totalFiltered === 0 && (
          <div className="px-4 py-4 text-sm text-text-muted text-center">
            No documents match &ldquo;{filter}&rdquo;.
          </div>
        )}

        {!loading && totalFiltered > 0 && (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border-light bg-gray-50/50">
                {/* QC expand-all column */}
                {hasAnyQc && (
                  <th className="px-1 py-2 text-left" style={{ width: 44 }}>
                    {qcDocIds.size > 1 && (
                      <button
                        type="button"
                        onClick={toggleExpandAll}
                        className="p-0.5 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors"
                        title={allExpanded ? "Collapse all" : "Expand all"}
                      >
                        <ToggleIcon open={allExpanded} />
                      </button>
                    )}
                  </th>
                )}
                <th
                  className="px-4 py-2 text-left font-semibold text-text-secondary cursor-pointer select-none"
                  style={{ width: "55%" }}
                  onClick={() => handleSort("document_name")}
                >
                  Document Name
                  <SortIcon field="document_name" sortField={sortField} sortDir={sortDir} />
                </th>
                <th
                  className="px-3 py-2 text-left font-semibold text-text-secondary cursor-pointer select-none whitespace-nowrap"
                  onClick={() => handleSort("document_date")}
                >
                  Date
                  <SortIcon field="document_date" sortField={sortField} sortDir={sortDir} />
                </th>
                <th className="px-3 py-2 text-center font-semibold text-text-secondary">Pages</th>
                {hasQcAccess && (
                  <>
                    <th className="px-3 py-2 text-center font-semibold text-text-secondary">Upload</th>
                    <th className="px-3 py-2 text-center font-semibold text-text-secondary">Update</th>
                  </>
                )}
              </tr>
            </thead>
            <tbody>
              {visibleDocs.map((doc, idx) => {
                const destroyed = Number(doc.frc_destroyed) === 1;
                const rowBg = idx % 2 === 0 ? "bg-white" : "bg-blue-50/40";
                const isQcRow = hasQcAccess && qcDocIds.has(doc.document_id);
                const isExpanded = expandedIds.has(doc.document_id);
                const isChecked = checkedIds.has(doc.document_id);

                return (
                  <Fragment key={doc.document_id}>
                    <tr className={`${rowBg} border-b border-border-light/50 hover:bg-blue-50/60 transition-colors`}>
                      {/* QC chevron + checkbox */}
                      {hasAnyQc && (
                        <td className="px-1 py-1.5 align-middle" style={{ width: 44 }}>
                          {isQcRow && (
                            <div className="flex items-center gap-0.5">
                              <button
                                type="button"
                                onClick={() => toggleExpand(doc.document_id)}
                                className="p-0.5 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors shrink-0"
                                title={isExpanded ? "Collapse" : "Expand"}
                              >
                                <ToggleIcon open={isExpanded} />
                              </button>
                              {isExpanded && doc.can_store === "y" && (
                                <input
                                  type="checkbox"
                                  checked={isChecked}
                                  onChange={() => toggleCheck(doc.document_id)}
                                  className="rounded border-gray-300 text-primary focus:ring-primary/30 h-3 w-3 shrink-0"
                                  title="Select for bulk action"
                                />
                              )}
                            </div>
                          )}
                        </td>
                      )}

                      {/* Document Name */}
                      <td className="px-4 py-1.5">
                        {destroyed ? (
                          <span className="line-through text-text-muted">{doc.document_name}</span>
                        ) : (
                          <a
                            href={doc.url ? getDownloadUrl(doc.document_id) : "#"}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-primary hover:underline"
                            title={doc.document_name}
                          >
                            {doc.document_name}
                          </a>
                        )}
                        {Number(doc.fsr_count) > 1 &&
                          (doc.document_name === "FFR" || doc.document_name === "Financial Report") &&
                          !destroyed && (
                            <span className="ml-2 text-xs text-primary hover:underline cursor-pointer font-medium">All FFR</span>
                          )}
                        {Number(doc.attachment_count) > 0 && !destroyed && (
                          <span className="ml-2 text-xs text-primary hover:underline cursor-pointer font-medium">Attachments</span>
                        )}
                      </td>

                      {/* Date */}
                      <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">
                        {formatDate(doc.document_date || doc.doc_date)}
                      </td>

                      {/* Pages */}
                      <td className="px-3 py-1.5 text-center text-text-primary">{doc.page_count || ""}</td>

                      {/* Upload */}
                      {hasQcAccess && (
                        <td className="px-3 py-1.5 text-center">
                          {doc.can_upload === "y" && !destroyed && (
                            <button type="button" className="p-0.5 rounded text-primary hover:text-primary-dark hover:bg-blue-50 transition-colors" title="Replace Document">
                              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
                              </svg>
                            </button>
                          )}
                        </td>
                      )}

                      {/* Update */}
                      {hasQcAccess && (
                        <td className="px-3 py-1.5 text-center">
                          {doc.can_modify_index === "y" && !destroyed && (
                            <button type="button" className="p-0.5 rounded text-primary hover:text-primary-dark hover:bg-blue-50 transition-colors" title="Update Document">
                              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                              </svg>
                            </button>
                          )}
                        </td>
                      )}
                    </tr>

                    {/* Expanded detail row */}
                    {isExpanded && (
                      <tr className={rowBg}>
                        {hasAnyQc && <td className="p-0" />}
                        <td colSpan={colCount - (hasAnyQc ? 1 : 0)} className="p-0">
                          <DocDetail doc={doc} onAction={handleQcAction} acting={acting} checkedIds={checkedIds} />
                        </td>
                      </tr>
                    )}
                  </Fragment>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* ── Footer: pagination + count ── */}
      {!loading && totalFiltered > 0 && (
        <div className="px-4 py-1.5 border-t border-border-light flex items-center gap-2 text-[11px] text-text-muted flex-wrap">
          <span>
            {isShowAll
              ? `Showing all ${totalFiltered}`
              : `Showing ${(currentPage - 1) * PAGE_SIZE + 1}–${Math.min(currentPage * PAGE_SIZE, totalFiltered)} of ${totalFiltered}`}
            {" "}document{totalFiltered !== 1 ? "s" : ""}
            {filter && ` (${documents.length} total)`}
          </span>

          <div className="flex-1" />

          {!isShowAll && totalPages > 1 && (
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

          {totalFiltered > PAGE_SIZE && (
            <button type="button" onClick={isShowAll ? handlePaginate : handleShowAll} className="inline-flex items-center gap-1 text-primary hover:underline font-medium ml-1">
              {isShowAll ? "Paginate" : "Show all"}
            </button>
          )}
        </div>
      )}
    </div>
  );
}
