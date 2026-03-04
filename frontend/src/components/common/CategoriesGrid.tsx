"use client";

import { useState, useCallback, useEffect } from "react";
import { getCategories } from "@/lib/api";

interface CategoryItem {
  category_id: number;
  category_name: string;
}

interface CategoriesGridProps {
  grantId: number;
  /** Comma-separated appl_ids, or "All" */
  years: string;
  selectedCatIds: Set<number>;
  onSelectionChange: (selected: Set<number>) => void;
  onBack: () => void;
  /** Called when categories finish loading — parent decides initial selection */
  onLoaded?: (categoryIds: number[]) => void;
}

export default function CategoriesGrid({
  grantId,
  years,
  selectedCatIds,
  onSelectionChange,
  onBack,
  onLoaded,
}: CategoriesGridProps) {
  const [categories, setCategories] = useState<CategoryItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    getCategories(grantId, years)
      .then((rows) => {
        if (cancelled) return;
        const cats: CategoryItem[] = rows.map((r) => ({
          category_id: Number(
            r.category_id ?? r.CategoryID ?? r.cat_id ?? Object.values(r)[0],
          ),
          category_name: String(
            r.category_name ?? r.CategoryName ?? r.cat_name ?? Object.values(r)[1] ?? "",
          ),
        }));
        setCategories(cats);
        const ids = cats.map((c) => c.category_id);
        if (onLoaded) {
          onLoaded(ids);
        } else {
          // Default: select all
          onSelectionChange(new Set(ids));
        }
      })
      .catch((err) => { if (!cancelled) console.error(err); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [grantId, years]);

  const allSelected =
    categories.length > 0 &&
    categories.every((c) => selectedCatIds.has(c.category_id));

  const toggleSelectAll = useCallback(() => {
    if (allSelected) {
      onSelectionChange(new Set());
    } else {
      onSelectionChange(new Set(categories.map((c) => c.category_id)));
    }
  }, [allSelected, categories, onSelectionChange]);

  const toggleOne = useCallback(
    (catId: number) => {
      const next = new Set(selectedCatIds);
      if (next.has(catId)) next.delete(catId);
      else next.add(catId);
      onSelectionChange(next);
    },
    [selectedCatIds, onSelectionChange],
  );

  // Distribute into 3 columns vertically (same pattern as GrantYearsGrid / old system)
  const perCol = Math.ceil(categories.length / 3);
  const col1 = categories.slice(0, perCol);
  const col2 = categories.slice(perCol, perCol * 2);
  const col3 = categories.slice(perCol * 2);

  return (
    <div className="rounded-lg border border-border bg-white shadow-sm overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between bg-[#f8fafc] px-4 py-2 border-b border-border-light">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={onBack}
            className="p-1 rounded text-text-muted hover:text-primary hover:bg-blue-50 transition-colors"
            title="Back to Grant Years"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
            </svg>
          </button>
          <span className="text-sm font-semibold text-text-primary">Category</span>
          <span className="text-xs text-text-muted">
            ({categories.length} total{selectedCatIds.size > 0 ? `, ${selectedCatIds.size} selected` : ""})
          </span>
        </div>
      </div>

      {loading && (
        <div className="px-4 py-4 flex items-center gap-2 text-sm text-text-muted">
          <svg className="animate-spin h-4 w-4 text-primary" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          Loading categories...
        </div>
      )}

      {!loading && categories.length === 0 && (
        <div className="px-4 py-4 text-sm text-text-muted">No categories found.</div>
      )}

      {!loading && categories.length > 0 && (
        <>
          {/* Select All */}
          <div className="px-4 pt-2 pb-1 border-b border-border-light">
            <label className="inline-flex items-center gap-2 cursor-pointer text-sm">
              <input
                type="checkbox"
                checked={allSelected}
                onChange={toggleSelectAll}
                className="rounded border-gray-300 text-primary focus:ring-primary/30"
              />
              <span className="font-semibold text-text-secondary">Select All</span>
            </label>
          </div>

          {/* 3-column grid */}
          <div className="grid grid-cols-3 gap-x-4 px-4 py-2">
            {[col1, col2, col3].map((col, ci) => (
              <div key={ci} className="flex flex-col gap-0.5">
                {col.map((cat) => (
                  <label
                    key={cat.category_id}
                    className="inline-flex items-center gap-1.5 cursor-pointer py-0.5 rounded hover:bg-blue-50/50 px-1 -mx-1 transition-colors"
                  >
                    <input
                      type="checkbox"
                      checked={selectedCatIds.has(cat.category_id)}
                      onChange={() => toggleOne(cat.category_id)}
                      className="rounded border-gray-300 text-primary focus:ring-primary/30 shrink-0"
                    />
                    <span className="text-xs text-text-primary font-medium truncate">
                      {cat.category_name}
                    </span>
                  </label>
                ))}
              </div>
            ))}
          </div>

          {/* Selected Category line */}
          {selectedCatIds.size > 0 && (
            <div className="px-4 py-1.5 border-t border-border-light text-sm">
              <span className="font-bold text-text-secondary">Selected Category:</span>{" "}
              <span className="text-text-primary">
                {allSelected
                  ? "All"
                  : categories
                      .filter((c) => selectedCatIds.has(c.category_id))
                      .map((c) => c.category_name)
                      .join(", ")}
              </span>
            </div>
          )}
        </>
      )}
    </div>
  );
}
