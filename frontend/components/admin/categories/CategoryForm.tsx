"use client";

import { useEffect, useState } from "react";
import { categoryService } from "@/services/category.service";
import type {
  Category,
  CategoryCreateInput,
  CategoryDetails,
} from "@/types/category";

interface CategoryFormProps {
  category?: CategoryDetails | null;
  onSuccess?: (category: CategoryDetails) => void;
  onCancel?: () => void;
}

export function CategoryForm({
  category,
  onSuccess,
  onCancel,
}: CategoryFormProps) {
  const isEditMode = Boolean(category);

  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [description, setDescription] = useState("");
  const [parentCategoryId, setParentCategoryId] =
    useState<number | null>(null);
  const [isActive, setIsActive] = useState(true);

  const [categories, setCategories] = useState<Category[]>([]);
  const [loadingCategories, setLoadingCategories] =
    useState(true);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        setLoadingCategories(true);

        const data = await categoryService.getAll();

        setCategories(data);
      } catch (error) {
        console.error(
          "Failed to load categories:",
          error,
        );

        setError("Failed to load categories.");
      } finally {
        setLoadingCategories(false);
      }
    };

    loadCategories();
  }, []);

  useEffect(() => {
    if (!category) {
      setName("");
      setSlug("");
      setDescription("");
      setParentCategoryId(null);
      setIsActive(true);

      return;
    }

    setName(category.name);
    setSlug(category.slug);
    setDescription(category.description ?? "");
    setParentCategoryId(category.parentCategoryId ?? null);
    setIsActive(category.isActive);
  }, [category]);

  const generateSlug = (value: string) => {
    return value
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");
  };

  const handleNameChange = (value: string) => {
    setName(value);

    if (!isEditMode) {
      setSlug(generateSlug(value));
    }
  };

  const handleSubmit = async (
    event: React.FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError(null);
    setSuccess(null);

    if (!name.trim()) {
      setError("Category name is required.");
      return;
    }

    if (!slug.trim()) {
      setError("Slug is required.");
      return;
    }

    const data: CategoryCreateInput = {
      name: name.trim(),
      slug: slug.trim(),
      description: description.trim() || null,
      parentCategoryId,
      isActive,
    };

    try {
      setSubmitting(true);

      let result: CategoryDetails;

      if (category) {
        result = await categoryService.update(
          category.id,
          data,
        );
      } else {
        result = await categoryService.create(data);
      }

      setSuccess(
        isEditMode
          ? "Category updated successfully."
          : "Category created successfully.",
      );

      if (!isEditMode) {
        setName("");
        setSlug("");
        setDescription("");
        setParentCategoryId(null);
        setIsActive(true);
      }

      onSuccess?.(result);
    } catch (error) {
      console.error(
        "Failed to save category:",
        error,
      );

      setError("Failed to save category.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-xl bg-white p-6 shadow-sm"
    >
      <div className="grid gap-6 md:grid-cols-2">
        {/* Name */}
        <div>
          <label
            htmlFor="category-name"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Category Name
          </label>

          <input
            id="category-name"
            type="text"
            value={name}
            onChange={(event) =>
              handleNameChange(event.target.value)
            }
            placeholder="e.g. Tablets"
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-1 focus:ring-[#1B2A4A]"
          />
        </div>

        {/* Slug */}
        <div>
          <label
            htmlFor="category-slug"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Slug
          </label>

          <input
            id="category-slug"
            type="text"
            value={slug}
            onChange={(event) =>
              setSlug(event.target.value)
            }
            placeholder="e.g. tablets"
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-1 focus:ring-[#1B2A4A]"
          />

          <p className="mt-1 text-xs text-gray-400">
            Used in category URLs.
          </p>
        </div>

        {/* Parent Category */}
        <div>
          <label
            htmlFor="parent-category"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Parent Category
          </label>

          <select
            id="parent-category"
            value={parentCategoryId ?? ""}
            onChange={(event) => {
              const value = event.target.value;

              setParentCategoryId(
                value ? Number(value) : null,
              );
            }}
            disabled={loadingCategories}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-1 focus:ring-[#1B2A4A]"
          >
            <option value="">
              {loadingCategories
                ? "Loading categories..."
                : "Root Category"}
            </option>

            {categories
              .filter(
                (item) => item.id !== category?.id,
              )
              .map((item) => (
                <option
                  key={item.id}
                  value={item.id}
                >
                  {item.name}
                </option>
              ))}
          </select>

          <p className="mt-1 text-xs text-gray-400">
            Leave empty to create a root category.
          </p>
        </div>

        {/* Status */}
        <div>
          <label className="mb-2 block text-sm font-medium text-[#1B2A4A]">
            Status
          </label>

          <label className="flex cursor-pointer items-center gap-3">
            <input
              type="checkbox"
              checked={isActive}
              onChange={(event) =>
                setIsActive(event.target.checked)
              }
              className="h-4 w-4 rounded border-gray-300"
            />

            <span className="text-sm text-gray-700">
              Active
            </span>
          </label>
        </div>

        {/* Description */}
        <div className="md:col-span-2">
          <label
            htmlFor="category-description"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Description
          </label>

          <textarea
            id="category-description"
            value={description}
            onChange={(event) =>
              setDescription(event.target.value)
            }
            placeholder="Enter category description..."
            rows={4}
            className="w-full resize-none rounded-lg border border-gray-300 px-4 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-1 focus:ring-[#1B2A4A]"
          />
        </div>
      </div>

      {/* Messages */}
      {error && (
        <div className="mt-6 rounded-lg bg-red-50 px-4 py-3">
          <p className="text-sm text-red-600">
            {error}
          </p>
        </div>
      )}

      {success && (
        <div className="mt-6 rounded-lg bg-green-50 px-4 py-3">
          <p className="text-sm text-green-700">
            {success}
          </p>
        </div>
      )}

      {/* Actions */}
      <div className="mt-6 flex items-center justify-end gap-3 border-t border-gray-100 pt-6">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={submitting}
            className="rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Cancel
          </button>
        )}

        <button
          type="submit"
          disabled={submitting}
          className="rounded-lg bg-[#1B2A4A] px-5 py-2.5 text-sm font-medium text-white transition hover:bg-[#142039] disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting
            ? "Saving..."
            : isEditMode
              ? "Update Category"
              : "Create Category"}
        </button>
      </div>
    </form>
  );
}