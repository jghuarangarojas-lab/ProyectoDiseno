import React from 'react';
import { CartItem as CartItemType } from '../../types/pos';
import { Button } from '../common/Button';

interface CartItemProps {
  item: CartItemType;
  onManualAdjust: (item: CartItemType) => void;
  onRemove: (itemId: string) => void;
}

export const CartItem: React.FC<CartItemProps> = ({ item, onManualAdjust, onRemove }) => {
  return (
    <div className="border-b border-gray-200 py-3 last:border-0">
      <div className="flex items-start justify-between">
        <div className="flex-1 min-w-0">
          <h4 className="font-medium text-gray-900 truncate">{item.product.name}</h4>
          <p className="text-xs text-gray-500">SKU: {item.product.sku}</p>
          <p className="text-sm text-gray-600 mt-1">
            Cantidad: {item.quantity} × S/. {item.originalPrice.toFixed(2)}
          </p>
          {item.isManualOverride && (
            <p className="text-xs text-amber-600 mt-1">
              Ajuste manual: {item.overrideReason}
            </p>
          )}
          <p className="text-sm font-semibold text-gray-900 mt-1">
            Subtotal: S/. {(item.finalPrice * item.quantity).toFixed(2)}
          </p>
        </div>
        <div className="flex flex-col gap-1 ml-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onManualAdjust(item)}
          >
            Ajuste Manual
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={() => onRemove(item.id)}
          >
            Quitar
          </Button>
        </div>
      </div>
    </div>
  );
};
