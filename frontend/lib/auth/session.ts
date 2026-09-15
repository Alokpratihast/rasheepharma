import type { AuthResponse } from "@/types/auth";

const AUTH_STORAGE_KEY = "rashepharma_auth";

export interface StoredAuth {
  user: AuthResponse;
  token: string;
}

export function getStoredAuth(): StoredAuth | null {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    const storedValue =
      window.localStorage.getItem(AUTH_STORAGE_KEY);

    if (!storedValue) {
      return null;
    }

    return JSON.parse(storedValue) as StoredAuth;
  } catch {
    return null;
  }
}

export function getStoredToken(): string | null {
  return getStoredAuth()?.token ?? null;
}

export function setStoredAuth(
  response: AuthResponse,
): void {
  if (typeof window === "undefined") {
    return;
  }

  const auth: StoredAuth = {
    user: response,
    token: response.token,
  };

  window.localStorage.setItem(
    AUTH_STORAGE_KEY,
    JSON.stringify(auth),
  );
}

export function clearStoredAuth(): void {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}