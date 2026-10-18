import { useEffect, useRef, useState, type FormEvent } from 'react';
import { ArrowLeftIcon, CameraIcon, UploadIcon } from '../icons';
import { MERMA_REASONS } from '../../constants/mermaReasons';
import type { Product, RegisterMermaRequest } from '../../types/inventory';

const MAX_PHOTO_BYTES = 10 * 1024 * 1024;
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

interface MermaFormProps {
  product: Product;
  isSubmitting: boolean;
  onSubmit: (payload: RegisterMermaRequest) => Promise<boolean>;
  onCancel: () => void;
}

export function MermaForm({ product, isSubmitting, onSubmit, onCancel }: MermaFormProps) {
  const [photo, setPhoto] = useState<File | null>(null);
  const [photoPreviewUrl, setPhotoPreviewUrl] = useState<string | null>(null);
  const [quantity, setQuantity] = useState('');
  const [reasonCode, setReasonCode] = useState('');
  const [customReason, setCustomReason] = useState('');
  const [error, setError] = useState<string | null>(null);

  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    return () => {
      if (photoPreviewUrl) {
        URL.revokeObjectURL(photoPreviewUrl);
      }
    };
  }, [photoPreviewUrl]);

  const applyPhoto = (file: File | undefined) => {
    if (!file) {
      return;
    }

    if (!ACCEPTED_TYPES.includes(file.type)) {
      setError('La evidencia debe ser una imagen (JPG, PNG, WEBP o GIF).');
      return;
    }

    if (file.size > MAX_PHOTO_BYTES) {
      setError('La foto no puede superar los 10 MB.');
      return;
    }

    if (photoPreviewUrl) {
      URL.revokeObjectURL(photoPreviewUrl);
    }

    setPhoto(file);
    setPhotoPreviewUrl(URL.createObjectURL(file));
    setError(null);
  };

  const selectedReason = MERMA_REASONS.find((option) => option.code === reasonCode);
  const needsCustomReason = reasonCode === 'OTROS';

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!photo) {
      setError('Debes adjuntar una foto de evidencia o abrir la cámara.');
      return;
    }

    const parsedQuantity = Number(quantity);

    if (!quantity || Number.isNaN(parsedQuantity) || parsedQuantity <= 0) {
      setError('Ingresa la cantidad perdida (mayor a cero).');
      return;
    }

    if (parsedQuantity > product.stockAvailable) {
      setError(
        `La cantidad excede el stock disponible (${product.stockAvailable}).`,
      );
      return;
    }

    if (!selectedReason) {
      setError('Selecciona el motivo de la merma.');
      return;
    }

    const reason = needsCustomReason
      ? customReason.trim() || 'Otros'
      : selectedReason.label;

    if (needsCustomReason && !customReason.trim()) {
      setError('Describe el motivo de la merma.');
      return;
    }

    setError(null);

    await onSubmit({
      productId: product.id,
      quantityLost: parsedQuantity,
      reason,
      evidencePhoto: photo,
    });
  };

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

      <div className="rounded-xl bg-rose-50 p-3 text-sm text-rose-800">
        Al declarar la merma de <strong>{product.commonName}</strong> ({product.sku}),
        se descontarán las unidades del stock disponible de forma inmediata y el
        registro quedará en revisión (<span className="font-mono">PENDING_REVIEW</span>).
      </div>

      <div>
        <span className="mb-1.5 block text-sm font-medium text-slate-700">
          Foto de evidencia *
        </span>

        <div className="flex flex-wrap items-center gap-3">
          <input
            ref={cameraInputRef}
            type="file"
            accept="image/*"
            capture="environment"
            className="hidden"
            onChange={(event) => {
              applyPhoto(event.target.files?.[0]);
              event.target.value = '';
            }}
          />
          <input
            ref={galleryInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={(event) => {
              applyPhoto(event.target.files?.[0]);
              event.target.value = '';
            }}
          />

          <button
            type="button"
            onClick={() => cameraInputRef.current?.click()}
            className="flex items-center gap-2 rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-700"
          >
            <CameraIcon className="h-4 w-4" />
            Abrir cámara
          </button>

          <button
            type="button"
            onClick={() => galleryInputRef.current?.click()}
            className="flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 transition hover:bg-slate-100"
          >
            <UploadIcon className="h-4 w-4" />
            Subir foto
          </button>

          {photo && (
            <button
              type="button"
              onClick={() => {
                if (photoPreviewUrl) {
                  URL.revokeObjectURL(photoPreviewUrl);
                }
                setPhoto(null);
                setPhotoPreviewUrl(null);
              }}
              className="text-sm font-medium text-rose-600 transition hover:text-rose-700"
            >
              Quitar foto
            </button>
          )}
        </div>

        <div className="mt-3">
          {photoPreviewUrl && photo ? (
            <div className="relative inline-block overflow-hidden rounded-xl ring-1 ring-slate-900/10">
              <img
                src={photoPreviewUrl}
                alt="Vista previa de la evidencia"
                className="h-40 w-auto object-cover"
              />
              <span className="absolute bottom-1.5 left-1.5 rounded-md bg-black/60 px-1.5 py-0.5 text-[10px] font-medium text-white">
                {(photo.size / 1024).toFixed(0)} KB
              </span>
            </div>
          ) : (
            <p className="rounded-lg border border-dashed border-slate-300 px-3 py-4 text-center text-sm text-slate-400">
              Sin evidencia adjunta. La foto es obligatoria para registrar la merma.
            </p>
          )}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-slate-700">
            Cantidad perdida *
          </span>
          <input
            type="number"
            min="0.01"
            step="any"
            value={quantity}
            onChange={(event) => setQuantity(event.target.value)}
            placeholder={`Máx. ${product.stockAvailable}`}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 outline-none transition focus:border-rose-400 focus:ring-2 focus:ring-rose-100"
          />
        </label>

        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-slate-700">
            Motivo *
          </span>
          <select
            value={reasonCode}
            onChange={(event) => {
              setReasonCode(event.target.value);
              setError(null);
            }}
            className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 outline-none transition focus:border-rose-400 focus:ring-2 focus:ring-rose-100"
          >
            <option value="">Selecciona un motivo...</option>
            {MERMA_REASONS.map((option) => (
              <option key={option.code} value={option.code}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
      </div>

      {selectedReason && (
        <p className="-mt-2 text-xs text-slate-500">{selectedReason.description}</p>
      )}

      {needsCustomReason && (
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-slate-700">
            Describe el motivo *
          </span>
          <textarea
            value={customReason}
            onChange={(event) => setCustomReason(event.target.value)}
            rows={2}
            maxLength={500}
            placeholder="Detalle qué ocurrió con el producto..."
            className="w-full resize-none rounded-lg border border-slate-200 px-3 py-2 outline-none transition focus:border-rose-400 focus:ring-2 focus:ring-rose-100"
          />
        </label>
      )}

      {error && (
        <p className="rounded-lg bg-rose-50 px-3 py-2 text-sm font-medium text-rose-700">
          {error}
        </p>
      )}

      <div className="flex gap-3 pt-1">
        <button
          type="submit"
          disabled={isSubmitting}
          className="flex-1 rounded-full bg-rose-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-rose-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? 'Registrando merma...' : 'Declarar merma'}
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
