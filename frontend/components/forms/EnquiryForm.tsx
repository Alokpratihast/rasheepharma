"use client";

import { FormEvent, useState } from "react";
import { Send } from "lucide-react";
import type { Country } from "react-phone-number-input";

import { Button } from "@/components/ui/button";
import { PhoneCountryFields } from "@/components/forms/PhoneCountryFields";

interface EnquiryFormProps {
  productName?: string;
  productVariantId?: number;
  onSuccess?: () => void;
}

export function EnquiryForm({
  productName,
  productVariantId,
  onSuccess,
}: EnquiryFormProps) {
  const [customerName, setCustomerName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState<string>();
  const [country, setCountry] = useState<Country>("IN");
  const [businessType, setBusinessType] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [message, setMessage] = useState("");

  const isProductEnquiry = Boolean(productVariantId);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    // API integration will be added separately.
    onSuccess?.();
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-2xl border border-[#e5e8e7] bg-white p-6 shadow-sm sm:p-8"
    >
      {/* =================================================
          HEADER
      ================================================== */}

      <div className="mb-6">
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
          Business Enquiry
        </p>

        <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A]">
          {isProductEnquiry
            ? `Enquire about ${productName ?? "this product"}`
            : "Send us your enquiry"}
        </h2>

        <p className="mt-2 text-sm leading-6 text-[#595959]">
          Share your requirements and our team will get back to you.
        </p>
      </div>

      {/* =================================================
          SELECTED PRODUCT
      ================================================== */}

      {isProductEnquiry && (
        <div className="mb-6 rounded-xl border border-[#dcebea] bg-[#F4FAF8] p-4">
          <p className="text-xs font-medium uppercase tracking-wide text-[#3E8F96]">
            Selected Product
          </p>

          <p className="mt-1 text-sm font-semibold text-[#1B2A4A]">
            {productName}
          </p>
        </div>
      )}

      {/* =================================================
          CUSTOMER DETAILS
      ================================================== */}

      <div className="grid gap-5 sm:grid-cols-2">
        {/* Customer Name */}
        <div>
          <label
            htmlFor="customer-name"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Customer Name{" "}
            <span className="text-[#F5821F]">*</span>
          </label>

          <input
            id="customer-name"
            name="customerName"
            type="text"
            required
            autoComplete="name"
            value={customerName}
            onChange={(event) =>
              setCustomerName(event.target.value)
            }
            placeholder="Enter your name"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />
        </div>

        {/* Email */}
        <div>
          <label
            htmlFor="email"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Email <span className="text-[#F5821F]">*</span>
          </label>

          <input
            id="email"
            name="email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) =>
              setEmail(event.target.value)
            }
            placeholder="you@company.com"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />
        </div>
      </div>

      {/* =================================================
          PHONE + COUNTRY
      ================================================== */}

      <div className="mt-5">
        <PhoneCountryFields
          country={country}
          phoneNumber={phoneNumber}
          onCountryChange={setCountry}
          onPhoneChange={setPhoneNumber}
        />
      </div>

      {/* =================================================
          BUSINESS + QUANTITY
      ================================================== */}

      <div className="mt-5 grid gap-5 sm:grid-cols-2">
        {/* Business Type */}
        <div>
          <label
            htmlFor="business-type"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Business Type
          </label>

          <select
            id="business-type"
            name="businessType"
            value={businessType}
            onChange={(event) =>
              setBusinessType(event.target.value)
            }
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
          >
            <option value="">
              Select business type
            </option>

            <option value="Distributor">
              Distributor
            </option>

            <option value="Wholesaler">
              Wholesaler
            </option>

            <option value="Retailer">
              Retailer
            </option>

            <option value="Exporter">
              Exporter
            </option>

            <option value="Hospital">
              Hospital
            </option>

            <option value="Pharmacy">
              Pharmacy
            </option>

            <option value="Other">
              Other
            </option>
          </select>
        </div>

        {/* Quantity */}
        {isProductEnquiry && (
          <div>
            <label
              htmlFor="quantity"
              className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
            >
              Quantity
            </label>

            <input
              id="quantity"
              name="quantity"
              type="number"
              min="1"
              inputMode="numeric"
              value={quantity}
              onChange={(event) =>
                setQuantity(event.target.value)
              }
              className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
            />
          </div>
        )}
      </div>

      {/* =================================================
          MESSAGE
      ================================================== */}

      <div className="mt-5">
        <label
          htmlFor="message"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Enquiry Details
        </label>

        <textarea
          id="message"
          name="message"
          rows={5}
          value={message}
          onChange={(event) =>
            setMessage(event.target.value)
          }
          placeholder={
            isProductEnquiry
              ? "Tell us about your product requirement..."
              : "Tell us how we can help..."
          }
          className="w-full resize-none rounded-lg border border-[#dfe4e3] bg-white px-3 py-3 text-sm leading-6 text-[#1B2A4A] outline-none placeholder:text-[#999] focus:border-[#3E8F96]"
        />
      </div>

      {/* =================================================
          SUBMIT
      ================================================== */}

      <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-xs leading-5 text-[#777]">
          Fields marked with * are required.
        </p>

        <Button
          type="submit"
          size="lg"
          className="h-11 rounded-lg bg-[#F5821F] px-6 text-white hover:bg-[#df7115]"
        >
          Send Enquiry
          <Send className="size-4" />
        </Button>
      </div>
    </form>
  );
}