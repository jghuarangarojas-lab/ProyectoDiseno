import { useMemo, useState } from 'react';
import { EMERGENCY_REASONS } from '../../constants/emergencyReasons';
import type {
  CancelState,
  EmergencyReasonCode,
  Sector,
} from '../../types/irrigation';
import { AlertTriangleIcon, CheckIcon, SpinnerIcon, StopIcon } from '../icons';
import { ModalShell } from './ModalShell';

interface EmergencyReasonScreenProps {
  sector: Sector;
  cancelState: CancelState;
  isSavingReason: boolean;
  onRetryCancel: () => void;
  onConfirm: (reason: string) => void;
}

export function EmergencyReasonScreen({
  sector,
  cancelState,
  isSavingReason,
  onRetryCancel,
  onConfirm,
}: EmergencyReasonScreenProps) {
  const [selected, setSelected] = useState<EmergencyReasonCode | null>(null);
  const [details, setDetails] = useState('');

  const reason = useMemo(() => {
    const option = EMERGENCY_REASONS.find((item) => item.code === selected);
    if (!option) {
      return '';
    }

    if (selected === 'OTROS') {
      const trimmed = details.trim();
      return trimmed.length > 0 ? `Otros: ${trimmed}` : 'Otros';
    }

    return option.label;
  }, [selected, details]);

  const canConfirm = cancelState === 'sent' && reason.length >= 3;

  return (
    <ModalShell
      ariaLabel="Registro de motivo de parada de emergencia"
      maxWidth="max-w-lg"
      zIndexClass="z-[70]"
      overlayClassName="bg-slate-950/85 backdrop-blur-sm"
    >
      <header className="bg-red-600 px-5 py-5 text-white sm:px-6">
        <div className="flex items-center gap-3">
          <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-white/15 ring-2 ring-white/40">
            <StopIcon className="h-7 w-7" />
          </span>
          <div>
            <h2 className="text-lg font-extrabold uppercase tracking-wide">
              Parada de emergencia activada
            </h2>
            <p className="text-xs text-red-100">
              Sector {sector.id} · {sector.deviceId}
            </p>
          </div>
        </div>
      </header>

      <div className="space-y-5 px-5 py-5 sm:px-6">
        {cancelState === 'sending' && (
          <div className="flex items-center gap-2.5 rounded-xl bg-amber-50 px-4 py-3 text-sm font-semibold text-amber-800 ring-1 ring-inset ring-amber-200">
            <SpinnerIcon className="h-4 w-4 animate-spin shrink-0" />
            Enviando orden de cancelación al dispositivo…
          </div>
        )}

        {cancelState === 'sent' && (
          <div className="flex items-center gap-2.5 rounded-xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-800 ring-1 ring-inset ring-emerald-200">
            <CheckIcon className="h-4 w-4 shrink-0" />
            Orden de cancelación enviada. El riego fue detenido.
          </div>
        )}

        {cancelState === 'error' && (
          <div className="space-y-3 rounded-xl bg-red-50 px-4 py-3 ring-1 ring-inset ring-red-200">
            <div className="flex items-center gap-2.5 text-sm font-semibold text-red-700">
              <AlertTriangleIcon className="h-4 w-4 shrink-0" />
              No se pudo cancelar el riego. Reintenta antes de registrar el motivo.
            </div>
            <button
              type="button"
              onClick={onRetryCancel}
              className="w-full rounded-lg border-2 border-red-300 bg-white px-4 py-2 text-xs font-bold text-red-600 transition hover:bg-red-100"
            >
              Reintentar cancelación
            </button>
          </div>
        )}

        <section>
          <h3 className="mb-1 text-sm font-extrabold text-slate-900">
            Selecciona el motivo de la parada
          </h3>
          <p className="mb-3 text-xs text-slate-500">
            El motivo es obligatorio y quedará registrado en el historial del
            riego.
          </p>

          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
            {EMERGENCY_REASONS.map((option) => {
              const isSelected = selected === option.code;

              return (
                <button
                  key={option.code}
                  type="button"
                  aria-pressed={isSelected}
                  onClick={() => setSelected(option.code)}
                  className={`rounded-xl border-2 p-3 text-left transition ${
                    isSelected
                      ? 'border-red-500 bg-red-50 shadow-sm'
                      : 'border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50'
                  }`}
                >
                  <span className="block text-sm font-bold text-slate-800">
                    {option.label}
                  </span>
                  <span className="mt-0.5 block text-[11px] leading-snug text-slate-500">
                    {option.description}
                  </span>
                </button>
              );
            })}
          </div>
        </section>

        {selected === 'OTROS' && (
          <div>
            <label
              htmlFor="emergency-reason-details"
              className="mb-1.5 block text-xs font-semibold text-slate-600"
            >
              Describe el motivo
            </label>
            <textarea
              id="emergency-reason-details"
              value={details}
              onChange={(event) => setDetails(event.target.value)}
              maxLength={256}
              rows={3}
              placeholder="Detalle qué ocurrió…"
              className="w-full resize-none rounded-xl border-2 border-slate-200 px-3 py-2 text-sm text-slate-800 outline-none transition focus:border-red-400"
            />
            <p className="mt-1 text-right text-[11px] text-slate-400">
              {details.length}/256
            </p>
          </div>
        )}

        <button
          type="button"
          disabled={!canConfirm || isSavingReason}
          onClick={() => onConfirm(reason)}
          className="flex w-full items-center justify-center gap-2 rounded-xl bg-slate-900 px-4 py-3 text-sm font-bold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:bg-slate-300"
        >
          {isSavingReason && <SpinnerIcon className="h-4 w-4 animate-spin" />}
          {isSavingReason ? 'Guardando motivo…' : 'Guardar motivo y finalizar'}
        </button>
      </div>
    </ModalShell>
  );
}
