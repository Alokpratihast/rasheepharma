"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { CheckCircle2, Send } from "lucide-react";
import { useRouter } from "next/navigation";
import type { Country } from "react-phone-number-input";

import { Button } from "@/components/ui/button";
import { PhoneCountryFields } from "@/components/forms/PhoneCountryFields";
import { productService } from "@/services/product.service";
import { enquiryService } from "@/services/enquiry.service";
import { useAuth } from "@/components/providers/AuthProvider";

import type { ProductList, ProductVariant } from "@/types/product";

interface EnquiryFormProps {
  productName?: string;
  productVariantId?: number;
  onSuccess?: () => void;
}

const PENDING_ENQUIRY_KEY = "pending_enquiry";

interface PendingEnquiry {
  customerName: string;
  email: string;
  phoneNumber?: string;
  country: Country;
  businessType: string;
  quantity: string;
  message: string;
  selectedProductId: string;
  selectedVariantId: string;
}

function readPendingEnquiry(): PendingEnquiry | null {
  if (typeof window === "undefined") return null;

  const stored = window.sessionStorage.getItem(PENDING_ENQUIRY_KEY);
  if (!stored) return null;

  try {
    return JSON.parse(stored) as PendingEnquiry;
  } catch (error) {
    console.error("Failed to parse pending enquiry:", error);
    return null;
  }
}

export function EnquiryForm(props: EnquiryFormProps) {
  const { token, isAuthenticated, isLoading: authLoading } = useAuth();
  const initialPending = useMemo(
    () => (!authLoading && isAuthenticated ? readPendingEnquiry() : null),
    [authLoading, isAuthenticated],
  );

  useEffect(() => {
    if (!authLoading && isAuthenticated) {
      window.sessionStorage.removeItem(PENDING_ENQUIRY_KEY);
    }
  }, [authLoading, isAuthenticated, initialPending]);

  if (authLoading) return null;

  return (
    <EnquiryFormContent
      key={isAuthenticated ? "authenticated" : "guest"}
      {...props}
      token={token}
      isAuthenticated={isAuthenticated}
      initialPending={initialPending}
    />
  );
}

interface EnquiryFormContentProps extends EnquiryFormProps {
  token: string | null;
  isAuthenticated: boolean;
  initialPending: PendingEnquiry | null;
}

function EnquiryFormContent({
  productName,
  productVariantId,
  onSuccess,
  token,
  isAuthenticated,
  initialPending,
}: EnquiryFormContentProps) {
  const router = useRouter();

  const [customerName, setCustomerName] = useState(initialPending?.customerName ?? "");
  const [email, setEmail] = useState(initialPending?.email ?? "");
  const [phoneNumber, setPhoneNumber] = useState<string | undefined>(initialPending?.phoneNumber);
  const [country, setCountry] = useState<Country>(initialPending?.country ?? "IN");
  const [businessType, setBusinessType] = useState(initialPending?.businessType ?? "");
  const [quantity, setQuantity] = useState(initialPending?.quantity ?? "1");
  const [message, setMessage] = useState(initialPending?.message ?? "");

  const [products, setProducts] = useState<ProductList[]>([]);
  const [productsLoaded, setProductsLoaded] = useState(false);
  const loadingProducts = !productsLoaded;
  const [selectedProductId, setSelectedProductId] = useState(
    initialPending?.selectedProductId ?? "",
  );

  const [variants, setVariants] = useState<ProductVariant[]>([]);
  const [loadedVariantProductId, setLoadedVariantProductId] = useState("");
  const [selectedVariantId, setSelectedVariantId] = useState(
    productVariantId ? String(productVariantId) : initialPending?.selectedVariantId ?? "",
  );
  const loadingVariants =
    Boolean(selectedProductId) && loadedVariantProductId !== selectedProductId;
  const [submitting, setSubmitting] = useState(false);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const isProductEnquiry = Boolean(productVariantId);

  /*
   * =================================================
   * LOAD PRODUCTS
   * =================================================
   */

  useEffect(() => {
    let isActive = true;

    const loadProducts = async () => {
      try {
        const data = await productService.getAll();
        if (!isActive) return;

        const activeProducts = data.filter(
          (product) => product.isActive,
        );

        setProducts(activeProducts);
      } catch (error) {
        if (!isActive) return;
        console.error("Failed to load products:", error);
        setError("Unable to load products. Please try again.");
      } finally {
        if (isActive) setProductsLoaded(true);
      }
    };

    void loadProducts();

    return () => {
      isActive = false;
    };
  }, []);

  /*
   * =================================================
   * LOAD VARIANTS
   * =================================================
   */

  useEffect(() => {
    if (!selectedProductId) return;

    let isActive = true;
    const requestedProductId = selectedProductId;

    const loadVariants = async () => {
      try {
        const product = await productService.getById(
          Number(requestedProductId),
        );
        if (!isActive) return;

        const activeVariants = product.variants.filter(
          (variant) => variant.isActive,
        );

        setVariants(activeVariants);
        setError("");
      } catch (error) {
        if (!isActive) return;
        console.error(
          "Failed to load product variants:",
          error,
        );
        setVariants([]);
        setError(
          "Unable to load product variants. Please try again.",
        );
      } finally {
        if (isActive) setLoadedVariantProductId(requestedProductId);
      }
    };

    void loadVariants();

    return () => {
      isActive = false;
    };
  }, [selectedProductId]);

  /*
   * =================================================
   * HANDLE SUBMIT
   * =================================================
   */

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    setError("");
    setSuccess("");

    /*
     * =================================================
     * GUEST USER
     * =================================================
     *
     * Save form data and redirect to login.
     */

    if (!isAuthenticated || !token) {
      const pendingEnquiry: PendingEnquiry = {
        customerName,
        email,
        phoneNumber,
        country,
        businessType,
        quantity,
        message,
        selectedProductId,
        selectedVariantId: selectedVariantId && variants.some((variant) => String(variant.id) === selectedVariantId) ? selectedVariantId : "",
      };

      sessionStorage.setItem(
        PENDING_ENQUIRY_KEY,
        JSON.stringify(pendingEnquiry),
      );

      router.push("/login?redirect=enquiry");

      return;
    }

    /*
     * =================================================
     * VALIDATION
     * =================================================
     */

    const validSelectedVariantId = variants.some(
      (variant) => String(variant.id) === selectedVariantId,
    )
      ? selectedVariantId
      : "";

    if (selectedProductId && !validSelectedVariantId) {
      setError(
        "Please select a product variant before submitting.",
      );

      return;
    }

    const parsedQuantity = Number(quantity);

    if (
      validSelectedVariantId &&
      (!Number.isInteger(parsedQuantity) ||
        parsedQuantity <= 0)
    ) {
      setError("Please enter a valid quantity.");

      return;
    }

    /*
     * =================================================
     * CREATE ENQUIRY
     * =================================================
     */

    try {
      setSubmitting(true);

      const items = validSelectedVariantId
        ? [
            {
              productVariantId: Number(validSelectedVariantId),
              quantity: parsedQuantity,
              message: message.trim() || null,
            },
          ]
        : [];

       const enquiry = await enquiryService.create(
        {
          customerName: customerName.trim(),
          email: email.trim(),
          phoneNumber: phoneNumber || null,
          country,
          businessType: businessType || null,
          message: message.trim() || null,
          items,
        },
        token,
      );

      setSuccess(
  enquiry.enquiryNumber
    ? `Your enquiry has been submitted successfully. Enquiry No: ${enquiry.enquiryNumber}`
    : "Your enquiry has been submitted successfully. Our team will get back to you soon.",
);

setCustomerName("");
setEmail("");
setPhoneNumber(undefined);
setBusinessType("");
setQuantity("1");
setMessage("");
setSelectedProductId("");
setSelectedVariantId("");
setVariants([]);
onSuccess?.();

    } catch (error) {
      console.error("Failed to submit enquiry:", error);

      if (
        error &&
        typeof error === "object" &&
        "message" in error &&
        typeof error.message === "string"
      ) {
        setError(error.message);
      } else {
        setError(
          "Unable to submit your enquiry. Please try again.",
        );
      }
    } finally {
      setSubmitting(false);
    }
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
            ? `Enquire about ${
                productName ?? "this product"
              }`
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

        <div>
          <label
            htmlFor="email"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Email{" "}
            <span className="text-[#F5821F]">*</span>
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
          BUSINESS TYPE
      ================================================== */}

      <div className="mt-5">
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

      {/* =================================================
          PRODUCT + VARIANT
      ================================================== */}

      <div className="mt-5 grid gap-5 sm:grid-cols-2">
        {/* Product */}

        <div>
          <label
            htmlFor="product"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Product
          </label>

          <select
            id="product"
            name="product"
            value={selectedProductId}
            onChange={(event) => {
              setSelectedProductId(event.target.value);
              setSelectedVariantId("");
              setVariants([]);
              setLoadedVariantProductId("");
              setError("");
            }}
            disabled={loadingProducts}
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96] disabled:cursor-not-allowed disabled:bg-[#f5f5f5]"
          >
            <option value="">
              {loadingProducts
                ? "Loading products..."
                : "Select product"}
            </option>

            {products.map((product) => (
              <option
                key={product.id}
                value={product.id}
              >
                {product.name}
              </option>
            ))}
          </select>
        </div>

        {/* Variant */}

        <div>
          <label
            htmlFor="product-variant"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Strength / Pack
          </label>

          <select
            id="product-variant"
            name="productVariant"
            value={variants.some((variant) => String(variant.id) === selectedVariantId) ? selectedVariantId : ""}
            onChange={(event) => {
              setSelectedVariantId(event.target.value);
              setError("");
            }}
            disabled={
              !selectedProductId ||
              loadingVariants ||
              variants.length === 0
            }
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96] disabled:cursor-not-allowed disabled:bg-[#f5f5f5]"
          >
            <option value="">
              {!selectedProductId
                ? "Select product first"
                : loadingVariants
                  ? "Loading variants..."
                  : variants.length === 0
                    ? "No variants available"
                    : "Select strength / pack"}
            </option>

            {variants.map((variant) => (
              <option
                key={variant.id}
                value={variant.id}
              >
                {[
                  variant.strength,
                  variant.packSize,
                ]
                  .filter(Boolean)
                  .join(" - ") ||
                  `Variant #${variant.id}`}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* =================================================
          QUANTITY
      ================================================== */}

      <div className="mt-5">
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
          disabled={!selectedVariantId}
          className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96] disabled:cursor-not-allowed disabled:bg-[#f5f5f5]"
        />
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
            selectedProductId
              ? "Tell us about your product requirement..."
              : "Tell us how we can help..."
          }
          className="w-full resize-none rounded-lg border border-[#dfe4e3] bg-white px-3 py-3 text-sm leading-6 text-[#1B2A4A] outline-none placeholder:text-[#999] focus:border-[#3E8F96]"
        />
      </div>

      {/* =================================================
          ERROR
      ================================================== */}

      {error && (
        <div className="mt-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {/* =================================================
          SUCCESS
      ================================================== */}

      {success && (
  <div className="mt-6 rounded-2xl border border-green-200 bg-green-50 p-6 text-center">
    <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-green-100">
      <CheckCircle2 className="size-8 text-green-600" />
    </div>

    <h3 className="mt-4 text-xl font-semibold text-[#1B2A4A]">
      Thank You!
    </h3>

    <p className="mt-2 text-sm leading-6 text-[#595959]">
      Your enquiry has been submitted successfully.
    </p>

    <p className="mt-1 text-sm leading-6 text-[#595959]">
      Our team will review your enquiry and get back to you soon.
    </p>

    <p className="mt-4 text-sm font-medium text-green-700">
      {success}
    </p>
  </div>
)}

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
          disabled={submitting}
          className="h-11 rounded-lg bg-[#F5821F] px-6 text-white hover:bg-[#df7115] disabled:cursor-not-allowed disabled:opacity-60"
        >
          {submitting ? "Sending..." : "Send Enquiry"}

          {!submitting && (
            <Send className="size-4" />
          )}
        </Button>
      </div>
    </form>
  );
}