import { API_BASE_URL, IRRIGATION_API_PREFIX } from '../config/api';
import { authStorage } from '../lib/authStorage';
import { forceLogoutAndRedirect, isAuthEndpoint } from '../lib/httpClient';
import type {
  CancelIrrigationRequest,
  IrrigationAction,
  StartIrrigationRequest,
} from '../types/irrigation';

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

interface ErrorPayload {
  statusCode?: number;
  code?: string;
  message?: string;
  title?: string;
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  let response: Response;

  try {
    const token = authStorage.getToken();
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(init.headers ?? {}),
      },
    });
  } catch {
    throw new ApiError(
      0,
      'NETWORK_ERROR',
      'No se pudo conectar con el servicio de riego. Verifica que el microservicio esté disponible.',
    );
  }

  if (response.status === 401 && !isAuthEndpoint(path)) {
    forceLogoutAndRedirect();
  }

  if (!response.ok) {
    const payload = (await response.json().catch(() => null)) as ErrorPayload | null;
    throw new ApiError(
      response.status,
      payload?.code ?? 'HTTP_ERROR',
      payload?.message ??
        payload?.title ??
        'El servicio de riego devolvió una respuesta inesperada.',
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const irrigationService = {
  start(data: StartIrrigationRequest): Promise<IrrigationAction> {
    return request<IrrigationAction>(`${IRRIGATION_API_PREFIX}/start`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  cancel(id: string, reason: string): Promise<IrrigationAction> {
    const payload: CancelIrrigationRequest = { reason };

    return request<IrrigationAction>(`${IRRIGATION_API_PREFIX}/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  updateCancellationReason(
    id: string,
    reason: string,
  ): Promise<IrrigationAction> {
    return request<IrrigationAction>(
      `${IRRIGATION_API_PREFIX}/${id}/cancellation-reason`,
      {
        method: 'PUT',
        body: JSON.stringify({ reason }),
      },
    );
  },

  getById(id: string): Promise<IrrigationAction> {
    return request<IrrigationAction>(`${IRRIGATION_API_PREFIX}/${id}`);
  },

  getHistory(deviceId: string, limit = 20): Promise<IrrigationAction[]> {
    const query = new URLSearchParams({
      deviceId,
      limit: String(limit),
    });

    return request<IrrigationAction[]>(`${IRRIGATION_API_PREFIX}?${query.toString()}`);
  },
};

export const toUserMessage = (error: unknown): string =>
  error instanceof ApiError
    ? error.message
    : 'Ocurrió un error inesperado. Intenta nuevamente.';
