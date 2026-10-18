import type { AxiosError } from 'axios';
import { INVENTORY_API_BASE_URL, INVENTORY_API_PREFIX } from '../config/api';
import { createHttpClient } from '../lib/httpClient';
import type {
  InventoryEntry,
  MermaRecord,
  Product,
  ProductSearchParams,
  RegisterEntryRequest,
  RegisterMermaRequest,
  ShoppingListItem,
} from '../types/inventory';

export class InventoryApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = 'InventoryApiError';
  }
}

interface ErrorPayload {
  status?: number;
  statusCode?: number;
  code?: string;
  title?: string;
  detail?: string;
  message?: string;
}

const client = createHttpClient({
  baseURL: `${INVENTORY_API_BASE_URL}${INVENTORY_API_PREFIX}`,
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
        new InventoryApiError(
          0,
          'NETWORK_ERROR',
          'No se pudo conectar con el servicio de inventario. Verifica que ms-inventory esté disponible.',
        ),
      );
    }

    const payload = error.response.data;

    return Promise.reject(
      new InventoryApiError(
        error.response.status,
        payload?.code ?? 'HTTP_ERROR',
        payload?.detail ??
          payload?.message ??
          payload?.title ??
          'El servicio de inventario devolvió una respuesta inesperada.',
      ),
    );
  },
);

export const inventoryService = {
  searchProducts(params: ProductSearchParams = {}): Promise<Product[]> {
    const query = new URLSearchParams();

    if (params.name?.trim()) {
      query.set('name', params.name.trim());
    }

    if (params.category?.trim()) {
      query.set('category', params.category.trim());
    }

    query.set('page', String(params.page ?? 1));
    query.set('pageSize', String(params.pageSize ?? 60));

    const search = query.toString();

    return client
      .get<Product[]>(`/products${search ? `?${search}` : ''}`)
      .then((response) => response.data);
  },

  getProduct(productId: string): Promise<Product> {
    return client
      .get<Product>(`/products/${encodeURIComponent(productId)}`)
      .then((response) => response.data);
  },

  registerEntryBatch(payload: RegisterEntryRequest): Promise<InventoryEntry[]> {
    return client
      .post<InventoryEntry[]>('/entries', payload)
      .then((response) => response.data);
  },

  registerMerma(payload: RegisterMermaRequest): Promise<MermaRecord> {
    const form = new FormData();
    form.set('productId', payload.productId);
    form.set('quantityLost', String(payload.quantityLost));
    form.set('reason', payload.reason);
    form.set('evidencePhoto', payload.evidencePhoto);

    return client
      .post<MermaRecord>('/mermas', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
        timeout: 30000,
      })
      .then((response) => response.data);
  },

  getMermas(productId?: string): Promise<MermaRecord[]> {
    const query = new URLSearchParams();

    if (productId) {
      query.set('productId', productId);
    }

    const search = query.toString();

    return client
      .get<MermaRecord[]>(`/mermas${search ? `?${search}` : ''}`)
      .then((response) => response.data);
  },

  getShoppingList(): Promise<ShoppingListItem[]> {
    return client
      .get<ShoppingListItem[]>('/shopping-list')
      .then((response) => response.data);
  },
};

export const toInventoryUserMessage = (error: unknown): string =>
  error instanceof InventoryApiError
    ? error.message
    : error instanceof Error && error.message
      ? error.message
      : 'Ocurrió un error inesperado. Intenta nuevamente.';
