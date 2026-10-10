
import {
  clearStoredAuth,
  getStoredAuth,
  getStoredToken,
  setStoredAuth,
} from "@/lib/auth/session";

import type { AuthResponse } from "@/types/auth";

const API_BASE_URL = (
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  "http://localhost:5104/api"
).replace(/\/+$/, "");

export function getApiAssetUrl(
  assetUrl: string | null | undefined,
): string | null {
  if (!assetUrl) {
    return null;
  }

  if (
    assetUrl.startsWith("http://") ||
    assetUrl.startsWith("https://")
  ) {
    return assetUrl;
  }

  const apiRoot = API_BASE_URL.replace(/\/api\/?$/, "");

  return `${apiRoot}${assetUrl.startsWith("/") ? "" : "/"}${assetUrl}`;
}

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

function getErrorMessage(data: unknown): string {
  if (
    data &&
    typeof data === "object" &&
    "message" in data &&
    typeof data.message === "string"
  ) {
    return data.message;
  }

  if (typeof data === "string" && data) {
    return data;
  }

  return "Something went wrong.";
}

/*
 * Share one refresh request across concurrent API failures.
 */
let refreshPromise: Promise<string | null> | null = null;

function notifySessionExpired(): void {
  clearStoredAuth();

  if (typeof window !== "undefined") {
    window.dispatchEvent(
      new Event("auth:session-expired"),
    );
  }
}

export async function refreshAccessToken(): Promise<string | null> {
  if (refreshPromise) {
    return refreshPromise;
  }

  const performRefresh = async (): Promise<string | null> => {
    try {
      const response = await fetch(
        API_BASE_URL + "/Auth/refresh",
        {
          method: "POST",
          credentials: "include",
          headers: {
            Accept: "application/json",
          },
          cache: "no-store",
        },
      );

      const data = await parseResponse(response);

      if (
        !response.ok ||
        !data ||
        typeof data !== "object" ||
        !("token" in data) ||
        typeof data.token !== "string" ||
        !data.token
      ) {
        notifySessionExpired();
        return null;
      }

      const currentAuth = getStoredAuth();

      if (currentAuth) {
        setStoredAuth({
          ...currentAuth.user,
          token: data.token,
        });
      } else {
        setStoredAuth(data as AuthResponse);
      }

      return data.token;
    } catch {
      notifySessionExpired();
      return null;
    }
  };

  refreshPromise = (async () => {
    try {
      // Web Locks serialize refreshes across tabs so one tab's rotation
      // does not make another tab look like a stolen-token replay.
      if (typeof navigator !== "undefined" && navigator.locks) {
        return await navigator.locks.request(
          "rashepharma-auth-refresh",
          performRefresh,
        );
      }

      return await performRefresh();
    } catch {
      notifySessionExpired();
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
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
    !(requestOptions.body instanceof FormData) &&
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

  const method = (
    requestOptions.method ?? "GET"
  ).toUpperCase();

  const fetchOptions: RequestInit = {
    ...requestOptions,
    credentials: "include",
    ...(method === "GET"
      ? { cache: "no-store" as const }
      : {}),
    headers: requestHeaders,
  };

  let response = await fetch(
    `${API_BASE_URL}${endpoint}`,
    fetchOptions,
  );

  /*
   * Do not attempt refresh for login, registration,
   * or refresh endpoint failures.
   */
  const isAuthEndpoint =
    /\/Auth\/(login|register|refresh)\/?$/i.test(endpoint);

  /*
   * Retry at most once. The retry is a direct fetch,
   * so another 401 cannot start a refresh loop.
   */
  if (response.status === 401 && authToken && !isAuthEndpoint) {
    const newToken = await refreshAccessToken();

    if (newToken) {
      const retryHeaders = new Headers(requestHeaders);

      retryHeaders.set(
        "Authorization",
        `Bearer ${newToken}`,
      );

      response = await fetch(
        `${API_BASE_URL}${endpoint}`,
        {
          ...fetchOptions,
          headers: retryHeaders,
        },
      );

      // Clear stale client auth if the one refreshed-token retry is still rejected.
      if (response.status === 401) {
        notifySessionExpired();
      }
    }
  }

  const data = await parseResponse(response);

  if (!response.ok) {
    throw new ApiError(
      getErrorMessage(data),
      response.status,
      data,
    );
  }

  return data as T;
}
