import { api } from './fetch';

export interface NpCity {
  ref: string;
  name: string;
  area: string;
}

export interface NpWarehouse {
  ref: string;
  number: string;
  address: string;
}

export interface NpCitiesResponse {
  cities: NpCity[];
}

export interface NpWarehousesResponse {
  warehouses: NpWarehouse[];
}

export const np = {
  searchCities: (q: string) =>
    api<NpCitiesResponse>(`/api/v1/shipments/cities/search?q=${encodeURIComponent(q)}`),

  getWarehouses: (cityRef: string) =>
    api<NpWarehousesResponse>(`/api/v1/shipments/warehouses?cityRef=${encodeURIComponent(cityRef)}`),
};

export interface PublicConfigModel {
  stripePublishableKey: string;
  signalRHubUrl: string;
  uahPerUsdDisplay: number;
  /** True when the server has `DemoMode:Enabled = true` — gates the /moderation/demo surface. */
  demoModeEnabled: boolean;
}

export const config = {
  getPublic: () => api<PublicConfigModel>('/api/v1/config/public'),
};
