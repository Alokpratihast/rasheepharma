"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { categoryService } from "@/services/category.service";
import { getAdminErrorMessage } from "@/lib/admin-error-message";
import { slugify } from "@/lib/slugify";
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

  const [name, setName] = useState(category?.name ?? "");
  const [slug, setSlug] = useState(category?.slug ?? "");
  const [slugIsCustom, setSlugIsCustom] = useState(Boolean(category));
  const [description, setDescription] = useState(category?.description ?? "");
  const [parentCategoryId, setParentCategoryId] =
    useState<number | null>(category?.parentCategoryId ?? null);
  const [isActive, setIsActive] = useState(category?.isActive ?? true);

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

        setError(getAdminErrorMessage(error, "We could not load the category list. Refresh the page and try again."));
      } finally {
        setLoadingCategories(false);
      }
    };

    loadCategories();
  }, []);

  const handleNameChange = (value: string) => {
    setError(null);
    setSuccess(null);
    setName(value);

    if (!slugIsCustom) {
      setSlug(slugify(value));
    }
  };

  const handleSubmit = async (
    event: React.FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError(null);
    setSuccess(null);

    if (!name.trim()) {
      const message = "Enter a category name before saving.";
      setError(message);
      toast.error(message);
      return;
    }

    if (!slugify(slug)) {
      const message = "Enter a URL slug or generate one from the category name.";
      setError(message);
      toast.error(message);
      return;
    }

    const data: CategoryCreateInput = {
      name: name.trim(),
      slug: slugify(slug),
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
      toast.success(
        isEditMode
          ? "Category changes saved."
          : "Category added successfully. You can add another category here.",
      );

      if (!isEditMode) {
        setName("");
        setSlug("");
        setSlugIsCustom(false);
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

      const message = getAdminErrorMessage(
        error,
        isEditMode
          ? "Could not update this category. Check the fields and try again."
          : "Could not add this category. Check the fields and try again.",
      );
      setError(message);
      toast.error(message);
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
            required
            autoComplete="off"
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
            required
            type="text"
            value={slug}
            onChange={(event) => {
              setError(null);
              setSuccess(null);
              setSlugIsCustom(true);
              setSlug(event.target.value);
            }}
            onBlur={() => setSlug((current) => slugify(current))}
            autoComplete="off"
            placeholder="e.g. tablets"
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-1 focus:ring-[#1B2A4A]"
          />

          <div className="mt-1 flex flex-wrap items-center justify-between gap-2">
            <p className="text-xs text-gray-500">
              {slugIsCustom
                ? "Custom URL. You can regenerate it from the category name."
                : `Auto-generated: /categories/${slug || "category-name"}`}
            </p>
            {slugIsCustom && (
              <button
                type="button"
                onClick={() => {
                  setSlugIsCustom(false);
                  setSlug(slugify(name));
                }}
                className="text-xs font-semibold text-[#007f68] hover:underline"
              >
                Generate from name
              </button>
            )}
          </div>
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
        <div role="alert" className="mt-6 rounded-lg border border-red-200 bg-red-50 px-4 py-3">
          <p className="text-sm text-red-600">
            {error}
          </p>
        </div>
      )}

      {success && (
        <div role="status" className="mt-6 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3">
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
