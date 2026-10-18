export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5083';

export const AUTH_API_BASE_URL =
  import.meta.env.VITE_AUTH_API_BASE_URL ?? 'http://localhost:5080';

export const HUB_URL = `${API_BASE_URL}/hubs/irrigation`;

export const IRRIGATION_API_PREFIX = '/api/irrigation';

export const IOT_API_BASE_URL =
  import.meta.env.VITE_IOT_API_BASE_URL ?? 'http://localhost:5084';

export const IOT_HUB_URL = `${IOT_API_BASE_URL}/hubs/iot`;

export const IOT_API_PREFIX = '/api';

export const INVENTORY_API_BASE_URL =
  import.meta.env.VITE_INVENTORY_API_BASE_URL ?? 'http://localhost:5213';

export const INVENTORY_HUB_URL = `${INVENTORY_API_BASE_URL}/hubs/inventory`;

export const INVENTORY_API_PREFIX = '/api/inventory';

export const DELIVERY_API_BASE_URL =
  import.meta.env.VITE_DELIVERY_API_BASE_URL ?? 'http://localhost:5214';

export const DELIVERY_API_PREFIX = '/api/delivery';
