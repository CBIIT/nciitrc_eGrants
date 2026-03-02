"use client";

import { useState } from "react";
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  flexRender,
  type ColumnDef,
  type SortingState,
  type Table,
} from "@tanstack/react-table";

interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T, unknown>[];
  pageSize?: number;
  searchable?: boolean;
  emptyMessage?: string;
  initialSorting?: SortingState;
}

function Pagination<T>({
  table,
  pageSize,
  dataLength,
  border,
}: {
  table: Table<T>;
  pageSize: number;
  dataLength: number;
  border: "top" | "bottom";
}) {
  const pageCount = table.getPageCount();
  const currentPage = table.getState().pagination.pageIndex;
  if (pageCount <= 1) return null;

  const borderClass =
    border === "top" ? "border-b border-border-light" : "border-t border-border-light";

  return (
    <div className={`flex items-center justify-between px-4 py-3 ${borderClass} text-sm`}>
      <span className="text-text-muted text-xs">
        Showing {currentPage * pageSize + 1} to{" "}
        {Math.min((currentPage + 1) * pageSize, dataLength)} of {dataLength} entries
      </span>
      <div className="flex items-center gap-1">
        <button
          type="button"
          onClick={() => table.previousPage()}
          disabled={!table.getCanPreviousPage()}
          className="rounded-md border border-border px-3 py-1.5 text-xs font-medium text-text-secondary transition-colors hover:bg-surface-alt disabled:opacity-40"
        >
          Previous
        </button>
        {Array.from({ length: Math.min(pageCount, 5) }, (_, i) => {
          const start = Math.max(0, Math.min(currentPage - 2, pageCount - 5));
          const pageNum = start + i;
          return (
            <button
              key={pageNum}
              type="button"
              onClick={() => table.setPageIndex(pageNum)}
              className={`rounded-md border px-3 py-1.5 text-xs font-medium transition-colors ${
                pageNum === currentPage
                  ? "border-transparent bg-gradient-to-r from-[#2563eb] to-[#3b82f6] text-white shadow-sm"
                  : "border-border text-text-secondary hover:bg-surface-alt"
              }`}
            >
              {pageNum + 1}
            </button>
          );
        })}
        <button
          type="button"
          onClick={() => table.nextPage()}
          disabled={!table.getCanNextPage()}
          className="rounded-md border border-border px-3 py-1.5 text-xs font-medium text-text-secondary transition-colors hover:bg-surface-alt disabled:opacity-40"
        >
          Next
        </button>
      </div>
    </div>
  );
}

export default function DataTable<T>({
  data,
  columns,
  pageSize = 10,
  searchable = true,
  emptyMessage = "No records found.",
  initialSorting,
}: DataTableProps<T>) {
  const [sorting, setSorting] = useState<SortingState>(initialSorting ?? []);
  const [globalFilter, setGlobalFilter] = useState("");

  const table = useReactTable({
    data,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize } },
  });

  return (
    <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
      {searchable && (
        <div className="px-4 py-3 border-b border-border-light">
          <div className="relative max-w-xs">
            <svg
              className="pointer-events-none absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-text-muted"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={2}
              stroke="currentColor"
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
            </svg>
            <input
              type="text"
              value={globalFilter}
              onChange={(e) => setGlobalFilter(e.target.value)}
              placeholder="Filter..."
              className="input-modern w-full pl-8"
            />
          </div>
        </div>
      )}

      <Pagination table={table} pageSize={pageSize} dataLength={data.length} border="top" />

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id} className="bg-[#f9fafb]">
                {headerGroup.headers.map((header) => (
                  <th
                    key={header.id}
                    className="px-4 py-3 text-left text-[12px] font-semibold uppercase tracking-wider text-text-secondary border-b border-border"
                    style={{
                      cursor: header.column.getCanSort() ? "pointer" : "default",
                    }}
                    onClick={header.column.getToggleSortingHandler()}
                  >
                    <div className="flex items-center gap-1">
                      {header.isPlaceholder
                        ? null
                        : flexRender(
                            header.column.columnDef.header,
                            header.getContext(),
                          )}
                      {header.column.getIsSorted() === "asc" && (
                        <span className="text-primary">&#9650;</span>
                      )}
                      {header.column.getIsSorted() === "desc" && (
                        <span className="text-primary">&#9660;</span>
                      )}
                    </div>
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-10 text-center text-text-muted"
                >
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr
                  key={row.id}
                  className="border-b border-border-light transition-colors hover:bg-surface-alt"
                >
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-2.5 text-text-primary">
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Pagination table={table} pageSize={pageSize} dataLength={data.length} border="bottom" />
    </div>
  );
}
