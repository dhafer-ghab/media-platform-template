export { authApi } from './auth';
export type { LoginRequest, RegisterRequest, AuthResponse } from './auth';
export { catalogApi, createCatalogApi } from './catalog';
export type {
  ActorProfile,
  ActorSummary,
  AlbumDetails,
  AlbumSummary,
  DiscoveryItem,
  Guid,
  LegacyRouteResolution,
  MediaType,
  PagedResult,
  PostDetails,
} from './catalog';
export { ApiError, ApiTimeoutError, createApiClient } from './client';
export type { ApiClient, ApiClientConfig, ApiRequestOptions } from './client';
