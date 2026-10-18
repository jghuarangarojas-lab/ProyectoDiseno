import { useState } from 'react';
import { AlertTriangleIcon, CloseIcon } from '../icons';
import type { DeviceAlert } from '../../types/iot';
import { alertIssueMeta, formatDateTime } from '../../utils/iot';

interface AlertsSidebarProps {
  alerts: DeviceAlert[];
  isLoading: boolean;
  resolvingId: string | null;
  onResolve: (alert: DeviceAlert) => Promise<void>;
}

export function AlertsSidebar({
  alerts,
  isLoading,
  resolvingId,
  onResolve,
}: AlertsSidebarProps) {
  const [isOpen, setIsOpen] = useState(true);

  return (
    <aside
      className={`relative flex h-[calc(100vh-128px)] w-96 max-w-full flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition ${
        isOpen ? '' : 'translate-x-full'
      }`}
    >
      <header className="flex items-center justify-between border-b border-slate-200 px-4 py-3">
        <div className="flex items-center gap-2">
          <AlertTriangleIcon className="h-5 w-5 text-red-500" />
          <h2 className="text-sm font-bold text-slate-800">Bandeja de alertas</h2>
          <span className="rounded-full bg-red-50 px-2 py-0.5 text-[11px] font-bold text-red-700 ring-1 ring-inset ring-red-200">
            {alerts.length}
          </span>
        </div>
        <button
          type="button"
          className="rounded-full p-1 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          onClick={() => setIsOpen((prev) => !prev)}
          aria-label={isOpen ? 'Ocultar alertas' : 'Mostrar alertas'}
        >
          <CloseIcon className="h-4 w-4" />
        </button>
      </header>

      <div className="flex-1 overflow-y-auto px-4 py-3">
        {isLoading && alerts.length === 0 && (
          <p className="text-sm text-slate-500">Cargando alertas críticas…</p>
        )}

        {alerts.length === 0 && !isLoading && (
          <p className="text-sm text-slate-500">
            No hay alertas críticas pendientes.
          </p>
        )}

        <ul className="space-y-2">
          {alerts.map((alert) => {
            const meta = alertIssueMeta(alert.issueType);

            return (
              <li
                key={alert.id}
                className="rounded-xl border border-slate-200 p-3"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <span
                      className={`inline-flex h-2.5 w-2.5 rounded-full ${meta.dotClassName}`}
                    />
                    <span className="text-xs font-semibold text-slate-700">
                      {alert.deviceCode}
                    </span>
                  </div>
                  <span
                    className={`rounded-full px-2 py-0.5 text-[10px] font-bold ring-1 ring-inset ${meta.badgeClassName}`}
                  >
                    {meta.label}
                  </span>
                </div>
                <p className="mt-2 text-xs text-slate-600">{alert.message}</p>
                <p className="mt-1 text-[11px] text-slate-400">
                  Registrada el {formatDateTime(alert.createdAtUtc)}
                </p>
                <button
                  type="button"
                  onClick={() => onResolve(alert)}
                  disabled={resolvingId === alert.id}
                  className="mt-2 w-full rounded-lg bg-emerald-600 px-2 py-1.5 text-xs font-bold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {resolvingId === alert.id
                    ? 'Marcando…'
                    : 'Marcar como resuelto'}
                </button>
              </li>
            );
          })}
        </ul>
      </div>
    </aside>
  );
}
