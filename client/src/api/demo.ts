import { useQuery } from '@tanstack/react-query';
import { api } from './fetch';
import { config, type PublicConfigModel } from './np';

/**
 * Demo-only control-surface client. Every action is keyed by lot id — after Option A every Sold
 * lot has exactly one Payment and one Shipment, so a single id is the natural handle. Each call
 * short-circuits one of the time-based triggers in the auction → payment → shipment workflow so
 * the thesis defense can demonstrate the full lifecycle in minutes instead of days.
 *
 * Every endpoint is Admin-gated server-side AND feature-flag-gated via `DemoMode:Enabled`. When
 * the flag is off, calls resolve with a 404 carrying error code `Demo.Disabled`.
 */
export const demo = {
  closeLotNow: (lotId: string) =>
    api<void>(`/api/v1/demo/lots/${lotId}/close-now`, { method: 'POST' }),

  sendPaymentReminderNow: (lotId: string) =>
    api<void>(`/api/v1/demo/lots/${lotId}/send-reminder`, { method: 'POST' }),

  cancelUnpaidNow: (lotId: string) =>
    api<void>(`/api/v1/demo/lots/${lotId}/cancel-unpaid`, { method: 'POST' }),

  forceShipmentDelivered: (lotId: string) =>
    api<void>(`/api/v1/demo/lots/${lotId}/force-delivered`, { method: 'POST' }),

  forceShipmentReturned: (lotId: string) =>
    api<void>(`/api/v1/demo/lots/${lotId}/force-returned`, { method: 'POST' }),
};

/** Reads the public-config endpoint once and surfaces only the demo-mode flag. */
export function useDemoModeEnabled() {
  const { data, isLoading } = useQuery<PublicConfigModel>({
    queryKey: ['config', 'public'],
    queryFn: () => config.getPublic(),
    staleTime: 5 * 60 * 1000, // 5 minutes — flag rarely changes
  });
  return { enabled: data?.demoModeEnabled ?? false, isLoading };
}
