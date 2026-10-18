import type { IrrigationStatus } from '../../types/irrigation';
import { STATUS_META, statusKeyOf } from '../../utils/status';

interface StatusBadgeProps {
  status?: IrrigationStatus | null;
  className?: string;
}

export function StatusBadge({ status, className = '' }: StatusBadgeProps) {
  const meta = STATUS_META[statusKeyOf(status)];

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1 ring-inset ${meta.badge} ${className}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${meta.dot}`} />
      {meta.label}
    </span>
  );
}
