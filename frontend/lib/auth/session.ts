import type { AuthResponse } from "@/types/auth";

const LEGACY_AUTH_STORAGE_KEY = "rashepharma_auth";

export interface StoredAuth {
  user: AuthResponse;
  token: string;
}

// Access tokens are kept only in this browser tab's memory. The HttpOnly
// refresh cookie provides persistence without exposing a durable token to JS.
let inMemoryAuth: StoredAuth | null = null;

function removeLegacyPersistentToken(): void {
  if (typeof window === "undefined") {
    return;
  }

  try {
    // Remove tokens saved by older versions so they are not left in browser storage.
    window.localStorage.removeItem(LEGACY_AUTH_STORAGE_KEY);
  } catch {
    // Auth remains usable when browser storage is disabled.
  }
}

export function getStoredAuth(): StoredAuth | null {
  if (typeof window === "undefined") {
    return null;
  }

  removeLegacyPersistentToken();
  return inMemoryAuth;
}

export function getStoredToken(): string | null {
  return getStoredAuth()?.token ?? null;
}

export function setStoredAuth(response: AuthResponse): void {
  if (typeof window === "undefined") {
    return;
  }

  inMemoryAuth = {
    user: response,
    token: response.token,
  };

  removeLegacyPersistentToken();
}

export function clearStoredAuth(): void {
  inMemoryAuth = null;
  removeLegacyPersistentToken();
}