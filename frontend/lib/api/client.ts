import { getStoredToken } from "@/lib/auth/session";


const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  "https://localhost:7001/api";

interface ApiRequestOptions extends RequestInit {
  token?: string;
}

export class ApiError extends Error {
  status: number;
  details?: unknown;

  constructor(
    message: string,
    status: number,
    details?: unknown,
  ) {
    super(message);

    this.name = "ApiError";
    this.status = status;
    this.details = details;
  }
}

async function parseResponse(
  response: Response,
): Promise<unknown> {
  const contentType =
    response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    return response.json();
  }

  const text = await response.text();

  return text || null;
}

export async function apiClient<T>(
  endpoint: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const {
    token,
    headers,
    ...requestOptions
  } = options;

  const requestHeaders = new Headers(headers);

  requestHeaders.set("Accept", "application/json");

  if (
    requestOptions.body &&
    !requestHeaders.has("Content-Type")
  ) {
    requestHeaders.set(
      "Content-Type",
      "application/json",
    );
  }

  const authToken = token ?? getStoredToken();

if (authToken) {
  requestHeaders.set(
    "Authorization",
    `Bearer ${authToken}`,
  );
}

  const response = await fetch(
    `${API_BASE_URL}${endpoint}`,
    {
      ...requestOptions,
      headers: requestHeaders,
    },
  );

  const data = await parseResponse(response);

  if (!response.ok) {
    let message = "Something went wrong.";

    if (
      data &&
      typeof data === "object" &&
      "message" in data &&
      typeof data.message === "string"
    ) {
      message = data.message;
    } else if (typeof data === "string" && data) {
      message = data;
    }

    throw new ApiError(
      message,
      response.status,
      data,
    );
  }

  return data as T;
}