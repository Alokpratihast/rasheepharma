"use client";

import { useEffect, useState } from "react";
import {
  AlertCircle,
  CalendarDays,
  Eye,
  FileText,
  Loader2,
  X,
} from "lucide-react";

import { quotationService } from "@/services/quotation.service";
import { useAuth } from "@/components/providers/AuthProvider";
import type {
  QuotationDetails,
  QuotationList,
} from "@/types/quotation";

export default function QuotationsPage() {
  const { token, isAuthenticated, isLoading: authLoading } = useAuth();

  const [quotations, setQuotations] = useState<QuotationList[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [selectedQuotation, setSelectedQuotation] =
    useState<QuotationDetails | null>(null);

  const [detailsLoading, setDetailsLoading] = useState(false);

  useEffect(() => {
    if (authLoading) return;

    if (!isAuthenticated || !token) {
      setLoading(false);
      return;
    }

    const loadQuotations = async () => {
      try {
        setLoading(true);
        setError("");

        const data = await quotationService.getAll(token);

        setQuotations(data);
      } catch (err) {
        console.error("Failed to load quotations:", err);

        setError(
          err instanceof Error
            ? err.message
            : "Failed to load quotations.",
        );
      } finally {
        setLoading(false);
      }
    };

    loadQuotations();
  }, [token, isAuthenticated, authLoading]);

  const handleViewQuotation = async (id: number) => {
    if (!token) return;

    try {
      setDetailsLoading(true);
      setError("");

      const details = await quotationService.getById(id, token);

      setSelectedQuotation(details);
    } catch (err) {
      console.error("Failed to load quotation details:", err);

      setError(
        err instanceof Error
          ? err.message
          : "Failed to load quotation details.",
      );
    } finally {
      setDetailsLoading(false);
    }
  };

  const getStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case "draft":
        return "bg-gray-100 text-gray-700 border-gray-200";

      case "sent":
        return "bg-blue-50 text-blue-700 border-blue-200";

      case "accepted":
        return "bg-green-50 text-green-700 border-green-200";

      case "rejected":
        return "bg-red-50 text-red-700 border-red-200";

      case "expired":
        return "bg-orange-50 text-orange-700 border-orange-200";

      default:
        return "bg-gray-100 text-gray-700 border-gray-200";
    }
  };

  const formatCurrency = (
    amount: number,
    currency: string,
  ) => {
    try {
      return new Intl.NumberFormat("en-IN", {
        style: "currency",
        currency,
        maximumFractionDigits: 2,
      }).format(amount);
    } catch {
      return `${currency} ${amount.toFixed(2)}`;
    }
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString("en-IN", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  };

  if (authLoading || loading) {
    return (
      <div className="flex min-h-[500px] items-center justify-center">
        <Loader2 className="size-8 animate-spin text-[#1B2A4A]" />
      </div>
    );
  }

  return (
    <div className="p-6 lg:p-8">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-3">
          <div className="flex size-11 items-center justify-center rounded-xl bg-[#1B2A4A]">
            <FileText className="size-5 text-white" />
          </div>

          <div>
            <h1 className="text-2xl font-bold text-[#1B2A4A]">
              Quotations
            </h1>

            <p className="mt-1 text-sm text-gray-500">
              Manage customer quotations and pricing.
            </p>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="mb-5 flex items-start gap-2 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          <AlertCircle className="mt-0.5 size-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Stats */}
      <div className="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-gray-500">
            Total Quotations
          </p>

          <p className="mt-2 text-2xl font-bold text-[#1B2A4A]">
            {quotations.length}
          </p>
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-gray-500">
            Draft
          </p>

          <p className="mt-2 text-2xl font-bold text-gray-700">
            {
              quotations.filter(
                (q) => q.status.toLowerCase() === "draft",
              ).length
            }
          </p>
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-gray-500">
            Sent
          </p>

          <p className="mt-2 text-2xl font-bold text-blue-700">
            {
              quotations.filter(
                (q) => q.status.toLowerCase() === "sent",
              ).length
            }
          </p>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <div className="border-b border-gray-200 px-5 py-4">
          <h2 className="font-semibold text-[#1B2A4A]">
            All Quotations
          </h2>

          <p className="mt-1 text-xs text-gray-500">
            {quotations.length} quotation
            {quotations.length === 1 ? "" : "s"} found
          </p>
        </div>

        {quotations.length === 0 ? (
          <div className="flex min-h-[300px] flex-col items-center justify-center px-5 text-center">
            <div className="flex size-14 items-center justify-center rounded-full bg-gray-100">
              <FileText className="size-6 text-gray-400" />
            </div>

            <h3 className="mt-4 font-semibold text-gray-700">
              No quotations yet
            </h3>

            <p className="mt-1 max-w-md text-sm text-gray-500">
              Quotations created from customer enquiries will
              appear here.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[850px]">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50">
                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Quote Number
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Enquiry
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Amount
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Status
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Valid Until
                  </th>

                  <th className="px-5 py-3 text-right text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Action
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100">
                {quotations.map((quotation) => (
                  <tr
                    key={quotation.id}
                    className="transition hover:bg-gray-50"
                  >
                    <td className="px-5 py-4">
                      <p className="font-medium text-[#1B2A4A]">
                        {quotation.quoteNumber}
                      </p>

                      <p className="mt-1 text-xs text-gray-400">
                        {formatDate(quotation.createdAt)}
                      </p>
                    </td>

                    <td className="px-5 py-4">
                      <span className="text-sm text-gray-700">
                        #{quotation.enquiryId}
                      </span>
                    </td>

                    <td className="px-5 py-4">
                      <span className="font-semibold text-gray-800">
                        {formatCurrency(
                          quotation.totalAmount,
                          quotation.currency,
                        )}
                      </span>
                    </td>

                    <td className="px-5 py-4">
                      <span
                        className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${getStatusClass(
                          quotation.status,
                        )}`}
                      >
                        {quotation.status}
                      </span>
                    </td>

                    <td className="px-5 py-4">
                      <div className="flex items-center gap-2 text-sm text-gray-600">
                        <CalendarDays className="size-4 text-gray-400" />
                        {formatDate(quotation.validUntil)}
                      </div>
                    </td>

                    <td className="px-5 py-4 text-right">
                      <button
                        type="button"
                        onClick={() =>
                          handleViewQuotation(quotation.id)
                        }
                        className="inline-flex items-center gap-2 rounded-lg border border-gray-200 px-3 py-2 text-sm font-medium text-[#1B2A4A] transition hover:bg-gray-50"
                      >
                        <Eye className="size-4" />
                        View
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Details Modal */}
      {selectedQuotation && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="flex max-h-[90vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl bg-white shadow-2xl">
            {/* Modal Header */}
            <div className="flex items-center justify-between border-b border-gray-200 px-6 py-5">
              <div>
                <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                  Quotation
                </p>

                <div className="mt-1 flex flex-wrap items-center gap-3">
                  <h2 className="text-xl font-bold text-[#1B2A4A]">
                    {selectedQuotation.quoteNumber}
                  </h2>

                  <span
                    className={`rounded-full border px-2.5 py-1 text-xs font-medium ${getStatusClass(
                      selectedQuotation.status,
                    )}`}
                  >
                    {selectedQuotation.status}
                  </span>
                </div>
              </div>

              <button
                type="button"
                onClick={() => setSelectedQuotation(null)}
                className="rounded-lg p-2 text-gray-400 transition hover:bg-gray-100 hover:text-gray-700"
              >
                <X className="size-5" />
              </button>
            </div>

            {/* Modal Body */}
            <div className="overflow-y-auto p-6">
              {detailsLoading ? (
                <div className="flex min-h-[300px] items-center justify-center">
                  <Loader2 className="size-7 animate-spin text-[#1B2A4A]" />
                </div>
              ) : (
                <>
                  {/* Summary */}
                  <div className="grid gap-4 sm:grid-cols-3">
                    <div className="rounded-xl bg-gray-50 p-4">
                      <p className="text-xs text-gray-500">
                        Enquiry
                      </p>

                      <p className="mt-1 font-semibold text-[#1B2A4A]">
                        #{selectedQuotation.enquiryId}
                      </p>
                    </div>

                    <div className="rounded-xl bg-gray-50 p-4">
                      <p className="text-xs text-gray-500">
                        Currency
                      </p>

                      <p className="mt-1 font-semibold text-[#1B2A4A]">
                        {selectedQuotation.currency}
                      </p>
                    </div>

                    <div className="rounded-xl bg-gray-50 p-4">
                      <p className="text-xs text-gray-500">
                        Valid Until
                      </p>

                      <p className="mt-1 font-semibold text-[#1B2A4A]">
                        {formatDate(
                          selectedQuotation.validUntil,
                        )}
                      </p>
                    </div>
                  </div>

                  {/* Products */}
                  <div className="mt-6 overflow-hidden rounded-xl border border-gray-200">
                    <div className="border-b border-gray-200 px-5 py-4">
                      <h3 className="font-semibold text-[#1B2A4A]">
                        Quotation Items
                      </h3>
                    </div>

                    <div className="overflow-x-auto">
                      <table className="w-full min-w-[700px]">
                        <thead>
                          <tr className="border-b border-gray-200 bg-gray-50">
                            <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                              Product
                            </th>

                            <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                              Quantity
                            </th>

                            <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                              Unit Price
                            </th>

                            <th className="px-5 py-3 text-right text-xs font-semibold uppercase tracking-wide text-gray-500">
                              Total
                            </th>
                          </tr>
                        </thead>

                        <tbody className="divide-y divide-gray-100">
                          {selectedQuotation.items.map(
                            (item) => (
                              <tr key={item.id}>
                                <td className="px-5 py-4">
                                  <p className="font-medium text-gray-800">
                                    {item.productName}
                                  </p>

                                  {(item.strength ||
                                    item.packSize) && (
                                    <p className="mt-1 text-xs text-gray-500">
                                      {[
                                        item.strength,
                                        item.packSize,
                                      ]
                                        .filter(Boolean)
                                        .join(" • ")}
                                    </p>
                                  )}
                                </td>

                                <td className="px-5 py-4 text-sm text-gray-700">
                                  {item.quantity}
                                </td>

                                <td className="px-5 py-4 text-sm text-gray-700">
                                  {formatCurrency(
                                    item.unitPrice,
                                    selectedQuotation.currency,
                                  )}
                                </td>

                                <td className="px-5 py-4 text-right font-semibold text-gray-800">
                                  {formatCurrency(
                                    item.totalPrice,
                                    selectedQuotation.currency,
                                  )}
                                </td>
                              </tr>
                            ),
                          )}
                        </tbody>
                      </table>
                    </div>

                    <div className="flex justify-end border-t border-gray-200 bg-gray-50 px-5 py-4">
                      <div className="text-right">
                        <p className="text-xs text-gray-500">
                          Total Amount
                        </p>

                        <p className="mt-1 text-xl font-bold text-[#1B2A4A]">
                          {formatCurrency(
                            selectedQuotation.totalAmount,
                            selectedQuotation.currency,
                          )}
                        </p>
                      </div>
                    </div>
                  </div>

                  {/* Notes */}
                  {selectedQuotation.notes && (
                    <div className="mt-6 rounded-xl border border-gray-200 p-5">
                      <h3 className="font-semibold text-[#1B2A4A]">
                        Notes
                      </h3>

                      <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-600">
                        {selectedQuotation.notes}
                      </p>
                    </div>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}