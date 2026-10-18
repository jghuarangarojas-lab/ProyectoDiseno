import { SaleRequest, SaleResponse } from '../types/pos';
import { createHttpClient } from '../lib/httpClient';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5050';

const client = createHttpClient({
  baseURL: API_BASE_URL,
});

export const posService = {
  async processSale(saleRequest: SaleRequest): Promise<SaleResponse> {
    const response = await client.post<SaleResponse>('/api/sales', saleRequest);
    return response.data;
  },
};
