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
import { refreshAccessToken } from "@/lib/api/client";

import {
  clearStoredAuth,
  getStoredAuth,
  getStoredToken,
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
  logout: () => Promise<void>;
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
        // A page reload clears the in-memory access token; renew through the HttpOnly cookie.
        const accessToken =
          storedAuth?.token ?? (await refreshAccessToken());

        if (!accessToken) {
          return;
        }

        const profile = await authService.getProfile(accessToken);
        // A 401 retry may have rotated the token while loading the profile.
        const currentToken = getStoredToken() ?? accessToken;

        setUser(profile);

        const authResponse: AuthResponse = {
          userId: profile.id,
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
          phoneNumber: profile.phoneNumber,
          country: profile.country,
          role: profile.role,
          token: currentToken,
        };

        setStoredAuth(authResponse);
        setToken(currentToken);
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
  useEffect(() => {
    function handleSessionExpired() {
      clearStoredAuth();
      setUser(null);
      setToken(null);
    }

    window.addEventListener(
      "auth:session-expired",
      handleSessionExpired,
    );

    return () => {
      window.removeEventListener(
        "auth:session-expired",
        handleSessionExpired,
      );
    };
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

 
  async function logout() {
    try {
      await authService.logout();
    } catch (error) {
      console.error("Backend logout failed:", error);
    } finally {
      clearStoredAuth();
      setUser(null);
      setToken(null);
    }
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