const BASE_URL = import.meta.env.PUBLIC_API_BASE_URL?.replace(/\/$/, '');

if (!BASE_URL) {
  throw new Error('PUBLIC_API_BASE_URL is required.');
}

type RequestOptions = Omit<RequestInit, 'body'> & { body?: unknown };

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, headers, ...rest } = options;

  const res = await fetch(`${BASE_URL}${path}`, {
    ...rest,
    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    throw new ApiError(res.status, `API error ${res.status} on ${path}`);
  }

  return res.json() as Promise<T>;
}
