import type { IrrigationStatus } from '../types/irrigation';

export type StatusKey = IrrigationStatus | 'NONE';

export const STATUS_META: Record<
  StatusKey,
  { label: string; badge: string; dot: string }
> = {
  PENDING: {
    label: 'Pendiente',
    badge: 'bg-amber-50 text-amber-700 ring-amber-200',
    dot: 'bg-amber-500',
  },
  IN_PROGRESS: {
    label: 'Riego activo',
    badge: 'bg-blue-50 text-blue-700 ring-blue-200',
    dot: 'bg-blue-500',
  },
  COMPLETED: {
    label: 'Completado',
    badge: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
    dot: 'bg-emerald-500',
  },
  FAILED: {
    label: 'Fallido',
    badge: 'bg-red-50 text-red-700 ring-red-200',
    dot: 'bg-red-500',
  },
  CANCELLED: {
    label: 'Cancelado',
    badge: 'bg-red-50 text-red-700 ring-red-200',
    dot: 'bg-red-500',
  },
  NONE: {
    label: 'Sin actividad',
    badge: 'bg-slate-50 text-slate-500 ring-slate-200',
    dot: 'bg-slate-300',
  },
};

export const isAttentionStatus = (status?: IrrigationStatus | null): boolean =>
  status === 'FAILED' || status === 'CANCELLED';

export const statusKeyOf = (status?: IrrigationStatus | null): StatusKey =>
  status ?? 'NONE';
