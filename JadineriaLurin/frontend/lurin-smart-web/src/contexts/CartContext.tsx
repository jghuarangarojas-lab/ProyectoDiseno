import React, { createContext, useContext, useState, useCallback } from 'react';
import { CartItem, Product } from '../types/pos';
import { getPriceForType } from '../utils/priceHelper';
const uuidv4 = () => crypto.randomUUID();

interface CartContextType {
  items: CartItem[];
  addToCart: (product: Product, quantity?: number) => void;
  updateManualPrice: (itemId: string, finalPrice: number, overrideReason: string) => void;
  removeItem: (itemId: string) => void;
  clearCart: () => void;
  total: number;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [items, setItems] = useState<CartItem[]>([]);

  const addToCart = useCallback((product: Product, quantity = 1) => {
    setItems(prev => {
      const existing = prev.find(item => item.product.id === product.id && !item.isManualOverride);
      if (existing) {
        return prev.map(item =>
          item.id === existing.id
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }
      const price = getPriceForType(product, product.priceType || 'minorista');
      return [
        ...prev,
        {
          id: uuidv4(),
          product,
          quantity,
          originalPrice: price,
          finalPrice: price,
          isManualOverride: false,
        },
      ];
    });
  }, []);

  const updateManualPrice = useCallback((itemId: string, finalPrice: number, overrideReason: string) => {
    setItems(prev =>
      prev.map(item =>
        item.id === itemId
          ? {
              ...item,
              finalPrice,
              isManualOverride: true,
              overrideReason,
            }
          : item
      )
    );
  }, []);

  const removeItem = useCallback((itemId: string) => {
    setItems(prev => prev.filter(item => item.id !== itemId));
  }, []);

  const clearCart = useCallback(() => {
    setItems([]);
  }, []);

  const total = items.reduce((sum, item) => sum + item.finalPrice * item.quantity, 0);

  return (
    <CartContext.Provider
      value={{
        items,
        addToCart,
        updateManualPrice,
        removeItem,
        clearCart,
        total,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
