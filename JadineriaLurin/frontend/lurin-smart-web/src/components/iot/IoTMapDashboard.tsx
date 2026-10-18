import { DndContext, DragEndEvent, DragOverlay, closestCenter } from '@dnd-kit/core';
import { useCallback, useState } from 'react';
import { groupTiles, IOT_TILES } from '../../data/iotBoard';
import type { IotDevice, TelemetryRecord } from '../../types/iot';
import { PotMosaicCard } from './PotMosaicCard';

const LEGEND = [
  { label: 'Desconectado', dot: 'bg-slate-500' },
  { label: 'Humedad óptima', dot: 'bg-sky-500' },
  { label: 'Peligro por umbral', dot: 'bg-yellow-500' },
  { label: 'Seco crítico', dot: 'bg-red-500' },
  { label: 'Sin sensor', dot: 'bg-slate-300' },
];

interface IoTMapDashboardProps {
  devices: IotDevice[];
  latestReadings: Record<string, TelemetryRecord>;
  onRelocate: (deviceCode: string, targetTileId: string) => Promise<boolean>;
  onSelectTile: (deviceCode: string | null, tileId: string) => void;
}

export function IoTMapDashboard({
  devices,
  latestReadings,
  onRelocate,
  onSelectTile,
}: IoTMapDashboardProps) {
  const [activeDeviceCode, setActiveDeviceCode] = useState<string | null>(null);

  // const deviceByCode = new Map<string, IotDevice>();
  const deviceByArea = new Map(devices.map((device) => [device.currentSectorArea, device]));

  const handleDragEnd = useCallback(
    async (event: DragEndEvent) => {
      const { active, over } = event;

      if (!over) {
        setActiveDeviceCode(null);
        return;
      }

      const activeData = active.data.current as
        | { type: 'device'; deviceCode: string | null }
        | undefined;
      const overData = over.data.current as
        | { type: 'tile'; tileId: string; deviceCode: string | null }
        | undefined;

      setActiveDeviceCode(null);

      if (!activeData?.deviceCode || !overData) {
        return;
      }

      if (activeData.deviceCode === overData.deviceCode) {
        return;
      }

      await onRelocate(activeData.deviceCode, overData.tileId);
    },
    [onRelocate],
  );

  const handleDragStart = useCallback((event: { active: { data: { current?: any } } }) => {
    const activeData = event.active.data.current as
      | { type: 'device'; deviceCode: string | null }
      | undefined;

    setActiveDeviceCode(activeData?.deviceCode ?? null);
  }, []);

  const groups = groupTiles(IOT_TILES);

  return (
    <DndContext
      collisionDetection={closestCenter}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
    >
      <div className="space-y-5">
        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 rounded-xl border border-slate-200 bg-white px-4 py-3">
          <span className="text-[11px] font-bold uppercase tracking-wider text-slate-400">
            Leyenda de macetas
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
                    {group.tiles[0]?.name.split(' · ')[0]} · {group.tiles.length}{' '}
                    mosaicos
                  </p>
                </div>
              </header>

              <div
                className="grid gap-3"
                style={{
                  gridTemplateColumns: `repeat(${group.maxColumn}, minmax(0, 1fr))`,
                }}
              >
                {group.tiles.map((tile) => {
                  const device = deviceByArea.get(tile.id) ?? null;
                  const reading = device
                    ? latestReadings[device.deviceCode] ?? null
                    : null;

                  return (
                    <div
                      key={tile.id}
                      style={{
                        gridColumn: tile.column,
                        gridRow: tile.row - group.minRow + 1,
                      }}
                    >
                      <PotMosaicCard
                        tile={tile}
                        device={device}
                        reading={reading}
                        onSelect={onSelectTile}
                      />
                    </div>
                  );
                })}
              </div>
            </section>
          ))}
        </div>
      </div>

      <DragOverlay>
        {activeDeviceCode ? (
          <div className="rounded-2xl border-2 border-indigo-400 bg-indigo-50 px-4 py-3 shadow-lg">
            <p className="font-mono text-sm font-bold text-indigo-800">
              {activeDeviceCode}
            </p>
            <p className="text-xs text-indigo-700">Soltar en un mosaico destino</p>
          </div>
        ) : null}
      </DragOverlay>
    </DndContext>
  );
}
