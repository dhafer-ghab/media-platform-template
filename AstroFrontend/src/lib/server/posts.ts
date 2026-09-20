export type PostSort = 'newest' | 'oldest' | 'title' | 'popular';

export interface PostSummary {
  id: string;
  authorId: string;
  title: string;
  description: string;
  mediaUrl: string | null;
  category: string;
  tags: string[];
  status: string;
  createdDate: string;
  updateDate: string | null;
  publishedAt: string | null;
}

export interface PagedPosts {
  items: PostSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PostFacet {
  name: string;
  count: number;
}

export interface PostFacets {
  categories: PostFacet[];
  tags: PostFacet[];
}

export interface PostsQuery {
  q?: string;
  category?: string;
  tag?: string;
  sort?: PostSort;
  page?: number;
  pageSize?: number;
}

export class PostsApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
  }
}

const runtimeEnvironment = (globalThis as {
  process?: { env?: Record<string, string | undefined> };
}).process?.env;

function getApiBaseUrl(): string {
  const configuredUrl = runtimeEnvironment?.POSTS_API_URL ?? import.meta.env.POSTS_API_URL;
  if (!configuredUrl)
    throw new PostsApiError('POSTS_API_URL is not configured.', 500);

  return configuredUrl.replace(/\/$/, '');
}

function createUrl(path: string, query?: PostsQuery): URL {
  const url = new URL(path, `${getApiBaseUrl()}/`);
  if (!query) return url;

  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== '')
      url.searchParams.set(key, String(value));
  }

  return url;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(createUrl(path), {
    ...init,
    headers: {
      Accept: 'application/json',
      ...init?.headers,
    },
    signal: AbortSignal.timeout(5_000),
  });

  if (!response.ok)
    throw new PostsApiError(`Posts API returned ${response.status}.`, response.status);

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export async function getPosts(query: PostsQuery = {}): Promise<PagedPosts> {
  const url = createUrl('/api/posts', query);
  return request<PagedPosts>(`${url.pathname}${url.search}`);
}

export function getPost(id: string): Promise<PostSummary> {
  return request<PostSummary>(`/api/posts/${encodeURIComponent(id)}`);
}

export function getPostFacets(): Promise<PostFacets> {
  return request<PostFacets>('/api/posts/facets');
}

export async function recordPostView(id: string): Promise<void> {
  await request<void>(`/api/posts/${encodeURIComponent(id)}/views`, { method: 'POST' });
}
