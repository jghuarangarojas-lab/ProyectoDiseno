import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { ActiveIrrigationView } from '../components/irrigation/ActiveIrrigationView';
import { ConfirmIrrigationModal } from '../components/irrigation/ConfirmIrrigationModal';
import { EmergencyReasonScreen } from '../components/irrigation/EmergencyReasonScreen';
import { IrrigationModal } from '../components/irrigation/IrrigationModal';
import { MapDashboard } from '../components/irrigation/MapDashboard';
import { DropIcon, RefreshIcon } from '../components/icons';
import { SECTORS, uniqueDeviceIds } from '../data/sectors';
import {
  useIrrigationWebSockets,
  type IrrigationSocketMode,
} from '../hooks/useIrrigationWebSockets';
import { irrigationService, toUserMessage } from '../services/irrigation.service';
import type { CancelState, IrrigationAction, Sector } from '../types/irrigation';
import { isAttentionStatus } from '../utils/status';

type ModalPhase =
  | { type: 'closed' }
  | { type: 'details'; sector: Sector }
  | { type: 'confirm'; sector: Sector; seconds: number }
  | { type: 'active'; sector: Sector; action: IrrigationAction }
  | { type: 'emergency'; sector: Sector; action: IrrigationAction };

const PROVISIONAL_CANCEL_REASON =
  'Parada de emergencia solicitada por el operador';

const SOCKET_MODE: IrrigationSocketMode =
  import.meta.env.VITE_WS_MODE === 'signalr' ? 'signalr' : 'simulation';

const CONNECTION_META: Record<
  string,
  { label: string; dot: string; classes: string }
> = {
  connected: {
    label: 'Tiempo real conectado',
    dot: 'bg-emerald-500',
    classes: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  },
  connecting: {
    label: 'Conectando…',
    dot: 'bg-amber-500',
    classes: 'bg-amber-50 text-amber-700 ring-amber-200',
  },
  reconnecting: {
    label: 'Reconectando…',
    dot: 'bg-amber-500',
    classes: 'bg-amber-50 text-amber-700 ring-amber-200',
  },
  disconnected: {
    label: 'Sin conexión',
    dot: 'bg-red-500',
    classes: 'bg-red-50 text-red-700 ring-red-200',
  },
};

export default function IrrigationDashboardPage() {
  const [phase, setPhase] = useState<ModalPhase>({ type: 'closed' });
  const [statuses, setStatuses] = useState<Record<string, IrrigationAction>>({});
  const [history, setHistory] = useState<IrrigationAction[]>([]);
  const [isHistoryLoading, setIsHistoryLoading] = useState(false);
  const [isStarting, setIsStarting] = useState(false);
  const [cancelState, setCancelState] = useState<CancelState>('idle');
  const [isSavingReason, setIsSavingReason] = useState(false);
  const [liveAction, setLiveAction] = useState<IrrigationAction | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const phaseRef = useRef(phase);
  phaseRef.current = phase;

  const historyRequestRef = useRef(0);
  const simulationTimersRef = useRef<number[]>([]);

  const { mode: socketMode, status: socketStatus, onIrrigationUpdated, emitSimulatedEvent } =
    useIrrigationWebSockets({ mode: SOCKET_MODE, joinDashboard: true });

  const clearSimulationTimers = useCallback(() => {
    for (const timerId of simulationTimersRef.current) {
      window.clearTimeout(timerId);
    }
    simulationTimersRef.current = [];
  }, []);

  const mergeStatus = useCallback((incoming: IrrigationAction) => {
    setStatuses((previous) => {
      const existing = previous[incoming.sectorArea];

      if (
        existing &&
        new Date(existing.updatedAtUtc).getTime() >
          new Date(incoming.updatedAtUtc).getTime()
      ) {
        return previous;
      }

      return { ...previous, [incoming.sectorArea]: incoming };
    });
  }, []);

  const refreshStatuses = useCallback(
    async (silent = false) => {
      setIsRefreshing(true);

      try {
        const results = await Promise.all(
          uniqueDeviceIds(SECTORS).map((deviceId) =>
            irrigationService.getHistory(deviceId, 100),
          ),
        );

        for (const actions of results) {
          for (const action of actions) {
            mergeStatus(action);
          }
        }
      } catch (error) {
        if (!silent) {
          toast.error(toUserMessage(error));
        }
      } finally {
        setIsRefreshing(false);
      }
    },
    [mergeStatus],
  );

  const closeFlow = useCallback(() => {
    clearSimulationTimers();
    historyRequestRef.current += 1;
    setPhase({ type: 'closed' });
    setHistory([]);
    setIsHistoryLoading(false);
    setLiveAction(null);
    setCancelState('idle');
    setIsSavingReason(false);
  }, [clearSimulationTimers]);

  const finishActive = useCallback(
    (kind: 'success' | 'error' | 'info', message: string) => {
      closeFlow();

      if (kind === 'success') {
        toast.success(message);
      } else if (kind === 'error') {
        toast.error(message);
      } else {
        toast(message);
      }

      void refreshStatuses(true);
    },
    [closeFlow, refreshStatuses],
  );

  const handleIrrigationUpdated = useCallback(
    (incoming: IrrigationAction) => {
      mergeStatus(incoming);

      const current = phaseRef.current;
      if (current.type !== 'active' && current.type !== 'emergency') {
        return;
      }

      if (current.action.id !== incoming.id) {
        return;
      }

      setLiveAction(incoming);

      if (current.type === 'emergency') {
        if (incoming.status === 'CANCELLED') {
          setCancelState('sent');
        }
        return;
      }

      if (incoming.status === 'COMPLETED') {
        finishActive('success', `Riego completado en ${current.sector.id}.`);
      } else if (incoming.status === 'FAILED') {
        finishActive(
          'error',
          incoming.failureReason ??
            `El riego en ${current.sector.id} falló.`,
        );
      } else if (incoming.status === 'CANCELLED') {
        finishActive('info', `Riego cancelado en ${current.sector.id}.`);
      }
    },
    [mergeStatus, finishActive],
  );

  useEffect(
    () => onIrrigationUpdated(handleIrrigationUpdated),
    [onIrrigationUpdated, handleIrrigationUpdated],
  );

  useEffect(() => {
    void refreshStatuses();

    const intervalId = window.setInterval(() => {
      void refreshStatuses(true);
    }, 15000);

    return () => window.clearInterval(intervalId);
  }, [refreshStatuses]);

  useEffect(
    () => () => {
      historyRequestRef.current += 1;
      clearSimulationTimers();
    },
    [clearSimulationTimers],
  );

  const scheduleSimulation = useCallback(
    (action: IrrigationAction) => {
      if (socketMode !== 'simulation') {
        return;
      }

      let startedAtIso: string | null = null;

      const startTimerId = window.setTimeout(() => {
        startedAtIso = new Date().toISOString();
        emitSimulatedEvent({
          ...action,
          status: 'IN_PROGRESS',
          startedAtUtc: startedAtIso,
          updatedAtUtc: startedAtIso,
        });
      }, 900);

      const finishTimerId = window.setTimeout(
        () => {
          const finishedAtIso = new Date().toISOString();
          emitSimulatedEvent({
            ...action,
            status: 'COMPLETED',
            startedAtUtc: startedAtIso ?? action.createdAtUtc,
            finishedAtUtc: finishedAtIso,
            actualDurationSeconds: action.requestedDurationSeconds,
            updatedAtUtc: finishedAtIso,
          });
        },
        900 + action.requestedDurationSeconds * 1000,
      );

      simulationTimersRef.current.push(startTimerId, finishTimerId);
    },
    [socketMode, emitSimulatedEvent],
  );

  const openSector = useCallback(async (sector: Sector) => {
    const requestId = historyRequestRef.current + 1;
    historyRequestRef.current = requestId;

    setPhase({ type: 'details', sector });
    setHistory([]);
    setIsHistoryLoading(true);

    try {
      const actions = await irrigationService.getHistory(sector.deviceId, 100);

      if (historyRequestRef.current !== requestId) {
        return;
      }

      setHistory(
        actions
          .filter((action) => action.sectorArea === sector.id)
          .slice(0, 3),
      );
    } catch (error) {
      if (historyRequestRef.current !== requestId) {
        return;
      }

      toast.error(toUserMessage(error));
    } finally {
      if (historyRequestRef.current === requestId) {
        setIsHistoryLoading(false);
      }
    }
  }, []);

  const closeDetails = useCallback(() => {
    if (phaseRef.current.type !== 'details') {
      return;
    }

    historyRequestRef.current += 1;
    setHistory([]);
    setIsHistoryLoading(false);
    setPhase({ type: 'closed' });
  }, []);

  const handleRequestIrrigation = useCallback((seconds: number) => {
    const current = phaseRef.current;
    if (current.type !== 'details') {
      return;
    }

    setPhase({ type: 'confirm', sector: current.sector, seconds });
  }, []);

  const handleConfirmIrrigation = useCallback(async () => {
    const current = phaseRef.current;
    if (current.type !== 'confirm') {
      return;
    }

    const { sector, seconds } = current;
    setIsStarting(true);

    try {
      const action = await irrigationService.start({
        deviceId: sector.deviceId,
        sectorArea: sector.id,
        triggerType: 'MANUAL',
        requestedDurationSeconds: seconds,
      });

      mergeStatus(action);
      setLiveAction(action);
      setCancelState('idle');
      setHistory([]);
      setPhase({ type: 'active', sector, action });
      toast.success(`Riego iniciado en ${sector.id} (${seconds}s).`);
      scheduleSimulation(action);
    } catch (error) {
      toast.error(toUserMessage(error));
    } finally {
      setIsStarting(false);
    }
  }, [mergeStatus, scheduleSimulation]);

  const sendCancellation = useCallback(
    async (action: IrrigationAction) => {
      setCancelState('sending');

      try {
        const updated = await irrigationService.cancel(
          action.id,
          PROVISIONAL_CANCEL_REASON,
        );

        mergeStatus(updated);
        setLiveAction(updated);
        setCancelState('sent');
        toast.success('Orden de cancelación enviada al dispositivo.');
      } catch (error) {
        setCancelState('error');
        toast.error(toUserMessage(error));
      }
    },
    [mergeStatus],
  );

  const handleEmergency = useCallback(() => {
    const current = phaseRef.current;
    if (current.type !== 'active') {
      return;
    }

    setPhase({ type: 'emergency', sector: current.sector, action: current.action });
    clearSimulationTimers();
    void sendCancellation(current.action);
  }, [clearSimulationTimers, sendCancellation]);

  const handleRetryCancel = useCallback(() => {
    const current = phaseRef.current;
    if (current.type !== 'emergency') {
      return;
    }

    void sendCancellation(current.action);
  }, [sendCancellation]);

  const handleSaveReason = useCallback(
    async (reason: string) => {
      const current = phaseRef.current;
      if (current.type !== 'emergency') {
        return;
      }

      setIsSavingReason(true);

      try {
        const updated = await irrigationService.updateCancellationReason(
          current.action.id,
          reason,
        );

        mergeStatus(updated);
        toast.success('Motivo de la parada registrado en el historial.');
        closeFlow();
        void refreshStatuses(true);
      } catch (error) {
        toast.error(toUserMessage(error));
      } finally {
        setIsSavingReason(false);
      }
    },
    [mergeStatus, closeFlow, refreshStatuses],
  );

  const activeCount = useMemo(
    () =>
      Object.values(statuses).filter((item) => item.status === 'IN_PROGRESS')
        .length,
    [statuses],
  );

  const attentionCount = useMemo(
    () =>
      Object.values(statuses).filter((item) => isAttentionStatus(item.status))
        .length,
    [statuses],
  );

  const connection = CONNECTION_META[socketStatus] ?? CONNECTION_META.disconnected;

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-4">
          <div className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-600 text-white shadow-sm">
              <DropIcon className="h-6 w-6" />
            </span>
            <div>
              <h1 className="text-base font-extrabold tracking-tight text-slate-900">
                Jardinería Lurín Smart
              </h1>
              <p className="text-xs font-medium text-slate-500">
                Módulo de Riego · Tablero de sectores
              </p>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-[11px] font-bold ring-1 ring-inset ${connection.classes}`}
            >
              <span className={`h-2 w-2 rounded-full ${connection.dot}`} />
              {connection.label}
            </span>

            <span className="rounded-full bg-slate-100 px-3 py-1.5 text-[11px] font-bold text-slate-500 ring-1 ring-inset ring-slate-200">
              {socketMode === 'simulation' ? 'Modo simulación' : 'SignalR'}
            </span>

            <button
              type="button"
              onClick={() => void refreshStatuses(true)}
              className="inline-flex items-center gap-1.5 rounded-full bg-white px-3 py-1.5 text-[11px] font-bold text-slate-600 ring-1 ring-inset ring-slate-200 transition hover:bg-slate-50"
            >
              <RefreshIcon
                className={`h-3.5 w-3.5 ${isRefreshing ? 'animate-spin' : ''}`}
              />
              Actualizar
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl space-y-6 px-4 py-6">
        <section className="flex flex-wrap items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-extrabold tracking-tight text-slate-900">
              Mapa de sectores
            </h2>
            <p className="mt-0.5 text-sm text-slate-500">
              Selecciona una tarjeta para ver el historial y controlar el riego
              manual.
            </p>
          </div>

          <div className="flex flex-wrap gap-2">
            <span className="rounded-xl bg-white px-3 py-2 text-xs font-bold text-slate-600 ring-1 ring-inset ring-slate-200">
              {SECTORS.length} sectores
            </span>
            <span className="rounded-xl bg-blue-50 px-3 py-2 text-xs font-bold text-blue-700 ring-1 ring-inset ring-blue-200">
              {activeCount} en riego
            </span>
            <span className="rounded-xl bg-red-50 px-3 py-2 text-xs font-bold text-red-700 ring-1 ring-inset ring-red-200">
              {attentionCount} con atención
            </span>
          </div>
        </section>

        <MapDashboard
          sectors={SECTORS}
          statuses={statuses}
          onSelect={(sector) => void openSector(sector)}
        />
      </main>

      {(phase.type === 'details' || phase.type === 'confirm') && (
        <IrrigationModal
          sector={phase.sector}
          history={history}
          isHistoryLoading={isHistoryLoading}
          currentAction={statuses[phase.sector.id]}
          onRegar={handleRequestIrrigation}
          onClose={phase.type === 'details' ? closeDetails : undefined}
        />
      )}

      {phase.type === 'confirm' && (
        <ConfirmIrrigationModal
          sector={phase.sector}
          seconds={phase.seconds}
          isSubmitting={isStarting}
          onConfirm={() => void handleConfirmIrrigation()}
          onCancel={() => setPhase({ type: 'details', sector: phase.sector })}
        />
      )}

      {phase.type === 'active' && (
        <ActiveIrrigationView
          sector={phase.sector}
          action={phase.action}
          liveAction={
            liveAction && liveAction.id === phase.action.id ? liveAction : null
          }
          cancelState={cancelState}
          onEmergency={handleEmergency}
        />
      )}

      {phase.type === 'emergency' && (
        <EmergencyReasonScreen
          sector={phase.sector}
          cancelState={cancelState}
          isSavingReason={isSavingReason}
          onRetryCancel={handleRetryCancel}
          onConfirm={(reason) => void handleSaveReason(reason)}
        />
      )}
    </div>
  );
}
