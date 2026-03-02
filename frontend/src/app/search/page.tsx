"use client";

import { Suspense, useState, useEffect } from "react";
import { useSearchParams } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import SearchForm from "@/components/common/SearchForm";
import GrantCard from "@/components/common/GrantCard";
import { searchByString, searchByFilters } from "@/lib/api";
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

  useEffect(() => {
    const q = searchParams.get("q");
    const fy = searchParams.get("fy");
    const mechanism = searchParams.get("mechanism");
    const serialNum = searchParams.get("serial_num");

    if (q) {
      setSearching(true);
      searchByString(q)
        .then(setResults)
        .catch(console.error)
        .finally(() => setSearching(false));
    } else if (fy || mechanism || serialNum) {
      setSearching(true);
      searchByFilters(fy || "", mechanism || "", searchParams.get("ic") || "", serialNum || "", 1)
        .then(setResults)
        .catch(console.error)
        .finally(() => setSearching(false));
    }
  }, [searchParams]);

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }
  if (!user) return null;

  return (
    <AppShell user={user}>
      <SearchForm user={user} />

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
        <div className="mt-6">
          <p className="mb-3 text-sm text-text-muted">
            Found {results.grants.length} grant{results.grants.length !== 1 ? "s" : ""}.
          </p>
          <div className="space-y-3 stagger-children">
            {results.grants.map((grant) => {
              const grantAppls = results.applications.filter(
                (a) => a.grant_id === grant.grant_id,
              );
              const grantDocCounts = results.doc_counts.filter(
                (d) => (d as Record<string, unknown>).grant_id === grant.grant_id,
              );
              return (
                <GrantCard
                  key={grant.grant_id}
                  grant={grant}
                  applications={grantAppls}
                  docCounts={grantDocCounts}
                />
              );
            })}
          </div>
        </div>
      )}
    </AppShell>
  );
}
