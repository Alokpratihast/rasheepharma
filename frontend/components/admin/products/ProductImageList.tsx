"use client";

import { useEffect, useState } from "react";
import Image from "next/image";

import { productImageService } from "@/services/productImageService";
import {  getApiAssetUrl } from "@/lib/api/client";
import type { ProductImage } from "@/types/product";

interface ProductImageListProps {
  productId: number;
}

type ImageSource = "url" | "device";

interface ImageFormState {
  imageUrl: string;
  altText: string;
  isPrimary: boolean;
  displayOrder: string;
  imageSource: ImageSource;
  file: File | null;
}

const initialForm: ImageFormState = {
  imageUrl: "",
  altText: "",
  isPrimary: false,
  displayOrder: "0",
  imageSource: "url",
  file: null,
};

export function ProductImageList({
  productId,
}: ProductImageListProps) {
  const [images, setImages] = useState<ProductImage[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [showForm, setShowForm] = useState(false);
  const [editingImage, setEditingImage] =
    useState<ProductImage | null>(null);

  const [form, setForm] =
    useState<ImageFormState>(initialForm);

  const [selectedFilePreview, setSelectedFilePreview] =
    useState<string | null>(null);

  const loadImages = async () => {
    try {
      setLoading(true);
      setError(null);

      const data =
        await productImageService.getByProductId(productId);

      setImages(data);
    } catch (error) {
      console.error(
        "Failed to load product images:",
        error,
      );

      setError("Failed to load product images.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadImages();
  }, [productId]);

  useEffect(() => {
    if (!form.file) {
      setSelectedFilePreview(null);
      return;
    }

    const objectUrl = URL.createObjectURL(form.file);

    setSelectedFilePreview(objectUrl);

    return () => {
      URL.revokeObjectURL(objectUrl);
    };
  }, [form.file]);

  const openAddForm = () => {
    setEditingImage(null);
    setForm({
      ...initialForm,
    });
    setError(null);
    setSuccess(null);
    setShowForm(true);
  };

  const openEditForm = (image: ProductImage) => {
    setEditingImage(image);

    setForm({
      imageUrl: image.imageUrl ?? "",
      altText: image.altText ?? "",
      isPrimary: image.isPrimary ?? false,
      displayOrder: String(image.displayOrder ?? 0),
      imageSource: "url",
      file: null,
    });

    setError(null);
    setSuccess(null);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingImage(null);
    setForm({
      ...initialForm,
    });
    setSelectedFilePreview(null);
  };

  const handleFileChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const file =
      event.target.files?.[0] ?? null;

    if (!file) {
      setForm((current) => ({
        ...current,
        file: null,
      }));

      return;
    }

    if (!file.type.startsWith("image/")) {
      setError(
        "Please select a valid image file.",
      );

      event.target.value = "";

      setForm((current) => ({
        ...current,
        file: null,
      }));

      return;
    }

    const maxFileSize = 5 * 1024 * 1024;

    if (file.size > maxFileSize) {
      setError(
        "Image size must be 5 MB or less.",
      );

      event.target.value = "";

      setForm((current) => ({
        ...current,
        file: null,
      }));

      return;
    }

    setError(null);

    setForm((current) => ({
      ...current,
      file,
    }));
  };

  const handleSubmit = async (
    event: React.FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    try {
      setSaving(true);
      setError(null);
      setSuccess(null);

      const displayOrder = Number(
        form.displayOrder,
      );

      if (
        Number.isNaN(displayOrder) ||
        displayOrder < 0
      ) {
        setError(
          "Display order must be 0 or greater.",
        );
        return;
      }

      /*
       * EDIT EXISTING IMAGE
       *
       * Existing edit flow continues to use Image URL.
       */
      if (editingImage) {
        if (!form.imageUrl.trim()) {
          setError("Image URL is required.");
          return;
        }

        const updatedImage =
          await productImageService.update(
            editingImage.id,
            {
              imageUrl:
                form.imageUrl.trim(),
              altText:
                form.altText.trim() || null,
              isPrimary: form.isPrimary,
              displayOrder,
            },
          );

        setImages((currentImages) =>
          currentImages.map((image) =>
            image.id === updatedImage.id
              ? updatedImage
              : image,
          ),
        );

        setSuccess(
          "Image updated successfully.",
        );

        closeForm();

        return;
      }

      /*
       * ADD IMAGE FROM DEVICE
       */
      if (
        form.imageSource === "device"
      ) {
        if (!form.file) {
          setError(
            "Please select an image file.",
          );
          return;
        }

        const createdImage =
          await productImageService.upload(
            productId,
            {
              file: form.file,
              altText:
                form.altText.trim() || null,
              isPrimary: form.isPrimary,
              displayOrder,
            },
          );

        setImages((currentImages) => [
          ...currentImages,
          createdImage,
        ]);

        setSuccess(
          "Image uploaded successfully.",
        );

        closeForm();

        return;
      }

      /*
       * ADD IMAGE FROM URL
       */
      if (!form.imageUrl.trim()) {
        setError("Image URL is required.");
        return;
      }

      const createdImage =
        await productImageService.create(
          productId,
          {
            imageUrl:
              form.imageUrl.trim(),
            altText:
              form.altText.trim() || null,
            isPrimary: form.isPrimary,
            displayOrder,
          },
        );

      setImages((currentImages) => [
        ...currentImages,
        createdImage,
      ]);

      setSuccess(
        "Image added successfully.",
      );

      closeForm();
    } catch (error) {
      console.error(
        "Failed to save product image:",
        error,
      );

      setError(
        error instanceof Error
          ? error.message
          : "Failed to save product image.",
      );
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (
    image: ProductImage,
  ) => {
    const confirmed = window.confirm(
      "Are you sure you want to delete this image?",
    );

    if (!confirmed) {
      return;
    }

    try {
      setError(null);
      setSuccess(null);

      await productImageService.delete(
        image.id,
      );

      setImages((currentImages) =>
        currentImages.filter(
          (item) => item.id !== image.id,
        ),
      );

      setSuccess(
        "Image deleted successfully.",
      );
    } catch (error) {
      console.error(
        "Failed to delete product image:",
        error,
      );

      setError(
        error instanceof Error
          ? error.message
          : "Failed to delete product image.",
      );
    }
  };

  /*
   * Returns the correct image URL.
   *
   * Device-uploaded images are stored in the
   * private Azure Blob container. Therefore they
   * must be loaded through the backend endpoint.
   *
   * Existing external/public URL images continue
   * to use their original URL.
   */
  const getProductImageUrl = (
  image: ProductImage,
): string | null => {
  const imageUrl = image.imageUrl ?? "";

  if (!imageUrl) {
    return null;
  }

  if (
    imageUrl.includes(
      "rasheepharmastorage01.blob.core.windows.net",
    )
  ) {
    const apiBaseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL ??
      "http://localhost:5104/api";

    return `${apiBaseUrl}/ProductImages/file/${image.id}`;
  }

  return getApiAssetUrl(imageUrl);
};

  return (
    <section className="mt-8">
      <div className="mb-5 flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[#1B2A4A]">
            Product Images
          </h2>

          <p className="mt-1 text-sm text-gray-500">
            Manage product images and choose the
            primary image.
          </p>
        </div>

        {!showForm && (
          <button
            type="button"
            onClick={openAddForm}
            className="rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white hover:bg-[#142039]"
          >
            Add Image
          </button>
        )}
      </div>

      {error && (
        <div className="mb-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      {success && (
        <div className="mb-4 rounded-lg bg-green-50 px-4 py-3 text-sm text-green-700">
          {success}
        </div>
      )}

      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="mb-6 rounded-xl bg-white p-6 shadow-sm"
        >
          <div className="mb-5">
            <h3 className="text-lg font-semibold text-[#1B2A4A]">
              {editingImage
                ? "Edit Image"
                : "Add Product Image"}
            </h3>
          </div>

          {!editingImage && (
            <div className="mb-6">
              <label className="mb-2 block text-sm font-medium text-gray-700">
                Image Source
              </label>

              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() =>
                    setForm((current) => ({
                      ...current,
                      imageSource: "url",
                      file: null,
                    }))
                  }
                  className={`rounded-lg border px-4 py-2.5 text-sm font-medium ${
                    form.imageSource === "url"
                      ? "border-[#1B2A4A] bg-[#1B2A4A] text-white"
                      : "border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
                  }`}
                >
                  Image URL
                </button>

                <button
                  type="button"
                  onClick={() =>
                    setForm((current) => ({
                      ...current,
                      imageSource: "device",
                      imageUrl: "",
                    }))
                  }
                  className={`rounded-lg border px-4 py-2.5 text-sm font-medium ${
                    form.imageSource ===
                    "device"
                      ? "border-[#1B2A4A] bg-[#1B2A4A] text-white"
                      : "border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
                  }`}
                >
                  Upload from Device
                </button>
              </div>
            </div>
          )}

          <div className="grid gap-5 md:grid-cols-2">
            <div className="md:col-span-2">
              {form.imageSource ===
              "url" ? (
                <>
                  <label className="mb-1.5 block text-sm font-medium text-gray-700">
                    Image URL
                  </label>

                  <input
                    type="text"
                    value={form.imageUrl}
                    onChange={(event) =>
                      setForm((current) => ({
                        ...current,
                        imageUrl:
                          event.target.value,
                      }))
                    }
                    placeholder="/images/products/example.svg"
                    required={
                      form.imageSource ===
                      "url"
                    }
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
                  />
                </>
              ) : (
                <>
                  <label className="mb-1.5 block text-sm font-medium text-gray-700">
                    Select Image
                  </label>

                  <input
                    type="file"
                    accept="image/*"
                    onChange={
                      handleFileChange
                    }
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none file:mr-4 file:rounded-md file:border-0 file:bg-[#1B2A4A] file:px-4 file:py-2 file:text-sm file:font-medium file:text-white hover:file:bg-[#142039]"
                  />

                  <p className="mt-1.5 text-xs text-gray-500">
                    Supported image files
                    only. Maximum size: 5 MB.
                  </p>

                  {form.file && (
                    <p className="mt-2 text-sm text-gray-600">
                      Selected:{" "}
                      <span className="font-medium">
                        {form.file.name}
                      </span>
                    </p>
                  )}
                </>
              )}
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-gray-700">
                Alt Text
              </label>

              <input
                type="text"
                value={form.altText}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    altText:
                      event.target.value,
                  }))
                }
                placeholder="Product image description"
                className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
              />
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-gray-700">
                Display Order
              </label>

              <input
                type="number"
                min="0"
                value={form.displayOrder}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    displayOrder:
                      event.target.value,
                  }))
                }
                className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
              />
            </div>
          </div>

          <div className="mt-5 flex items-center gap-3">
            <input
              id="product-image-primary"
              type="checkbox"
              checked={form.isPrimary}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  isPrimary:
                    event.target.checked,
                }))
              }
              className="h-4 w-4"
            />

            <label
              htmlFor="product-image-primary"
              className="text-sm font-medium text-gray-700"
            >
              Set as primary image
            </label>
          </div>

          {(form.imageUrl ||
            selectedFilePreview) && (
            <div className="mt-5">
              <p className="mb-2 text-sm font-medium text-gray-700">
                Preview
              </p>

              <div className="relative h-40 w-40 overflow-hidden rounded-lg border border-gray-200 bg-gray-50">
                {selectedFilePreview ? (
                  <img
                    src={selectedFilePreview}
                    alt={
                      form.altText ||
                      "Product image preview"
                    }
                    className="h-full w-full object-contain"
                  />
                ) : (
                  <Image
                    src={
                      getApiAssetUrl(
                        form.imageUrl,
                      ) ?? form.imageUrl
                    }
                    alt={
                      form.altText ||
                      "Product image preview"
                    }
                    fill
                    className="object-contain"
                    sizes="160px"
                  />
                )}
              </div>
            </div>
          )}

          <div className="mt-6 flex gap-3">
            <button
              type="submit"
              disabled={saving}
              className="rounded-lg bg-[#1B2A4A] px-5 py-2.5 text-sm font-medium text-white hover:bg-[#142039] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {saving
                ? "Saving..."
                : editingImage
                  ? "Update Image"
                  : "Add Image"}
            </button>

            <button
              type="button"
              onClick={closeForm}
              disabled={saving}
              className="rounded-lg border border-gray-300 px-5 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      {loading ? (
        <div className="rounded-xl bg-white p-6 shadow-sm">
          <p className="text-sm text-gray-500">
            Loading images...
          </p>
        </div>
      ) : images.length === 0 ? (
        <div className="rounded-xl bg-white p-8 text-center shadow-sm">
          <p className="text-sm text-gray-500">
            No images added yet.
          </p>

          {!showForm && (
            <button
              type="button"
              onClick={openAddForm}
              className="mt-4 text-sm font-medium text-[#1B2A4A] hover:underline"
            >
              Add the first image
            </button>
          )}
        </div>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {images.map((image) => {
            const imageUrl =
              getProductImageUrl(image);

            return (
              <div
                key={image.id}
                className="overflow-hidden rounded-xl bg-white shadow-sm"
              >
                <div className="relative h-48 bg-gray-50">
                  {imageUrl && (
  <img
    src={imageUrl}
    alt={
      image.altText ||
      "Product image"
    }
    className="h-full w-full object-contain p-4"
  />
)}

                  {image.isPrimary && (
                    <span className="absolute left-3 top-3 rounded-full bg-[#1B2A4A] px-3 py-1 text-xs font-medium text-white">
                      Primary
                    </span>
                  )}
                </div>

                <div className="p-4">
                  <p className="truncate text-sm font-medium text-gray-900">
                    {image.altText ||
                      "No alt text"}
                  </p>

                  <p className="mt-1 text-xs text-gray-500">
                    Display order:{" "}
                    {image.displayOrder}
                  </p>

                  <div className="mt-4 flex gap-4">
                    <button
                      type="button"
                      onClick={() =>
                        openEditForm(image)
                      }
                      className="text-sm font-medium text-[#1B2A4A] hover:underline"
                    >
                      Edit
                    </button>

                    <button
                      type="button"
                      onClick={() =>
                        handleDelete(image)
                      }
                      className="text-sm font-medium text-red-600 hover:underline"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </section>
  );
}