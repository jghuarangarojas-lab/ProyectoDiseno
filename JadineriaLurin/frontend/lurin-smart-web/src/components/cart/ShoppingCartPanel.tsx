import React from 'react';
import { CartItem } from './CartItem';
import { Button } from '../common/Button';
import { useCart } from '../../contexts/CartContext';
import { CartItem as CartItemType } from '../../types/pos';

interface ShoppingCartPanelProps {
  onManualAdjust: (item: CartItemType) => void;
  onCheckout: () => void;
}

export const ShoppingCartPanel: React.FC<ShoppingCartPanelProps> = ({ onManualAdjust, onCheckout }) => {
  const { items, removeItem, total } = useCart();

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 flex flex-col h-full">
      <div className="p-4 border-b">
        <h2 className="text-lg font-semibold text-gray-900">Carrito de Compras</h2>
      </div>
      <div className="flex-1 overflow-y-auto p-4">
        {items.length === 0 ? (
          <p className="text-gray-500 text-center py-8">Carrito vacío</p>
        ) : (
          <div className="space-y-2">
            {items.map(item => (
              <CartItem
                key={item.id}
                item={item}
                onManualAdjust={onManualAdjust}
                onRemove={removeItem}
              />
            ))}
          </div>
        )}
      </div>
      <div className="p-4 border-t bg-gray-50">
        <div className="flex items-center justify-between mb-4">
          <span className="text-lg font-semibold text-gray-900">Total:</span>
          <span className="text-xl font-bold text-green-600">S/. {total.toFixed(2)}</span>
        </div>
        <Button
          className="w-full"
          size="lg"
          onClick={onCheckout}
          disabled={items.length === 0}
        >
          Cobrar
        </Button>
      </div>
    </div>
  );
};
