import type { AxiosError } from 'axios';
import { DELIVERY_API_BASE_URL, DELIVERY_API_PREFIX } from '../config/api';
import { createHttpClient } from '../lib/httpClient';
import type {
  DeliveryOrder,
  DeliveryOrderPage,
  DeliveryOrderQuery,
  Driver,
  UpdateDeliveryStatusDto,
} from '../types/logistics';

export class DeliveryApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = 'DeliveryApiError';
  }
}

interface ErrorPayload {
  status?: number;
  title?: string;
  detail?: string;
  message?: string;
}

const client = createHttpClient({
  baseURL: `${DELIVERY_API_BASE_URL}${DELIVERY_API_PREFIX}`,
  timeout: 20000,
});

client.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ErrorPayload>) => {
    if (!error.response) {
      return Promise.reject(
        new DeliveryApiError(
          0,
          'NETWORK_ERROR',
          'No se pudo conectar con el servicio de entregas. Verifica que ms-delivery esté disponible.',
        ),
      );
    }

    const payload = error.response.data;

    return Promise.reject(
      new DeliveryApiError(
        error.response.status,
        'HTTP_ERROR',
        payload?.detail ??
          payload?.message ??
          payload?.title ??
          'El servicio de entregas devolvió una respuesta inesperada.',
      ),
    );
  },
);

export const toDeliveryUserMessage = (error: unknown): string =>
  error instanceof Error ? error.message : 'Ocurrió un error inesperado.';

export const logisticsService = {
  async getOrders(query: DeliveryOrderQuery = {}): Promise<DeliveryOrderPage> {
    const { data } = await client.get<DeliveryOrderPage>('/orders', {
      params: {
        status: query.status,
        driverId: query.driverId,
        deliveryDate: query.deliveryDate,
        page: query.page ?? 1,
        pageSize: query.pageSize ?? 50,
      },
    });
    return data;
  },

  async getActiveOrders(): Promise<DeliveryOrder[]> {
    const [pending, dispatched] = await Promise.all([
      logisticsService.getOrders({ status: 'PENDING', pageSize: 50 }),
      logisticsService.getOrders({ status: 'DISPATCHED', pageSize: 50 }),
    ]);
    return [...pending.items, ...dispatched.items];
  },

  async getOrdersByDriver(driverId: string): Promise<DeliveryOrder[]> {
    const { data } = await client.get<DeliveryOrder[]>(`/orders/driver/${driverId}`);
    return data;
  },

  async getOrder(orderId: string): Promise<DeliveryOrder> {
    const { data } = await client.get<DeliveryOrder>(`/orders/${orderId}`);
    return data;
  },

  async updateStatus(orderId: string, dto: UpdateDeliveryStatusDto): Promise<DeliveryOrder> {
    const form = new FormData();
    form.append('status', dto.status);
    if (dto.failureReason) form.append('failureReason', dto.failureReason);
    if (dto.proofPhoto) form.append('proofPhoto', dto.proofPhoto);
    if (dto.digitalSignature) form.append('digitalSignature', dto.digitalSignature);

    const { data } = await client.put<DeliveryOrder>(`/orders/${orderId}/status`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  },

  async reassignOrder(orderId: string, driverId: string): Promise<DeliveryOrder> {
    const { data } = await client.put<DeliveryOrder>(`/orders/${orderId}/driver`, { driverId });
    return data;
  },

  async getDrivers(isActive?: boolean): Promise<Driver[]> {
    const { data } = await client.get<Driver[]>('/drivers', { params: { isActive } });
    return data;
  },
};
