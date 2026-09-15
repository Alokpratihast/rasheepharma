export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string | null;
  country?: string | null;
  password: string;
}

export interface AuthResponse {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  country: string | null;
  role: string;
  token: string;
}

export interface UserProfile {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  country: string | null;
  role: string;
  isActive: boolean;
  createdAt: string;
}