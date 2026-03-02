"use client";

import { useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import { getFundingDocs } from "@/lib/api";
import type { ColumnDef } from "@tanstack/react-table";

interface FundingDoc {
  document_id: number;
  appl_id: number;
  category_name: string;
  serial_num: string;
  fy: string;
}

const fundingColumns: ColumnDef<FundingDoc, unknown>[] = [
  { accessorKey: "serial_num", header: "Serial Number" },
  { accessorKey: "fy", header: "FY" },
  { accessorKey: "category_name", header: "Category" },
  { accessorKey: "document_id", header: "Doc ID" },
];

export default function FundingPage() {
  const { user, loading } = useAuth();
  const [serialNum, setSerialNum] = useState("");
  const [fy, setFy] = useState("");
  const [docs, setDocs] = useState<FundingDoc[]>([]);
  const [searching, setSearching] = useState(false);

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }
  if (!user) return null;

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setSearching(true);
    try {
      const data = await getFundingDocs(serialNum, fy);
      setDocs(data as unknown as FundingDoc[]);
    } catch (err) {
      console.error(err);
    } finally {
      setSearching(false);
    }
  }

  return (
    <AppShell user={user}>
      <h2 className="mb-4 text-lg font-bold">Funding Files</h2>

      <form onSubmit={handleSearch} className="mb-4">
        <div className="flex gap-3">
          <input
            type="text"
            value={serialNum}
            onChange={(e) => setSerialNum(e.target.value)}
            placeholder="Serial Number"
            className="input-modern max-w-xs"
          />
          <input
            type="text"
            value={fy}
            onChange={(e) => setFy(e.target.value)}
            placeholder="Fiscal Year"
            className="input-modern max-w-xs"
          />
          <button
            type="submit"
            disabled={searching}
            className="btn-primary"
          >
            {searching ? "Searching..." : "Search"}
          </button>
        </div>
      </form>

      {docs.length > 0 && (
        <DataTable data={docs} columns={fundingColumns} />
      )}
    </AppShell>
  );
}
