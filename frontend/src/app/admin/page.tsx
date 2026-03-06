"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import AppShell from "@/components/layout/AppShell";
import DataTable from "@/components/common/DataTable";
import { getAccessControl } from "@/lib/api";
import type { PersonOut } from "@/lib/types";
import type { ColumnDef } from "@tanstack/react-table";

const userColumns: ColumnDef<PersonOut, unknown>[] = [
  { accessorKey: "userid", header: "User ID" },
  { accessorKey: "first_name", header: "First Name" },
  { accessorKey: "last_name", header: "Last Name" },
  { accessorKey: "email", header: "Email" },
  { accessorKey: "position_name", header: "Position" },
  { accessorKey: "ic", header: "IC" },
  {
    accessorKey: "active",
    header: "Active",
    cell: ({ getValue }) => (getValue() === 1 ? "Yes" : "No"),
  },
  {
    id: "permissions",
    header: "Permissions",
    cell: ({ row }) => {
      const perms = [];
      if (row.original.can_egrants) perms.push("eGrants");
      if (row.original.can_admin) perms.push("Admin");
      if (row.original.can_mgt) perms.push("Mgt");
      if (row.original.can_docman) perms.push("DocMan");
      if (row.original.can_cft) perms.push("CFT");
      if (row.original.can_dashboard) perms.push("Dashboard");
      if (row.original.can_iccoord) perms.push("IC Coord");
      return (
        <span className="text-xs text-gray-500">{perms.join(", ")}</span>
      );
    },
  },
];

export default function AdminPage() {
  const { user, loading } = useAuth();
  const [users, setUsers] = useState<PersonOut[]>([]);

  useEffect(() => {
    if (user) {
      getAccessControl()
        .then(setUsers)
        .catch(console.error);
    }
  }, [user]);

  if (loading || !user) return null;

  return (
    <AppShell user={user}>
      <h2 className="mb-4 text-lg font-bold">Admin - Access Control</h2>
      <DataTable data={users} columns={userColumns} pageSize={20} />
    </AppShell>
  );
}
