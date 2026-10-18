import React from 'react';
import { Product } from '../../types/pos';
import { Button } from '../common/Button';
import { getPriceForType } from '../../utils/priceHelper';

interface PosProductCardProps {
  product: Product;
  onAddToCart: (product: Product) => void;
}

export const PosProductCard: React.FC<PosProductCardProps> = ({ product, onAddToCart }) => {
  const price = getPriceForType(product, product.priceType || 'minorista');

  return (
    <div
      className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md transition-shadow cursor-pointer"
      onClick={() => onAddToCart(product)}
    >
      {product.imageUrl && (
        <img
          src={product.imageUrl}
          alt={product.name}
          className="w-full h-32 object-cover rounded-md mb-3"
        />
      )}
      <h3 className="font-semibold text-gray-900 mb-1 truncate">{product.name}</h3>
      <p className="text-sm text-gray-500 mb-2">SKU: {product.sku}</p>
      <div className="flex items-center justify-between">
        <span className="text-lg font-bold text-green-600">
          S/. {price.toFixed(2)}
        </span>
        <Button size="sm" onClick={(e) => { e.stopPropagation(); onAddToCart(product); }}>
          Agregar
        </Button>
      </div>
    </div>
  );
};
