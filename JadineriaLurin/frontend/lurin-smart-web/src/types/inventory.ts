export interface Product {
  id: string;
  sku: string;
  commonName: string;
  imageUrl: string | null;
  category: string;
  stockAvailable: number;
  stockMinimum: number;
  isBelowMinimum: boolean;
  updatedAt: string;
}

export interface InventoryEntry {
  id: string;
  productId: string;
  sku: string;
  commonName: string;
  quantity: number;
  supplier: string;
  entryDate: string;
  expirationDate: string | null;
}

export interface RegisterEntryItem {
  productId?: string;
  sku?: string;
  quantity: number;
}

export interface RegisterEntryRequest {
  supplier: string;
  entryDate?: string;
  expirationDate?: string | null;
  items: RegisterEntryItem[];
}

export type ManagerReviewStatus = 'PENDING_REVIEW' | 'APPROVED' | 'REJECTED';

export interface MermaRecord {
  id: string;
  productId: string;
  sku: string;
  commonName: string;
  quantityLost: number;
  reason: string;
  evidencePhotoUrl: string | null;
  managerReviewStatus: ManagerReviewStatus;
  createdAt: string;
}

export interface RegisterMermaRequest {
  productId: string;
  quantityLost: number;
  reason: string;
  evidencePhoto: File;
}

export interface ShoppingListItem {
  productId: string;
  sku: string;
  commonName: string;
  category: string;
  imageUrl: string | null;
  stockAvailable: number;
  stockMinimum: number;
  suggestedOrderQuantity: number;
}

export interface LowStockAlertPayload {
  productId: string;
  sku: string;
  commonName: string;
  category: string;
  stockAvailable: number;
  stockMinimum: number;
  suggestedOrderQuantity: number;
  detectedAtUtc: string;
}

export interface ProductSearchParams {
  name?: string;
  category?: string;
  page?: number;
  pageSize?: number;
}

export interface MermaReasonOption {
  code: string;
  label: string;
  description: string;
}
