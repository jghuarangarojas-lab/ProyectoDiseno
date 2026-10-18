import { useEffect, useState } from 'react';
import { useCountdown } from '../../hooks/useCountdown';
import type { CancelState, IrrigationAction, Sector } from '../../types/irrigation';
import { StopIcon } from '../icons';
import { ModalShell } from './ModalShell';
import { PlantIrrigationAnimation } from './PlantIrrigationAnimation';
import { StatusBadge } from './StatusBadge';

interface ActiveIrrigationViewProps {
  sector: Sector;
  action: IrrigationAction;
  liveAction: IrrigationAction | null;
  cancelState: CancelState;
  onEmergency: () => void;
}

const formatTime = (totalSeconds: number): string => {
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
};

export function ActiveIrrigationView({
  sector,
  action,
  liveAction,
  cancelState,
  onEmergency,
}: ActiveIrrigationViewProps) {
  const { remainingSeconds, start, stop, sync } = useCountdown(
    action.requestedDurationSeconds,
  );
  const [awaitingDevice, setAwaitingDevice] = useState(false);

  useEffect(() => {
    start();
  }, [start]);

  useEffect(() => {
    if (!liveAction || liveAction.id !== action.id) {
      return;
    }

    if (liveAction.status === 'IN_PROGRESS' && liveAction.startedAtUtc) {
      const startedAt = new Date(liveAction.startedAtUtc).getTime();

      if (!Number.isNaN(startedAt)) {
        const elapsed = Math.max(0, Math.floor((Date.now() - startedAt) / 1000));
        sync(Math.max(0, action.requestedDurationSeconds - elapsed));
        setAwaitingDevice(false);
        start();
      }
      return;
    }

    if (liveAction.status === 'COMPLETED') {
      sync(0);
      stop();
      setAwaitingDevice(false);
    }
  }, [liveAction, action, sync, start, stop]);

  useEffect(() => {
    if (remainingSeconds === 0) {
      stop();
      setAwaitingDevice(true);
    }
  }, [remainingSeconds, stop]);

  const totalSeconds = Math.max(1, action.requestedDurationSeconds);
  const progressPercentage = Math.max(
    0,
    Math.min(100, (remainingSeconds / totalSeconds) * 100),
  );

  const status = liveAction?.status ?? action.status;
  const statusLabel =
    status === 'PENDING' ? 'Enviando comando al dispositivo…' : 'Bomba en operación';

  return (
    <ModalShell
      ariaLabel={`Riego activo en ${sector.id}`}
      maxWidth="max-w-lg"
      overlayClassName="bg-slate-950/75 backdrop-blur-sm"
    >
      <header className="border-b border-slate-200 bg-emerald-50/80 px-5 py-4 pr-12 sm:px-6">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <p className="text-[11px] font-bold uppercase tracking-wider text-emerald-700">
              Riego en curso
            </p>
            <h2 className="font-mono text-xl font-extrabold tracking-tight text-slate-900">
              {sector.id}
            </h2>
            <p className="text-xs text-slate-500">{sector.deviceId}</p>
          </div>
          <StatusBadge status={status} />
        </div>
      </header>

      <div className="space-y-5 px-5 py-5 sm:px-6">
        <div className="flex justify-center rounded-2xl bg-sky-50 p-4 ring-1 ring-sky-100">
          <PlantIrrigationAnimation className="h-36 w-full max-w-[16rem]" />
        </div>

        <div className="text-center">
          <p className="text-[11px] font-bold uppercase tracking-[0.2em] text-slate-400">
            {awaitingDevice
              ? 'Tiempo cumplido · esperando al dispositivo'
              : statusLabel}
          </p>
          <p className="mt-1 font-mono text-6xl font-extrabold tabular-nums text-slate-900">
            {formatTime(remainingSeconds)}
          </p>
        </div>

        <div>
          <div className="h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
            <div
              className="h-full rounded-full bg-emerald-500 transition-all duration-1000 ease-linear"
              style={{ width: `${progressPercentage}%` }}
            />
          </div>
          <div className="mt-2 flex items-center justify-between text-[11px] font-medium text-slate-500">
            <span>Duración solicitada: {action.requestedDurationSeconds}s</span>
            <span>Límite bomba: {action.maxPumpDurationSeconds}s</span>
          </div>
        </div>

        <div className="relative">
          <span
            aria-hidden="true"
            className="irrigation-pulse-ring pointer-events-none absolute inset-0 rounded-2xl ring-4 ring-red-400"
          />
          <button
            type="button"
            onClick={onEmergency}
            disabled={cancelState === 'sending'}
            className="relative flex w-full items-center justify-center gap-3 rounded-2xl bg-red-600 px-6 py-5 text-lg font-extrabold uppercase tracking-wide text-white shadow-xl shadow-red-300/70 transition hover:bg-red-700 focus:outline-none focus-visible:ring-4 focus-visible:ring-red-300 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <StopIcon className="h-7 w-7" />
            Parada de emergencia
          </button>
        </div>

        <p className="text-center text-[11px] text-slate-400">
          Al activarla se cancela el riego de inmediato y deberás registrar el
          motivo.
        </p>
      </div>
    </ModalShell>
  );
}
