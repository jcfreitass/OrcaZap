import type {
  ApiEnvelope,
  AuthResponse,
  Customer,
  Quote,
  QuoteStatus,
  Service,
  User,
  WhatsAppLink,
} from './types';

const BASE = 'http://localhost:5000';
const TOKEN_KEY = 'orcazap_token';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

let unauthorizedHandler: (() => void) | null = null;

export function onUnauthorized(handler: () => void) {
  unauthorizedHandler = handler;
}

async function request<T>(path: string, init?: RequestInit): Promise<T | null> {
  const token = getToken();
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  });

  if (res.status === 401) {
    setToken(null);
    unauthorizedHandler?.();
    throw new Error('Sessão expirada. Faça login novamente.');
  }

  if (res.status === 204) return null;

  const text = await res.text();
  if (!text) return null;

  const envelope = JSON.parse(text) as ApiEnvelope<T>;
  if (envelope.StatusCode >= 400) {
    const msg = envelope.Notificacoes?.map((n) => n.Mensagem).join('; ') ?? 'Erro desconhecido';
    throw new Error(msg);
  }
  return envelope.Data;
}

export const api = {
  auth: {
    login: (dto: { Email: string; Password: string }) =>
      request<AuthResponse>('/api/users/login', { method: 'POST', body: JSON.stringify(dto) }),
  },
  users: {
    list: () => request<User[]>('/api/users'),
    create: (dto: { Name: string; Email: string; Password: string }) =>
      request<AuthResponse>('/api/users', { method: 'POST', body: JSON.stringify(dto) }),
  },
  customers: {
    list: () => request<Customer[]>('/api/customers'),
    get: (id: number) => request<Customer>(`/api/customers/${id}`),
    create: (dto: { Name: string; Phone: string; Email: string }) =>
      request<Customer>('/api/customers', { method: 'POST', body: JSON.stringify(dto) }),
    update: (id: number, dto: { Name: string; Phone: string; Email: string }) =>
      request<null>(`/api/customers/${id}`, { method: 'PUT', body: JSON.stringify(dto) }),
    remove: (id: number) => request<null>(`/api/customers/${id}`, { method: 'DELETE' }),
  },
  services: {
    list: () => request<Service[]>('/api/services'),
    get: (id: number) => request<Service>(`/api/services/${id}`),
    create: (dto: { Name: string; Description: string; Price: number }) =>
      request<Service>('/api/services', { method: 'POST', body: JSON.stringify(dto) }),
    update: (id: number, dto: { Name: string; Description: string; Price: number }) =>
      request<null>(`/api/services/${id}`, { method: 'PUT', body: JSON.stringify(dto) }),
    remove: (id: number) => request<null>(`/api/services/${id}`, { method: 'DELETE' }),
  },
  quotes: {
    list: () => request<Quote[]>('/api/quotes'),
    get: (id: number) => request<Quote>(`/api/quotes/${id}`),
    create: (dto: {
      CustomerId: number;
      ValidUntil: string | null;
      Items: { ServiceId: number | null; Description: string; Quantity: number; UnitPrice: number }[];
    }) => request<Quote>('/api/quotes', { method: 'POST', body: JSON.stringify(dto) }),
    updateStatus: (id: number, status: QuoteStatus) =>
      request<null>(`/api/quotes/${id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ Status: status }),
      }),
    remove: (id: number) => request<null>(`/api/quotes/${id}`, { method: 'DELETE' }),
    whatsappLink: (id: number) => request<WhatsAppLink>(`/api/quotes/${id}/whatsapp-link`),
  },
};
