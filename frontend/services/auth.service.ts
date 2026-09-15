import { apiClient } from "@/lib/api/client";

import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserProfile,
} from "@/types/auth";

const AUTH_ENDPOINT = "/Auth";

export const authService = {
  async register(
    data: RegisterRequest,
  ): Promise<AuthResponse> {
    return apiClient<AuthResponse>(
      `${AUTH_ENDPOINT}/register`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async login(
    data: LoginRequest,
  ): Promise<AuthResponse> {
    return apiClient<AuthResponse>(
      `${AUTH_ENDPOINT}/login`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async getProfile(
    token: string,
  ): Promise<UserProfile> {
    return apiClient<UserProfile>(
      `${AUTH_ENDPOINT}/profile`,
      {
        method: "GET",
        token,
      },
    );
  },
};