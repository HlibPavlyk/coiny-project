import { api } from './fetch';
import type { Paginated, PageRequest } from './lots';

export interface ConnectOnboardResponse {
  onboardingUrl: string;
  expiresAt: string;
}

export interface ConnectStatusResponse {
  stripeOnboarded: boolean;
  requirementsRemaining: string[];
}

export interface ExpressDashboardLinkResponse {
  url: string;
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

export type PaymentStatus =
  | 'PendingAuthorization'
  | 'Authorized'
  | 'Captured'
  | 'Cancelled'
  | 'Failed';

export type ShipmentStatus =
  | 'PendingTtn'
  | 'TtnCreated'
  | 'AcceptedByCarrier'
  | 'InTransit'
  | 'ArrivedAtDestination'
  | 'Delivered'
  | 'Refused'
  | 'Returned'
  | 'Lost';

export interface MyPurchaseItemModel {
  paymentId: string;
  paymentStatus: PaymentStatus;
  amountUahKopiykas: number;
  dueAt: string;
  createdAt: string;
  lot: { id: string; title: string; coverUrl: string };
  shipment: { id: string; status: ShipmentStatus; novaPoshtaTtn: string | null } | null;
}

export const payments = {
  connectOnboard: () =>
    api<ConnectOnboardResponse>('/api/v1/payments/connect/onboard', { method: 'POST' }),

  connectStatus: () => api<ConnectStatusResponse>('/api/v1/payments/connect/status'),

  expressDashboardLink: () =>
    api<ExpressDashboardLinkResponse>('/api/v1/payments/connect/dashboard-link'),

  checkoutDetails: (lotId: string, body: CheckoutDetailsBody) =>
    api<void>(`/api/v1/lots/${lotId}/checkout-details`, { method: 'POST', body }),

  createIntent: (lotId: string) =>
    api<CreatePaymentIntentResponse>(`/api/v1/lots/${lotId}/payment-intent`, { method: 'POST' }),

  getById: (paymentId: string) => api<PaymentDetailModel>(`/api/v1/payments/${paymentId}`),

  myPurchasesSearch: (paginate: PageRequest) =>
    api<Paginated<MyPurchaseItemModel>>('/api/v1/users/me/payments/list', {
      method: 'POST',
      body: paginate,
    }),
};
