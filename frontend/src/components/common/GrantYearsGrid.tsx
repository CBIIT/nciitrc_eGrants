"use client";

import { useState, useCallback, type ReactNode } from "react";
import type { ApplicationResult } from "@/lib/types";
import {
  UmbrellaIcon, RocketIcon, GovernmentIcon, FlaskIcon, StopIcon, CompetingIcon, FlagBadge,
} from "./FlagIcons";

interface GrantYearsGridProps {
  applications: ApplicationResult[];
  selectedApplIds: Set<number>;
  onSelectionChange: (selected: Set<number>) => void;
}

const MAX_DEFAULT = 12;

/** Competing year: SP returns competing="yes" when appl_type_code IN (1,2,6,9) */
function isCompeting(a: Record<string, unknown>): boolean {
  // Primary: SP returns "yes"/"no" string in `competing` column
  if (String(a.competing ?? "").toLowerCase() === "yes") return true;
  // Fallback: derive from appl_type_code (same logic as vw_appls CASE expression)
  const tc = Number(a.appl_type_code);
  return tc === 1 || tc === 2 || tc === 6 || tc === 9;
}

/* ── Per-year flag badges (same icons/colors as GrantCard) ── */
function YearFlags({ appl }: { appl: ApplicationResult }) {
  const a = appl as unknown as Record<string, unknown>;
  const flags: { label: string; color: string; icon: ReactNode; title: string }[] = [];
  if (isCompeting(a)) flags.push({ label: "C", color: "blue", icon: <CompetingIcon />, title: "Competing Years" });
  if (String(a.appl_ds_flag ?? a.ds_flag ?? "") === "y") flags.push({ label: "DS", color: "purple", icon: <UmbrellaIcon />, title: "Diversity Supplement" });
  if (String(a.appl_fda_flag ?? a.fda_flag ?? "") === "y") flags.push({ label: "FDA", color: "rose", icon: <FlaskIcon />, title: "FDA Grant" });
  if (String(a.appl_ms_flag ?? a.ms_flag ?? "") === "y") flags.push({ label: "MS", color: "amber", icon: <RocketIcon />, title: "Moonshot Funded" });
  if (String(a.appl_od_flag ?? a.od_flag ?? "") === "y") flags.push({ label: "OD", color: "emerald", icon: <GovernmentIcon />, title: "OD Funded" });
  if (appl.deleted_by_impac === "y" || appl.deleted_by_impac === "Y") flags.push({ label: "DEL", color: "rose", icon: <StopIcon />, title: "Deleted by IMPAC" });
  if (!flags.length) return null;
  return (
    <span className="ml-1 inline-flex gap-0.5">
      {flags.map((f) => (
        <FlagBadge key={f.label} label={f.label} color={f.color} icon={f.icon} title={f.title} small />
      ))}
    </span>
  );
}

export default function GrantYearsGrid({ applications, selectedApplIds, onSelectionChange }: GrantYearsGridProps) {
  const [expanded, setExpanded] = useState(false);

  const hasMore = applications.length > MAX_DEFAULT;
  const visible = expanded ? applications : applications.slice(0, MAX_DEFAULT);
  const allSelected = applications.length > 0 && applications.every((a) => selectedApplIds.has(a.appl_id));

  const toggleSelectAll = useCallback(() => {
    if (allSelected) {
      onSelectionChange(new Set());
    } else {
      onSelectionChange(new Set(applications.map((a) => a.appl_id)));
    }
  }, [allSelected, applications, onSelectionChange]);

  const toggleOne = useCallback((applId: number) => {
    const next = new Set(selectedApplIds);
    if (next.has(applId)) next.delete(applId);
    else next.add(applId);
    onSelectionChange(next);
  }, [selectedApplIds, onSelectionChange]);

  const handleExpand = () => setExpanded(true);
  const handleCollapse = () => setExpanded(false);

  // Distribute items into 3 columns vertically (old system pattern)
  const perCol = Math.ceil(visible.length / 3);
  const col1 = visible.slice(0, perCol);
  const col2 = visible.slice(perCol, perCol * 2);
  const col3 = visible.slice(perCol * 2);

  return (
    <div className="rounded-lg border border-border bg-white shadow-sm overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between bg-[#f8fafc] px-4 py-2 border-b border-border-light">
        <div className="flex items-center gap-3">
          <span className="text-sm font-semibold text-text-primary">Grant Years</span>
          <span className="text-xs text-text-muted">
            ({applications.length} total{selectedApplIds.size > 0 ? `, ${selectedApplIds.size} selected` : ""})
          </span>
        </div>
        {hasMore && (
          <button
            type="button"
            onClick={expanded ? handleCollapse : handleExpand}
            className="inline-flex items-center justify-center w-6 h-6 rounded-full text-xs font-bold text-primary bg-blue-50 hover:bg-blue-100 transition-colors"
            title={expanded ? "Show only 12 grant years" : "View all grant years"}
          >
            {expanded ? "−" : "+"}
          </button>
        )}
      </div>

      {/* Select All */}
      <div className="px-4 pt-2 pb-1 border-b border-border-light">
        <label className="inline-flex items-center gap-2 cursor-pointer text-sm">
          <input
            type="checkbox"
            checked={allSelected}
            onChange={toggleSelectAll}
            className="rounded border-gray-300 text-primary focus:ring-primary/30"
          />
          <span className="font-semibold text-text-secondary">Select All</span>
        </label>
      </div>

      {/* 3-column grid */}
      <div className="grid grid-cols-3 gap-x-4 px-4 py-2">
        {[col1, col2, col3].map((col, ci) => (
          <div key={ci} className="flex flex-col gap-0.5">
            {col.map((appl) => (
              <label key={appl.appl_id} className="inline-flex items-center gap-1.5 cursor-pointer py-0.5 rounded hover:bg-blue-50/50 px-1 -mx-1 transition-colors">
                <input
                  type="checkbox"
                  checked={selectedApplIds.has(appl.appl_id)}
                  onChange={() => toggleOne(appl.appl_id)}
                  className="rounded border-gray-300 text-primary focus:ring-primary/30 shrink-0"
                />
                <span className="text-xs text-primary font-medium truncate">
                  {appl.full_grant_num || appl.support_year || `#${appl.appl_id}`}
                </span>
                <YearFlags appl={appl} />
              </label>
            ))}
          </div>
        ))}
      </div>

      {/* Footer: Show more / Show less */}
      {hasMore && (
        <div className="px-4 pb-2 flex items-center gap-3 text-[11px] text-text-muted">
          {!expanded && (
            <>
              Showing {MAX_DEFAULT} of {applications.length} years.{" "}
              <button type="button" onClick={handleExpand} className="inline-flex items-center gap-1 text-primary hover:underline font-medium">
                <span className="inline-flex items-center justify-center w-4 h-4 rounded-full bg-blue-50 text-[10px] font-bold leading-none">+</span>
                Show more
              </button>
            </>
          )}
          {expanded && (
            <button type="button" onClick={handleCollapse} className="inline-flex items-center gap-1 text-primary hover:underline font-medium">
              <span className="inline-flex items-center justify-center w-4 h-4 rounded-full bg-blue-50 text-[10px] font-bold leading-none">−</span>
              Show less
            </button>
          )}
        </div>
      )}
      {/* Selected Category line (matches legacy "Selected Category: All") */}
      {selectedApplIds.size > 0 && (
        <div className="px-4 py-1.5 border-t border-border-light text-sm">
          <span className="font-bold text-text-secondary">Selected Category:</span>{" "}
          <span className="text-text-primary">All</span>
        </div>
      )}
    </div>
  );
}
