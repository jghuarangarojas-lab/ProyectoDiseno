import { useState } from 'react';
import { LeafIcon } from '../icons';
import type { Product } from '../../types/inventory';

interface ProductVisualCardProps {
  product: Product;
  onSelect: (product: Product) => void;
}

export function ProductVisualCard({ product, onSelect }: ProductVisualCardProps) {
  const [imageFailed, setImageFailed] = useState(false);

  const isLow = product.stockAvailable <= product.stockMinimum;

  return (
    <button
      type="button"
      onClick={() => onSelect(product)}
      className={`group overflow-hidden rounded-2xl bg-white text-left shadow-sm ring-1 transition hover:-translate-y-0.5 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-emerald-300 ${
        isLow ? 'ring-rose-200' : 'ring-slate-900/5'
      }`}
    >
      <div className="relative aspect-[4/3] overflow-hidden bg-gradient-to-br from-emerald-50 via-lime-50 to-teal-50">
        {product.imageUrl && !imageFailed ? (
          <img
            src={product.imageUrl}
            alt={product.commonName}
            loading="lazy"
            onError={() => setImageFailed(true)}
            className="h-full w-full object-cover transition duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <LeafIcon className="h-16 w-16 text-emerald-300" />
          </div>
        )}

        <span className="absolute left-2.5 top-2.5 rounded-md bg-white/95 px-2 py-0.5 font-mono text-xs font-bold tracking-wide text-slate-700 shadow-sm ring-1 ring-slate-900/10">
          {product.sku}
        </span>

        <span
          className={`absolute right-2.5 top-2.5 flex h-12 w-12 flex-col items-center justify-center rounded-full text-white shadow-md ring-2 ring-white/80 ${
            isLow ? 'bg-rose-600' : 'bg-emerald-600'
          }`}
          title={
            isLow
              ? 'Stock en el mínimo o inferior: requiere reposición'
              : 'Stock disponible por encima del mínimo'
          }
        >
          <span className="text-sm font-bold leading-none">
            {product.stockAvailable}
          </span>
          <span className="text-[9px] leading-none opacity-90">
            mín {product.stockMinimum}
          </span>
        </span>

        {isLow && (
          <span className="absolute bottom-2 left-2.5 rounded-full bg-rose-600 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-white shadow-sm">
            Reponer
          </span>
        )}
      </div>

      <div className="space-y-1 px-3.5 py-3">
        <p className="truncate text-sm font-semibold text-slate-800">
          {product.commonName}
        </p>
        <div className="flex items-center justify-between">
          <span className="rounded-full bg-slate-100 px-2 py-0.5 text-[11px] font-medium text-slate-500">
            {product.category}
          </span>
          <span
            className={`text-xs font-semibold ${
              isLow ? 'text-rose-600' : 'text-emerald-600'
            }`}
          >
            {isLow ? 'Stock bajo' : 'Disponible'}
          </span>
        </div>
      </div>
    </button>
  );
}
