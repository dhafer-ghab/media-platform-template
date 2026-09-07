import { apiFetch, type ApiClient, type ApiRequestOptions } from './client';

export type Guid = string;
export type MediaType = 'photo' | 'video';

export type ActorSummary = {
  id: Guid;
  slug: string;
  displayName: string;
  profession: string;
  avatarStorageKey: string;
  followerCount: number;
  editorialRank: number;
  publishedAt: string;
  updatedAt: string;
  albumCount: number;
  mediaCount: number;
};

export type ActorProfile = ActorSummary & { bio: string };

export type AlbumSummary = {
  id: Guid;
  actorId: Guid;
  slug: string;
  title: string;
  description: string;
  coverStorageKey: string;
  coverAltText: string;
  coverAspectRatio: number;
  isSeries: boolean;
  tags: string[];
  editorialRank: number;
  publishedAt: string;
  updatedAt: string;
  photoCount: number;
  videoCount: number;
};

export type AlbumDetails = AlbumSummary & {
  actorSlug: string;
  actorName: string;
};

export type PostDetails = {
  id: Guid;
  albumId: Guid;
  actorId: Guid;
  slug: string;
  storageKey: string;
  mediaType: MediaType;
  width: number;
  height: number;
  aspectRatio: number;
  durationSeconds: number | null;
  mimeType: string;
  byteSize: number;
  caption: string | null;
  altText: string;
  displayOrder: number;
  tags: string[];
  editorialRank: number;
  publishedAt: string;
  updatedAt: string;
};

export type DiscoveryItem = {
  id: Guid;
  actorId: Guid;
  actorSlug: string;
  actorName: string;
  slug: string;
  title: string;
  description: string;
  coverStorageKey: string;
  coverAltText: string;
  coverAspectRatio: number;
  tags: string[];
  editorialRank: number;
  publishedAt: string;
  updatedAt: string;
};

export type LegacyRouteResolution = {
  id: Guid;
  sourcePath: string;
  destinationPath: string;
  createdAt: string;
  updatedAt: string;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPrev: boolean;
  hasNext: boolean;
};

export type PageQuery = { page?: number; pageSize?: number };
export type DiscoveryQuery = PageQuery & { q?: string; tag?: string };

function queryString(values: Record<string, string | number | undefined>): string {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) {
    if (value !== undefined && value !== '') params.set(key, String(value));
  }
  const query = params.toString();
  return query ? `?${query}` : '';
}

const segment = (value: string) => encodeURIComponent(value);

export function createCatalogApi(client: ApiClient = apiFetch) {
  return {
    actors: (query: PageQuery = {}, options?: ApiRequestOptions) =>
      client<PagedResult<ActorSummary>>(`/api/catalog/actors${queryString(query)}`, options),
    actor: (slug: string, options?: ApiRequestOptions) =>
      client<ActorProfile>(`/api/catalog/actors/${segment(slug)}`, options),
    actorAlbums: (actorSlug: string, query: PageQuery = {}, options?: ApiRequestOptions) =>
      client<PagedResult<AlbumSummary>>(
        `/api/catalog/actors/${segment(actorSlug)}/albums${queryString(query)}`,
        options,
      ),
    album: (actorSlug: string, albumSlug: string, options?: ApiRequestOptions) =>
      client<AlbumDetails>(
        `/api/catalog/actors/${segment(actorSlug)}/albums/${segment(albumSlug)}`,
        options,
      ),
    albumPosts: (
      actorSlug: string,
      albumSlug: string,
      query: PageQuery = {},
      options?: ApiRequestOptions,
    ) =>
      client<PagedResult<PostDetails>>(
        `/api/catalog/actors/${segment(actorSlug)}/albums/${segment(albumSlug)}/posts${queryString(query)}`,
        options,
      ),
    postById: (id: Guid, options?: ApiRequestOptions) =>
      client<PostDetails>(`/api/catalog/posts/by-id/${segment(id)}`, options),
    postBySlug: (slug: string, options?: ApiRequestOptions) =>
      client<PostDetails>(`/api/catalog/posts/${segment(slug)}`, options),
    discovery: (query: DiscoveryQuery = {}, options?: ApiRequestOptions) =>
      client<PagedResult<DiscoveryItem>>(`/api/catalog/discovery${queryString(query)}`, options),
    tags: (options?: ApiRequestOptions) => client<string[]>('/api/catalog/tags', options),
    resolveLegacyRoute: (path: string, options?: ApiRequestOptions) =>
      client<LegacyRouteResolution>(
        `/api/catalog/legacy-routes/resolve${queryString({ path })}`,
        options,
      ),
  };
}

export const catalogApi = createCatalogApi();
