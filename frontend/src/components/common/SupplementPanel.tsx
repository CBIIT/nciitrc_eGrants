"use client";

import { useState, useEffect, useCallback } from "react";
import { useAuth } from "@/hooks/useAuth";
import { getSupplement, getDownloadUrl } from "@/lib/api";

interface SupplementRow {
  tag: number;
  id: number;
  full_grant_num: string;
  former_num: string;
  former_appl_id: number;
  supp_appl_id: number;
  support_year: string;
  suffix_code: string;
  category_name: string;
  sub_category_name: string;
  submitted_date: string;
  date_of_submitted: string;
  status: string;
  moved_date: string;
  moved_by: string;
  accession_number: string;
  url: string;
  document_id: number;
  [key: string]: unknown;
}

interface SupplementPanelProps {
  grantId: number;
  onClose: () => void;
}

function formatDate(dateStr: string): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}/${d.getFullYear()}`;
}

export default function SupplementPanel({ grantId, onClose }: SupplementPanelProps) {
  const { user } = useAuth();
  const [rows, setRows] = useState<SupplementRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [view, setView] = useState<"current" | "history">("current");

  const positionId = user?.position_id ?? 0;
  const hasQcAccess = positionId >= 2;

  const fetchData = useCallback((act: string) => {
    setLoading(true);
    getSupplement(grantId, act)
      .then((data) => {
        // SP returns rows with tag field; tag=2 is the detail rows
        const filtered = (data as unknown as SupplementRow[]).filter((r) => r.tag === 2);
        setRows(filtered);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [grantId]);

  useEffect(() => {
    fetchData(view === "current" ? "to_view" : "to_history");
  }, [view, fetchData]);

  const handleRefresh = () => fetchData(view === "current" ? "to_view" : "to_history");

  return (
    <div className="rounded-xl border border-border bg-white shadow-sm transition-all duration-200">
      {/* Header */}
      <div className="flex items-center gap-2 bg-blue-50 px-4 py-2 border-b border-blue-200">
        <svg className="h-4 w-4 text-blue-600 shrink-0" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
        </svg>
        <span className="font-semibold text-sm text-blue-800">Supplement Requests</span>

        <div className="flex-1" />

        {/* View toggle */}
        <div className="flex items-center rounded-lg border border-blue-200 overflow-hidden text-[11px] font-medium">
          <button
            type="button"
            onClick={() => setView("current")}
            className={`px-3 py-1 transition-colors ${view === "current" ? "bg-blue-600 text-white" : "bg-white text-blue-700 hover:bg-blue-50"}`}
          >
            Current
          </button>
          <button
            type="button"
            onClick={() => setView("history")}
            className={`px-3 py-1 transition-colors ${view === "history" ? "bg-blue-600 text-white" : "bg-white text-blue-700 hover:bg-blue-50"}`}
          >
            History
          </button>
        </div>

        {/* Refresh */}
        <button
          type="button"
          onClick={handleRefresh}
          className="p-1 rounded text-blue-600 hover:bg-blue-100 transition-colors"
          title="Refresh"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0l3.181 3.183a8.25 8.25 0 0013.803-3.7M4.031 9.865a8.25 8.25 0 0113.803-3.7l3.181 3.182" />
          </svg>
        </button>

        {/* Close */}
        <button
          type="button"
          onClick={onClose}
          className="p-1 rounded text-blue-600 hover:bg-blue-100 transition-colors"
          title="Close"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        {loading && (
          <div className="px-4 py-6 flex items-center justify-center gap-2 text-sm text-text-muted">
            <svg className="animate-spin h-4 w-4 text-primary" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            Loading supplements...
          </div>
        )}

        {!loading && rows.length === 0 && (
          <div className="px-4 py-4 text-sm text-text-muted text-center">No supplement requests found.</div>
        )}

        {!loading && rows.length > 0 && (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border-light bg-gray-50/50">
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Parent Grant</th>
                <th className="px-3 py-2 text-center font-semibold text-text-secondary">Year</th>
                <th className="px-3 py-2 text-center font-semibold text-text-secondary">Suffix</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Category</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Submitted</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Status</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Moved Date</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Moved By</th>
                <th className="px-3 py-2 text-left font-semibold text-text-secondary">Accession #</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row, idx) => {
                const rowBg = idx % 2 === 0 ? "bg-white" : "bg-blue-50/40";
                const catDisplay = [row.category_name, row.sub_category_name].filter(Boolean).join(" - ");
                const submittedDate = formatDate(row.submitted_date || row.date_of_submitted);
                const hasDoc = row.url || row.document_id;

                return (
                  <tr key={row.id || idx} className={`${rowBg} border-b border-border-light/50 hover:bg-blue-50/60 transition-colors`}>
                    <td className="px-3 py-1.5 text-text-primary">{row.former_num}</td>
                    <td className="px-3 py-1.5 text-center text-text-primary">{row.support_year}</td>
                    <td className="px-3 py-1.5 text-center text-text-primary">{row.suffix_code}</td>
                    <td className="px-3 py-1.5">
                      {hasDoc ? (
                        <a
                          href={row.document_id ? getDownloadUrl(row.document_id) : row.url}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-primary hover:underline"
                        >
                          {catDisplay || "Document"}
                        </a>
                      ) : (
                        <span className="text-text-primary">{catDisplay}</span>
                      )}
                    </td>
                    <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">{submittedDate}</td>
                    <td className="px-3 py-1.5 text-text-primary">{row.status}</td>
                    <td className="px-3 py-1.5 whitespace-nowrap text-text-primary">{formatDate(row.moved_date)}</td>
                    <td className="px-3 py-1.5 text-text-primary">{row.moved_by}</td>
                    <td className="px-3 py-1.5 text-text-primary">{row.accession_number}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Footer */}
      {!loading && rows.length > 0 && (
        <div className="px-4 py-1.5 border-t border-border-light text-[11px] text-text-muted">
          {rows.length} supplement{rows.length !== 1 ? "s" : ""}
        </div>
      )}
    </div>
  );
}
