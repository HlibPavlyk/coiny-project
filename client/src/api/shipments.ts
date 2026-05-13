import { api } from './fetch';
import type { ShipmentStatus } from './payments';

export interface ShipmentEventModel {
  id: number;
  status: ShipmentStatus;
  npStatusCode: number;
  description: string | null;
  observedAt: string;
}

export interface ShipmentDetailModel {
  id: string;
  paymentId: string | null;
  lotId: string;
  buyerId: string;
  sellerId: string;
  novaPoshtaTtn: string | null;
  intDocNumber: string | null;
  recipientCityRef: string;
  recipientCityLabel: string;
  recipientWarehouseRef: string;
  recipientWarehouseLabel: string;
  recipientName: string;
  recipientPhone: string;
  declaredValueUahKopiykas: number;
  status: ShipmentStatus;
  lastNpStatusCode: number;
  deliveredAt: string | null;
  lastPolledAt: string | null;
  events: ShipmentEventModel[];
  createdAt: string;
  updatedAt: string;
}

export const shipments = {
  getByPaymentId: (paymentId: string) =>
    api<ShipmentDetailModel>(`/api/v1/shipments/${paymentId}`),
};

/** Statuses the polling job no longer touches — used to stop the timeline auto-refetch. */
export const TERMINAL_SHIPMENT_STATUSES: ShipmentStatus[] = [
  'Delivered',
  'Refused',
  'Returned',
  'Lost',
];
