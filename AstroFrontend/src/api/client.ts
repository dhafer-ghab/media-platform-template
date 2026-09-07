const DEFAULT_TIMEOUT_MS = 10_000;

export type ApiRequestOptions = Omit<RequestInit, 'body' | 'signal'> & {
  body?: unknown;
  signal?: AbortSignal;
  timeoutMs?: number;
};

export type ApiClient = <T>(path: string, options?: ApiRequestOptions) => Promise<T>;

export type ApiErrorBody = {
  code?: string;
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly body: ApiErrorBody | string | null = null,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export class ApiTimeoutError extends Error {
  constructor(public readonly timeoutMs: number) {
    super(`API request timed out after ${timeoutMs}ms.`);
    this.name = 'ApiTimeoutError';
  }
}

export type ApiClientConfig = {
  baseUrl: string;
  timeoutMs?: number;
  fetch?: typeof globalThis.fetch;
};

function normalizeBaseUrl(value: string): string {
  if (!value) throw new Error('PUBLIC_API_BASE_URL is required.');
  return value.replace(/\/$/, '');
}

async function responseBody(response: Response): Promise<ApiErrorBody | string | null> {
  if (response.status === 204) return null;
  const text = await response.text();
  if (!text) return null;
  try {
    return JSON.parse(text) as ApiErrorBody;
  } catch {
    return text;
  }
}

export function createApiClient(config: ApiClientConfig): ApiClient {
  const baseUrl = normalizeBaseUrl(config.baseUrl);
  const fetchImplementation = config.fetch ?? globalThis.fetch;
  const defaultTimeoutMs = config.timeoutMs ?? DEFAULT_TIMEOUT_MS;

  return async function request<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
    const { body, headers, signal, timeoutMs = defaultTimeoutMs, ...init } = options;
    const controller = new AbortController();
    const abort = () => controller.abort(signal?.reason);
    signal?.addEventListener('abort', abort, { once: true });
    if (signal?.aborted) abort();

    let timedOut = false;
    const timer = timeoutMs > 0
      ? setTimeout(() => {
          timedOut = true;
          controller.abort();
        }, timeoutMs)
      : undefined;

    try {
      const requestHeaders = new Headers(headers);
      if (body !== undefined && !requestHeaders.has('Content-Type')) {
        requestHeaders.set('Content-Type', 'application/json');
      }

      const response = await fetchImplementation(`${baseUrl}${path}`, {
        ...init,
        headers: requestHeaders,
        signal: controller.signal,
        body: body !== undefined ? JSON.stringify(body) : undefined,
      });

      if (!response.ok) {
        const errorBody = await responseBody(response);
        const message =
          typeof errorBody === 'object' && errorBody
            ? errorBody.message ?? errorBody.detail ?? errorBody.title
            : null;
        throw new ApiError(
          response.status,
          message ?? `API error ${response.status} on ${path}`,
          errorBody,
        );
      }

      if (response.status === 204) return undefined as T;
      return (await response.json()) as T;
    } catch (error) {
      if (timedOut) throw new ApiTimeoutError(timeoutMs);
      throw error;
    } finally {
      if (timer) clearTimeout(timer);
      signal?.removeEventListener('abort', abort);
    }
  };
}

export const apiFetch = createApiClient({
  baseUrl: import.meta.env.PUBLIC_API_BASE_URL,
});
