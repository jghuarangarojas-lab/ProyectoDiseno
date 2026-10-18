import { AlertTriangleIcon, DropIcon, SpinnerIcon } from '../icons';
import type { Sector } from '../../types/irrigation';
import { ModalShell } from './ModalShell';

interface ConfirmIrrigationModalProps {
  sector: Sector;
  seconds: number;
  isSubmitting: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmIrrigationModal({
  sector,
  seconds,
  isSubmitting,
  onConfirm,
  onCancel,
}: ConfirmIrrigationModalProps) {
  return (
    <ModalShell
      ariaLabel="Confirmar inicio de riego"
      onClose={isSubmitting ? undefined : onCancel}
      maxWidth="max-w-md"
      zIndexClass="z-[60]"
      overlayClassName="bg-slate-950/70 backdrop-blur-sm"
    >
      <div className="p-6">
        <div className="mb-4 flex items-center gap-3">
          <span className="flex h-11 w-11 items-center justify-center rounded-full bg-emerald-100 text-emerald-700">
            <DropIcon className="h-6 w-6" />
          </span>
          <div>
            <h2 className="text-lg font-extrabold text-slate-900">
              Confirmar riego
            </h2>
            <p className="text-xs text-slate-500">
              La orden se enviará de forma inmediata al dispositivo.
            </p>
          </div>
        </div>

        <dl className="mb-4 divide-y divide-slate-100 rounded-xl border border-slate-200 bg-slate-50/70">
          <div className="flex items-center justify-between px-4 py-2.5">
            <dt className="text-xs font-semibold text-slate-500">Sector</dt>
            <dd className="font-mono text-sm font-bold text-slate-800">
              {sector.id}
            </dd>
          </div>
          <div className="flex items-center justify-between px-4 py-2.5">
            <dt className="text-xs font-semibold text-slate-500">Dispositivo</dt>
            <dd className="font-mono text-sm font-bold text-slate-800">
              {sector.deviceId}
            </dd>
          </div>
          <div className="flex items-center justify-between px-4 py-2.5">
            <dt className="text-xs font-semibold text-slate-500">Duración</dt>
            <dd className="text-sm font-extrabold text-emerald-700">
              {seconds} segundos
            </dd>
          </div>
        </dl>

        <div className="mb-5 flex items-start gap-2 rounded-xl bg-amber-50 px-3 py-2.5 text-xs text-amber-800 ring-1 ring-inset ring-amber-200">
          <AlertTriangleIcon className="mt-0.5 h-4 w-4 shrink-0" />
          <p>
            Se activará la bomba del dispositivo durante {seconds}s. No podrás
            cerrar esta ventana hasta que el riego finalice o actives la parada
            de emergencia.
          </p>
        </div>

        <div className="flex gap-3">
          <button
            type="button"
            disabled={isSubmitting}
            onClick={onCancel}
            className="flex-1 rounded-xl border-2 border-slate-200 px-4 py-2.5 text-sm font-bold text-slate-600 transition hover:bg-slate-100 disabled:opacity-50"
          >
            Volver
          </button>
          <button
            type="button"
            disabled={isSubmitting}
            onClick={onConfirm}
            className="flex flex-1 items-center justify-center gap-2 rounded-xl bg-emerald-600 px-4 py-2.5 text-sm font-bold text-white transition hover:bg-emerald-700 disabled:cursor-wait disabled:opacity-70"
          >
            {isSubmitting && <SpinnerIcon className="h-4 w-4 animate-spin" />}
            {isSubmitting ? 'Enviando…' : 'Sí, regar ahora'}
          </button>
        </div>
      </div>
    </ModalShell>
  );
}
