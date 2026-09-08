export type WorkshopSummary = {
  id: string;
  title: string;
  location: string;
  startsAtUtc: string;
  capacity: number;
  registered: number;
};

export type WorkshopDetail = WorkshopSummary & { description: string };

const TENANT_KEY = 'minihub.tenant';

export function currentTenant(): string {
  try {
    return localStorage.getItem(TENANT_KEY) || 'demo';
  } catch {
    return 'demo';
  }
}

export function setTenant(slug: string) {
  try {
    localStorage.setItem(TENANT_KEY, slug);
  } catch {
    /* ignore */
  }
}

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const res = await fetch(`/api${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant': currentTenant(),
      ...(init.headers ?? {}),
    },
  });
  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = (await res.json()) as { error?: string };
      if (body.error) message = body.error;
    } catch {
      /* not json */
    }
    throw new ApiError(res.status, message);
  }
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const api = {
  listWorkshops: () => request<WorkshopSummary[]>('/workshops'),
  getWorkshop: (id: string) => request<WorkshopDetail>(`/workshops/${id}`),
  register: (id: string, name: string, email: string) =>
    request<{ id: string }>(`/workshops/${id}/register`, {
      method: 'POST',
      body: JSON.stringify({ name, email }),
    }),
};
