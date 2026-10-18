import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import type {
  BatteryChartRow,
  HumidityLevelSlice,
} from '../../types/iot';
import { ALL_AREAS } from '../../hooks/useTelemetryAnalytics';

interface BatteryEnergyChartProps {
  data: BatteryChartRow[];
  deviceCodes: string[];
}

const BATTERY_COLORS = [
  '#2563eb',
  '#16a34a',
  '#f59e0b',
  '#ef4444',
  '#a855f7',
  '#0891b2',
  '#f97316',
  '#4b5563',
];

export function BatteryEnergyChart({ data, deviceCodes }: BatteryEnergyChartProps) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
      <header className="mb-4">
        <h2 className="text-base font-bold text-slate-800">
          Consumo de batería / energía en el tiempo
        </h2>
        <p className="text-xs text-slate-500">
          Evolución de la batería (%) para cada sensor activo
        </p>
      </header>
      <div className="h-72 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={data} margin={{ left: 8, right: 16, top: 8, bottom: 8 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
            <XAxis dataKey="time" tick={{ fontSize: 12, fill: '#475569' }} />
            <YAxis
              domain={[0, 100]}
              tick={{ fontSize: 12, fill: '#475569' }}
              label={{
                value: '% Batería',
                angle: -90,
                position: 'insideLeft',
                style: { textAnchor: 'middle', fill: '#475569', fontSize: 12 },
              }}
            />
            <Tooltip
              contentStyle={{ borderRadius: '12px', border: '1px solid #e2e8f0' }}
              formatter={(value: any, name: any) => [`${value}%`, String(name ?? '')]}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            {deviceCodes.map((code, index) => (
              <Line
                key={code}
                type="monotone"
                dataKey={code}
                name={code}
                stroke={BATTERY_COLORS[index % BATTERY_COLORS.length]}
                strokeWidth={2}
                dot={false}
                connectNulls
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </section>
  );
}

interface HumidityDistributionChartProps {
  data: HumidityLevelSlice[];
  areas: string[];
  selectedArea: string;
  onSelectArea: (area: string) => void;
}

export function HumidityDistributionChart({
  data,
  areas,
  selectedArea,
  onSelectArea,
}: HumidityDistributionChartProps) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
      <header className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <div>
          <h2 className="text-base font-bold text-slate-800">
            Distribución de niveles de humedad por área
          </h2>
          <p className="text-xs text-slate-500">
            Basado en la última telemetría registrada
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => onSelectArea(ALL_AREAS)}
            className={`rounded-full px-3 py-1 text-[11px] font-bold ring-1 ring-inset transition ${
              selectedArea === ALL_AREAS
                ? 'bg-indigo-50 text-indigo-700 ring-indigo-200'
                : 'bg-white text-slate-600 ring-slate-200 hover:bg-slate-50'
            }`}
          >
            Todas las áreas
          </button>
          {areas.map((area) => (
            <button
              key={area}
              type="button"
              onClick={() => onSelectArea(area)}
              className={`rounded-full px-3 py-1 text-[11px] font-bold ring-1 ring-inset transition ${
                selectedArea === area
                  ? 'bg-indigo-50 text-indigo-700 ring-indigo-200'
                  : 'bg-white text-slate-600 ring-slate-200 hover:bg-slate-50'
              }`}
            >
              {area}
            </button>
          ))}
        </div>
      </header>
      <div className="h-72 w-full">
        {data.length === 0 ? (
          <div className="flex h-full items-center justify-center text-sm text-slate-500">
            No hay telemetría disponible para el filtro seleccionado.
          </div>
        ) : (
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={data}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={90}
                label={({ name, percent }) =>
                  `${name}: ${((percent ?? 0) * 100).toFixed(0)}%`
                }
                labelLine={false}
              >
                {data.map((entry) => (
                  <Cell key={entry.name} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        )}
      </div>
    </section>
  );
}
