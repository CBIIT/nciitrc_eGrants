"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import type { UserInfo } from "@/lib/types";
import { getAdminCodes, getDataYears } from "@/lib/api";

interface SearchFormProps {
  user: UserInfo;
}

export default function SearchForm({ user }: SearchFormProps) {
  const router = useRouter();
  const [keyword, setKeyword] = useState("");
  const [fy, setFy] = useState("");
  const [mechanism, setMechanism] = useState("");
  const [ic, setIc] = useState("");
  const [serialNum, setSerialNum] = useState("");
  const [adminCodes, setAdminCodes] = useState<string[]>([]);
  const [years, setYears] = useState<string[]>([]);
  const [yearError, setYearError] = useState("");
  const isStaff = (user.position_id ?? 0) > 1;

  // Fetch admin codes for IC dropdown
  useEffect(() => {
    getAdminCodes()
      .then((rows) => {
        const codes = rows
          .map((r) => String(r.admin_phs_org_code ?? r.AdminPhsOrgCode ?? Object.values(r)[0] ?? ""))
          .filter(Boolean);
        setAdminCodes(codes);
      })
      .catch(console.error);
  }, []);

  // Fetch years when serial number changes
  useEffect(() => {
    if (!serialNum.trim()) {
      setYears([]);
      return;
    }
    getDataYears(fy, mechanism, ic, serialNum)
      .then((rows) => {
        const yrs = rows
          .map((r) => String(r.year ?? r.support_year ?? Object.values(r)[0] ?? ""))
          .filter(Boolean);
        setYears(yrs);
      })
      .catch(() => setYears([]));
  }, [serialNum, fy, mechanism, ic]);

  function handleKeywordSearch(e: React.FormEvent) {
    e.preventDefault();
    if (!keyword.trim()) return;
    router.push(`/search?q=${encodeURIComponent(keyword.trim())}`);
  }

  function handleFilterSearch(e: React.FormEvent) {
    e.preventDefault();
    const params = new URLSearchParams();
    if (fy) params.set("fy", fy);
    if (mechanism) params.set("mechanism", mechanism);
    if (ic) params.set("ic", ic);
    if (serialNum) params.set("serial_num", serialNum);
    router.push(`/search?${params.toString()}`);
  }

  function handleClear() {
    setKeyword("");
    setFy("");
    setMechanism("");
    setIc("CA");
    setSerialNum("");
    setYears([]);
    setYearError("");
  }

  function handleYearClick() {
    if (!serialNum.trim()) {
      setYearError("Please enter a Serial # first");
      setTimeout(() => setYearError(""), 3000);
    }
  }

  return (
    <div
      className="search-card"
      style={{ display: "grid", gridTemplateColumns: "auto 1fr auto", gap: "10px 12px", alignItems: "center", maxWidth: 920, margin: "0 auto" }}
    >
      {/* Row 1: Keyword Search */}
      <form onSubmit={handleKeywordSearch} style={{ display: "contents" }}>
        <label className="property-label whitespace-nowrap">Keyword Search</label>
        <input
          type="text"
          value={keyword}
          onChange={(e) => {
            setKeyword(e.target.value);
            if (e.target.value) { setFy(""); setMechanism(""); setSerialNum(""); }
          }}
          placeholder="Serial #, Grant #, PI Name etc..."
          className="input-modern w-full"
        />
        <div className="flex items-center gap-2">
          <button type="submit" className="btn-primary" style={{ minWidth: 80 }}>
            Search
          </button>
          <button type="button" onClick={handleClear} className="btn-clear" style={{ minWidth: 80 }}>
            Clear
          </button>
        </div>
      </form>

      {/* OR divider — left-aligned */}
      <div style={{ gridColumn: "1 / -1" }} className="flex items-center gap-3 my-1">
        <span className="property-label shrink-0">or</span>
        <div className="flex-1 h-px bg-border" />
      </div>

      {/* Filter column headers */}
      <span />
      <div className="flex items-center gap-3">
        <span className="property-label" style={{ width: 70 }}>FY</span>
        <span className="property-label" style={{ width: 100 }}>Mechanism</span>
        <span className="property-label" style={{ width: 75 }}>IC</span>
        <span className="property-label" style={{ width: 120 }}>Serial #</span>
        <span className="property-label flex-1">Year</span>
      </div>
      <span />

      {/* Row 2: Filter fields */}
      <form onSubmit={handleFilterSearch} style={{ display: "contents" }}>
        <label className="property-label whitespace-nowrap">Filters</label>
        <div className="flex items-center gap-3">
          <input
            type="text"
            value={fy}
            onChange={(e) => {
              setFy(e.target.value);
              if (e.target.value) setKeyword("");
            }}
            placeholder="FY"
            className="input-modern"
            style={{ width: 70 }}
          />
          <input
            type="text"
            value={mechanism}
            onChange={(e) => {
              setMechanism(e.target.value);
              if (e.target.value) setKeyword("");
            }}
            placeholder="Mechanism"
            className="input-modern"
            style={{ width: 100 }}
          />
          <select
            value={ic}
            onChange={(e) => {
              setIc(e.target.value);
              if (e.target.value) setKeyword("");
            }}
            className="input-modern"
            style={{ width: 75 }}
          >
            <option value="">--</option>
            {adminCodes.map((code) => (
              <option key={code} value={code}>{code}</option>
            ))}
          </select>
          <input
            type="text"
            value={serialNum}
            onChange={(e) => {
              setSerialNum(e.target.value);
              if (e.target.value) setKeyword("");
            }}
            placeholder="Serial No"
            className="input-modern"
            style={{ width: 120 }}
          />
          <div className="relative flex-1">
            <select
              className="input-modern w-full"
              onClick={handleYearClick}
              disabled={!serialNum.trim()}
            >
              <option value="">Select Year</option>
              {years.map((yr) => (
                <option key={yr} value={yr}>{yr}</option>
              ))}
            </select>
            {yearError && (
              <div className="absolute left-0 top-full mt-1 z-50 rounded-md bg-amber-50 border border-amber-200 px-3 py-1.5 text-[11px] text-amber-700 shadow-sm whitespace-nowrap fade-in">
                {yearError}
              </div>
            )}
          </div>
        </div>
        <div className="flex items-center justify-end">
          {isStaff && (
            <button type="button" className="btn-add-doc whitespace-nowrap">
              Add Document
            </button>
          )}
        </div>
      </form>
    </div>
  );
}
