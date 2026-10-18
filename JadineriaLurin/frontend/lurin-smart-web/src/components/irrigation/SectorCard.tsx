import type { IrrigationAction, Sector } from '../../types/irrigation';
import { isAttentionStatus, statusKeyOf, STATUS_META } from '../../utils/status';
import { AlertTriangleIcon } from '../icons';
import { StatusBadge } from './StatusBadge';

interface SectorCardProps {
  sector: Sector;
  action?: IrrigationAction | null;
  onSelect: (sector: Sector) => void;
}

export function SectorCard({ sector, action, onSelect }: SectorCardProps) {
  const status = action?.status;
  const attention = isAttentionStatus(status);
  const inProgress = status === 'IN_PROGRESS';
  const meta = STATUS_META[statusKeyOf(status)];

  const frameClasses = attention
    ? 'border-red-500 bg-red-50 shadow-sm shadow-red-200/70 hover:shadow-md hover:shadow-red-300/70'
    : inProgress
      ? 'border-blue-400 bg-blue-50/60 shadow-sm shadow-blue-200/60 hover:shadow-md'
      : 'border-slate-200 bg-white hover:-translate-y-0.5 hover:border-emerald-400 hover:shadow-md';

  return (
    <button
      type="button"
      onClick={() => onSelect(sector)}
      className={`relative flex w-full flex-col gap-1.5 rounded-xl border-2 p-3 text-left transition duration-150 focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 ${frameClasses}`}
    >
      {attention && (
        <span className="absolute -right-2 -top-2 flex h-6 w-6 items-center justify-center rounded-full bg-red-600 text-white shadow-lg ring-2 ring-white">
          <AlertTriangleIcon className="h-3.5 w-3.5" />
        </span>
      )}

      <div className="flex w-full items-center justify-between gap-2">
        <span className="font-mono text-sm font-bold tracking-tight text-slate-800">
          {sector.id}
        </span>
        <span className={`h-2.5 w-2.5 shrink-0 rounded-full ${meta.dot}`} />
      </div>

      <span className="w-full truncate text-[11px] text-slate-500">
        {sector.name}
      </span>

      <StatusBadge status={status} className="mt-0.5 self-start" />

      {attention && (
        <span className="flex items-center gap-1 text-[11px] font-bold text-red-600">
          <AlertTriangleIcon className="h-3.5 w-3.5" />
          Atención requerida
        </span>
      )}
    </button>
  );
}
