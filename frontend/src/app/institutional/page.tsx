"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import {
  getInstitutionalOrgs,
  searchInstitutionalOrgs,
  getInstitutionalDocs,
} from "@/lib/api";
import type { OrgOut, InstitutionalDoc } from "@/lib/types";
import type { ColumnDef } from "@tanstack/react-table";

const orgColumns: ColumnDef<OrgOut, unknown>[] = [
  { accessorKey: "org_name", header: "Organization" },
  { accessorKey: "doc_count", header: "Documents" },
];

const docColumns: ColumnDef<InstitutionalDoc, unknown>[] = [
  { accessorKey: "document_id", header: "Doc ID" },
  { accessorKey: "category_name", header: "Category" },
  {
    accessorKey: "start_date",
    header: "Start",
    cell: ({ getValue }) => {
      const val = getValue() as string | null;
      return val ? new Date(val).toLocaleDateString() : "";
    },
  },
  {
    accessorKey: "end_date",
    header: "End",
    cell: ({ getValue }) => {
      const val = getValue() as string | null;
      return val ? new Date(val).toLocaleDateString() : "";
    },
  },
  { accessorKey: "comments", header: "Comments" },
];

export default function InstitutionalPage() {
  const { user, loading } = useAuth();
  const [orgs, setOrgs] = useState<OrgOut[]>([]);
  const [selectedOrg, setSelectedOrg] = useState<OrgOut | null>(null);
  const [docs, setDocs] = useState<InstitutionalDoc[]>([]);
  const [searchStr, setSearchStr] = useState("");

  useEffect(() => {
    if (user) {
      getInstitutionalOrgs().then(setOrgs).catch(console.error);
    }
  }, [user]);

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
    if (!searchStr.trim()) return;
    const results = await searchInstitutionalOrgs(searchStr);
    setOrgs(results);
  }

  async function handleSelectOrg(org: OrgOut) {
    setSelectedOrg(org);
    if (org.org_id) {
      const data = await getInstitutionalDocs(org.org_id);
      setDocs(data);
    }
  }

  return (
    <AppShell user={user}>
      <h2 className="mb-4 text-lg font-bold">Institutional Files</h2>

      <form onSubmit={handleSearch} className="mb-4">
        <div className="flex gap-3">
          <input
            type="text"
            value={searchStr}
            onChange={(e) => setSearchStr(e.target.value)}
            placeholder="Search organizations..."
            className="input-modern flex-1"
          />
          <button type="submit" className="btn-primary">
            Search
          </button>
        </div>
      </form>

      <div className="grid gap-4 lg:grid-cols-2">
        <div>
          <h3 className="mb-2 text-sm font-semibold text-gray-600">
            Organizations
          </h3>
          <div className="max-h-96 overflow-y-auto border border-gray-200">
            {orgs.map((org) => (
              <button
                key={org.org_id}
                type="button"
                onClick={() => handleSelectOrg(org)}
                className={`w-full border-b border-gray-100 px-3 py-2 text-left text-sm hover:bg-gray-50 ${
                  selectedOrg?.org_id === org.org_id ? "bg-blue-50" : ""
                }`}
              >
                <span className="font-medium">{org.org_name}</span>
                <span className="ml-2 text-xs text-gray-400">
                  ({org.doc_count} docs)
                </span>
              </button>
            ))}
          </div>
        </div>

        <div>
          <h3 className="mb-2 text-sm font-semibold text-gray-600">
            {selectedOrg ? `Documents - ${selectedOrg.org_name}` : "Select an organization"}
          </h3>
          {selectedOrg ? (
            <DataTable data={docs} columns={docColumns} pageSize={10} />
          ) : (
            <p className="text-sm text-gray-400">
              Select an organization to view its documents.
            </p>
          )}
        </div>
      </div>
    </AppShell>
  );
}
