"use client";

import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import { authService } from "@/services/auth.service";

import {
  clearStoredAuth,
  getStoredAuth,
  setStoredAuth,
} from "@/lib/auth/session";

import type {
  AuthResponse,
  UserProfile,
} from "@/types/auth";

interface AuthContextValue {
  user: UserProfile | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (response: AuthResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<
  AuthContextValue | undefined
>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({
  children,
}: AuthProviderProps) {
  const [user, setUser] = useState<UserProfile | null>(
    null,
  );

  const [token, setToken] = useState<string | null>(
    null,
  );

  const [isLoading, setIsLoading] = useState(true);

  /* =================================================
     RESTORE SESSION
  ================================================== */

  useEffect(() => {
    async function restoreSession() {
      try {
        const storedAuth = getStoredAuth();

        if (!storedAuth) {
          return;
        }

        setToken(storedAuth.token);

        /*
         * Verify the stored JWT with backend.
         */
        const profile = await authService.getProfile(
          storedAuth.token,
        );

        setUser(profile);

        /*
         * Keep the fresh profile while preserving
         * the stored JWT.
         */
        const authResponse: AuthResponse = {
          userId: profile.id,
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
          phoneNumber: profile.phoneNumber,
          country: profile.country,
          role: profile.role,
          token: storedAuth.token,
        };

        setStoredAuth(authResponse);
      } catch {
        clearStoredAuth();

        setUser(null);
        setToken(null);
      } finally {
        setIsLoading(false);
      }
    }

    restoreSession();
  }, []);

  /* =================================================
     LOGIN
  ================================================== */

  function login(response: AuthResponse) {
    setStoredAuth(response);

    const profile: UserProfile = {
      id: response.userId,
      firstName: response.firstName,
      lastName: response.lastName,
      email: response.email,
      phoneNumber: response.phoneNumber,
      country: response.country,
      role: response.role,
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    setUser(profile);
    setToken(response.token);
  }

  /* =================================================
     LOGOUT
  ================================================== */

  function logout() {
    clearStoredAuth();

    setUser(null);
    setToken(null);
  }

  /* =================================================
     CONTEXT VALUE
  ================================================== */

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token),
      isLoading,
      login,
      logout,
    }),
    [user, token, isLoading],
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

/* =====================================================
   useAuth HOOK
===================================================== */

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth must be used inside AuthProvider.",
    );
  }

  return context;
}