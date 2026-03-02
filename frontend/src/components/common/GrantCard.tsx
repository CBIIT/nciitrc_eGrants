"use client";

import { useState } from "react";
import Link from "next/link";
import type { GrantResult, ApplicationResult } from "@/lib/types";

interface GrantCardProps {
  grant: GrantResult;
  applications: ApplicationResult[];
  docCounts: Record<string, unknown>[];
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

/* ── Flag icons ── */
function SupplementIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" /></svg>);
}
function UmbrellaIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M12 3v1.5M12 21v-3m0 0a2.25 2.25 0 01-2.25-2.25M12 18a2.25 2.25 0 002.25-2.25M12 3C7.029 3 3 7.029 3 12h18c0-4.971-4.029-9-9-9z" /></svg>);
}
function RocketIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M15.59 14.37a6 6 0 01-5.84 7.38v-4.8m5.84-2.58a14.98 14.98 0 006.16-12.12A14.98 14.98 0 009.631 8.41m5.96 5.96a14.926 14.926 0 01-5.841 2.58m-.119-8.54a6 6 0 00-7.381 5.84h4.8m2.581-5.84a14.927 14.927 0 00-2.58 5.84m2.699 2.7c-.103.021-.207.041-.311.06a15.09 15.09 0 01-2.448-2.448 14.9 14.9 0 01.06-.312m-2.24 2.39a4.493 4.493 0 00-1.757 4.306 4.493 4.493 0 004.306-1.758M16.5 9a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z" /></svg>);
}
function GovernmentIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M12 21v-8.25M15.75 21v-8.25M8.25 21v-8.25M3 9l9-6 9 6m-1.5 12V10.332A48.36 48.36 0 0012 9.75c-2.551 0-5.056.2-7.5.582V21M3 21h18M12 6.75h.008v.008H12V6.75z" /></svg>);
}
function FlaskIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0112 15a9.065 9.065 0 00-6.23.693L5 14.5m14.8.8l1.402 1.402c1.232 1.232.65 3.318-1.067 3.611A48.309 48.309 0 0112 21c-2.773 0-5.491-.235-8.135-.687-1.718-.293-2.3-2.379-1.067-3.61L5 14.5" /></svg>);
}
function UsersIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" /></svg>);
}
function StopIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" /></svg>);
}
function BuildingIcon() {
  return (<svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" d="M3.75 21h16.5M4.5 3h15M5.25 3v18m13.5-18v18M9 6.75h1.5m-1.5 3h1.5m-1.5 3h1.5m3-6H15m-1.5 3H15m-1.5 3H15M9 21v-3.375c0-.621.504-1.125 1.125-1.125h3.75c.621 0 1.125.504 1.125 1.125V21" /></svg>);
}

/* ── Badge pill with icon ── */
function FlagBadge({ label, color, icon, title }: { label: string; color: string; icon: React.ReactNode; title?: string }) {
  const colorMap: Record<string, string> = {
    blue: "bg-blue-50 text-blue-700 border-blue-200",
    amber: "bg-amber-50 text-amber-700 border-amber-200",
    rose: "bg-rose-50 text-rose-700 border-rose-200",
    emerald: "bg-emerald-50 text-emerald-700 border-emerald-200",
    purple: "bg-purple-50 text-purple-700 border-purple-200",
    slate: "bg-slate-50 text-slate-600 border-slate-200",
    cyan: "bg-cyan-50 text-cyan-700 border-cyan-200",
  };
  return (
    <span title={title} className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded-full text-[10px] font-semibold border cursor-pointer transition-opacity hover:opacity-80 ${colorMap[color] || colorMap.slate}`}>
      {icon}
      {label}
    </span>
  );
}

/* ── Main GrantCard ── */
export default function GrantCard({ grant, applications, docCounts }: GrantCardProps) {
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
  const flags: { label: string; color: string; icon: React.ReactNode; title: string }[] = [];
  // Tooltips match old system cshtml titles
  if (hasSupplement) flags.push({ label: "Supplement", color: "blue", icon: <SupplementIcon />, title: "View Supplement Files" });
  if (hasDS) flags.push({ label: "Diversity", color: "purple", icon: <UmbrellaIcon />, title: "Diversity Supplement Funded Grant" });
  if (hasMoonshot) flags.push({ label: "Moonshot", color: "amber", icon: <RocketIcon />, title: "Moonshot Funded Grant" });
  if (hasOD) flags.push({ label: "OD Funded", color: "emerald", icon: <GovernmentIcon />, title: "OD Funded Grant" });
  if (hasMPI) flags.push({ label: "MPI", color: "cyan", icon: <UsersIcon />, title: "Has Multi-PI Grant Year(s)" });
  if (hasFDA) flags.push({ label: "FDA", color: "rose", icon: <FlaskIcon />, title: "FDA Grant" });
  if (hasStopNotice) flags.push({ label: "Stop Notice", color: "rose", icon: <StopIcon />, title: `View Stop Notice for ${grantNum}` });
  if (hasInstitutional) flags.push({ label: "Institutional", color: "slate", icon: <BuildingIcon />, title: instTitle });

  // Years — preserve SP return order (old system did not sort)
  const years = applications
    .map((a) => a.support_year)
    .filter(Boolean);
  const MAX_YEARS_SHOWN = 12;
  const visibleYears = showAllYears ? years : years.slice(0, MAX_YEARS_SHOWN);

  const labelClass = "text-[13px] font-bold text-text-secondary whitespace-nowrap";

  return (
    <div className="grant-card rounded-xl border border-border bg-white shadow-sm overflow-hidden transition-all duration-200 hover:shadow-md">
      {/* ── Header: [folder] GrantNum ... [optional badges] | [All] [# Grant Years] [Yrs] [filter] [search] ── */}
      <div className="flex items-center gap-2 bg-[#f8fafc] px-4 py-2 border-b border-border-light">
        {/* Left: grant number */}
        <FolderIcon />
        <span className="font-semibold text-sm text-primary shrink-0">{grantNum}</span>

        {/* Spacer pushes everything else to the right */}
        <div className="flex-1" />

        {/* Optional flag badges (left of fixed buttons) */}
        {flags.map((f) => (
          <FlagBadge key={f.label} label={f.label} color={f.color} icon={f.icon} title={f.title} />
        ))}

        {/* Fixed action buttons (far right) */}
        <button
          type="button"
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
          className="p-1 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors"
          title="Filter categories"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 01-.659 1.591l-5.432 5.432a2.25 2.25 0 00-.659 1.591v2.927a2.25 2.25 0 01-1.244 2.013L9.75 21v-6.568a2.25 2.25 0 00-.659-1.591L3.659 7.409A2.25 2.25 0 013 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0112 3z" />
          </svg>
        </button>
        <button
          type="button"
          className="p-1 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors"
          title="Search"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
          </svg>
        </button>
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
