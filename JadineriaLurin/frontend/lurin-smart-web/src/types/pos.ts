export type PaymentMethod = 'Efectivo' | 'Tarjeta' | 'Yape' | 'Plin';
export type DocumentType = 'Boleta' | 'Factura';
export type PriceType = 'minorista' | 'mayorista';

export interface Product {
  id: string;
  sku: string;
  name: string;
  priceMinorista: number;
  priceMayorista: number;
  imageUrl?: string;
  stock?: number;
  priceType?: PriceType;
}

export interface CartItem {
  id: string;
  product: Product;
  quantity: number;
  originalPrice: number;
  finalPrice: number;
  isManualOverride: boolean;
  overrideReason?: string;
}

export interface SaleRequest {
  totalAmount: number;
  paymentMethod: PaymentMethod;
  documentType: DocumentType;
  documentCorrelative: string;
  isDelivery: boolean;
  saleDetails: SaleDetailRequest[];
  deliveryAddress?: string;
  deliveryDate?: string;
  deliveryTime?: string;
  deliveryPhone?: string;
  deliveryReference?: string;
}

export interface SaleDetailRequest {
  productSku: string;
  quantity: number;
  originalPrice: number;
  finalPrice: number;
  isManualOverride: boolean;
  overrideReason?: string;
}

export interface SaleResponse {
  saleId: string;
  totalAmount: number;
  paymentMethod: PaymentMethod;
  documentType: DocumentType;
  documentCorrelative: string;
  isDelivery: boolean;
  createdAt: string;
  sunatStatus?: string;
  pdfUrl?: string;
  sunatHash?: string;
}
