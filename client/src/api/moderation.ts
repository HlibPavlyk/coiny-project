import { useQuery } from '@tanstack/react-query';
import { api } from './fetch';
import type { Paginated, PageRequest } from './lots';

export type ReportStatus = 'Open' | 'Dismissed' | 'ActionTaken';

export interface ReportItemModel {
  id: string;
  lot: { id: string; title: string; coverImageUrl: string };
  reporterDisplayName: string | null;
  reporterIp: string | null;
  reason: string;
  note: string | null;
  status: ReportStatus;
  createdAt: string;
  resolvedAt: string | null;
}

export interface GetReportsRequest extends PageRequest {
  filters?: { status?: ReportStatus };
}

export const moderation = {
  searchReports: (request: GetReportsRequest) =>
    api<Paginated<ReportItemModel>>('/api/v1/moderation/reports/search', { method: 'POST', body: request }),
  dismissReport: (id: string, resolutionNote?: string) =>
    api<void>(`/api/v1/moderation/reports/${id}/dismiss`, { method: 'POST', body: { resolutionNote } }),
  takeAction: (id: string, resolutionNote: string) =>
    api<void>(`/api/v1/moderation/reports/${id}/take-action`, { method: 'POST', body: { resolutionNote } }),
  takedownLot: (lotId: string) =>
    api<void>(`/api/v1/moderation/lots/${lotId}/takedown`, { method: 'POST' }),
  banUser: (userId: string, reason: string) =>
    api<void>(`/api/v1/moderation/users/${userId}/ban`, { method: 'POST', body: { reason } }),
  unbanUser: (userId: string) =>
    api<void>(`/api/v1/moderation/users/${userId}/unban`, { method: 'POST' }),
};

/** Paginated reports for the moderation table; the full request is the cache key. */
export function useReports(request: GetReportsRequest) {
  return useQuery({
    queryKey: ['moderation', 'reports', request],
    queryFn: () => moderation.searchReports(request),
  });
}
