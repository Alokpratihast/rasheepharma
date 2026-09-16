"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types/category";

export function CategoryList() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);
const [deleteError, setDeleteError] = useState<string | null>(null);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        setLoading(true);
        setError(null);

        const data = await categoryService.getAll();

        setCategories(data);
      } catch (error) {
        console.error(
          "Failed to load categories:",
          error,
        );

        setError("Failed to load categories.");
      } finally {
        setLoading(false);
      }
    };

    loadCategories();
  }, []);

  const handleDelete = async (id: number, name: string) => {
  const confirmed = window.confirm(
    `Are you sure you want to delete "${name}"?`,
  );

  if (!confirmed) {
    return;
  }

  try {
    setDeletingId(id);
    setDeleteError(null);

    await categoryService.delete(id);

    setCategories((currentCategories) =>
      currentCategories.filter(
        (category) => category.id !== id,
      ),
    );
  } catch (error) {
    console.error(
      "Failed to delete category:",
      error,
    );

    const apiError = error as {
      message?: string;
    };

    setDeleteError(
      apiError.message ??
        "Failed to delete category.",
    );
  } finally {
    setDeletingId(null);
  }
};

  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-7xl">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold text-[#1B2A4A]">
              Categories
            </h1>

            <p className="mt-2 text-sm text-[#595959]">
              Manage product categories and subcategories.
            </p>
          </div>

          <Link
            href="/admin/categories/new"
            className="rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white transition hover:bg-[#142039]"
          >
            Add Category
          </Link>
        </div>

        {/* Loading */}
        {loading && (
          <div className="mt-8 rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-gray-500">
              Loading categories...
            </p>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="mt-8 rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-red-600">
              {error}
            </p>
          </div>
        )}

        {deleteError && (
  <div className="mt-4 rounded-xl border border-red-200 bg-red-50 p-4">
    <p className="text-sm text-red-700">
      {deleteError}
    </p>
  </div>
)}

        {/* Content */}
        {!loading && !error && (
          <>
            {/* Count */}
            <div className="mt-8 flex items-center justify-between">
              <p className="text-sm text-gray-600">
                Total{" "}
                <span className="font-semibold text-[#1B2A4A]">
                  {categories.length}
                </span>{" "}
                categories
              </p>
            </div>

            {/* Empty */}
            {categories.length === 0 ? (
              <div className="mt-4 rounded-xl bg-white p-10 text-center shadow-sm">
                <h2 className="text-lg font-semibold text-[#1B2A4A]">
                  No categories found
                </h2>

                <p className="mt-2 text-sm text-gray-500">
                  Create your first product category.
                </p>
              </div>
            ) : (
              /* Table */
              <div className="mt-4 overflow-hidden rounded-xl bg-white shadow-sm">
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[1000px] text-left">
                    <thead className="border-b border-gray-200 bg-gray-50">
                      <tr>
                        <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Category
                        </th>

                        <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Description
                        </th>

                        <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Slug
                        </th>

                        <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Parent Category
                        </th>

                        <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Status
                        </th>

                        <th className="px-6 py-4 text-right text-xs font-semibold uppercase tracking-wide text-gray-500">
                          Action
                        </th>
                      </tr>
                    </thead>

                    <tbody className="divide-y divide-gray-100">
                      {categories.map((category) => (
                        <tr
                          key={category.id}
                          className="transition hover:bg-gray-50"
                        >
                          {/* Category */}
                          <td className="px-6 py-4">
                            <div>
                              <p className="font-medium text-[#1B2A4A]">
                                {category.name}
                              </p>

                              <p className="mt-1 text-xs text-gray-400">
                                ID: {category.id}
                              </p>
                            </div>
                          </td>

                          {/* Description */}
                          <td className="max-w-xs px-6 py-4">
                            {category.description ? (
                              <p
                                className="line-clamp-2 text-sm text-gray-600"
                                title={category.description}
                              >
                                {category.description}
                              </p>
                            ) : (
                              <span className="text-sm text-gray-400">
                                No description
                              </span>
                            )}
                          </td>

                          {/* Slug */}
                          <td className="px-6 py-4">
                            <span className="rounded-md bg-gray-100 px-2.5 py-1 text-xs text-gray-600">
                              {category.slug}
                            </span>
                          </td>

                          {/* Parent */}
                          <td className="px-6 py-4">
                            {category.parentCategoryName ? (
                              <span className="text-sm text-gray-700">
                                {category.parentCategoryName}
                              </span>
                            ) : (
                              <span className="text-sm text-gray-400">
                                Root Category
                              </span>
                            )}
                          </td>

                          {/* Status */}
                          <td className="px-6 py-4">
                            <span
                              className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                                category.isActive
                                  ? "bg-green-100 text-green-700"
                                  : "bg-gray-100 text-gray-600"
                              }`}
                            >
                              {category.isActive
                                ? "Active"
                                : "Inactive"}
                            </span>
                          </td>

                          {/* Action */}
                          <td className="px-6 py-4 text-right">
  <div className="flex items-center justify-end gap-4">
    <Link
      href={`/admin/categories/${category.id}`}
      className="text-sm font-medium text-[#1B2A4A] hover:underline"
    >
      Edit
    </Link>

    <button
      type="button"
      onClick={() =>
        handleDelete(category.id, category.name)
      }
      disabled={deletingId === category.id}
      className="text-sm font-medium text-red-600 hover:underline disabled:cursor-not-allowed disabled:opacity-50"
    >
      {deletingId === category.id
        ? "Deleting..."
        : "Delete"}
    </button>
  </div>
</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </main>
  );
}