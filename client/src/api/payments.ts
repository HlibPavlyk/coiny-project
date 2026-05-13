import { api } from './fetch';

export interface ConnectOnboardResponse {
  onboardingUrl: string;
  expiresAt: string;
}

export interface ConnectStatusResponse {
  stripeOnboarded: boolean;
  requirementsRemaining: string[];
}

export interface CreatePaymentIntentResponse {
  paymentId: string;
  clientSecret: string;
  publishableKey: string;
  amountUahKopiykasDisplay: number;
  amountUsdCentsCharged: number;
  rateUsedUahPerUsd: number;
}

export interface PaymentDetailModel {
  id: string;
  lotId: string;
  buyerId: string;
  sellerId: string;
  amountUahKopiykas: number;
  amountUsdCents: number;
  rateUsedUahPerUsd: number;
  stripePaymentIntentId: string;
  status: 'PendingAuthorization' | 'Authorized' | 'Captured' | 'Cancelled' | 'Failed';
  dueAt: string;
  authorizedAt?: string;
  capturedAt?: string;
  cancelledAt?: string;
  shipmentId?: string;
  shipmentStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CheckoutDetailsBody {
  recipientCityRef: string;
  recipientCityLabel: string;
  recipientWarehouseRef: string;
  recipientWarehouseLabel: string;
  recipientName: string;
  recipientPhone: string;
}

export const payments = {
  connectOnboard: () =>
    api<ConnectOnboardResponse>('/api/v1/payments/connect/onboard', { method: 'POST' }),

  connectStatus: () => api<ConnectStatusResponse>('/api/v1/payments/connect/status'),

  checkoutDetails: (lotId: string, body: CheckoutDetailsBody) =>
    api<void>(`/api/v1/payments/${lotId}/checkout-details`, { method: 'POST', body }),

  createIntent: (lotId: string) =>
    api<CreatePaymentIntentResponse>(`/api/v1/payments/${lotId}/intent`, { method: 'POST' }),

  getById: (paymentId: string) => api<PaymentDetailModel>(`/api/v1/payments/${paymentId}`),
};
