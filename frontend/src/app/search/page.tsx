"use client";

import { Suspense, useState, useEffect, useCallback, useMemo, useRef } from "react";
import { useSearchParams } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import SearchForm from "@/components/common/SearchForm";
import GrantCard from "@/components/common/GrantCard";
import GrantYearsGrid from "@/components/common/GrantYearsGrid";
import CategoriesGrid from "@/components/common/CategoriesGrid";
import DocumentCard from "@/components/common/DocumentCard";
import SupplementPanel from "@/components/common/SupplementPanel";
import { searchByString, searchByFilters, searchByGrant } from "@/lib/api";
import type { SearchResult } from "@/lib/types";

export default function SearchPage() {
  return (
    <Suspense fallback={<div className="flex min-h-screen items-center justify-center"><p className="text-gray-500">Loading...</p></div>}>
      <SearchContent />
    </Suspense>
  );
}

function SearchContent() {
  const { user, loading } = useAuth();
  const searchParams = useSearchParams();
  const [results, setResults] = useState<SearchResult | null>(null);
  const [searching, setSearching] = useState(false);
  const [selectedApplIds, setSelectedApplIds] = useState<Set<number>>(new Set());
  const [selectedCatIds, setSelectedCatIds] = useState<Set<number>>(new Set());
  const [showCategories, setShowCategories] = useState(false);
  const [activeFlag, setActiveFlag] = useState<string | null>(null);
  const [showSupplement, setShowSupplement] = useState(false);
  // Category init mode — ref (not state) to avoid closure timing issues with useCallback
  // 'select-all': first load should select all categories (toolbar toggle / normal grant view)
  // 'select-none': first load should leave categories empty (filter icon from search results)
  // 'done': already initialized — keep user's existing selection on re-mount
  const catInitRef = useRef<"select-all" | "select-none" | "done">("select-all");

  const grantId = searchParams.get("grant_id");
  const isGrantView = !!grantId;

  // Map flag type to the application-level field that indicates that flag
  const flagFieldMap: Record<string, string[]> = {
    DS: ["appl_ds_flag", "ds_flag"],
    MS: ["appl_ms_flag", "ms_flag"],
    OD: ["appl_od_flag", "od_flag"],
    FDA: ["appl_fda_flag", "fda_flag"],
  };

  // Collapse search form when viewing a specific grant
  const [showSearchForm, setShowSearchForm] = useState(!isGrantView);

  // Reset form visibility when navigation mode changes
  useEffect(() => {
    setShowSearchForm(!searchParams.get("grant_id"));
  }, [searchParams]);

  useEffect(() => {
    let cancelled = false;
    const q = searchParams.get("q");
    const gid = searchParams.get("grant_id");
    const fy = searchParams.get("fy");
    const mechanism = searchParams.get("mechanism");
    const serialNum = searchParams.get("serial_num");

    const flagParam = searchParams.get("flag");
    const viewParam = searchParams.get("view");

    if (gid) {
      setSearching(true);
      catInitRef.current = "select-all";
      setSelectedCatIds(new Set());
      searchByGrant(Number(gid))
        .then((res) => {
          if (cancelled) return;
          setResults(res);
          setSelectedApplIds(new Set(res.applications.map((a) => a.appl_id)));

          if (viewParam === "categories") {
            // Open categories grid directly — start with none selected
            setShowCategories(true);
            catInitRef.current = "select-none";
            setActiveFlag(null);
            setShowSupplement(false);
          } else if (flagParam === "Supplement") {
            setShowSupplement(true);
            setActiveFlag(null);
          } else if (flagParam && flagFieldMap[flagParam]) {
            // Pre-filter grant years by flag
            const fields = flagFieldMap[flagParam];
            const matched = res.applications.filter((a) => {
              const r = a as unknown as Record<string, unknown>;
              return fields.some((f) => String(r[f] ?? "") === "y");
            });
            setSelectedApplIds(new Set(matched.map((a) => a.appl_id)));
            setActiveFlag(flagParam);
          } else {
            setActiveFlag(null);
            setShowSupplement(false);
          }
        })
        .catch((err) => { if (!cancelled) console.error(err); })
        .finally(() => { if (!cancelled) setSearching(false); });
    } else if (q) {
      setSearching(true);
      searchByString(q)
        .then((res) => { if (!cancelled) setResults(res); })
        .catch((err) => { if (!cancelled) console.error(err); })
        .finally(() => { if (!cancelled) setSearching(false); });
    } else if (fy || mechanism || serialNum) {
      setSearching(true);
      searchByFilters(fy || "", mechanism || "", searchParams.get("ic") || "", serialNum || "", 1)
        .then((res) => { if (!cancelled) setResults(res); })
        .catch((err) => { if (!cancelled) console.error(err); })
        .finally(() => { if (!cancelled) setSearching(false); });
    }
    return () => { cancelled = true; };
  }, [searchParams]);

  const handleSelectionChange = useCallback((selected: Set<number>) => {
    setSelectedApplIds(selected);
    setActiveFlag(null);
  }, []);

  const handleCatSelectionChange = useCallback((selected: Set<number>) => {
    setSelectedCatIds(selected);
  }, []);

  const handleBackToYears = useCallback(() => {
    setShowCategories(false);
  }, []);

  const handleCatLoaded = useCallback((_ids: number[]) => {
    const mode = catInitRef.current;
    if (mode === "done") return; // Re-mount — keep user's existing selection
    catInitRef.current = "done";
    if (mode === "select-all") {
      setSelectedCatIds(new Set(_ids));
    }
    // "select-none" (filter icon) — leave empty, user picks what they want
  }, []);

  const handleFlagClick = useCallback((flagType: string) => {
    if (!results) return;

    // Supplement is special — toggles supplement panel
    if (flagType === "Supplement") {
      setShowSupplement((v) => !v);
      return;
    }

    // Toggle off if clicking the same flag
    if (activeFlag === flagType) {
      setActiveFlag(null);
      setSelectedApplIds(new Set(results.applications.map((a) => a.appl_id)));
      return;
    }
    setActiveFlag(flagType);
    // Filter grant years that have this flag
    const fields = flagFieldMap[flagType];
    if (fields) {
      const matched = results.applications.filter((a) => {
        const r = a as unknown as Record<string, unknown>;
        return fields.some((f) => String(r[f] ?? "") === "y");
      });
      setSelectedApplIds(new Set(matched.map((a) => a.appl_id)));
    }
  }, [activeFlag, results]);

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }
  if (!user) return null;

  // For grant view, get the single grant's applications
  const grantAppls = isGrantView && results?.grants[0]
    ? results.applications.filter((a) => a.grant_id === results.grants[0].grant_id)
    : [];

  // Build category list string for document queries
  // When categories mode is active but nothing selected → no documents shown
  const categoryList = useMemo(() => {
    if (!showCategories) return "All";
    if (selectedCatIds.size === 0) return "";
    return Array.from(selectedCatIds).join(",");
  }, [showCategories, selectedCatIds]);

  return (
    <AppShell user={user}>
      {/* ── Toggle toolbar (shown when viewing a specific grant) ── */}
      {isGrantView && (
        <div className="flex items-center gap-1 mb-3">
          <button
            type="button"
            onClick={() => setShowSearchForm((v) => !v)}
            className={`p-2 rounded-lg transition-colors ${showSearchForm ? "bg-primary text-white" : "bg-white text-text-muted border border-border hover:text-primary hover:bg-blue-50"}`}
            title={showSearchForm ? "Hide search form" : "Show search form"}
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
            </svg>
          </button>
          <button
            type="button"
            onClick={() => setShowCategories((v) => !v)}
            className={`p-2 rounded-lg transition-colors ${showCategories ? "bg-primary text-white" : "bg-white text-text-muted border border-border hover:text-primary hover:bg-blue-50"}`}
            title={showCategories ? "Back to Grant Years" : "Filter by category"}
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 01-.659 1.591l-5.432 5.432a2.25 2.25 0 00-.659 1.591v2.927a2.25 2.25 0 01-1.244 2.013L9.75 21v-6.568a2.25 2.25 0 00-.659-1.591L3.659 7.409A2.25 2.25 0 013 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0112 3z" />
            </svg>
          </button>
        </div>
      )}

      {/* ── Search form (collapsible) ── */}
      {showSearchForm && <SearchForm user={user} />}

      {searching && (
        <div className="mt-6 flex items-center gap-2 text-sm text-text-muted">
          <svg className="animate-spin h-4 w-4 text-primary" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          Searching...
        </div>
      )}

      {results && !searching && (
        <div className="mt-4">
          {/* ── Grant view: Grant Years or Categories grid + single GrantCard ── */}
          {isGrantView && grantAppls.length > 0 && (
            <div className="space-y-3">
              {showCategories ? (
                <CategoriesGrid
                  grantId={Number(grantId)}
                  years={
                    selectedApplIds.size === grantAppls.length
                      ? "All"
                      : Array.from(selectedApplIds).join(",")
                  }
                  selectedCatIds={selectedCatIds}
                  onSelectionChange={handleCatSelectionChange}
                  onBack={handleBackToYears}
                  onLoaded={handleCatLoaded}
                />
              ) : (
                <GrantYearsGrid
                  applications={grantAppls}
                  selectedApplIds={selectedApplIds}
                  onSelectionChange={handleSelectionChange}
                />
              )}

              {results.grants.map((grant) => {
                const appls = results.applications.filter((a) => a.grant_id === grant.grant_id);
                const docCnts = results.doc_counts.filter((d) => (d as Record<string, unknown>).grant_id === grant.grant_id);
                return (
                  <GrantCard
                    key={grant.grant_id}
                    grant={grant}
                    applications={appls}
                    docCounts={docCnts}
                    grantView
                    onFlagClick={handleFlagClick}
                    activeFlag={showSupplement ? "Supplement" : activeFlag}
                  />
                );
              })}

              {/* ── Supplement panel ── */}
              {showSupplement && results.grants[0] && (
                <SupplementPanel
                  grantId={results.grants[0].grant_id}
                  onClose={() => setShowSupplement(false)}
                />
              )}

              {/* ── Document cards: one per selected grant year ── */}
              {/* Hidden when categories mode is active but nothing selected yet */}
              {categoryList !== "" && grantAppls
                .filter((a) => selectedApplIds.has(a.appl_id))
                .map((appl) => (
                  <DocumentCard
                    key={appl.appl_id}
                    application={appl}
                    searchType="by_grant"
                    categoryList={categoryList}
                  />
                ))}
            </div>
          )}

          {/* ── Normal search: list of GrantCards ── */}
          {!isGrantView && (
            <>
              <p className="mb-3 text-sm text-text-muted">
                Found {results.grants.length} grant{results.grants.length !== 1 ? "s" : ""}.
              </p>
              <div className="space-y-3 stagger-children">
                {results.grants.map((grant) => {
                  const appls = results.applications.filter((a) => a.grant_id === grant.grant_id);
                  const docCnts = results.doc_counts.filter((d) => (d as Record<string, unknown>).grant_id === grant.grant_id);
                  return (
                    <GrantCard
                      key={grant.grant_id}
                      grant={grant}
                      applications={appls}
                      docCounts={docCnts}
                    />
                  );
                })}
              </div>
            </>
          )}
        </div>
      )}
    </AppShell>
  );
}
