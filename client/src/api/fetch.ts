/**
 * Base fetch wrapper. Sends cookies (for `coiny_auth`), expects JSON, parses ProblemDetails on failure.
 * All feature API modules build on top of this.
 */

const API_BASE = import.meta.env.VITE_API_BASE_URL || '';

export class ApiError extends Error {
  status: number;
  code?: string;
  detail?: string;

  constructor(status: number, message: string, code?: string, detail?: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.detail = detail;
  }
}

interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export interface FetchOptions extends Omit<RequestInit, 'body'> {
  body?: unknown;
  skipJsonBody?: boolean;
}

export async function api<T = unknown>(path: string, opts: FetchOptions = {}): Promise<T> {
  const { body, skipJsonBody, headers, ...rest } = opts;

  const init: RequestInit = {
    credentials: 'include',
    ...rest,
    headers: {
      Accept: 'application/json',
      ...(body !== undefined && !skipJsonBody ? { 'Content-Type': 'application/json' } : {}),
      ...headers,
    },
    body: body === undefined ? undefined : skipJsonBody ? (body as BodyInit) : JSON.stringify(body),
  };

  const url = path.startsWith('http') ? path : `${API_BASE}${path}`;
  const res = await fetch(url, init);

  if (res.status === 204 || res.headers.get('Content-Length') === '0') {
    return undefined as T;
  }

  const contentType = res.headers.get('Content-Type') ?? '';
  const isJson = contentType.includes('application/json') || contentType.includes('application/problem+json');

  if (!res.ok) {
    if (isJson) {
      const problem = (await res.json().catch(() => ({}))) as ProblemDetails;
      throw new ApiError(res.status, problem.title ?? `HTTP ${res.status}`, undefined, problem.detail);
    }
    throw new ApiError(res.status, `HTTP ${res.status}`);
  }

  return isJson ? ((await res.json()) as T) : (undefined as T);
}
