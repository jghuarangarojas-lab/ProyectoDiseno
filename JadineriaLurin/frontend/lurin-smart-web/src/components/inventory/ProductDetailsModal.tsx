import { useEffect, useState } from 'react';
import { ModalShell } from '../irrigation/ModalShell';
import { AlertTriangleIcon, CartIcon, LeafIcon } from '../icons';
import { AddLotForm } from './AddLotForm';
import { MermaForm } from './MermaForm';
import type {
  Product,
  RegisterEntryRequest,
  RegisterMermaRequest,
} from '../../types/inventory';

type ModalView = 'details' | 'lot' | 'merma';

interface ProductDetailsModalProps {
  product: Product;
  isSubmittingEntry: boolean;
  isSubmittingMerma: boolean;
  onClose: () => void;
  onSubmitEntry: (payload: RegisterEntryRequest) => Promise<boolean>;
  onSubmitMerma: (payload: RegisterMermaRequest) => Promise<boolean>;
}

export function ProductDetailsModal({
  product,
  isSubmittingEntry,
  isSubmittingMerma,
  onClose,
  onSubmitEntry,
  onSubmitMerma,
}: ProductDetailsModalProps) {
  const [view, setView] = useState<ModalView>('details');
  const [imageFailed, setImageFailed] = useState(false);

  useEffect(() => {
    setView('details');
    setImageFailed(false);
  }, [product.id]);

  const isLow = product.stockAvailable <= product.stockMinimum;

  const handleMermaSubmit = async (payload: RegisterMermaRequest) => {
    const succeeded = await onSubmitMerma(payload);

    if (succeeded) {
      onClose();
    }

    return succeeded;
  };

  return (
    <ModalShell ariaLabel={`Detalles de ${product.commonName}`} onClose={onClose} maxWidth="max-w-3xl">
      <div className="relative h-44 overflow-hidden rounded-t-2xl bg-gradient-to-br from-emerald-100 via-lime-50 to-teal-100 sm:h-52">
        {product.imageUrl && !imageFailed ? (
          <img
            src={product.imageUrl}
            alt={product.commonName}
            onError={() => setImageFailed(true)}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <LeafIcon className="h-24 w-24 text-emerald-300" />
          </div>
        )}

        <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 to-transparent px-5 pb-4 pt-10">
          <div className="flex items-end justify-between gap-3">
            <div>
              <span className="rounded-md bg-white/95 px-2 py-0.5 font-mono text-xs font-bold text-slate-700">
                {product.sku}
              </span>
              <h2 className="mt-1.5 text-xl font-bold text-white drop-shadow">
                {product.commonName}
              </h2>
            </div>
            <span className="rounded-full bg-white/95 px-2.5 py-1 text-xs font-semibold text-slate-600">
              {product.category}
            </span>
          </div>
        </div>
      </div>

      {view === 'details' && (
        <div className="px-6 pb-6 pt-5">
          <div className="grid grid-cols-3 gap-3">
            <div className="rounded-xl bg-slate-50 p-3 text-center ring-1 ring-slate-900/5">
              <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                Disponible
              </p>
              <p
                className={`mt-0.5 text-2xl font-bold ${
                  isLow ? 'text-rose-600' : 'text-emerald-600'
                }`}
              >
                {product.stockAvailable}
              </p>
            </div>

            <div className="rounded-xl bg-slate-50 p-3 text-center ring-1 ring-slate-900/5">
              <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                Stock mínimo
              </p>
              <p className="mt-0.5 text-2xl font-bold text-slate-700">
                {product.stockMinimum}
              </p>
            </div>

            <div
              className={`rounded-xl p-3 text-center ring-1 ${
                isLow
                  ? 'bg-rose-50 ring-rose-200'
                  : 'bg-emerald-50 ring-emerald-200'
              }`}
            >
              <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                Estado
              </p>
              <p
                className={`mt-1 text-xs font-bold ${
                  isLow ? 'text-rose-600' : 'text-emerald-600'
                }`}
              >
                {isLow ? 'Reposición requerida' : 'Stock saludable'}
              </p>
            </div>
          </div>

          {isLow && (
            <p className="mt-3 flex items-start gap-2 rounded-xl bg-rose-50 px-3 py-2.5 text-sm text-rose-700">
              <AlertTriangleIcon className="mt-0.5 h-4 w-4 shrink-0 text-rose-500" />
              El stock está por debajo del mínimo. Se recomienda registrar un lote
              de al menos{' '}
              <strong className="font-semibold">
                {Math.ceil(product.stockMinimum - product.stockAvailable)} unidades
              </strong>
              .
            </p>
          )}

          <div className="mt-5 grid gap-3 sm:grid-cols-2">
            <button
              type="button"
              onClick={() => setView('lot')}
              className="flex items-center justify-center gap-2 rounded-full bg-emerald-600 px-4 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-700"
            >
              <CartIcon className="h-4 w-4" />
              Agregar Lote
            </button>

            <button
              type="button"
              onClick={() => setView('merma')}
              className="flex items-center justify-center gap-2 rounded-full bg-rose-600 px-4 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-rose-700"
            >
              <AlertTriangleIcon className="h-4 w-4 text-white" />
              Declarar Merma
            </button>
          </div>
        </div>
      )}

      {view === 'lot' && (
        <AddLotForm
          product={product}
          isSubmitting={isSubmittingEntry}
          onSubmit={onSubmitEntry}
          onCancel={() => setView('details')}
        />
      )}

      {view === 'merma' && (
        <MermaForm
          product={product}
          isSubmitting={isSubmittingMerma}
          onSubmit={handleMermaSubmit}
          onCancel={() => setView('details')}
        />
      )}
    </ModalShell>
  );
}
