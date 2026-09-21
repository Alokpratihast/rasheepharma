"use client";

import { useEffect, useState } from "react";
import { MapPin, Plus, Check, Loader2 } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { addressService } from "@/services/address.service";
import type {
  Address,
  CreateAddressRequest,
} from "@/types/address";

interface CheckoutAddressProps {
  selectedAddressId: number | null;
  onAddressSelect: (addressId: number) => void;
}

const emptyAddressForm: CreateAddressRequest = {
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  postalCode: "",
  country: "India",
  addressType: "Home",
  isDefault: false,
};

export function CheckoutAddress({
  selectedAddressId,
  onAddressSelect,
}: CheckoutAddressProps) {
  const [addresses, setAddresses] = useState<Address[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAdding, setIsAdding] = useState(false);
  const [showForm, setShowForm] = useState(false);

  const [form, setForm] =
    useState<CreateAddressRequest>(emptyAddressForm);

  useEffect(() => {
    const loadAddresses = async () => {
      try {
        setIsLoading(true);

        const data = await addressService.getAll();

        setAddresses(data);

        if (data.length > 0 && selectedAddressId === null) {
          const defaultAddress =
            data.find((address) => address.isDefault) ?? data[0];

          onAddressSelect(defaultAddress.id);
        }
      } catch (error) {
        console.error("Failed to load addresses:", error);
        toast.error("Unable to load your saved addresses.");
      } finally {
        setIsLoading(false);
      }
    };

    loadAddresses();
  }, [ selectedAddressId]);

  const handleInputChange = (
    field: keyof CreateAddressRequest,
    value: string | boolean
  ) => {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleAddAddress = async () => {
    if (
      !form.addressLine1.trim() ||
      !form.city.trim() ||
      !form.postalCode.trim() ||
      !form.country.trim()
    ) {
      toast.error(
        "Please fill in address, city, postal code and country."
      );
      return;
    }

    try {
      setIsAdding(true);

      const newAddress = await addressService.create(form);

      setAddresses((current) => {
        if (newAddress.isDefault) {
          return [
            ...current.map((address) => ({
              ...address,
              isDefault: false,
            })),
            newAddress,
          ];
        }

        return [...current, newAddress];
      });

      onAddressSelect(newAddress.id);

      setForm(emptyAddressForm);
      setShowForm(false);

      toast.success("Address added successfully.");
    } catch (error) {
      console.error("Failed to add address:", error);
      toast.error("Unable to add address. Please try again.");
    } finally {
      setIsAdding(false);
    }
  };

  if (isLoading) {
    return (
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
        <div className="flex items-center gap-2 text-sm text-[#666]">
          <Loader2 className="size-4 animate-spin" />
          Loading saved addresses...
        </div>
      </section>
    );
  }

  return (
    <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-lg font-bold text-[#1B2A4A]">
            Delivery Address
          </h2>

          <p className="mt-1 text-sm text-[#777]">
            Select where you want your order delivered.
          </p>
        </div>

        <MapPin className="size-5 text-[#3E8F96]" />
      </div>

      {addresses.length > 0 && (
        <div className="mt-5 space-y-3">
          {addresses.map((address) => {
            const isSelected =
              selectedAddressId === address.id;

            return (
              <button
                key={address.id}
                type="button"
                onClick={() => onAddressSelect(address.id)}
                className={`w-full rounded-xl border p-4 text-left transition ${
                  isSelected
                    ? "border-[#3E8F96] bg-[#E8F4F4] ring-1 ring-[#3E8F96]"
                    : "border-[#e1e5e4] bg-white hover:border-[#3E8F96]/50"
                }`}
              >
                <div className="flex items-start gap-3">
                  <div
                    className={`mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full border ${
                      isSelected
                        ? "border-[#3E8F96] bg-[#3E8F96] text-white"
                        : "border-[#bbb] bg-white"
                    }`}
                  >
                    {isSelected && (
                      <Check className="size-3.5" />
                    )}
                  </div>

                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-semibold text-[#1B2A4A]">
                        {address.addressType || "Address"}
                      </p>

                      {address.isDefault && (
                        <span className="rounded-full bg-[#E8F4F4] px-2 py-0.5 text-[11px] font-medium text-[#3E8F96]">
                          Default
                        </span>
                      )}
                    </div>

                    <p className="mt-2 text-sm leading-6 text-[#555]">
                      {address.addressLine1}
                      {address.addressLine2 && (
                        <>
                          <br />
                          {address.addressLine2}
                        </>
                      )}
                      <br />
                      {address.city}
                      {address.state && `, ${address.state}`}{" "}
                      - {address.postalCode}
                      <br />
                      {address.country}
                    </p>
                  </div>
                </div>
              </button>
            );
          })}
        </div>
      )}

      {addresses.length === 0 && !showForm && (
        <div className="mt-5 rounded-xl border border-dashed border-[#d7dcdb] bg-[#FAFAFA] p-5 text-center">
          <MapPin className="mx-auto size-8 text-[#999]" />

          <p className="mt-3 font-medium text-[#1B2A4A]">
            No saved address
          </p>

          <p className="mt-1 text-sm text-[#777]">
            Add an address to continue with your order.
          </p>
        </div>
      )}

      {!showForm && (
        <Button
          type="button"
          variant="outline"
          onClick={() => setShowForm(true)}
          className="mt-5 w-full border-[#3E8F96] text-[#3E8F96] hover:bg-[#E8F4F4]"
        >
          <Plus className="mr-2 size-4" />
          Add New Address
        </Button>
      )}

      {showForm && (
        <div className="mt-5 rounded-xl border border-[#e5e8e7] bg-[#FAFAFA] p-4 sm:p-5">
          <div className="mb-4">
            <h3 className="font-semibold text-[#1B2A4A]">
              Add New Address
            </h3>

            <p className="mt-1 text-xs text-[#777]">
              Enter your complete delivery address.
            </p>
          </div>

          <div className="space-y-4">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                Address Line 1 *
              </label>

              <input
                type="text"
                value={form.addressLine1}
                onChange={(event) =>
                  handleInputChange(
                    "addressLine1",
                    event.target.value
                  )
                }
                placeholder="House / Flat / Street"
                className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
              />
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                Address Line 2
              </label>

              <input
                type="text"
                value={form.addressLine2 ?? ""}
                onChange={(event) =>
                  handleInputChange(
                    "addressLine2",
                    event.target.value
                  )
                }
                placeholder="Apartment, landmark, etc."
                className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                  City *
                </label>

                <input
                  type="text"
                  value={form.city}
                  onChange={(event) =>
                    handleInputChange(
                      "city",
                      event.target.value
                    )
                  }
                  placeholder="Bangalore"
                  className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
                />
              </div>

              <div>
                <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                  State
                </label>

                <input
                  type="text"
                  value={form.state ?? ""}
                  onChange={(event) =>
                    handleInputChange(
                      "state",
                      event.target.value
                    )
                  }
                  placeholder="Karnataka"
                  className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                  Postal Code *
                </label>

                <input
                  type="text"
                  inputMode="numeric"
                  value={form.postalCode}
                  onChange={(event) =>
                    handleInputChange(
                      "postalCode",
                      event.target.value
                    )
                  }
                  placeholder="560001"
                  className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
                />
              </div>

              <div>
                <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                  Country *
                </label>

                <input
                  type="text"
                  value={form.country}
                  onChange={(event) =>
                    handleInputChange(
                      "country",
                      event.target.value
                    )
                  }
                  placeholder="India"
                  className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none transition focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
                />
              </div>
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-[#1B2A4A]">
                Address Type
              </label>

              <select
                value={form.addressType ?? "Home"}
                onChange={(event) =>
                  handleInputChange(
                    "addressType",
                    event.target.value
                  )
                }
                className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm outline-none focus:border-[#3E8F96] focus:ring-1 focus:ring-[#3E8F96]"
              >
                <option value="Home">Home</option>
                <option value="Office">Office</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <label className="flex cursor-pointer items-center gap-2 text-sm text-[#555]">
              <input
                type="checkbox"
                checked={form.isDefault}
                onChange={(event) =>
                  handleInputChange(
                    "isDefault",
                    event.target.checked
                  )
                }
                className="size-4 accent-[#3E8F96]"
              />

              Make this my default address
            </label>

            <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setForm(emptyAddressForm);
                  setShowForm(false);
                }}
                disabled={isAdding}
                className="border-[#dfe4e3]"
              >
                Cancel
              </Button>

              <Button
                type="button"
                onClick={handleAddAddress}
                disabled={isAdding}
                className="bg-[#3E8F96] text-white hover:bg-[#347b81]"
              >
                {isAdding ? (
                  <>
                    <Loader2 className="mr-2 size-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Plus className="mr-2 size-4" />
                    Save Address
                  </>
                )}
              </Button>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}