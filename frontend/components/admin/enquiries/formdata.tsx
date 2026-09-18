  "use client";

  import { useEffect, useState } from "react";
  import {
    AlertCircle,
    CalendarDays,
    ChevronRight,
    ClipboardList,
    Eye,
    Loader2,
    Mail,
    Package,
    Phone,
    User,
    X,
    FileText,
  } from "lucide-react";

  import { enquiryService } from "@/services/enquiry.service";
  import { useAuth } from "@/components/providers/AuthProvider";
  import type { EnquiryDetails } from "@/types/enquiry";
  import { quotationService } from "@/services/quotation.service";
  import type { CreateQuotation } from "@/types/quotation";

  export default function EnquiriesPage() {
    const { token, isAuthenticated, isLoading: authLoading } = useAuth();

    const [enquiries, setEnquiries] = useState<EnquiryDetails[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [selectedEnquiry, setSelectedEnquiry] =
      useState<EnquiryDetails | null>(null);

    const [showQuotationForm, setShowQuotationForm] = useState(false);
const [quotationLoading, setQuotationLoading] = useState(false);
const [quotationError, setQuotationError] = useState("");
const [quotationSuccess, setQuotationSuccess] = useState("");

const [quotationCurrency, setQuotationCurrency] = useState("USD");
const [quotationValidUntil, setQuotationValidUntil] = useState("");
const [quotationNotes, setQuotationNotes] = useState("");

const [quotationPrices, setQuotationPrices] = useState<
  Record<number, string>
>({});

    useEffect(() => {
      if (authLoading) return;

      if (!isAuthenticated || !token) {
        setLoading(false);
        setError("You must be logged in to view enquiries.");
        return;
      }

      const loadEnquiries = async () => {
        try {
          setLoading(true);
          setError("");

          const data = await enquiryService.getAll(token);

          setEnquiries(data);
        } catch (err) {
          console.error("Failed to load enquiries:", err);

          setError(
            err instanceof Error
              ? err.message
              : "Failed to load enquiries.",
          );
        } finally {
          setLoading(false);
        }
      };

      loadEnquiries();
    }, [authLoading, isAuthenticated, token]);

    const formatDate = (date: string) => {
      return new Date(date).toLocaleDateString("en-IN", {
        day: "2-digit",
        month: "short",
        year: "numeric",
      });
    };

    const formatDateTime = (date: string) => {
      return new Date(date).toLocaleString("en-IN", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
    };

    const handleViewEnquiry = async (id: number) => {
  if (!token) return;

  try {
    setLoading(true);
    setError("");

    const details = await enquiryService.getById(id, token);

    setSelectedEnquiry(details);
  } catch (err) {
    console.error("Failed to load enquiry details:", err);

    setError(
      err instanceof Error
        ? err.message
        : "Failed to load enquiry details.",
    );
  } finally {
    setLoading(false);
  }
};

    const resetQuotationForm = () => {
      setShowQuotationForm(false);
      setQuotationLoading(false);
      setQuotationError("");
      setQuotationSuccess("");
      setQuotationCurrency("USD");
      setQuotationValidUntil("");
      setQuotationNotes("");
      setQuotationPrices({});
    };

    const handleCreateQuotation = async () => {
      if (!token || !selectedEnquiry) return;

      setQuotationError("");
      setQuotationSuccess("");

      if (!quotationValidUntil) {
        setQuotationError("Please select quotation validity date.");
        return;
      }

      const validUntil = new Date(`${quotationValidUntil}T23:59:59`);

      if (validUntil <= new Date()) {
        setQuotationError("Quotation validity date must be in the future.");
        return;
      }

      if (!selectedEnquiry.items?.length) {
        setQuotationError("No products found in this enquiry.");
        return;
      }

      const items = [];

      for (const item of selectedEnquiry.items) {
        const rawPrice = quotationPrices[item.productVariantId];
        const unitPrice = Number(rawPrice);

        if (!rawPrice || !Number.isFinite(unitPrice) || unitPrice <= 0) {
          setQuotationError(
            `Please enter a valid unit price for ${item.productName}.`,
          );
          return;
        }

        items.push({
          productVariantId: item.productVariantId,
          quantity: item.quantity,
          unitPrice,
        });
      }

      const payload: CreateQuotation = {
        enquiryId: selectedEnquiry.id,
        currency: quotationCurrency,
        validUntil: validUntil.toISOString(),
        notes: quotationNotes.trim() || null,
        items,
      };

      try {
        setQuotationLoading(true);

        const quotation = await quotationService.create(payload, token);

        setQuotationSuccess(
          `Quotation ${quotation.quoteNumber} created successfully.`,
        );
        setQuotationLoading(false);
      } catch (err) {
        console.error("Failed to create quotation:", err);

        setQuotationError(
          err instanceof Error
            ? err.message
            : "Failed to create quotation.",
        );
        setQuotationLoading(false);
      }
    };

    const getStatusClass = (status: string) => {
      switch (status.toLowerCase()) {
        case "pending":
          return "bg-yellow-50 text-yellow-700 border-yellow-200";

        case "approved":
          return "bg-green-50 text-green-700 border-green-200";

        case "rejected":
          return "bg-red-50 text-red-700 border-red-200";

        case "completed":
          return "bg-blue-50 text-blue-700 border-blue-200";

        default:
          return "bg-gray-50 text-gray-700 border-gray-200";
      }
    };

    if (authLoading || loading) {
      return (
        <div className="flex min-h-[400px] items-center justify-center">
          <div className="flex items-center gap-3 text-sm text-gray-500">
            <Loader2 className="size-5 animate-spin" />
            Loading enquiries...
          </div>
        </div>
      );
    }

    if (error) {
      return (
        <div className="p-6">
          <div className="rounded-xl border border-red-200 bg-red-50 p-5">
            <div className="flex items-start gap-3">
              <AlertCircle className="mt-0.5 size-5 text-red-600" />

              <div>
                <h2 className="font-semibold text-red-800">
                  Unable to load enquiries
                </h2>

                <p className="mt-1 text-sm text-red-700">
                  {error}
                </p>
              </div>
            </div>
          </div>
        </div>
      );
    }

    return (
      <>
        <div className="space-y-6 p-6">
          {/* Header */}
          <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
            <div>
              <div className="flex items-center gap-3">
                <div className="flex size-10 items-center justify-center rounded-xl bg-[#1B2A4A]">
                  <ClipboardList className="size-5 text-white" />
                </div>

                <div>
                  <h1 className="text-2xl font-semibold text-[#1B2A4A]">
                    Enquiries
                  </h1>

                  <p className="mt-1 text-sm text-gray-500">
                    Manage and review customer enquiries
                  </p>
                </div>
              </div>
            </div>

            <div className="rounded-xl border border-gray-200 bg-white px-4 py-3">
              <p className="text-xs text-gray-500">
                Total Enquiries
              </p>

              <p className="mt-1 text-xl font-semibold text-[#1B2A4A]">
                {enquiries.length}
              </p>
            </div>
          </div>

          {/* Empty State */}
          {enquiries.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-gray-300 bg-white px-6 py-16 text-center">
              <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-gray-100">
                <ClipboardList className="size-7 text-gray-400" />
              </div>

              <h2 className="mt-4 text-lg font-semibold text-[#1B2A4A]">
                No enquiries found
              </h2>

              <p className="mt-2 text-sm text-gray-500">
                Customer enquiries will appear here once submitted.
              </p>
            </div>
          ) : (
            /* Table */
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[900px]">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50">
                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Enquiry
                      </th>

                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Customer
                      </th>

                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Business
                      </th>

                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Items
                      </th>

                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Status
                      </th>

                      <th className="px-5 py-4 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Date
                      </th>

                      <th className="px-5 py-4 text-right text-xs font-semibold uppercase tracking-wide text-gray-500">
                        Action
                      </th>
                    </tr>
                  </thead>

                  <tbody className="divide-y divide-gray-100">
                    {enquiries.map((enquiry) => (
                      <tr
                        key={enquiry.id}
                        className="transition hover:bg-gray-50"
                      >
                        {/* Enquiry Number */}
                        <td className="px-5 py-4">
                          <p className="font-semibold text-[#1B2A4A]">
                            {enquiry.enquiryNumber}
                          </p>

                          <p className="mt-1 text-xs text-gray-400">
                            ID: #{enquiry.id}
                          </p>
                        </td>

                        {/* Customer */}
                        <td className="px-5 py-4">
                          <div className="flex items-start gap-3">
                            <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-gray-100">
                              <User className="size-4 text-gray-500" />
                            </div>

                            <div className="min-w-0">
                              <p className="font-medium text-gray-800">
                                {enquiry.customerName}
                              </p>

                              <p className="mt-1 flex items-center gap-1 text-xs text-gray-500">
                                <Mail className="size-3" />
                                {enquiry.email}
                              </p>

                              {enquiry.phoneNumber && (
                                <p className="mt-1 flex items-center gap-1 text-xs text-gray-500">
                                  <Phone className="size-3" />
                                  {enquiry.phoneNumber}
                                </p>
                              )}
                            </div>
                          </div>
                        </td>

                        {/* Business */}
                        <td className="px-5 py-4">
                          <p className="text-sm text-gray-700">
                            {enquiry.businessType || "—"}
                          </p>

                          <p className="mt-1 text-xs text-gray-400">
                            {enquiry.country}
                          </p>
                        </td>

                        {/* Items */}
                        <td className="px-5 py-4">
                          <div className="flex items-center gap-2">
                            <Package className="size-4 text-gray-400" />

                            <span className="text-sm font-medium text-gray-700">
                              {enquiry.items?.length??0}
                            </span>

                            {(enquiry.items?.length ?? 0) === 1
    ? "product"
    : "products"}
                          </div>
                        </td>

                        {/* Status */}
                        <td className="px-5 py-4">
                          <span
                            className={`inline-flex rounded-full border px-3 py-1 text-xs font-medium ${getStatusClass(
                              enquiry.status,
                            )}`}
                          >
                            {enquiry.status}
                          </span>
                        </td>

                        {/* Date */}
                        <td className="px-5 py-4">
                          <div className="flex items-center gap-2 text-sm text-gray-600">
                            <CalendarDays className="size-4 text-gray-400" />

                            {formatDate(enquiry.createdAt)}
                          </div>
                        </td>

                        {/* Action */}
                        <td className="px-5 py-4 text-right">
                          <button
  type="button"
  onClick={() => handleViewEnquiry(enquiry.id)}
  className="inline-flex items-center gap-2 rounded-lg border border-gray-200 px-3 py-2 text-sm font-medium text-[#1B2A4A] transition hover:border-[#1B2A4A] hover:bg-gray-50"
>
  <Eye className="size-4" />
  View
  <ChevronRight className="size-4" />
</button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        {/* Details Modal */}
        {selectedEnquiry && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="max-h-[90vh] w-full max-w-4xl overflow-hidden rounded-2xl bg-white shadow-2xl">
              {/* Modal Header */}
              <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
                <div>
                  <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                    Enquiry
                  </p>

                  <div className="mt-1 flex items-center gap-3">
                    <h2 className="text-xl font-semibold text-[#1B2A4A]">
                      {selectedEnquiry.enquiryNumber}
                    </h2>

                    <span
                      className={`rounded-full border px-3 py-1 text-xs font-medium ${getStatusClass(
                        selectedEnquiry.status,
                      )}`}
                    >
                      {selectedEnquiry.status}
                    </span>
                  </div>
                </div>

                <button
                  type="button"
                  onClick={() => {
                    setSelectedEnquiry(null);
                    resetQuotationForm();
                  }}
                  className="flex size-9 items-center justify-center rounded-lg text-gray-500 transition hover:bg-gray-100 hover:text-gray-700"
                >
                  <X className="size-5" />
                </button>
              </div>

              {/* Modal Body */}
              <div className="max-h-[calc(90vh-80px)] overflow-y-auto p-6">
                <div className="grid gap-6 lg:grid-cols-2">
                  {/* Customer Information */}
                  <div className="rounded-xl border border-gray-200 p-5">
                    <h3 className="font-semibold text-[#1B2A4A]">
                      Customer Information
                    </h3>

                    <div className="mt-4 space-y-4">
                      <div className="flex gap-3">
                        <User className="mt-0.5 size-4 text-gray-400" />

                        <div>
                          <p className="text-xs text-gray-400">
                            Name
                          </p>

                          <p className="mt-1 text-sm font-medium text-gray-800">
                            {selectedEnquiry.customerName}
                          </p>
                        </div>
                      </div>

                      <div className="flex gap-3">
                        <Mail className="mt-0.5 size-4 text-gray-400" />

                        <div>
                          <p className="text-xs text-gray-400">
                            Email
                          </p>

                          <p className="mt-1 text-sm font-medium text-gray-800">
                            {selectedEnquiry.email}
                          </p>
                        </div>
                      </div>

                      {selectedEnquiry.phoneNumber && (
                        <div className="flex gap-3">
                          <Phone className="mt-0.5 size-4 text-gray-400" />

                          <div>
                            <p className="text-xs text-gray-400">
                              Phone
                            </p>

                            <p className="mt-1 text-sm font-medium text-gray-800">
                              {selectedEnquiry.phoneNumber}
                            </p>
                          </div>
                        </div>
                      )}

                      <div className="flex gap-3">
                        <Package className="mt-0.5 size-4 text-gray-400" />

                        <div>
                          <p className="text-xs text-gray-400">
                            Business Type
                          </p>

                          <p className="mt-1 text-sm font-medium text-gray-800">
                            {selectedEnquiry.businessType || "—"}
                          </p>
                        </div>
                      </div>

                      <div className="flex gap-3">
                        <CalendarDays className="mt-0.5 size-4 text-gray-400" />

                        <div>
                          <p className="text-xs text-gray-400">
                            Submitted
                          </p>

                          <p className="mt-1 text-sm font-medium text-gray-800">
                            {formatDateTime(
                              selectedEnquiry.createdAt,
                            )}
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Message */}
                  <div className="rounded-xl border border-gray-200 p-5">
                    <h3 className="font-semibold text-[#1B2A4A]">
                      Customer Message
                    </h3>

                    <div className="mt-4 rounded-lg bg-gray-50 p-4">
                      <p className="whitespace-pre-wrap text-sm leading-6 text-gray-600">
                        {selectedEnquiry.message ||
                          "No message provided."}
                      </p>
                    </div>

                    <div className="mt-5">
                      <p className="text-xs text-gray-400">
                        Country
                      </p>

                      <p className="mt-1 text-sm font-medium text-gray-800">
                        {selectedEnquiry.country}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Products */}
                <div className="mt-6 rounded-xl border border-gray-200">
                  <div className="border-b border-gray-200 px-5 py-4">
                    <h3 className="font-semibold text-[#1B2A4A]">
                      Requested Products
                    </h3>

                    <p className="mt-1 text-xs text-gray-500">
                      {selectedEnquiry.items?.length ?? 0}{" "}
                      {(selectedEnquiry.items?.length ?? 0) === 1
                        ? "product"
                        : "products"}{" "}
                      requested
                    </p>
                  </div>

                  <div className="divide-y divide-gray-100">
                    {(selectedEnquiry.items ?? []).map((item) => (
                      <div key={item.id} className="p-5">
                        <div className="flex flex-col justify-between gap-4 sm:flex-row">
                          <div>
                            <h4 className="font-medium text-gray-800">
                              {item.productName}
                            </h4>

                            <div className="mt-2 flex flex-wrap gap-2">
                              {item.strength && (
                                <span className="rounded-md bg-gray-100 px-2.5 py-1 text-xs text-gray-600">
                                  Strength: {item.strength}
                                </span>
                              )}

                              {item.packSize && (
                                <span className="rounded-md bg-gray-100 px-2.5 py-1 text-xs text-gray-600">
                                  Pack: {item.packSize}
                                </span>
                              )}
                            </div>
                          </div>

                          <div className="shrink-0">
                            <p className="text-xs text-gray-400">
                              Quantity
                            </p>

                            <p className="mt-1 text-lg font-semibold text-[#1B2A4A]">
                              {item.quantity}
                            </p>
                          </div>
                        </div>

                        {item.message && (
                          <div className="mt-4 rounded-lg bg-gray-50 p-3">
                            <p className="text-xs font-medium text-gray-500">
                              Product Message
                            </p>

                            <p className="mt-1 whitespace-pre-wrap text-sm text-gray-600">
                              {item.message}
                            </p>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>

                {/* Quotation */}
                <div className="mt-6 rounded-xl border border-gray-200">
                  <div className="flex flex-col justify-between gap-3 border-b border-gray-200 px-5 py-4 sm:flex-row sm:items-center">
                    <div>
                      <h3 className="font-semibold text-[#1B2A4A]">
                        Quotation
                      </h3>
                      <p className="mt-1 text-xs text-gray-500">
                        Set pricing and validity for this enquiry.
                      </p>
                    </div>

                    {!showQuotationForm && !quotationSuccess && (
                      <button
                        type="button"
                        onClick={() => {
                          setQuotationError("");
                          setQuotationSuccess("");
                          setShowQuotationForm(true);
                        }}
                        className="inline-flex items-center justify-center gap-2 rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white transition hover:bg-[#24385f]"
                      >
                        <FileText className="size-4" />
                        Create Quotation
                      </button>
                    )}
                  </div>

                  {quotationSuccess && (
                    <div className="m-5 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
                      {quotationSuccess}
                    </div>
                  )}

                  {showQuotationForm && (
                    <div className="p-5">
                      {quotationError && (
                        <div className="mb-5 flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                          <AlertCircle className="mt-0.5 size-4 shrink-0" />
                          <span>{quotationError}</span>
                        </div>
                      )}

                      <div className="grid gap-5 md:grid-cols-2">
                        <div>
                          <label className="text-sm font-medium text-gray-700">
                            Currency
                          </label>
                          <select
                            value={quotationCurrency}
                            onChange={(e) =>
                              setQuotationCurrency(e.target.value)
                            }
                            className="mt-2 w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-2 focus:ring-[#1B2A4A]/10"
                          >
                            <option value="USD">USD</option>
                            <option value="EUR">EUR</option>
                            <option value="GBP">GBP</option>
                            <option value="INR">INR</option>
                          </select>
                        </div>

                        <div>
                          <label className="text-sm font-medium text-gray-700">
                            Valid Until
                          </label>
                          <input
                            type="date"
                            value={quotationValidUntil}
                            min={new Date().toISOString().split("T")[0]}
                            onChange={(e) =>
                              setQuotationValidUntil(e.target.value)
                            }
                            className="mt-2 w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-2 focus:ring-[#1B2A4A]/10"
                          />
                        </div>
                      </div>

                      <div className="mt-5 rounded-xl border border-gray-200">
                        <div className="border-b border-gray-200 bg-gray-50 px-4 py-3">
                          <div className="grid grid-cols-[1fr_100px_150px] gap-3 text-xs font-semibold uppercase tracking-wide text-gray-500">
                            <span>Product</span>
                            <span>Quantity</span>
                            <span>Unit Price</span>
                          </div>
                        </div>

                        <div className="divide-y divide-gray-100">
                          {(selectedEnquiry.items ?? []).map((item) => (
                            <div
                              key={item.id}
                              className="grid grid-cols-[1fr_100px_150px] items-center gap-3 px-4 py-4"
                            >
                              <div>
                                <p className="text-sm font-medium text-gray-800">
                                  {item.productName}
                                </p>
                                {(item.strength || item.packSize) && (
                                  <p className="mt-1 text-xs text-gray-500">
                                    {[item.strength, item.packSize]
                                      .filter(Boolean)
                                      .join(" • ")}
                                  </p>
                                )}
                              </div>

                              <p className="text-sm font-medium text-gray-700">
                                {item.quantity}
                              </p>

                              <input
                                type="number"
                                min="0.01"
                                step="0.01"
                                value={
                                  quotationPrices[item.productVariantId] ?? ""
                                }
                                onChange={(e) =>
                                  setQuotationPrices((current) => ({
                                    ...current,
                                    [item.productVariantId]: e.target.value,
                                  }))
                                }
                                placeholder="Enter price"
                                className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-2 focus:ring-[#1B2A4A]/10"
                              />
                            </div>
                          ))}
                        </div>
                      </div>

                      <div className="mt-5">
                        <label className="text-sm font-medium text-gray-700">
                          Notes
                        </label>
                        <textarea
                          value={quotationNotes}
                          onChange={(e) => setQuotationNotes(e.target.value)}
                          rows={4}
                          placeholder="Add payment terms, shipping details, or any other quotation notes..."
                          className="mt-2 w-full resize-none rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A] focus:ring-2 focus:ring-[#1B2A4A]/10"
                        />
                      </div>

                      <div className="mt-5 flex flex-col-reverse justify-end gap-3 sm:flex-row">
                        <button
                          type="button"
                          onClick={resetQuotationForm}
                          disabled={quotationLoading}
                          className="rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                          Cancel
                        </button>

                        <button
                          type="button"
                          onClick={handleCreateQuotation}
                          disabled={quotationLoading}
                          className="inline-flex items-center justify-center gap-2 rounded-lg bg-[#1B2A4A] px-5 py-2.5 text-sm font-medium text-white transition hover:bg-[#24385f] disabled:cursor-not-allowed disabled:opacity-60"
                        >
                          {quotationLoading ? (
                            <>
                              <Loader2 className="size-4 animate-spin" />
                              Creating...
                            </>
                          ) : (
                            <>
                              <FileText className="size-4" />
                              Create Quotation
                            </>
                          )}
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </>
    );
  }