"use client";

import { useState, useEffect, useCallback } from "react";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import ConfirmDialog from "@/components/common/ConfirmDialog";
import {
  getQcReasons,
  getSpecialists,
  getQcPersons,
  getQcReport,
  qcAssign,
  getDocTransactionReport,
  getAccessions,
  getSystemReport,
} from "@/lib/api";
import type { ColumnDef } from "@tanstack/react-table";

// ---------------------------------------------------------------------------
// Spinner
// ---------------------------------------------------------------------------

function Spinner({ label = "Loading..." }: { label?: string }) {
  return (
    <div className="flex items-center gap-2 text-sm text-text-muted py-3">
      <svg
        className="animate-spin h-4 w-4 text-primary"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
      >
        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
      </svg>
      {label}
    </div>
  );
}

// ---------------------------------------------------------------------------
// useConfirm hook — replaces window.confirm / window.alert
// ---------------------------------------------------------------------------

interface ConfirmState {
  open: boolean;
  title: string;
  message: string;
  confirmLabel: string;
  variant: "danger" | "primary";
  resolve: ((ok: boolean) => void) | null;
}

function useConfirm() {
  const [state, setState] = useState<ConfirmState>({
    open: false,
    title: "Confirm",
    message: "",
    confirmLabel: "Confirm",
    variant: "primary",
    resolve: null,
  });

  function confirm(
    message: string,
    opts?: { title?: string; confirmLabel?: string; variant?: "danger" | "primary" },
  ): Promise<boolean> {
    return new Promise((resolve) => {
      setState({
        open: true,
        message,
        title: opts?.title ?? "Confirm",
        confirmLabel: opts?.confirmLabel ?? "Confirm",
        variant: opts?.variant ?? "primary",
        resolve,
      });
    });
  }

  function notify(message: string, title = "Notice") {
    return new Promise<boolean>((resolve) => {
      setState({
        open: true,
        message,
        title,
        confirmLabel: "OK",
        variant: "primary",
        resolve,
      });
    });
  }

  function handleConfirm() {
    state.resolve?.(true);
    setState((s) => ({ ...s, open: false, resolve: null }));
  }

  function handleCancel() {
    state.resolve?.(false);
    setState((s) => ({ ...s, open: false, resolve: null }));
  }

  const dialog = (
    <ConfirmDialog
      open={state.open}
      title={state.title}
      message={state.message}
      confirmLabel={state.confirmLabel}
      variant={state.variant}
      onConfirm={handleConfirm}
      onCancel={handleCancel}
    />
  );

  return { confirm, notify, dialog };
}

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface QcPerson {
  qc_reason: string;
  userid: string;
  person_id: string;
  person_name: string;
}

interface QcReportRow {
  qc_person_name: string;
  files_to_qc: string;
  qc_days: string;
  qc_person_id: string;
}

interface DocTxRow {
  full_grant_num: string;
  category_name: string;
  person_name: string;
  transaction_date: string;
  url?: string;
}

interface FolderRow {
  bar_code: string;
  id_string: string;
  current_status: string;
  latest_move_date: string;
  accession_destroyed_date: string;
}

interface Specialist {
  person_id: string;
  person_name: string;
}

interface Accession {
  accession_id: string;
  accession_number: string;
}

// ---------------------------------------------------------------------------
// Column definitions
// ---------------------------------------------------------------------------

const docTxCols: ColumnDef<DocTxRow, unknown>[] = [
  { accessorKey: "full_grant_num", header: "Grant Number" },
  {
    accessorKey: "category_name",
    header: "Category Name",
    cell: ({ row }) => {
      const url = row.original.url;
      const name = row.original.category_name;
      if (!url) return name ?? "";
      return (
        <a
          href={url}
          target="_blank"
          rel="noopener noreferrer"
          className="text-primary hover:underline"
        >
          {name}
        </a>
      );
    },
  },
  { accessorKey: "person_name", header: "Transaction By" },
  {
    accessorKey: "transaction_date",
    header: "Transaction Date",
    cell: ({ getValue }) => {
      const val = getValue() as string | null;
      return val ?? "";
    },
  },
];

const folderCols: ColumnDef<FolderRow, unknown>[] = [
  { accessorKey: "bar_code", header: "Folder" },
  { accessorKey: "id_string", header: "Grant Year" },
  { accessorKey: "current_status", header: "Location/Box" },
  { accessorKey: "latest_move_date", header: "Last Moved in Date" },
  { accessorKey: "accession_destroyed_date", header: "Accession Destroyed Date" },
];

// ---------------------------------------------------------------------------
// Tabs
// ---------------------------------------------------------------------------

const TABS = [
  "QC Assignment Report",
  "Document Transaction Report",
  "System Report",
] as const;
type TabName = (typeof TABS)[number];

// ---------------------------------------------------------------------------
// Page
// ---------------------------------------------------------------------------

export default function ManagementPage() {
  const { user, loading } = useAuth();
  const [activeTab, setActiveTab] = useState<TabName>("QC Assignment Report");

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    );
  }
  if (!user) return null;

  return (
    <AppShell user={user}>
      {/* Tab navigation */}
      <div className="mb-4 flex gap-1 border-b border-border">
        {TABS.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 text-sm font-medium transition-colors ${
              activeTab === tab
                ? "border-b-2 border-primary text-primary"
                : "text-text-secondary hover:text-text-primary"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {activeTab === "QC Assignment Report" && <QcAssignmentTab />}
      {activeTab === "Document Transaction Report" && <DocTransactionTab />}
      {activeTab === "System Report" && <SystemReportTab />}
    </AppShell>
  );
}

// ===========================================================================
// Tab 1: QC Assignment Report
// ===========================================================================

function QcAssignmentTab() {
  const { confirm, notify, dialog } = useConfirm();

  const [qcReasons, setQcReasons] = useState<string[]>([]);
  const [specialists, setSpecialists] = useState<Specialist[]>([]);
  const [qcPersons, setQcPersons] = useState<QcPerson[]>([]);
  const [qcReport, setQcReport] = useState<QcReportRow[]>([]);

  // Per-section loading
  const [loadingAssignment, setLoadingAssignment] = useState(true);
  const [loadingReport, setLoadingReport] = useState(true);

  // Assignment form
  const [selReason, setSelReason] = useState("");
  const [selQcPerson, setSelQcPerson] = useState("");
  const [assigning, setAssigning] = useState(false);

  // Route form
  const [fromPerson, setFromPerson] = useState("");
  const [toPerson, setToPerson] = useState("");
  const [percent, setPercent] = useState("");
  const [routing, setRouting] = useState(false);

  const refreshAssignment = useCallback(async () => {
    Promise.all([getQcReasons(), getSpecialists(), getQcPersons()])
      .then(([reasons, specs, persons]) => {
        setQcReasons(reasons.map((r) => r.qc_reason as string));
        setSpecialists(specs as unknown as Specialist[]);
        setQcPersons(persons as unknown as QcPerson[]);
      })
      .catch((err) => console.error("qc-assignment data:", err))
      .finally(() => setLoadingAssignment(false));
  }, []);

  const refreshReport = useCallback(async () => {
    getQcReport()
      .then((data) => setQcReport(data as unknown as QcReportRow[]))
      .catch((err) => console.error("qc-report:", err))
      .finally(() => setLoadingReport(false));
  }, []);

  useEffect(() => {
    refreshAssignment();
    refreshReport();
  }, [refreshAssignment, refreshReport]);

  async function handleAssign() {
    if (!selReason) {
      await notify("Please select a QC reason to assign.", "Missing Selection");
      return;
    }
    if (!selQcPerson) {
      await notify("Please select a QC person to assign.", "Missing Selection");
      return;
    }
    const person = specialists.find((s) => String(s.person_id) === selQcPerson);
    const ok = await confirm(
      `Are you sure that you want to assign QC ${selReason} documents to ${person?.person_name ?? selQcPerson}?`,
      { title: "Assign QC", confirmLabel: "Assign" },
    );
    if (!ok) return;

    setAssigning(true);
    try {
      await qcAssign({
        act: "to_assign",
        qc_person_id: Number(selQcPerson),
        qc_reason: selReason,
      });
      await refreshAssignment();
      setSelReason("");
      setSelQcPerson("");
    } catch (err) {
      console.error(err);
    } finally {
      setAssigning(false);
    }
  }

  async function handleRemove(qcReason: string, personId: string, personName: string) {
    const ok = await confirm(
      `Are you sure that you want to remove ${personName} from list for QC ${qcReason} documents?`,
      { title: "Remove Assignment", confirmLabel: "Remove", variant: "danger" },
    );
    if (!ok) return;
    try {
      await qcAssign({
        act: "to_remove",
        qc_person_id: Number(personId),
        qc_reason: qcReason,
      });
      await refreshAssignment();
    } catch (err) {
      console.error(err);
    }
  }

  async function handleRoute() {
    if (!fromPerson) {
      await notify("Please select the person whose documents will be routed out.", "Missing Selection");
      return;
    }
    if (!percent) {
      await notify("Please select the percent of documents to route.", "Missing Selection");
      return;
    }
    if (!toPerson) {
      await notify("Please select the person whom documents will route to.", "Missing Selection");
      return;
    }
    const from = specialists.find((s) => String(s.person_id) === fromPerson);
    const to = specialists.find((s) => String(s.person_id) === toPerson);
    const ok = await confirm(
      `Are you sure that you want to route ${percent}% QC documents from ${from?.person_name ?? fromPerson} to ${to?.person_name ?? toPerson}?`,
      { title: "Route Documents", confirmLabel: "Route" },
    );
    if (!ok) return;

    setRouting(true);
    try {
      await qcAssign({
        act: "to_route",
        person_id: Number(fromPerson),
        qc_person_id: Number(toPerson),
        percent: Number(percent),
      });
      await refreshReport();
      setFromPerson("");
      setToPerson("");
      setPercent("");
    } catch (err) {
      console.error(err);
    } finally {
      setRouting(false);
    }
  }

  const qcPersonColsWithAction: ColumnDef<QcPerson, unknown>[] = [
    { accessorKey: "qc_reason", header: "QC Reason" },
    { accessorKey: "person_name", header: "Person Name" },
    {
      id: "actions",
      header: "",
      enableSorting: false,
      cell: ({ row }) => (
        <button
          type="button"
          className="text-sm text-red-600 hover:underline"
          onClick={() =>
            handleRemove(row.original.qc_reason, row.original.person_id, row.original.person_name)
          }
        >
          Remove
        </button>
      ),
    },
  ];

  const qcReportCols: ColumnDef<QcReportRow, unknown>[] = [
    { accessorKey: "qc_person_name", header: "Specialist Name" },
    { accessorKey: "files_to_qc", header: "Documents to QC" },
    { accessorKey: "qc_days", header: "Days in QC" },
  ];

  return (
    <div className="space-y-6">
      {dialog}

      {/* QC Assignment Report */}
      <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-border-light bg-[#f9fafb]">
          <h3 className="text-sm font-bold text-text-primary">QC Assignment Report</h3>
          <p className="mt-1 text-xs text-text-muted">
            Select QC reason and assign it to a particular specialist.
          </p>
        </div>
        <div className="px-5 py-4">
          {loadingAssignment ? (
            <Spinner label="Loading QC assignments..." />
          ) : (
            <div className="flex flex-wrap items-end gap-3">
              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">QC Reason:</label>
                <select
                  value={selReason}
                  onChange={(e) => setSelReason(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  {qcReasons.map((r) => (
                    <option key={r} value={r}>{r}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">QC Person:</label>
                <select
                  value={selQcPerson}
                  onChange={(e) => setSelQcPerson(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  {specialists.map((s) => (
                    <option key={s.person_id} value={s.person_id}>{s.person_name}</option>
                  ))}
                </select>
              </div>
              <button
                type="button"
                onClick={handleAssign}
                disabled={assigning}
                className="btn-primary whitespace-nowrap"
              >
                {assigning ? "Assigning..." : "Assign"}
              </button>
            </div>
          )}
        </div>
        {loadingAssignment ? null : (
          <DataTable data={qcPersons} columns={qcPersonColsWithAction} emptyMessage="No QC assignments found." initialSorting={[{ id: "qc_reason", desc: false }]} />
        )}
      </div>

      {/* QC Route Report */}
      <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-border-light bg-[#f9fafb]">
          <h3 className="text-sm font-bold text-text-primary">QC Route Report</h3>
          <p className="mt-1 text-xs text-text-muted">
            Select specialist name and percent of documents, route them to another specialist for QC.
          </p>
        </div>
        <div className="px-5 py-4">
          {loadingAssignment ? (
            <Spinner label="Loading specialists..." />
          ) : (
            <div className="flex flex-wrap items-end gap-3">
              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">From:</label>
                <select
                  value={fromPerson}
                  onChange={(e) => setFromPerson(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  {specialists.map((s) => (
                    <option key={s.person_id} value={s.person_id}>{s.person_name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">To:</label>
                <select
                  value={toPerson}
                  onChange={(e) => setToPerson(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  {specialists.map((s) => (
                    <option key={s.person_id} value={s.person_id}>{s.person_name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-gray-600">Percent:</label>
                <select
                  value={percent}
                  onChange={(e) => setPercent(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  <option value="100">100%</option>
                  <option value="50">50%</option>
                  <option value="25">25%</option>
                  <option value="10">10%</option>
                </select>
              </div>
              <button
                type="button"
                onClick={handleRoute}
                disabled={routing}
                className="btn-primary whitespace-nowrap"
              >
                {routing ? "Routing..." : "Route"}
              </button>
            </div>
          )}
        </div>
        {loadingReport ? (
          <Spinner label="Loading QC route report..." />
        ) : (
          <DataTable data={qcReport} columns={qcReportCols} initialSorting={[{ id: "qc_person_name", desc: false }]} />
        )}
      </div>
    </div>
  );
}

// ===========================================================================
// Tab 2: Document Transaction Report
// ===========================================================================

function DocTransactionTab() {
  const [specialists, setSpecialists] = useState<Specialist[]>([]);
  const [loadingSpecs, setLoadingSpecs] = useState(true);
  const [personId, setPersonId] = useState("");
  const [transactionType, setTransactionType] = useState("");
  const [dateRange, setDateRange] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [showCustomDates, setShowCustomDates] = useState(false);
  const [docs, setDocs] = useState<DocTxRow[]>([]);
  const [searching, setSearching] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);
  const [touched, setTouched] = useState(false);

  useEffect(() => {
    getSpecialists()
      .then((data) => setSpecialists(data as unknown as Specialist[]))
      .catch((err) => console.error("specialists:", err))
      .finally(() => setLoadingSpecs(false));
  }, []);

  function validate(): boolean {
    setTouched(true);
    return !!personId && !!transactionType;
  }

  async function handleDateRangeChange(value: string) {
    setDateRange(value);
    if (!value) return;
    if (!validate()) return;

    setSearching(true);
    setHasSearched(true);
    try {
      const data = await getDocTransactionReport(transactionType, Number(personId), {
        dateRange: value,
      });
      setDocs(data as unknown as DocTxRow[]);
    } catch (err) {
      console.error(err);
    } finally {
      setSearching(false);
    }
  }

  async function handleViewReport() {
    if (!validate()) return;
    if (!startDate || !endDate) {
      setTouched(true);
      return;
    }
    if (startDate > endDate) {
      setTouched(true);
      return;
    }

    setSearching(true);
    setHasSearched(true);
    try {
      const data = await getDocTransactionReport(transactionType, Number(personId), {
        startDate,
        endDate,
      });
      setDocs(data as unknown as DocTxRow[]);
    } catch (err) {
      console.error(err);
    } finally {
      setSearching(false);
    }
  }

  const errorBorder = "ring-2 ring-red-400";

  return (
    <div className="space-y-4">
      {/* Controls panel */}
      <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-border-light bg-[#f9fafb]">
          <p className="text-xs text-text-muted">
            Select user and transaction type, then search by date range or custom dates.
          </p>
        </div>

        <div className="px-5 py-4 space-y-4">
          {loadingSpecs ? (
            <Spinner label="Loading users..." />
          ) : (
            <>
              <div className="flex flex-wrap items-end gap-4">
                <div>
                  <label className="mb-1 block text-xs font-medium text-gray-600">
                    <span className="text-red-500">*</span> User
                  </label>
                  <select
                    value={personId}
                    onChange={(e) => setPersonId(e.target.value)}
                    className={`input-modern ${touched && !personId ? errorBorder : ""}`}
                  >
                    <option value=""></option>
                    <option value="0">NCI</option>
                    {specialists.map((s) => (
                      <option key={s.person_id} value={s.person_id}>
                        {s.person_name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="mb-1 block text-xs font-medium text-gray-600">
                    <span className="text-red-500">*</span> Transaction Type
                  </label>
                  <select
                    value={transactionType}
                    onChange={(e) => setTransactionType(e.target.value)}
                    className={`input-modern ${touched && !transactionType ? errorBorder : ""}`}
                  >
                    <option value=""></option>
                    <option value="created">created</option>
                    <option value="deleted">deleted</option>
                    <option value="image modified">image modified</option>
                    <option value="index modified">index modified</option>
                    <option value="stored">stored</option>
                  </select>
                </div>
                <div>
                  <label className="mb-1 block text-xs font-medium text-gray-600">Date Range</label>
                  <select
                    value={dateRange}
                    onChange={(e) => handleDateRangeChange(e.target.value)}
                    className="input-modern"
                  >
                    <option value=""></option>
                    <option value="today">Today</option>
                    <option value="last_week">Last Week</option>
                    <option value="last_month">Last Month</option>
                    <option value="this_week">This Week</option>
                    <option value="this_month">This Month</option>
                  </select>
                </div>
                <button
                  type="button"
                  onClick={() => setShowCustomDates(!showCustomDates)}
                  className="text-sm font-medium text-primary hover:underline pb-1"
                >
                  {showCustomDates ? "Hide custom dates" : "Use custom date range"}
                </button>
              </div>

              {showCustomDates && (
                <div className="flex flex-wrap items-end gap-4">
                  <div>
                    <label className="mb-1 block text-xs font-medium text-gray-600">
                      <span className="text-red-500">*</span> From
                    </label>
                    <input
                      type="date"
                      value={startDate}
                      onChange={(e) => setStartDate(e.target.value)}
                      className={`input-modern ${touched && showCustomDates && !startDate ? errorBorder : ""}`}
                    />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-gray-600">
                      <span className="text-red-500">*</span> To
                    </label>
                    <input
                      type="date"
                      value={endDate}
                      onChange={(e) => setEndDate(e.target.value)}
                      className={`input-modern ${touched && showCustomDates && (!endDate || (startDate && endDate && startDate > endDate)) ? errorBorder : ""}`}
                    />
                  </div>
                  <button
                    type="button"
                    onClick={handleViewReport}
                    disabled={searching}
                    className="btn-primary"
                  >
                    {searching ? "Searching..." : "View Report"}
                  </button>
                </div>
              )}

              {touched && showCustomDates && startDate && endDate && startDate > endDate && (
                <p className="text-xs text-red-500">End date must be later than start date.</p>
              )}
            </>
          )}
        </div>
      </div>

      {searching && <Spinner label="Loading report..." />}

      {!searching && docs.length > 0 && (
        <DataTable data={docs} columns={docTxCols} pageSize={50} />
      )}

      {!searching && hasSearched && docs.length === 0 && (
        <p className="text-sm text-text-muted">No documents found.</p>
      )}
    </div>
  );
}

// ===========================================================================
// Tab 3: System Report
// ===========================================================================

function SystemReportTab() {
  const { notify, dialog } = useConfirm();

  const [accessions, setAccessions] = useState<Accession[]>([]);
  const [loadingAccessions, setLoadingAccessions] = useState(true);
  const [serialNumber, setSerialNumber] = useState("");
  const [accessionId, setAccessionId] = useState("");
  const [folders, setFolders] = useState<FolderRow[]>([]);
  const [searching, setSearching] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);

  useEffect(() => {
    getAccessions()
      .then((data) => setAccessions(data as unknown as Accession[]))
      .catch((err) => console.error("accessions:", err))
      .finally(() => setLoadingAccessions(false));
  }, []);

  async function handleSerialSearch() {
    if (!serialNumber) {
      await notify("Please enter a serial number to search.", "Missing Input");
      return;
    }
    if (!/^\d+$/.test(serialNumber)) {
      await notify("Please enter a valid numeric serial number.", "Invalid Input");
      setSerialNumber("");
      return;
    }

    setSearching(true);
    setHasSearched(true);
    try {
      const data = await getSystemReport("by_serialnumber", Number(serialNumber));
      setFolders(data as unknown as FolderRow[]);
    } catch (err) {
      console.error(err);
    } finally {
      setSearching(false);
    }
  }

  async function handleAccessionChange(value: string) {
    setAccessionId(value);
    if (!value) return;

    setSearching(true);
    setHasSearched(true);
    try {
      const data = await getSystemReport("by_accessionid", Number(value));
      setFolders(data as unknown as FolderRow[]);
    } catch (err) {
      console.error(err);
    } finally {
      setSearching(false);
    }
  }

  return (
    <div className="space-y-4">
      {dialog}

      {/* Controls panel */}
      <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b border-border-light bg-[#f9fafb]">
          <p className="text-xs text-text-muted">
            Search by serial number or select an accession number.
          </p>
        </div>

        <div className="px-5 py-4">
          <div className="flex flex-wrap items-end gap-4">
            {/* Serial Number */}
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-600">Serial Number</label>
              <input
                type="text"
                value={serialNumber}
                onChange={(e) => setSerialNumber(e.target.value)}
                placeholder="Serial Number Only"
                className="input-modern"
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleSerialSearch();
                }}
              />
            </div>
            <button
              type="button"
              onClick={handleSerialSearch}
              disabled={searching}
              className="btn-primary"
            >
              {searching ? "Searching..." : "Search"}
            </button>

            {/* OR divider */}
            <div className="flex items-center self-stretch pb-1">
              <div className="h-full w-px bg-border" />
              <span className="px-3 text-xs font-semibold uppercase tracking-wider text-text-muted">or</span>
              <div className="h-full w-px bg-border" />
            </div>

            {/* Accession Number */}
            <div>
              <label className="mb-1 block text-xs font-medium text-gray-600">Accession Number</label>
              {loadingAccessions ? (
                <Spinner label="Loading accessions..." />
              ) : (
                <select
                  value={accessionId}
                  onChange={(e) => handleAccessionChange(e.target.value)}
                  className="input-modern"
                >
                  <option value=""></option>
                  {accessions.map((a) => (
                    <option key={a.accession_id} value={a.accession_id}>
                      {a.accession_number}
                    </option>
                  ))}
                </select>
              )}
            </div>
          </div>
        </div>
      </div>

      {searching && <Spinner label="Loading system report..." />}

      {!searching && folders.length > 0 && (
        <DataTable data={folders} columns={folderCols} pageSize={50} />
      )}

      {!searching && hasSearched && folders.length === 0 && (
        <p className="text-sm text-text-muted">No documents found.</p>
      )}
    </div>
  );
}
