import { Product } from '../types/pos';

export const getPriceForType = (product: Product, type: 'minorista' | 'mayorista' = 'minorista'): number => {
  return type === 'mayorista' ? product.priceMayorista : product.priceMinorista;
};

export const calculateCartTotal = (items: Array<{ finalPrice: number; quantity: number }>): number => {
  return items.reduce((sum, item) => sum + item.finalPrice * item.quantity, 0);
};
