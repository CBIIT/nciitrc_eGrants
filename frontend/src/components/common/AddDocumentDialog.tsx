"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { Dialog, DialogPanel, DialogTitle, DialogBackdrop } from "@headlessui/react";
import {
  getAllApplsList,
  getLookupCategories,
  getSubCategories,
  createDocument,
  uploadDocumentFile,
  createGrantYear,
} from "@/lib/api";
import type { ApplicationResult, Category, SubCategory } from "@/lib/types";

/* ── Types ── */

interface AddDocumentDialogProps {
  application: ApplicationResult;
  onClose: () => void;
  onSuccess: () => void;
}

interface ApplOption {
  full_grant_num: string;
  appl_id: number;
}

const ALLOWED_EXTENSIONS = ".pdf,.doc,.docx,.msg,.rtf,.jpg,.png,.gif,.tif,.html,.htm,.log,.dat,.txt";

const APPL_TYPE_CODES: Record<number, string> = {
  1: "New",
  2: "Competing Renewal",
  3: "Non-Competing Continuation",
  4: "Competing Supplement",
  5: "Non-Competing Supplement",
  7: "Change of Grantee Institution",
  9: "Administrative Supplement",
};

/* ── Helpers ── */

/** Parse admin code and serial number from a full grant number like "5R01CA125123-04". */
function parseGrantNum(fullGrantNum: string): { adminCode: string; serialNum: string; activityCode: string } {
  // Format: {type_digit}{mechanism_letter}{mechanism_digits}{admin_code}{serial}-{year}{suffix}
  // e.g. "5R01CA125123-04" → activity="R01", admin="CA", serial="125123"
  const m = fullGrantNum.match(/^\d([A-Z]\d{2})([A-Z]{2})(\d+)/);
  if (m) return { activityCode: m[1], adminCode: m[2], serialNum: m[3] };
  return { activityCode: "", adminCode: "", serialNum: "" };
}

function todayStr(): string {
  const d = new Date();
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}/${d.getFullYear()}`;
}

function isValidDate(s: string): boolean {
  const m = s.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
  if (!m) return false;
  const d = new Date(Number(m[3]), Number(m[1]) - 1, Number(m[2]));
  return d.getMonth() === Number(m[1]) - 1 && d.getDate() === Number(m[2]);
}

function isFutureDate(s: string): boolean {
  const m = s.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
  if (!m) return false;
  const d = new Date(Number(m[3]), Number(m[1]) - 1, Number(m[2]));
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return d > today;
}

function getFileExtension(name: string): string {
  const idx = name.lastIndexOf(".");
  return idx >= 0 ? name.substring(idx).toLowerCase() : "";
}

/* ── Create Grant Year Sub-Dialog ── */

function CreateGrantYearDialog({
  grantId,
  adminCode,
  serialNum,
  defaultActivityCode,
  applsList,
  onCreated,
  onCancel,
}: {
  grantId: number;
  adminCode: string;
  serialNum: string;
  defaultActivityCode: string;
  applsList: ApplOption[];
  onCreated: (appl: ApplOption) => void;
  onCancel: () => void;
}) {
  const [typeCode, setTypeCode] = useState(3);
  const [activityCode, setActivityCode] = useState(defaultActivityCode);
  const [year, setYear] = useState("");
  const [suffixCode, setSuffixCode] = useState("");
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState("");

  const handleCreate = async () => {
    if (!activityCode.trim()) { setError("Activity code is required"); return; }
    if (!year.trim()) { setError("Year is required"); return; }

    setCreating(true);
    setError("");
    try {
      const result = await createGrantYear({
        grant_id: grantId,
        appl_type_code: typeCode,
        activity_code: activityCode,
        admin_code: adminCode,
        serial_num: serialNum,
        support_year: year,
        suffix_code: suffixCode,
      });
      onCreated({ appl_id: result.appl_id, full_grant_num: result.full_grant_num });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create grant year");
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="mt-3 rounded-lg border border-blue-200 bg-blue-50/50 p-3 space-y-2.5">
      <div className="flex items-center justify-between">
        <h4 className="text-xs font-semibold text-text-primary">Create Grant Year</h4>
        <button type="button" onClick={onCancel} className="text-text-muted hover:text-text-primary transition-colors">
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <p className="text-[10px] text-text-muted leading-relaxed">
        This is designed to force entry of appls that cannot be found in IMPAC.
      </p>

      <div className="grid grid-cols-2 gap-2">
        {/* Type */}
        <div>
          <label className="block text-[10px] font-semibold text-text-secondary mb-0.5">Type</label>
          <select
            value={typeCode}
            onChange={(e) => setTypeCode(Number(e.target.value))}
            className="w-full rounded border border-border px-2 py-1 text-xs focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
          >
            {Object.entries(APPL_TYPE_CODES).map(([code, name]) => (
              <option key={code} value={code}>{code} - {name}</option>
            ))}
          </select>
        </div>

        {/* Activity Code */}
        <div>
          <label className="block text-[10px] font-semibold text-text-secondary mb-0.5">Activity Code</label>
          <input
            type="text"
            value={activityCode}
            onChange={(e) => setActivityCode(e.target.value.toUpperCase())}
            maxLength={5}
            placeholder="e.g. R01"
            className="w-full rounded border border-border px-2 py-1 text-xs focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
          />
        </div>

        {/* Grant identifier (read-only) */}
        <div>
          <label className="block text-[10px] font-semibold text-text-secondary mb-0.5">Admin + Serial</label>
          <input
            type="text"
            value={`${adminCode}${serialNum}`}
            readOnly
            className="w-full rounded border border-border px-2 py-1 text-xs bg-gray-50 text-text-muted"
          />
        </div>

        {/* Year */}
        <div>
          <label className="block text-[10px] font-semibold text-text-secondary mb-0.5">Year</label>
          <input
            type="text"
            value={year}
            onChange={(e) => setYear(e.target.value)}
            maxLength={5}
            placeholder="e.g. 04"
            className="w-full rounded border border-border px-2 py-1 text-xs focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
          />
        </div>

        {/* Suffix Code */}
        <div className="col-span-2">
          <label className="block text-[10px] font-semibold text-text-secondary mb-0.5">Suffix Code</label>
          <input
            type="text"
            value={suffixCode}
            onChange={(e) => setSuffixCode(e.target.value.toUpperCase())}
            maxLength={10}
            placeholder="e.g. A1 (optional)"
            className="w-full rounded border border-border px-2 py-1 text-xs focus:border-primary focus:ring-1 focus:ring-primary/20 outline-none"
          />
        </div>
      </div>

      {error && <p className="text-[10px] text-red-600">{error}</p>}

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={handleCreate}
          disabled={creating}
          className="px-3 py-1 rounded text-xs font-semibold bg-primary text-white hover:bg-primary/90 disabled:opacity-50 transition-colors"
        >
          {creating ? "Creating..." : "Create New"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={creating}
          className="px-3 py-1 rounded text-xs font-semibold bg-gray-100 text-text-secondary hover:bg-gray-200 disabled:opacity-50 transition-colors"
        >
          Cancel
        </button>
      </div>

      {/* Existing grant years for reference */}
      {applsList.length > 0 && (
        <div>
          <p className="text-[10px] font-semibold text-text-secondary mb-0.5">Existing grant years:</p>
          <ul className="text-[10px] text-text-muted space-y-0.5 max-h-20 overflow-y-auto">
            {applsList.map((a) => (
              <li key={a.appl_id}>{a.full_grant_num}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

/* ── Main Component ── */

export default function AddDocumentDialog({ application, onClose, onSuccess }: AddDocumentDialogProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Parsed from context
  const parsed = parseGrantNum(application.full_grant_num || "");
  const adminCode = parsed.adminCode;

  // Form state
  const [serialNum, setSerialNum] = useState(parsed.serialNum);
  const [applsList, setApplsList] = useState<ApplOption[]>([]);
  const [selectedApplId, setSelectedApplId] = useState<number>(0);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number>(0);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [subCategories, setSubCategories] = useState<SubCategory[]>([]);
  const [subCategoryValue, setSubCategoryValue] = useState("");
  const [documentDate, setDocumentDate] = useState(todayStr());
  const [file, setFile] = useState<File | null>(null);
  const [dragOver, setDragOver] = useState(false);

  // Create grant year sub-dialog
  const [showCreateGrantYear, setShowCreateGrantYear] = useState(false);

  // UI state
  const [loadingAppls, setLoadingAppls] = useState(false);
  const [loadingSubCats, setLoadingSubCats] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState("");

  // Load categories on mount
  useEffect(() => {
    getLookupCategories()
      .then(setCategories)
      .catch(console.error);
  }, []);

  // Auto-search appls list on mount if we have parsed values
  useEffect(() => {
    if (adminCode && parsed.serialNum) {
      handleSearchAppls(adminCode, parsed.serialNum);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSearchAppls = useCallback(
    (ac?: string, sn?: string) => {
      const code = ac ?? adminCode;
      const serial = sn ?? serialNum;
      if (!code || !serial) return;
      setLoadingAppls(true);
      getAllApplsList(code, serial)
        .then((list) => {
          setApplsList(list);
          setSelectedApplId(0);
        })
        .catch(console.error)
        .finally(() => setLoadingAppls(false));
    },
    [adminCode, serialNum],
  );

  // Load sub-categories when category changes
  useEffect(() => {
    const cat = categories.find((c) => c.category_id === selectedCategoryId) || null;
    setSelectedCategory(cat);
    setSubCategoryValue("");
    setSubCategories([]);

    if (!cat) return;

    if (cat.input_type === "D" && selectedCategoryId > 0) {
      setLoadingSubCats(true);
      getSubCategories(selectedCategoryId)
        .then(setSubCategories)
        .catch(console.error)
        .finally(() => setLoadingSubCats(false));
    }
  }, [selectedCategoryId, categories]);

  // Grant year dropdown change handler
  const handleGrantYearChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    const val = e.target.value;
    if (val === "__create__") {
      setShowCreateGrantYear(true);
    } else {
      setSelectedApplId(Number(val));
      setShowCreateGrantYear(false);
    }
  }, []);

  // Handle new grant year created
  const handleGrantYearCreated = useCallback((newAppl: ApplOption) => {
    setApplsList((prev) => [newAppl, ...prev]);
    setSelectedApplId(newAppl.appl_id);
    setShowCreateGrantYear(false);
  }, []);

  // Validation
  const validate = useCallback((): Record<string, string> => {
    const errs: Record<string, string> = {};

    if (!serialNum.trim()) errs.serialNum = "Serial number is required";
    else if (!/^\d+$/.test(serialNum.trim())) errs.serialNum = "Serial number must be numeric";

    if (!selectedApplId || applsList.length === 0) errs.grantYear = "Grant year must be selected";

    if (!selectedCategoryId) errs.category = "Category is required";

    if (selectedCategory?.input_constraint === 1 && !subCategoryValue.trim()) {
      errs.subCategory = "Sub-category is required";
    }

    if (!documentDate.trim()) errs.documentDate = "Document date is required";
    else if (!isValidDate(documentDate)) errs.documentDate = "Invalid date (MM/DD/YYYY)";
    else if (isFutureDate(documentDate)) errs.documentDate = "Date cannot be in the future";

    if (!file) errs.file = "File is required";
    else {
      const ext = getFileExtension(file.name);
      const allowed = ALLOWED_EXTENSIONS.split(",");
      if (!allowed.includes(ext)) errs.file = `File type "${ext}" is not allowed`;
    }

    return errs;
  }, [serialNum, selectedApplId, applsList, selectedCategoryId, selectedCategory, subCategoryValue, documentDate, file]);

  // Submit
  const handleSubmit = useCallback(async () => {
    const errs = validate();
    setErrors(errs);
    if (Object.keys(errs).length > 0) return;

    setSubmitting(true);
    setSubmitError("");

    try {
      const ext = file ? getFileExtension(file.name) : "";

      // Step 1: Create document record
      const { document_id } = await createDocument({
        appl_id: selectedApplId,
        category_id: selectedCategoryId,
        sub_category: subCategoryValue,
        document_date: documentDate,
        file_type: ext,
      });

      if (!document_id) throw new Error("Failed to create document record");

      // Step 2: Upload file
      await uploadDocumentFile(document_id, file!);

      // Step 3: Success
      onSuccess();
      onClose();
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setSubmitting(false);
    }
  }, [validate, file, selectedApplId, selectedCategoryId, subCategoryValue, documentDate, onSuccess, onClose]);

  // Drag & drop handlers
  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile) {
      setFile(droppedFile);
      setErrors((prev) => {
        const next = { ...prev };
        delete next.file;
        return next;
      });
    }
  }, []);

  const handleFileSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (selected) {
      setFile(selected);
      setErrors((prev) => {
        const next = { ...prev };
        delete next.file;
        return next;
      });
    }
  }, []);

  return (
    <Dialog open onClose={onClose} className="relative z-50">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-black/30 transition-opacity duration-200 data-closed:opacity-0"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel
          transition
          className="w-full max-w-lg rounded-xl bg-white shadow-xl transition-all duration-200 data-closed:scale-95 data-closed:opacity-0 max-h-[90vh] overflow-y-auto"
        >
          <div className="p-6">
            <DialogTitle className="text-base font-semibold text-text-primary">
              Add New Document
            </DialogTitle>
            <p className="mt-1 text-xs text-text-muted">
              {application.full_grant_num}
            </p>

            {/* ── PII/PHI Reminder ── */}
            <div className="mt-3 rounded-lg bg-amber-50 border border-amber-200 px-3 py-2 text-xs text-amber-800">
              <strong>Reminder:</strong> Sensitive Personally Identifiable Information (PII) includes
              Social Security Numbers, financial account numbers, and medical records. Documents containing
              sensitive PII or PHI must not be uploaded into eGrants.
            </div>

            <div className="mt-4 space-y-3">
              {/* ── Institute (read-only) ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">Institute</label>
                <input
                  type="text"
                  value={adminCode}
                  readOnly
                  className="w-full rounded-md border border-border bg-gray-50 px-2.5 py-1.5 text-sm text-text-muted cursor-default"
                />
              </div>

              {/* ── Serial Number + Search ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">Serial Number</label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={serialNum}
                    onChange={(e) => setSerialNum(e.target.value)}
                    placeholder="e.g. 125123"
                    className={`flex-1 rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.serialNum ? "border-red-400" : "border-border"}`}
                  />
                  <button
                    type="button"
                    onClick={() => handleSearchAppls()}
                    disabled={!adminCode || !serialNum || loadingAppls}
                    className="px-3 py-1.5 rounded-md text-xs font-semibold bg-primary text-white hover:bg-primary/90 disabled:opacity-50 transition-colors"
                  >
                    {loadingAppls ? "Searching..." : "Search"}
                  </button>
                </div>
                {errors.serialNum && <p className="mt-0.5 text-xs text-red-600">{errors.serialNum}</p>}
              </div>

              {/* ── Grant Year ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">Grant Year</label>
                <select
                  value={showCreateGrantYear ? "__create__" : selectedApplId}
                  onChange={handleGrantYearChange}
                  className={`w-full rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.grantYear ? "border-red-400" : "border-border"}`}
                >
                  <option value="0">-- Select grant year --</option>
                  {applsList.map((a) => (
                    <option key={a.appl_id} value={a.appl_id}>{a.full_grant_num}</option>
                  ))}
                  <option value="__create__">-- Create grant year --</option>
                </select>
                {errors.grantYear && <p className="mt-0.5 text-xs text-red-600">{errors.grantYear}</p>}

                {/* Create Grant Year sub-dialog */}
                {showCreateGrantYear && (
                  <CreateGrantYearDialog
                    grantId={application.grant_id ?? 0}
                    adminCode={adminCode}
                    serialNum={serialNum}
                    defaultActivityCode={parsed.activityCode}
                    applsList={applsList}
                    onCreated={handleGrantYearCreated}
                    onCancel={() => {
                      setShowCreateGrantYear(false);
                      // Reset dropdown to first appl if available
                      if (applsList.length > 0) setSelectedApplId(applsList[0].appl_id);
                    }}
                  />
                )}
              </div>

              {/* ── Category ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">Category</label>
                <select
                  value={selectedCategoryId}
                  onChange={(e) => setSelectedCategoryId(Number(e.target.value))}
                  className={`w-full rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.category ? "border-red-400" : "border-border"}`}
                >
                  <option value={0}>-- Select Category --</option>
                  {categories.map((c) => (
                    <option key={c.category_id} value={c.category_id}>{c.category_name}</option>
                  ))}
                </select>
                {errors.category && <p className="mt-0.5 text-xs text-red-600">{errors.category}</p>}
              </div>

              {/* ── Sub-Category (dynamic) ── */}
              {selectedCategory && (selectedCategory.input_type === "T" || selectedCategory.input_type === "D") && (
                <div>
                  <label className="block text-xs font-semibold text-text-secondary mb-1">
                    Sub-Category
                    {selectedCategory.input_constraint === 1 && <span className="text-red-500 ml-0.5">*</span>}
                  </label>
                  {selectedCategory.input_type === "T" ? (
                    <input
                      type="text"
                      maxLength={35}
                      value={subCategoryValue}
                      onChange={(e) => setSubCategoryValue(e.target.value)}
                      placeholder="Enter sub-category"
                      className={`w-full rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.subCategory ? "border-red-400" : "border-border"}`}
                    />
                  ) : (
                    <select
                      value={subCategoryValue}
                      onChange={(e) => setSubCategoryValue(e.target.value)}
                      disabled={loadingSubCats}
                      className={`w-full rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.subCategory ? "border-red-400" : "border-border"}`}
                    >
                      <option value="">{loadingSubCats ? "Loading..." : "-- Select Sub-Category --"}</option>
                      {subCategories.map((sc) => (
                        <option key={sc.sub_category_name} value={sc.sub_category_name}>{sc.sub_category_name}</option>
                      ))}
                    </select>
                  )}
                  {errors.subCategory && <p className="mt-0.5 text-xs text-red-600">{errors.subCategory}</p>}
                </div>
              )}

              {/* ── Document Date ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">Document Date (MM/DD/YYYY)</label>
                <input
                  type="text"
                  value={documentDate}
                  onChange={(e) => setDocumentDate(e.target.value)}
                  placeholder="MM/DD/YYYY"
                  className={`w-full rounded-md border px-2.5 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none ${errors.documentDate ? "border-red-400" : "border-border"}`}
                />
                {errors.documentDate && <p className="mt-0.5 text-xs text-red-600">{errors.documentDate}</p>}
              </div>

              {/* ── File Upload ── */}
              <div>
                <label className="block text-xs font-semibold text-text-secondary mb-1">File</label>
                <div
                  onDragOver={handleDragOver}
                  onDragLeave={handleDragLeave}
                  onDrop={handleDrop}
                  onClick={() => fileInputRef.current?.click()}
                  className={`rounded-lg border-2 border-dashed px-4 py-5 text-center cursor-pointer transition-colors ${
                    dragOver
                      ? "border-primary bg-primary/5"
                      : errors.file
                        ? "border-red-300 bg-red-50/30"
                        : "border-border hover:border-primary/50 hover:bg-primary/5"
                  }`}
                >
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept={ALLOWED_EXTENSIONS}
                    onChange={handleFileSelect}
                    className="hidden"
                  />
                  {file ? (
                    <div className="flex items-center justify-center gap-2 text-sm text-text-primary">
                      <svg className="h-5 w-5 text-primary" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      <span className="font-medium">{file.name}</span>
                      <button
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          setFile(null);
                          if (fileInputRef.current) fileInputRef.current.value = "";
                        }}
                        className="ml-1 text-text-muted hover:text-red-600 transition-colors"
                      >
                        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  ) : (
                    <div>
                      <svg className="mx-auto h-8 w-8 text-text-muted" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
                      </svg>
                      <p className="mt-1 text-xs text-text-muted">
                        Drag & drop a file here, or <span className="text-primary font-medium">click to browse</span>
                      </p>
                      <p className="mt-0.5 text-[10px] text-text-muted">
                        Allowed: PDF, DOC, DOCX, MSG, RTF, JPG, PNG, GIF, TIF, HTML, HTM, LOG, DAT, TXT
                      </p>
                    </div>
                  )}
                </div>
                {errors.file && <p className="mt-0.5 text-xs text-red-600">{errors.file}</p>}
              </div>

              {/* ── PII/PHI Confirmation ── */}
              <p className="text-[11px] text-text-muted leading-relaxed">
                By clicking Add, I confirm that no sensitive PII or PHI is included in this file.
              </p>
            </div>

            {/* ── Error banner ── */}
            {submitError && (
              <div className="mt-3 rounded-lg bg-red-50 border border-red-200 px-3 py-2 text-xs text-red-700">
                {submitError}
              </div>
            )}

            {/* ── Actions ── */}
            <div className="mt-5 flex justify-end gap-3">
              <button
                type="button"
                onClick={onClose}
                disabled={submitting}
                className="rounded-lg border border-border px-4 py-2 text-sm font-medium text-text-secondary transition-colors hover:bg-surface-alt disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleSubmit}
                disabled={submitting}
                className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary disabled:opacity-50"
              >
                {submitting ? (
                  <span className="flex items-center gap-2">
                    <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                    </svg>
                    Uploading...
                  </span>
                ) : (
                  "Add"
                )}
              </button>
            </div>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
