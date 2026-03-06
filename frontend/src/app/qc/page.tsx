"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import { getQcQueue } from "@/lib/api";
import type { ColumnDef } from "@tanstack/react-table";

interface QcItem {
  document_id: number;
  appl_id: number;
  category_name: string;
  sub_category_name: string;
  created_by: string;
  created_date: string;
  problem_msg: string;
}

const qcColumns: ColumnDef<QcItem, unknown>[] = [
  { accessorKey: "document_id", header: "Doc ID" },
  { accessorKey: "appl_id", header: "Appl ID" },
  { accessorKey: "category_name", header: "Category" },
  { accessorKey: "sub_category_name", header: "Sub-Category" },
  { accessorKey: "created_by", header: "Created By" },
  {
    accessorKey: "created_date",
    header: "Created",
    cell: ({ getValue }) => {
      const val = getValue() as string | null;
      return val ? new Date(val).toLocaleDateString() : "";
    },
  },
  { accessorKey: "problem_msg", header: "Issue" },
];

export default function QcPage() {
  const { user, loading } = useAuth();
  const [items, setItems] = useState<QcItem[]>([]);

  useEffect(() => {
    if (user) {
      getQcQueue()
        .then((data) => setItems(data as unknown as QcItem[]))
        .catch(console.error);
    }
  }, [user]);

  if (loading || !user) return null;

  return (
    <AppShell user={user}>
      <h2 className="mb-4 text-lg font-bold">QC Queue</h2>
      <DataTable data={items} columns={qcColumns} pageSize={20} />
    </AppShell>
  );
}
