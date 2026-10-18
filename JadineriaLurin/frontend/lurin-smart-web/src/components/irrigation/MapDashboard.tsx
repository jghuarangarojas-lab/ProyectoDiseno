import { useMemo } from 'react';
import { groupSectors } from '../../data/sectors';
import type { IrrigationAction, Sector } from '../../types/irrigation';
import { SectorCard } from './SectorCard';

interface MapDashboardProps {
  sectors: Sector[];
  statuses: Record<string, IrrigationAction>;
  onSelect: (sector: Sector) => void;
}

const LEGEND = [
  { label: 'Sin actividad', dot: 'bg-slate-300' },
  { label: 'Pendiente', dot: 'bg-amber-500' },
  { label: 'Riego activo', dot: 'bg-blue-500' },
  { label: 'Completado', dot: 'bg-emerald-500' },
  { label: 'Fallido / Cancelado', dot: 'bg-red-500' },
];

export function MapDashboard({ sectors, statuses, onSelect }: MapDashboardProps) {
  const groups = useMemo(() => groupSectors(sectors), [sectors]);

  if (sectors.length === 0) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-10 text-center text-sm text-slate-500">
        No hay sectores configurados en el tablero.
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2 rounded-xl border border-slate-200 bg-white px-4 py-3">
        <span className="text-[11px] font-bold uppercase tracking-wider text-slate-400">
          Leyenda
        </span>
        {LEGEND.map((item) => (
          <span
            key={item.label}
            className="inline-flex items-center gap-1.5 text-xs font-medium text-slate-600"
          >
            <span className={`h-2.5 w-2.5 rounded-full ${item.dot}`} />
            {item.label}
          </span>
        ))}
      </div>

      <div className="space-y-5">
        {groups.map((group) => (
          <section
            key={group.group}
            className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
          >
            <header className="mb-4 flex flex-wrap items-center justify-between gap-2">
              <div>
                <h2 className="text-base font-bold text-slate-800">
                  Grupo {group.group}
                </h2>
                <p className="text-xs text-slate-500">
                  {group.sectors[0]?.name.split(' · ')[0]} · {group.sectors.length}{' '}
                  sectores
                </p>
              </div>
              <span className="rounded-full bg-slate-100 px-3 py-1 font-mono text-xs font-semibold text-slate-600 ring-1 ring-inset ring-slate-200">
                {group.deviceId}
              </span>
            </header>

            <div
              className="grid gap-3"
              style={{ gridTemplateColumns: `repeat(${group.maxColumn}, minmax(0, 1fr))` }}
            >
              {group.sectors.map((item) => (
                <div
                  key={item.id}
                  style={{
                    gridColumn: item.column,
                    gridRow: item.row - group.minRow + 1,
                  }}
                >
                  <SectorCard
                    sector={item}
                    action={statuses[item.id]}
                    onSelect={onSelect}
                  />
                </div>
              ))}
            </div>
          </section>
        ))}
      </div>
    </div>
  );
}
