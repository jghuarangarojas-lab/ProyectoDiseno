import { useState } from 'react';
import { CartIcon, LeafIcon, RefreshIcon, SpinnerIcon } from '../icons';
import type { ShoppingListItem } from '../../types/inventory';

interface RestockPanelProps {
  items: ShoppingListItem[];
  isLoading: boolean;
  isRefreshing: boolean;
  lastLoadedAt: string | null;
  onRefresh: () => void;
}

export function RestockPanel({
  items,
  isLoading,
  isRefreshing,
  lastLoadedAt,
  onRefresh,
}: RestockPanelProps) {
  const [failedImages, setFailedImages] = useState<Set<string>>(new Set());

  const totalSuggested = items.reduce(
    (accumulator, item) => accumulator + item.suggestedOrderQuantity,
    0,
  );

  const markImageFailed = (productId: string) => {
    setFailedImages((previous) => new Set(previous).add(productId));
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center gap-2 py-20 text-slate-500">
        <SpinnerIcon className="h-5 w-5 animate-spin" />
        <span className="text-sm">Cargando lista de compras...</span>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <div className="overflow-hidden rounded-2xl bg-white shadow-sm ring-1 ring-slate-900/5">
        <div className="flex flex-col gap-3 border-b border-slate-100 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="flex items-center gap-2 text-base font-bold text-slate-800">
              <CartIcon className="h-5 w-5 text-emerald-600" />
              Lista de Compras Sugerida
            </h2>
            <p className="mt-0.5 text-sm text-slate-500">
              {items.length === 0
                ? 'Todos los productos están por encima de su stock mínimo.'
                : `${items.length} producto${items.length === 1 ? '' : 's'} por debajo del mínimo · ${totalSuggested} unidades a reponer`}
              {lastLoadedAt &&
                ` · Actualizado ${new Date(lastLoadedAt).toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })}`}
            </p>
          </div>

          <button
            type="button"
            onClick={onRefresh}
            disabled={isRefreshing}
            className="flex items-center gap-2 self-start rounded-full border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 transition hover:bg-slate-100 disabled:opacity-60 sm:self-auto"
          >
            {isRefreshing ? (
              <SpinnerIcon className="h-4 w-4 animate-spin" />
            ) : (
              <RefreshIcon className="h-4 w-4" />
            )}
            Actualizar
          </button>
        </div>

        {items.length === 0 ? (
          <div className="flex flex-col items-center gap-2 px-6 py-16 text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-emerald-100">
              <LeafIcon className="h-7 w-7 text-emerald-600" />
            </div>
            <p className="font-semibold text-slate-700">Sin faltantes por ahora</p>
            <p className="max-w-sm text-sm text-slate-400">
              Ningún producto requiere reposición. Cuando el stock de algún
              producto baje del mínimo, aparecerá aquí automáticamente.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-sm">
              <thead>
                <tr className="bg-slate-50 text-left text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-5 py-3">Producto</th>
                  <th className="px-3 py-3">SKU</th>
                  <th className="px-3 py-3">Categoría</th>
                  <th className="px-3 py-3 text-right">Disponible</th>
                  <th className="px-3 py-3 text-right">Mínimo</th>
                  <th className="px-5 py-3 text-right">A comprar</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.map((item) => (
                  <tr key={item.productId} className="transition hover:bg-rose-50/40">
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-gradient-to-br from-emerald-50 to-lime-50 ring-1 ring-slate-900/5">
                          {item.imageUrl && !failedImages.has(item.productId) ? (
                            <img
                              src={item.imageUrl}
                              alt={item.commonName}
                              loading="lazy"
                              onError={() => markImageFailed(item.productId)}
                              className="h-full w-full object-cover"
                            />
                          ) : (
                            <LeafIcon className="h-5 w-5 text-emerald-400" />
                          )}
                        </div>
                        <span className="font-medium text-slate-700">
                          {item.commonName}
                        </span>
                      </div>
                    </td>
                    <td className="px-3 py-3">
                      <span className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-xs font-bold text-slate-600">
                        {item.sku}
                      </span>
                    </td>
                    <td className="px-3 py-3 text-slate-500">{item.category}</td>
                    <td className="px-3 py-3 text-right font-semibold text-rose-600">
                      {item.stockAvailable}
                    </td>
                    <td className="px-3 py-3 text-right text-slate-600">
                      {item.stockMinimum}
                    </td>
                    <td className="px-5 py-3 text-right">
                      <span className="inline-flex items-center rounded-full bg-rose-600 px-2.5 py-1 text-xs font-bold text-white shadow-sm">
                        {item.suggestedOrderQuantity} u.
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
