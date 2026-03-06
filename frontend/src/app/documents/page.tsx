"use client";

import { Suspense, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import FileUpload from "@/components/common/FileUpload";
import { getDocumentGrid, getDownloadUrl } from "@/lib/api";
import type { DocumentOut } from "@/lib/types";
import type { ColumnDef } from "@tanstack/react-table";

export default function DocumentsPage() {
  return (
    <Suspense fallback={<div className="flex min-h-screen items-center justify-center"><p className="text-gray-500">Loading...</p></div>}>
      <DocumentsContent />
    </Suspense>
  );
}

function DocumentsContent() {
  const { user, loading } = useAuth();
  const searchParams = useSearchParams();
  const applId = Number(searchParams.get("appl_id")) || 0;
  const [documents, setDocuments] = useState<DocumentOut[]>([]);
  const [loadingDocs, setLoadingDocs] = useState(false);

  const docColumns: ColumnDef<DocumentOut, unknown>[] = [
    { accessorKey: "document_id", header: "Doc ID" },
    { accessorKey: "category_name", header: "Category" },
    { accessorKey: "sub_category_name", header: "Sub-Category" },
    {
      accessorKey: "document_date",
      header: "Date",
      cell: ({ getValue }) => {
        const val = getValue() as string | null;
        return val ? new Date(val).toLocaleDateString() : "";
      },
    },
    { accessorKey: "created_by", header: "Created By" },
    { accessorKey: "page_count", header: "Pages" },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }) => (
        <a
          href={getDownloadUrl(row.original.document_id)}
          className="text-sm font-medium text-blue-600 hover:underline"
          target="_blank"
          rel="noopener noreferrer"
        >
          Download
        </a>
      ),
    },
  ];

  useEffect(() => {
    if (user && applId) {
      setLoadingDocs(true);
      getDocumentGrid(applId)
        .then((data) => setDocuments(data.documents as DocumentOut[]))
        .catch(console.error)
        .finally(() => setLoadingDocs(false));
    }
  }, [user, applId]);

  if (loading || !user) return null;

  async function handleUpload(file: File) {
    console.log("Upload file:", file.name);
  }

  return (
    <AppShell user={user}>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-lg font-bold">
          Documents {applId ? `- Application ${applId}` : ""}
        </h2>
        {applId > 0 && <FileUpload onUpload={handleUpload} />}
      </div>

      {!applId && (
        <div className="p-8 text-center text-gray-400">
          Select an application from the search results to view documents.
        </div>
      )}

      {loadingDocs && (
        <p className="text-gray-500">Loading documents...</p>
      )}

      {applId > 0 && !loadingDocs && (
        <DataTable data={documents} columns={docColumns} pageSize={20} />
      )}
    </AppShell>
  );
}
