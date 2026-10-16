import { ApiError } from "@/lib/api/client";

function getDetailsMessage(details: unknown): string | null {
  if (!details || typeof details !== "object") return null;

  const record = details as Record<string, unknown>;
  for (const key of ["message", "Message", "detail", "title"]) {
    const message = record[key];
    if (typeof message === "string" && message.trim()) {
      return message.trim();
    }
  }

  const errors = record.errors;
  if (errors && typeof errors === "object") {
    const messages = Object.entries(errors as Record<string, unknown>)
      .flatMap(([field, value]) => {
        const values = Array.isArray(value) ? value : [value];
        return values
          .filter((message): message is string => typeof message === "string" && Boolean(message.trim()))
          .map((message) => `${field}: ${message.trim()}`);
      })
      .slice(0, 3);

    if (messages.length) return messages.join(" ");
  }

  return null;
}

/** Converts API failures into messages that help an admin choose what to do next. */
export function getAdminErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (error instanceof ApiError) {
    if (error.status === 401) {
      return "Your session has expired. Sign in again and retry.";
    }
    if (error.status === 403) {
      return "Your account does not have permission to do this.";
    }
    if (error.status === 404) {
      return "This item could not be found. Refresh the page and try again.";
    }
    if (error.status === 409) {
      return getDetailsMessage(error.details) || error.message || "This value is already in use. Choose a different one.";
    }
    if (error.status === 400 || error.status === 422) {
      return getDetailsMessage(error.details) || error.message || "Some information needs correction. Review the form and try again.";
    }
    if (error.status === 429) {
      return "Too many requests. Wait a moment, then try again.";
    }
    if (error.status === 408) {
      return "The request took too long to finish. Check your connection and try again.";
    }
    if (error.status === 413) {
      return "The selected file is too large. Choose a smaller file and try again.";
    }
    if (error.status === 415) {
      return "This file type is not supported. Choose a supported file and try again.";
    }
    if (error.status >= 500) {
      return "The server could not complete this request. Your data was not confirmed as saved; retry in a moment.";
    }

    return getDetailsMessage(error.details) || error.message || fallback;
  }

  if (error instanceof TypeError) {
    return "Could not connect to the server. Check your connection and try again.";
  }

  return error instanceof Error && error.message
    ? error.message
    : fallback;
}
