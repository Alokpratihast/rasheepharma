"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import {
  ArrowLeft,
  Edit3,
  Loader2,
  MapPin,
  Plus,
  Trash2,
  X,
} from "lucide-react";
import { toast } from "sonner";

import { addressService } from "@/services/address.service";
import type {
  Address,
  CreateAddressRequest,
} from "@/types/address";

const emptyForm: CreateAddressRequest = {
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  postalCode: "",
  country: "India",
  addressType: "Home",
  isDefault: false,
};

export function AddressesPage() {
  const [addresses, setAddresses] = useState<Address[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [form, setForm] =
    useState<CreateAddressRequest>(emptyForm);

  const [isSaving, setIsSaving] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  async function loadAddresses() {
    try {
      setIsLoading(true);

      const data = await addressService.getAll();

      setAddresses(data);
    } catch (error) {
      console.error("Failed to load addresses:", error);
      toast.error("Unable to load addresses.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadAddresses();
  }, []);

  function openAddForm() {
    setEditingId(null);
    setForm(emptyForm);
    setIsFormOpen(true);
  }

  function openEditForm(address: Address) {
    setEditingId(address.id);

    setForm({
      addressLine1: address.addressLine1,
      addressLine2: address.addressLine2 ?? "",
      city: address.city,
      state: address.state ?? "",
      postalCode: address.postalCode,
      country: address.country,
      addressType: address.addressType ?? "Home",
      isDefault: address.isDefault,
    });

    setIsFormOpen(true);
  }

  function closeForm() {
    if (isSaving) return;

    setIsFormOpen(false);
    setEditingId(null);
    setForm(emptyForm);
  }

  function handleChange(
    field: keyof CreateAddressRequest,
    value: string | boolean
  ) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  }

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (
      !form.addressLine1.trim() ||
      !form.city.trim() ||
      !form.postalCode.trim() ||
      !form.country.trim()
    ) {
      toast.error("Please fill all required fields.");
      return;
    }

    try {
      setIsSaving(true);

      const payload: CreateAddressRequest = {
        addressLine1: form.addressLine1.trim(),
        addressLine2: form.addressLine2?.trim() || null,
        city: form.city.trim(),
        state: form.state?.trim() || null,
        postalCode: form.postalCode.trim(),
        country: form.country.trim(),
        addressType: form.addressType?.trim() || null,
        isDefault: form.isDefault,
      };

      if (editingId) {
        const updated = await addressService.update(
          editingId,
          payload
        );

        setAddresses((current) =>
          current.map((address) =>
            address.id === editingId ? updated : address
          )
        );

        toast.success("Address updated successfully.");
      } else {
        const created = await addressService.create(payload);

        setAddresses((current) => {
          if (created.isDefault) {
            return [
              created,
              ...current.map((address) => ({
                ...address,
                isDefault: false,
              })),
            ];
          }

          return [created, ...current];
        });

        toast.success("Address added successfully.");
      }

      closeForm();
    } catch (error) {
      console.error("Failed to save address:", error);
      toast.error("Unable to save address.");
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      "Are you sure you want to delete this address?"
    );

    if (!confirmed) return;

    try {
      setDeletingId(id);

      await addressService.remove(id);

      setAddresses((current) =>
        current.filter((address) => address.id !== id)
      );

      toast.success("Address deleted successfully.");
    } catch (error) {
      console.error("Failed to delete address:", error);
      toast.error("Unable to delete address.");
    } finally {
      setDeletingId(null);
    }
  }

  if (isLoading) {
    return (
      <div className="flex min-h-[500px] items-center justify-center">
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Loader2 className="size-5 animate-spin" />
          Loading addresses...
        </div>
      </div>
    );
  }

  return (
    <section className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <Link
            href="/profile"
            className="mb-3 inline-flex items-center gap-2 text-sm text-gray-500 transition-colors hover:text-[#1B2A4A]"
          >
            <ArrowLeft className="size-4" />
            Back to Profile
          </Link>

          <p className="text-sm font-medium text-[#3E8F96]">
            My Account
          </p>

          <h1 className="mt-1 text-2xl font-semibold text-[#1B2A4A] sm:text-3xl">
            My Addresses
          </h1>

          <p className="mt-2 text-sm text-gray-500">
            Manage your delivery and shipping addresses.
          </p>
        </div>

        <button
          type="button"
          onClick={openAddForm}
          className="inline-flex w-fit items-center gap-2 rounded-lg bg-[#3E8F96] px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-[#347a80]"
        >
          <Plus className="size-4" />
          Add Address
        </button>
      </div>

      {/* Address List */}
      {addresses.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-gray-300 bg-white px-5 py-16 text-center">
          <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-[#EAF5F3]">
            <MapPin className="size-7 text-[#3E8F96]" />
          </div>

          <h2 className="mt-4 text-lg font-semibold text-[#1B2A4A]">
            No addresses saved
          </h2>

          <p className="mt-2 text-sm text-gray-500">
            Add an address to make checkout faster.
          </p>

          <button
            type="button"
            onClick={openAddForm}
            className="mt-5 inline-flex items-center gap-2 rounded-lg bg-[#3E8F96] px-4 py-2.5 text-sm font-medium text-white hover:bg-[#347a80]"
          >
            <Plus className="size-4" />
            Add Your First Address
          </button>
        </div>
      ) : (
        <div className="grid gap-5 md:grid-cols-2">
          {addresses.map((address) => (
            <div
              key={address.id}
              className="rounded-2xl border border-[#e5e8e7] bg-white p-5 shadow-sm"
            >
              {/* Card Header */}
              <div className="flex items-start justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className="flex size-10 items-center justify-center rounded-lg bg-[#EAF5F3]">
                    <MapPin className="size-5 text-[#3E8F96]" />
                  </div>

                  <div>
                    <h2 className="font-semibold text-[#1B2A4A]">
                      {address.addressType || "Address"}
                    </h2>

                    {address.isDefault && (
                      <span className="mt-1 inline-flex rounded-full bg-[#EAF5F3] px-2 py-1 text-[10px] font-semibold uppercase tracking-wide text-[#3E8F96]">
                        Default
                      </span>
                    )}
                  </div>
                </div>
              </div>

              {/* Address */}
              <div className="mt-5 space-y-1 text-sm leading-6 text-gray-600">
                <p>{address.addressLine1}</p>

                {address.addressLine2 && (
                  <p>{address.addressLine2}</p>
                )}

                <p>
                  {address.city}
                  {address.state && `, ${address.state}`}
                </p>

                <p>{address.postalCode}</p>

                <p>{address.country}</p>
              </div>

              {/* Actions */}
              <div className="mt-5 flex gap-3 border-t border-gray-100 pt-4">
                <button
                  type="button"
                  onClick={() => openEditForm(address)}
                  className="inline-flex items-center gap-2 rounded-lg border border-gray-200 px-3 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
                >
                  <Edit3 className="size-4" />
                  Edit
                </button>

                <button
                  type="button"
                  onClick={() => handleDelete(address.id)}
                  disabled={deletingId === address.id}
                  className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm font-medium text-red-600 transition-colors hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {deletingId === address.id ? (
                    <Loader2 className="size-4 animate-spin" />
                  ) : (
                    <Trash2 className="size-4" />
                  )}

                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add / Edit Modal */}
      {isFormOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 px-4 py-6">
          <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white shadow-2xl">
            {/* Modal Header */}
            <div className="flex items-center justify-between border-b border-gray-100 px-5 py-4 sm:px-6">
              <div>
                <h2 className="text-lg font-semibold text-[#1B2A4A]">
                  {editingId ? "Edit Address" : "Add New Address"}
                </h2>

                <p className="mt-1 text-xs text-gray-500">
                  Enter your delivery address details.
                </p>
              </div>

              <button
                type="button"
                onClick={closeForm}
                disabled={isSaving}
                className="flex size-9 items-center justify-center rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-800"
              >
                <X className="size-5" />
              </button>
            </div>

            {/* Form */}
            <form
              onSubmit={handleSubmit}
              className="space-y-5 p-5 sm:p-6"
            >
              {/* Address Type */}
              <div>
                <label
                  htmlFor="addressType"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Address Type
                </label>

                <select
                  id="addressType"
                  value={form.addressType ?? ""}
                  onChange={(event) =>
                    handleChange(
                      "addressType",
                      event.target.value
                    )
                  }
                  disabled={isSaving}
                  className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                >
                  <option value="Home">Home</option>
                  <option value="Work">Work</option>
                  <option value="Office">Office</option>
                  <option value="Other">Other</option>
                </select>
              </div>

              {/* Address Line 1 */}
              <div>
                <label
                  htmlFor="addressLine1"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Address Line 1 *
                </label>

                <input
                  id="addressLine1"
                  type="text"
                  value={form.addressLine1}
                  onChange={(event) =>
                    handleChange(
                      "addressLine1",
                      event.target.value
                    )
                  }
                  placeholder="House number, street name"
                  disabled={isSaving}
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                />
              </div>

              {/* Address Line 2 */}
              <div>
                <label
                  htmlFor="addressLine2"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Address Line 2
                </label>

                <input
                  id="addressLine2"
                  type="text"
                  value={form.addressLine2 ?? ""}
                  onChange={(event) =>
                    handleChange(
                      "addressLine2",
                      event.target.value
                    )
                  }
                  placeholder="Apartment, landmark, area"
                  disabled={isSaving}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                />
              </div>

              {/* City / State */}
              <div className="grid gap-5 sm:grid-cols-2">
                <div>
                  <label
                    htmlFor="city"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    City *
                  </label>

                  <input
                    id="city"
                    type="text"
                    value={form.city}
                    onChange={(event) =>
                      handleChange(
                        "city",
                        event.target.value
                      )
                    }
                    placeholder="City"
                    disabled={isSaving}
                    required
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                  />
                </div>

                <div>
                  <label
                    htmlFor="state"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    State
                  </label>

                  <input
                    id="state"
                    type="text"
                    value={form.state ?? ""}
                    onChange={(event) =>
                      handleChange(
                        "state",
                        event.target.value
                      )
                    }
                    placeholder="State"
                    disabled={isSaving}
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                  />
                </div>
              </div>

              {/* Postal / Country */}
              <div className="grid gap-5 sm:grid-cols-2">
                <div>
                  <label
                    htmlFor="postalCode"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    Postal Code *
                  </label>

                  <input
                    id="postalCode"
                    type="text"
                    value={form.postalCode}
                    onChange={(event) =>
                      handleChange(
                        "postalCode",
                        event.target.value
                      )
                    }
                    placeholder="Postal code"
                    disabled={isSaving}
                    required
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                  />
                </div>

                <div>
                  <label
                    htmlFor="country"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    Country *
                  </label>

                  <input
                    id="country"
                    type="text"
                    value={form.country}
                    onChange={(event) =>
                      handleChange(
                        "country",
                        event.target.value
                      )
                    }
                    placeholder="Country"
                    disabled={isSaving}
                    required
                    className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20"
                  />
                </div>
              </div>

              {/* Default */}
              <label className="flex cursor-pointer items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3">
                <input
                  type="checkbox"
                  checked={form.isDefault}
                  onChange={(event) =>
                    handleChange(
                      "isDefault",
                      event.target.checked
                    )
                  }
                  disabled={isSaving}
                  className="size-4 accent-[#3E8F96]"
                />

                <span className="text-sm text-gray-700">
                  Make this my default address
                </span>
              </label>

              {/* Buttons */}
              <div className="flex justify-end gap-3 border-t border-gray-100 pt-5">
                <button
                  type="button"
                  onClick={closeForm}
                  disabled={isSaving}
                  className="rounded-lg border border-gray-200 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  disabled={isSaving}
                  className="inline-flex items-center gap-2 rounded-lg bg-[#3E8F96] px-5 py-2.5 text-sm font-medium text-white hover:bg-[#347a80] disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {isSaving && (
                    <Loader2 className="size-4 animate-spin" />
                  )}

                  {isSaving
                    ? "Saving..."
                    : editingId
                      ? "Update Address"
                      : "Save Address"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}