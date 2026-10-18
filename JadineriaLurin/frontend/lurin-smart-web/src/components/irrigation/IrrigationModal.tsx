import { useState } from 'react';
import toast from 'react-hot-toast';
import { ClockIcon, DropIcon } from '../icons';
import type { IrrigationAction, Sector } from '../../types/irrigation';
import { ModalShell } from './ModalShell';
import { StatusBadge } from './StatusBadge';

const MAX_SECONDS = 60;
const PRESET_SECONDS = [5, 10, 30];

interface IrrigationModalProps {
  sector: Sector;
  history: IrrigationAction[];
  isHistoryLoading: boolean;
  currentAction?: IrrigationAction | null;
  onRegar: (seconds: number) => void;
  onClose?: () => void;
}

const formatDate = (iso: string): string => {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return '—';
  }

  return date.toLocaleString('es-PE', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const historyReason = (action: IrrigationAction): string | null => {
  if (action.status === 'CANCELLED') {
    return action.cancellationReason;
  }
  if (action.status === 'FAILED') {
    return action.failureReason;
  }
  return null;
};

export function IrrigationModal({
  sector,
  history,
  isHistoryLoading,
  currentAction,
  onRegar,
  onClose,
}: IrrigationModalProps) {
  const [secondsInput, setSecondsInput] = useState('30');

  const numericSeconds = Number(secondsInput);
  const isValidDuration =
    secondsInput.trim() !== '' &&
    Number.isInteger(numericSeconds) &&
    numericSeconds >= 1 &&
    numericSeconds <= MAX_SECONDS;

  const handleInputChange = (raw: string) => {
    if (raw.trim() === '') {
      setSecondsInput('');
      return;
    }

    const digits = raw.replace(/[^0-9]/g, '');
    if (digits === '') {
      return;
    }

    const parsed = Number.parseInt(digits, 10);

    if (parsed > MAX_SECONDS) {
      setSecondsInput(String(MAX_SECONDS));
      toast.error(`Límite estricto: no se permite regar más de ${MAX_SECONDS} segundos.`);
      return;
    }

    setSecondsInput(String(parsed));
  };

  const handleInputBlur = () => {
    const parsed = Number(secondsInput);
    if (secondsInput.trim() !== '' && (!Number.isFinite(parsed) || parsed < 1)) {
      setSecondsInput('1');
    }
  };

  return (
    <ModalShell
      ariaLabel={`Detalles del sector ${sector.id}`}
      onClose={onClose}
      maxWidth="max-w-2xl"
    >
      <header className="border-b border-slate-200 bg-slate-50/80 px-5 py-4 pr-12 sm:px-6">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <p className="text-[11px] font-bold uppercase tracking-wider text-slate-400">
              Sector
            </p>
            <h2 className="font-mono text-xl font-extrabold tracking-tight text-slate-900">
              {sector.id}
            </h2>
            <p className="text-xs text-slate-500">
              {sector.name} · {sector.deviceId}
            </p>
          </div>
          <StatusBadge status={currentAction?.status} />
        </div>
      </header>

      <div className="space-y-5 px-5 py-5 sm:px-6">
        <section>
          <h3 className="mb-2.5 flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-slate-400">
            <ClockIcon className="h-4 w-4" />
            Historial · últimos 3 riegos
          </h3>

          {isHistoryLoading ? (
            <ul className="space-y-2">
              {[0, 1, 2].map((item) => (
                <li
                  key={item}
                  className="h-16 animate-pulse rounded-xl bg-slate-100"
                />
              ))}
            </ul>
          ) : history.length === 0 ? (
            <p className="rounded-xl border border-dashed border-slate-200 bg-slate-50/60 p-4 text-center text-sm text-slate-500">
              Este sector aún no tiene riegos registrados.
            </p>
          ) : (
            <ul className="space-y-2">
              {history.map((item) => {
                const reason = historyReason(item);

                return (
                  <li
                    key={item.id}
                    className="flex items-start justify-between gap-3 rounded-xl border border-slate-200 bg-slate-50/70 p-3"
                  >
                    <div className="min-w-0">
                      <p className="text-xs font-bold text-slate-700">
                        {formatDate(item.createdAtUtc)}
                      </p>
                      <p className="mt-0.5 text-[11px] text-slate-500">
                        {item.requestedDurationSeconds}s solicitados
                        {item.actualDurationSeconds !== null
                          ? ` · ${item.actualDurationSeconds}s reales`
                          : ''}{' '}
                        ·{' '}
                        {item.triggerType === 'AUTOMATIC' ? 'Automático' : 'Manual'}
                      </p>
                      {reason && (
                        <p className="mt-1 truncate text-[11px] font-medium text-red-600">
                          {reason}
                        </p>
                      )}
                    </div>
                    <StatusBadge status={item.status} />
                  </li>
                );
              })}
            </ul>
          )}
        </section>

        <section className="rounded-2xl border border-emerald-100 bg-emerald-50/50 p-4">
          <h3 className="mb-3 text-xs font-bold uppercase tracking-wider text-emerald-700">
            Control manual
          </h3>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-end">
            <div className="flex-1">
              <label
                htmlFor="irrigation-seconds"
                className="mb-1.5 block text-xs font-semibold text-slate-600"
              >
                Duración del riego
              </label>

              <div className="flex items-center gap-2">
                <input
                  id="irrigation-seconds"
                  type="number"
                  inputMode="numeric"
                  min={1}
                  max={MAX_SECONDS}
                  step={1}
                  value={secondsInput}
                  onChange={(event) => handleInputChange(event.target.value)}
                  onBlur={handleInputBlur}
                  className="w-28 rounded-xl border-2 border-slate-200 bg-white px-3 py-2.5 text-center font-mono text-lg font-bold text-slate-900 outline-none transition focus:border-emerald-500"
                />
                <span className="text-sm font-semibold text-slate-500">
                  segundos
                </span>
              </div>

              <div className="mt-2.5 flex flex-wrap items-center gap-2">
                {PRESET_SECONDS.map((preset) => {
                  const isActive = numericSeconds === preset;

                  return (
                    <button
                      key={preset}
                      type="button"
                      onClick={() => setSecondsInput(String(preset))}
                      className={`rounded-lg px-3 py-1.5 text-xs font-bold transition ${
                        isActive
                          ? 'bg-emerald-600 text-white shadow-sm'
                          : 'bg-white text-slate-600 ring-1 ring-inset ring-slate-200 hover:bg-slate-100'
                      }`}
                    >
                      {preset}s
                    </button>
                  );
                })}
              </div>

              <p
                className={`mt-2 text-[11px] ${
                  isValidDuration || secondsInput.trim() === ''
                    ? 'text-slate-500'
                    : 'font-semibold text-red-600'
                }`}
              >
                {isValidDuration || secondsInput.trim() === ''
                  ? `Máximo permitido: ${MAX_SECONDS} segundos.`
                  : 'Ingresa un valor entero entre 1 y 60 segundos.'}
              </p>
            </div>

            <button
              type="button"
              disabled={!isValidDuration}
              onClick={() => onRegar(numericSeconds)}
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-emerald-600 px-6 py-3 text-sm font-bold text-white shadow-sm transition hover:bg-emerald-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-slate-300"
            >
              <DropIcon className="h-5 w-5" />
              Regar
            </button>
          </div>
        </section>
      </div>
    </ModalShell>
  );
}
