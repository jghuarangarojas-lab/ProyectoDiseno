import { useDraggable, useDroppable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import type { IotTile } from '../../data/iotBoard';
import type { IotDevice, TelemetryRecord } from '../../types/iot';
import {
  batteryBarClassName,
  deviceStatusLabel,
  formatDateTime,
  resolvePotState,
} from '../../utils/iot';

interface PotMosaicCardProps {
  tile: IotTile;
  device: IotDevice | null;
  reading: TelemetryRecord | null;
  onSelect: (deviceCode: string | null, tileId: string) => void;
}

export function PotMosaicCard({
  tile,
  device,
  reading,
  onSelect,
}: PotMosaicCardProps) {
  const potState = resolvePotState(device, reading);

  const {
    setNodeRef: setDraggableRef,
    attributes,
    listeners,
    transform,
    isDragging,
  } = useDraggable({
    id: `device:${device?.deviceCode ?? `empty:${tile.id}`}`,
    data: {
      type: 'device',
      deviceCode: device?.deviceCode ?? null,
      fromTileId: tile.id,
    },
    disabled: !device,
  });

  const { isOver, setNodeRef: setDroppableRef } = useDroppable({
    id: `tile:${tile.id}`,
    data: {
      type: 'tile',
      tileId: tile.id,
      deviceCode: device?.deviceCode ?? null,
    },
  });

  const style = {
    transform: CSS.Translate.toString(transform),
    opacity: isDragging ? 0.4 : 1,
    cursor: device ? 'grab' : 'not-allowed',
  };

  return (
    <div
      ref={(element) => {
        setDroppableRef(element);
        setDraggableRef(element);
      }}
      className={`group relative rounded-2xl border-2 p-3 shadow-sm transition ${potState.cardClassName} ${
        isOver ? 'ring-4 ring-indigo-300 ring-offset-2 ring-offset-white' : ''
      }`}
      style={style}
      {...attributes}
      {...listeners}
      onClick={() => onSelect(device?.deviceCode ?? null, tile.id)}
      role="button"
      tabIndex={0}
      onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          onSelect(device?.deviceCode ?? null, tile.id);
        }
      }}
    >
      <div className="flex items-start justify-between">
        <div>
          <p className="font-mono text-xs font-semibold text-slate-600">
            {tile.id}
          </p>
          <p className="text-[11px] text-slate-500">{tile.name}</p>
        </div>
        <span
          className={`rounded-full px-2 py-0.5 text-[11px] font-bold ring-1 ring-inset ${potState.accentText} bg-white/70 ring-white/80`}
        >
          {potState.label}
        </span>
      </div>

      <div className="mt-3 flex items-center justify-between gap-3">
        <svg width="110" height="90" viewBox="0 0 110 90" aria-hidden="true">
          <ellipse cx="55" cy="78" rx="38" ry="6" fill="#e2e8f0" opacity="0.6" />
          <path
            d="M25 54 L34 30 H76 L85 54 V70 H25 Z"
            fill={potState.potFill}
            stroke={potState.potRimFill}
            strokeWidth="2"
            strokeLinejoin="round"
          />
          <rect
            x="24"
            y="54"
            width="62"
            height="6"
            rx="3"
            fill={potState.potRimFill}
            opacity="0.8"
          />
          <ellipse
            cx="55"
            cy="36"
            rx="16"
            ry="8"
            fill={potState.plantFill}
            opacity="0.95"
          />
          <ellipse cx="44" cy="36" rx="9" ry="5" fill={potState.plantFill} />
          <ellipse cx="66" cy="36" rx="9" ry="5" fill={potState.plantFill} />
          <path
            d="M36 62 C46 68 64 68 74 62"
            stroke="#1f2937"
            strokeWidth="2"
            strokeLinecap="round"
            opacity={device && device.status === 'ONLINE' ? 0.2 : 0.12}
          />
        </svg>

        <div className="flex flex-1 flex-col gap-1 text-xs text-slate-600">
          <div className="flex items-center justify-between">
            <span className="font-semibold">Sensor</span>
            <span className="font-mono text-[11px]">
              {device?.deviceCode ?? '—'}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="font-semibold">Estado</span>
            <span className="font-medium">
              {device ? deviceStatusLabel(device.status) : 'Sin asignar'}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="font-semibold">Batería</span>
            <span className="font-mono text-[11px]">
              {device ? `${device.batteryPercentage}%` : '—'}
            </span>
          </div>
          <div className="mt-1 h-1.5 w-full rounded-full bg-white/70">
            <div
              className={`h-1.5 rounded-full ${batteryBarClassName(
                device?.batteryPercentage ?? 0,
                device?.isBatteryLow ?? false,
              )}`}
              style={{ width: `${Math.max(0, Math.min(100, device?.batteryPercentage ?? 0))}%` }}
            />
          </div>
          {reading && (
            <p className="mt-0.5 text-[11px] text-slate-500">
              Humedad {reading.humidityPercentage.toFixed(1)}% ·{' '}
              {formatDateTime(reading.recordedAtUtc)}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
