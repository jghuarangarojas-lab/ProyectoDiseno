import type { AxiosError } from 'axios';
import { IOT_API_BASE_URL, IOT_API_PREFIX } from '../config/api';
import { createHttpClient } from '../lib/httpClient';
import type {
  BotanicalThreshold,
  DeviceAlert,
  IotDevice,
  RelocateSensorRequest,
  ResolveAlertRequest,
  TelemetryRecord,
  UpdateThresholdRequest,
} from '../types/iot';

export class IotApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = 'IotApiError';
  }
}

interface ErrorPayload {
  statusCode?: number;
  code?: string;
  message?: string;
  title?: string;
}

const client = createHttpClient({
  baseURL: `${IOT_API_BASE_URL}${IOT_API_PREFIX}`,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

client.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ErrorPayload>) => {
    if (!error.response) {
      return Promise.reject(
        new IotApiError(
          0,
          'NETWORK_ERROR',
          'No se pudo conectar con el servicio IoT. Verifica que ms-iot esté disponible.',
        ),
      );
    }

    const payload = error.response.data;

    return Promise.reject(
      new IotApiError(
        error.response.status,
        payload?.code ?? 'HTTP_ERROR',
        payload?.message ??
          payload?.title ??
          'El servicio IoT devolvió una respuesta inesperada.',
      ),
    );
  },
);

export const iotService = {
  getDevices(): Promise<IotDevice[]> {
    return client.get<IotDevice[]>('/devices').then((response) => response.data);
  },

  getDevice(deviceCode: string): Promise<IotDevice> {
    return client
      .get<IotDevice>(`/devices/${encodeURIComponent(deviceCode)}`)
      .then((response) => response.data);
  },

  relocate(deviceCode: string, currentSectorArea: string): Promise<IotDevice> {
    const payload: RelocateSensorRequest = { currentSectorArea };

    return client
      .put<IotDevice>(
        `/devices/${encodeURIComponent(deviceCode)}/relocate`,
        payload,
      )
      .then((response) => response.data);
  },

  getThreshold(deviceCode: string): Promise<BotanicalThreshold> {
    return client
      .get<BotanicalThreshold>(`/devices/${encodeURIComponent(deviceCode)}/thresholds`)
      .then((response) => response.data);
  },

  updateThreshold(
    deviceCode: string,
    payload: UpdateThresholdRequest,
  ): Promise<BotanicalThreshold> {
    return client
      .put<BotanicalThreshold>(
        `/devices/${encodeURIComponent(deviceCode)}/thresholds`,
        payload,
      )
      .then((response) => response.data);
  },

  getTelemetry(deviceCode: string, limit = 10): Promise<TelemetryRecord[]> {
    const query = new URLSearchParams({ limit: String(limit) });

    return client
      .get<TelemetryRecord[]>(
        `/devices/${encodeURIComponent(deviceCode)}/telemetry?${query.toString()}`,
      )
      .then((response) => response.data);
  },

  getAlerts(params: { deviceCode?: string; includeResolved?: boolean } = {}): Promise<DeviceAlert[]> {
    const query = new URLSearchParams();

    if (params.deviceCode) {
      query.set('deviceCode', params.deviceCode);
    }

    query.set('includeResolved', String(params.includeResolved ?? false));

    return client
      .get<DeviceAlert[]>(`/alerts?${query.toString()}`)
      .then((response) => response.data);
  },

  resolveAlert(alertId: string, payload: ResolveAlertRequest = {}): Promise<DeviceAlert> {
    return client
      .post<DeviceAlert>(
        `/alerts/${encodeURIComponent(alertId)}/resolve`,
        payload,
      )
      .then((response) => response.data);
  },
};

export const toUserMessage = (error: unknown): string =>
  error instanceof IotApiError
    ? error.message
    : error instanceof Error && error.message
      ? error.message
      : 'Ocurrió un error inesperado. Intenta nuevamente.';
