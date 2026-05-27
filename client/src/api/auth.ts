import { api } from './fetch';

export interface MeModel {
  id: string;
  email: string;
  emailVerified: boolean;
  displayName: string;
  trustScore: number;
  isBanned: boolean;
  stripeOnboarded: boolean;
  roles: string[];
}

export interface RegisterPayload {
  email: string;
  password: string;
  displayName?: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export const auth = {
  me: () => api<MeModel>('/api/v1/users/me'),

  register: (payload: RegisterPayload) =>
    api<MeModel>('/api/v1/auth/register', { method: 'POST', body: payload }),

  login: (payload: LoginPayload) =>
    api<MeModel>('/api/v1/auth/login', { method: 'POST', body: payload }),

  logout: () => api<void>('/api/v1/auth/logout', { method: 'POST' }),

  verifyEmail: (token: string) =>
    api<void>('/api/v1/auth/verify-email', { method: 'POST', body: { token } }),

  resendVerification: () =>
    api<void>('/api/v1/auth/verify-email/resend', { method: 'POST' }),

  googleStart: (next?: string) => {
    const url = `${import.meta.env.VITE_API_BASE_URL || ''}/api/v1/auth/google${
      next ? `?next=${encodeURIComponent(next)}` : ''
    }`;
    window.location.href = url;
  },
};
