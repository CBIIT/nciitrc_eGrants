"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import type { GrantResult, ApplicationResult } from "@/lib/types";
import {
  SupplementIcon, UmbrellaIcon, RocketIcon, GovernmentIcon,
  FlaskIcon, UsersIcon, StopIcon, BuildingIcon, FlagBadge,
} from "./FlagIcons";

interface GrantCardProps {
  grant: GrantResult;
  applications: ApplicationResult[];
  docCounts: Record<string, unknown>[];
  grantView?: boolean;
  onFlagClick?: (flagType: string) => void;
  activeFlag?: string | null;
}

/* ── helper: get field value with fallback ── */
function gv(obj: Record<string, unknown>, ...keys: string[]): string {
  for (const k of keys) {
    if (obj[k] != null && String(obj[k]).trim() !== "") return String(obj[k]);
  }
  return "";
}


/* ── Folder icon ── */
function FolderIcon() {
  return (
    <svg className="h-4 w-4 text-primary/60 shrink-0" fill="currentColor" viewBox="0 0 20 20">
      <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
    </svg>
  );
}

/* ── Main GrantCard ── */
export default function GrantCard({ grant, applications, docCounts, grantView, onFlagClick, activeFlag }: GrantCardProps) {
  const router = useRouter();
  const [yearsInput, setYearsInput] = useState("");
  const [showAllYears, setShowAllYears] = useState(false);

  const g = grant as unknown as Record<string, unknown>;
  const firstAppl = applications[0] as unknown as Record<string, unknown> | undefined;

  // Grant number: all_activity_code + admin_phs_org_code + serial_num
  const activityCode = gv(g, "all_activity_code", "activity_code");
  const adminCode = gv(g, "admin_phs_org_code");
  const serialNum = gv(g, "serial_num");
  const grantNum = (activityCode || adminCode || serialNum)
    ? `${activityCode}${adminCode}${serialNum}`
    : gv(g, "full_grant_num", "grant_num") || (firstAppl && gv(firstAppl, "full_grant_num")) || `Grant #${grant.grant_id}`;

  // Project title
  const projectTitle =
    applications.reduce((title, a) => {
      if (title) return title;
      const ar = a as unknown as Record<string, unknown>;
      return gv(ar, "project_title", "ProjectTitle", "title");
    }, "") || gv(g, "project_title", "ProjectTitle", "title");

  const piName = gv(g, "current_pi_name", "pi_name", "PI");
  const piEmail = gv(g, "current_pi_email_address", "pi_email");
  const pdName = gv(g, "current_pd_name", "pd_name", "ProgramDirector");
  const pdEmail = gv(g, "current_pd_email_address", "pd_email");
  const specName = gv(g, "current_spec_name", "spec_name", "Specialist");
  const specEmail = gv(g, "current_spec_email_address", "spec_email");
  const orgName = gv(g, "org_name", "organization");
  const programCode = gv(g, "prog_class_code", "program_code", "code");

  // Flags
  const suppCount = Number(gv(g, "adm_supp", "supplement")) || 0;
  const hasSupplement = suppCount > 0;
  const hasDS = gv(g, "ds_flag") === "y";
  const hasMoonshot = gv(g, "ms_flag") === "y";
  const hasOD = gv(g, "od_flag") === "y";
  // MPI — old code first used SP column is_MPI ("y"/"n"), later used MPIContacts list from IRDB
  // Old cshtml: @if (grant.MPIContacts != null && grant.MPIContacts.Count > 1)
  // SP still returns is_MPI column; check that as primary, MPIContacts as fallback
  const mpiFlag = g.is_MPI ?? g.is_mpi ?? g.mpi_flag;
  const mpiRaw = g.MPIContacts ?? g.mpi_contacts;
  const hasMPI = mpiFlag === "y" || mpiFlag === "Y" || mpiFlag === true || mpiFlag === 1
    || (Array.isArray(mpiRaw) ? mpiRaw.length > 1 : false);
  const hasFDA = gv(g, "fda_flag") === "y";
  const hasStopNotice = gv(g, "stop_flag") === "y";
  // Institutional — SP returns institutional_flag1 and institutional_flag2 as int (0/1) or BIT
  // Old code: if (grant.institutional_flag1 == true) show inst_icon_flag
  //           else if (grant.AnyOrgDoc == true) show inst_icon   (AnyOrgDoc = institutional_flag2)
  // Old C#: grant.institutional_flag1 = value.institutional_flag1.ToString() == "1" ? true : false;
  //         grant.AnyOrgDoc = value.institutional_flag2.ToString() == "1" ? true : false;
  const instFlag1 = g.institutional_flag1;
  const instFlag2 = g.institutional_flag2;
  const isInstFlag1 = instFlag1 != null && instFlag1 !== 0 && instFlag1 !== false && instFlag1 !== "0";
  const isInstFlag2 = instFlag2 != null && instFlag2 !== 0 && instFlag2 !== false && instFlag2 !== "0";
  const hasInstitutional = isInstFlag1 || isInstFlag2;
  const instOrgSuffix = orgName ? ` for ${orgName}` : "";
  const instTitle = isInstFlag1
    ? `Follow-Up File Present, View Institutional File(s)${instOrgSuffix}`
    : `View Institutional File(s)${instOrgSuffix}`;

  // Build flags array (order: Supplement, DS, Moonshot, OD, MPI, FDA, Stop, Institutional)
  // flagType: used by onFlagClick to filter grant years; null = not filterable
  const flags: { label: string; color: string; icon: React.ReactNode; title: string; flagType: string | null }[] = [];
  // Tooltips match old system cshtml titles
  if (hasSupplement) flags.push({ label: "Supplement", color: "blue", icon: <SupplementIcon />, title: "View Supplement Files", flagType: "Supplement" });  // handled specially — not a year filter
  if (hasDS) flags.push({ label: "Diversity", color: "purple", icon: <UmbrellaIcon />, title: "Diversity Supplement Funded Grant", flagType: "DS" });
  if (hasMoonshot) flags.push({ label: "Moonshot", color: "amber", icon: <RocketIcon />, title: "Moonshot Funded Grant", flagType: "MS" });
  if (hasOD) flags.push({ label: "OD Funded", color: "emerald", icon: <GovernmentIcon />, title: "OD Funded Grant", flagType: "OD" });
  if (hasMPI) flags.push({ label: "MPI", color: "cyan", icon: <UsersIcon />, title: "Has Multi-PI Grant Year(s)", flagType: null });
  if (hasFDA) flags.push({ label: "FDA", color: "rose", icon: <FlaskIcon />, title: "FDA Grant", flagType: "FDA" });
  if (hasStopNotice) flags.push({ label: "Stop Notice", color: "rose", icon: <StopIcon />, title: `View Stop Notice for ${grantNum}`, flagType: null });
  if (hasInstitutional) flags.push({ label: "Institutional", color: "slate", icon: <BuildingIcon />, title: instTitle, flagType: "Institutional" });

  // Years — preserve SP return order (old system did not sort)
  const years = applications
    .map((a) => a.support_year)
    .filter(Boolean);
  const MAX_YEARS_SHOWN = 12;
  const visibleYears = showAllYears ? years : years.slice(0, MAX_YEARS_SHOWN);

  const labelClass = "text-[13px] font-bold text-text-secondary whitespace-nowrap";

  return (
    <div className="grant-card rounded-xl border border-border bg-white shadow-sm transition-all duration-200 hover:shadow-md">
      {/* ── Header: [folder] GrantNum ... [optional badges] | [All] [# Grant Years] [Yrs] [filter] [search] ── */}
      <div className="flex items-center gap-2 bg-[#f8fafc] px-4 py-2 border-b border-border-light">
        {/* Left: grant number */}
        <FolderIcon />
        <span className="font-semibold text-sm text-primary shrink-0">{grantNum}</span>

        {/* Spacer pushes everything else to the right */}
        <div className="flex-1" />

        {/* Optional flag badges — clickable everywhere */}
        {flags.map((f) => {
          let handleClick: (() => void) | undefined;
          if (f.flagType) {
            if (f.flagType === "Institutional") {
              // Institutional always navigates to the institutional page
              const orgId = gv(g, "OrgId", "org_id", "orgid");
              handleClick = () => router.push(`/institutional${orgId ? `?org_id=${orgId}` : ""}`);
            } else if (grantView && onFlagClick) {
              // Already in grant view — filter years in-place
              handleClick = () => onFlagClick(f.flagType!);
            } else if (!grantView) {
              // Search results — navigate to grant view with flag pre-filter
              handleClick = () => router.push(`/search?grant_id=${grant.grant_id}&flag=${f.flagType}`);
            }
          }
          return (
            <FlagBadge
              key={f.label}
              label={f.label}
              color={f.color}
              icon={f.icon}
              title={f.flagType === "Institutional" ? f.title : (f.flagType ? `Filter by ${f.label}` : f.title)}
              onClick={handleClick}
              active={activeFlag === f.flagType}
            />
          );
        })}

        {/* Fixed action buttons (hidden in grant view — the Grant Years grid handles this) */}
        {!grantView && (
          <>
            <button
              type="button"
              onClick={() => router.push(`/search?grant_id=${grant.grant_id}`)}
              className="px-2 py-0.5 rounded text-[11px] font-semibold bg-primary text-white hover:bg-primary-dark transition-colors"
              title="View all categories and years"
            >
              All
            </button>
            <span className="text-[10px] text-text-muted whitespace-nowrap"># of Grant Years</span>
            <input
              type="text"
              value={yearsInput}
              onChange={(e) => setYearsInput(e.target.value)}
              placeholder="Yrs"
              className="w-10 rounded border border-border px-1 py-0.5 text-[11px] text-center focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
              title="# of Grant Years"
            />
            <button
              type="button"
              onClick={() => router.push(`/search?grant_id=${grant.grant_id}&view=categories`)}
              className="p-1 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors"
              title="Filter by categories"
            >
              <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 01-.659 1.591l-5.432 5.432a2.25 2.25 0 00-.659 1.591v2.927a2.25 2.25 0 01-1.244 2.013L9.75 21v-6.568a2.25 2.25 0 00-.659-1.591L3.659 7.409A2.25 2.25 0 013 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0112 3z" />
              </svg>
            </button>
          </>
        )}
      </div>

      {/* ── Body: project title + grant details ── */}
      <div className="px-4 py-3 text-sm" style={{ display: "grid", gridTemplateColumns: "140px 1fr", gap: "5px 8px", alignItems: "baseline" }}>
        {projectTitle && (
          <>
            <span className={labelClass}>Project Title:</span>
            <span className="text-text-primary uppercase" title={projectTitle}>{projectTitle.length > 60 ? projectTitle.slice(0, 60) + "..." : projectTitle}</span>
          </>
        )}

        {piName && (
          <>
            <span className={labelClass}>PI:</span>
            <span className="text-text-primary">
              {piEmail ? (
                <a href={`mailto:${piEmail}`} className="text-primary hover:underline">{piName}</a>
              ) : piName}
              {orgName && <span className="text-text-muted ml-2">({orgName})</span>}
            </span>
          </>
        )}

        {pdName && (
          <>
            <span className={labelClass}>Program Director:</span>
            <span className="text-text-primary">
              {pdEmail ? (
                <a href={`mailto:${pdEmail}`} className="text-primary hover:underline">{pdName}</a>
              ) : pdName}
              {programCode && <span className="text-text-muted ml-2"><span className="font-bold">Code:</span> {programCode}</span>}
            </span>
          </>
        )}

        {specName && (
          <>
            <span className={labelClass}>Specialist:</span>
            <span className="text-text-primary">
              {specEmail ? (
                <a href={`mailto:${specEmail}`} className="text-primary hover:underline">{specName}</a>
              ) : specName}
            </span>
          </>
        )}

        {years.length > 0 && (
          <>
            <span className={labelClass}>Years:</span>
            <div className="flex flex-wrap gap-x-1 gap-y-0.5 items-center">
              {visibleYears.map((yr, idx) => (
                <Link
                  key={yr}
                  href={`/documents?grant_id=${grant.grant_id}&year=${yr}`}
                  className={`inline-flex items-center justify-center min-w-[28px] py-0.5 rounded text-xs font-medium text-primary hover:bg-blue-50 hover:underline transition-colors${idx === 0 ? "" : " pl-1"}`}
                >
                  {yr}
                </Link>
              ))}
              {years.length > MAX_YEARS_SHOWN && !showAllYears && (
                <button
                  type="button"
                  onClick={() => setShowAllYears(true)}
                  className="inline-flex items-center justify-center w-6 h-6 rounded-full text-xs font-bold text-primary bg-blue-50 hover:bg-blue-100 transition-colors"
                  title="Show all years"
                >
                  +
                </button>
              )}
              {showAllYears && years.length > MAX_YEARS_SHOWN && (
                <button
                  type="button"
                  onClick={() => setShowAllYears(false)}
                  className="inline-flex items-center justify-center w-6 h-6 rounded-full text-xs font-bold text-primary bg-blue-50 hover:bg-blue-100 transition-colors"
                  title="Show fewer years"
                >
                  −
                </button>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
