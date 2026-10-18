import { useState, type FormEvent } from 'react';
import { ArrowLeftIcon } from '../icons';
import type { Product, RegisterEntryRequest } from '../../types/inventory';

interface AddLotFormProps {
  product: Product;
  isSubmitting: boolean;
  onSubmit: (payload: RegisterEntryRequest) => Promise<boolean>;
  onCancel: () => void;
}

export function AddLotForm({
  product,
  isSubmitting,
  onSubmit,
  onCancel,
}: AddLotFormProps) {
  const [quantity, setQuantity] = useState('');
  const [supplier, setSupplier] = useState('');
  const [expirationDate, setExpirationDate] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const parsedQuantity = Number(quantity);

    if (!quantity || Number.isNaN(parsedQuantity) || parsedQuantity <= 0) {
      setError('Ingresa una cantidad mayor a cero.');
      return;
    }

    if (!supplier.trim()) {
      setError('El proveedor es obligatorio.');
      return;
    }

    setError(null);

    const succeeded = await onSubmit({
      items: [{ productId: product.id, quantity: parsedQuantity }],
      supplier: supplier.trim(),
      entryDate: new Date().toISOString(),
      expirationDate: expirationDate
        ? new Date(expirationDate).toISOString()
        : null,
    });

    if (succeeded) {
      setQuantity('');
      setSupplier('');
      setExpirationDate('');
    }
  };

  const today = new Date().toISOString().slice(0, 10);

  return (
    <form onSubmit={handleSubmit} className="space-y-4 px-6 pb-6 pt-4">
      <button
        type="button"
        onClick={onCancel}
        className="flex items-center gap-1.5 text-sm font-medium text-slate-500 transition hover:text-slate-700"
      >
        <ArrowLeftIcon className="h-4 w-4" />
        Volver a detalles
      </button>

      <div className="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-800">
        Ingreso de lote para <strong>{product.commonName}</strong> ({product.sku}).
        Stock actual: <strong>{product.stockAvailable}</strong> · Mínimo:{' '}
        <strong>{product.stockMinimum}</strong>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-slate-700">
            Cantidad *
          </span>
          <input
            type="number"
            min="0.01"
            step="any"
            value={quantity}
            onChange={(event) => setQuantity(event.target.value)}
            placeholder="Ej. 25"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 outline-none transition focus:border-emerald-400 focus:ring-2 focus:ring-emerald-100"
          />
        </label>

        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-slate-700">
            Proveedor *
          </span>
          <input
            type="text"
            value={supplier}
            onChange={(event) => setSupplier(event.target.value)}
            placeholder="Ej. Viveros El Huerto"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 outline-none transition focus:border-emerald-400 focus:ring-2 focus:ring-emerald-100"
          />
        </label>
      </div>

      <label className="block text-sm">
        <span className="mb-1.5 block font-medium text-slate-700">
          Fecha de vencimiento
          <span className="ml-1 font-normal text-slate-400">
            (opcional, solo insumos perecederos)
          </span>
        </span>
        <input
          type="date"
          min={today}
          value={expirationDate}
          onChange={(event) => setExpirationDate(event.target.value)}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 outline-none transition focus:border-emerald-400 focus:ring-2 focus:ring-emerald-100 sm:max-w-[240px]"
        />
      </label>

      {error && (
        <p className="rounded-lg bg-rose-50 px-3 py-2 text-sm font-medium text-rose-700">
          {error}
        </p>
      )}

      <div className="flex gap-3 pt-1">
        <button
          type="submit"
          disabled={isSubmitting}
          className="flex-1 rounded-full bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? 'Registrando...' : 'Registrar lote'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          className="rounded-full border border-slate-200 px-5 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-100 disabled:opacity-60"
        >
          Cancelar
        </button>
      </div>
    </form>
  );
}
