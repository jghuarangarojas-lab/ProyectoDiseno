import { useEffect, useState } from 'react';
import { ModalShell } from '../irrigation/ModalShell';
import type { BotanicalThreshold, IotDevice, TelemetryRecord } from '../../types/iot';
import {
  deviceStatusLabel,
  formatDateTime,
  humidityLevelOf,
} from '../../utils/iot';

interface ThresholdFormState {
  minHumidityPercentage: string;
  maxHumidityPercentage: string;
  minTemperatureCelsius: string;
  maxTemperatureCelsius: string;
  autoIrrigationSeconds: string;
}

const toNumberOrNull = (value: string): number | null => {
  const trimmed = value.trim();
  if (trimmed === '') {
    return null;
  }
  const parsed = Number(trimmed);
  return Number.isNaN(parsed) ? null : parsed;
};

interface DeviceDetailsModalProps {
  device: IotDevice | null;
  reading: TelemetryRecord | null;
  onClose: () => void;
  onSaveThreshold: (
    deviceCode: string,
    payload: {
      minHumidityPercentage: number | null;
      maxHumidityPercentage: number | null;
      minTemperatureCelsius: number | null;
      maxTemperatureCelsius: number | null;
      autoIrrigationSeconds: number | null;
    },
  ) => Promise<BotanicalThreshold | null>;
  isSavingThreshold: boolean;
}

export function DeviceDetailsModal({
  device,
  reading,
  onClose,
  onSaveThreshold,
  isSavingThreshold,
}: DeviceDetailsModalProps) {
  const [form, setForm] = useState<ThresholdFormState>({
    minHumidityPercentage: '',
    maxHumidityPercentage: '',
    minTemperatureCelsius: '',
    maxTemperatureCelsius: '',
    autoIrrigationSeconds: '',
  });

  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    if (!device?.threshold) {
      setForm({
        minHumidityPercentage: '',
        maxHumidityPercentage: '',
        minTemperatureCelsius: '',
        maxTemperatureCelsius: '',
        autoIrrigationSeconds: '',
      });
      setHasChanges(false);
      return;
    }

    const threshold = device.threshold;
    setForm({
      minHumidityPercentage: String(threshold.minHumidityPercentage),
      maxHumidityPercentage: String(threshold.maxHumidityPercentage),
      minTemperatureCelsius: String(threshold.minTemperatureCelsius),
      maxTemperatureCelsius: String(threshold.maxTemperatureCelsius),
      autoIrrigationSeconds: String(threshold.autoIrrigationSeconds),
    });
    setHasChanges(false);
  }, [device]);

  if (!device) {
    return null;
  }

  const handleChange = (field: keyof ThresholdFormState, value: string) => {
    setForm((previous) => ({ ...previous, [field]: value }));
    setHasChanges(true);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const result = await onSaveThreshold(device.deviceCode, {
      minHumidityPercentage: toNumberOrNull(form.minHumidityPercentage),
      maxHumidityPercentage: toNumberOrNull(form.maxHumidityPercentage),
      minTemperatureCelsius: toNumberOrNull(form.minTemperatureCelsius),
      maxTemperatureCelsius: toNumberOrNull(form.maxTemperatureCelsius),
      autoIrrigationSeconds: toNumberOrNull(form.autoIrrigationSeconds),
    });

    if (result) {
      setHasChanges(false);
    }
  };

  const humidityLevel = reading
    ? humidityLevelOf(reading.humidityPercentage, device)
    : 'Sin lectura';

  return (
    <ModalShell
      ariaLabel="Detalles del sensor IoT"
      onClose={onClose}
      maxWidth="max-w-2xl"
    >
      <div className="p-6">
        <header className="mb-4 flex items-start justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-800">
              {device.deviceCode}
            </h2>
            <p className="text-sm text-slate-500">
              Sector {device.currentSectorArea} · MAC {device.macAddress}
            </p>
          </div>
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600 ring-1 ring-inset ring-slate-200">
            {deviceStatusLabel(device.status)}
          </span>
        </header>

        <div className="grid gap-4 md:grid-cols-2">
          <section className="rounded-xl border border-slate-200 p-4">
            <h3 className="text-sm font-bold text-slate-700">Estado actual</h3>
            <dl className="mt-2 space-y-1 text-sm text-slate-600">
              <div className="flex justify-between">
                <dt>Batería</dt>
                <dd className="font-mono">
                  {device.batteryPercentage}% {device.isBatteryLow ? '(Baja)' : ''}
                </dd>
              </div>
              <div className="flex justify-between">
                <dt>Última vez visto</dt>
                <dd>{formatDateTime(device.lastSeenAtUtc)}</dd>
              </div>
              <div className="flex justify-between">
                <dt>Última solicitud de riego</dt>
                <dd>{formatDateTime(device.lastIrrigationRequestedAtUtc)}</dd>
              </div>
            </dl>

            {reading && (
              <div className="mt-3 rounded-lg bg-slate-50 p-3 text-sm text-slate-600">
                <p className="font-semibold text-slate-700">Última telemetría</p>
                <dl className="mt-1 space-y-1">
                  <div className="flex justify-between">
                    <dt>Humedad</dt>
                    <dd>
                      {reading.humidityPercentage.toFixed(1)}% · Nivel{' '}
                      {humidityLevel}
                    </dd>
                  </div>
                  <div className="flex justify-between">
                    <dt>Temperatura</dt>
                    <dd>{reading.temperatureCelsius.toFixed(1)} °C</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt>Registrado</dt>
                    <dd>{formatDateTime(reading.recordedAtUtc)}</dd>
                  </div>
                </dl>
              </div>
            )}
          </section>

          <section className="rounded-xl border border-slate-200 p-4">
            <h3 className="text-sm font-bold text-slate-700">
              Umbrales botánicos
            </h3>
            <form className="mt-3 space-y-3" onSubmit={handleSubmit}>
              <div className="grid grid-cols-2 gap-3">
                <label className="text-xs font-semibold text-slate-600">
                  Humedad mínima (%)
                  <input
                    type="number"
                    className="mt-1 w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
                    value={form.minHumidityPercentage}
                    onChange={(event) =>
                      handleChange('minHumidityPercentage', event.target.value)
                    }
                  />
                </label>
                <label className="text-xs font-semibold text-slate-600">
                  Humedad máxima (%)
                  <input
                    type="number"
                    className="mt-1 w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
                    value={form.maxHumidityPercentage}
                    onChange={(event) =>
                      handleChange('maxHumidityPercentage', event.target.value)
                    }
                  />
                </label>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <label className="text-xs font-semibold text-slate-600">
                  Temperatura mínima (°C)
                  <input
                    type="number"
                    step="0.1"
                    className="mt-1 w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
                    value={form.minTemperatureCelsius}
                    onChange={(event) =>
                      handleChange('minTemperatureCelsius', event.target.value)
                    }
                  />
                </label>
                <label className="text-xs font-semibold text-slate-600">
                  Temperatura máxima (°C)
                  <input
                    type="number"
                    step="0.1"
                    className="mt-1 w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
                    value={form.maxTemperatureCelsius}
                    onChange={(event) =>
                      handleChange('maxTemperatureCelsius', event.target.value)
                    }
                  />
                </label>
              </div>
              <label className="text-xs font-semibold text-slate-600">
                Duración de riego automático (s)
                <input
                  type="number"
                  className="mt-1 w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
                  value={form.autoIrrigationSeconds}
                  onChange={(event) =>
                    handleChange('autoIrrigationSeconds', event.target.value)
                  }
                />
              </label>
              <p className="text-[11px] text-slate-500">
                Al guardar, se aplicará la validación de ms-iot (humedad
                min&lt;max, riego entre 1 y 60 segundos).
              </p>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm font-medium text-slate-600 transition hover:bg-slate-50"
                >
                  Cerrar
                </button>
                <button
                  type="submit"
                  disabled={!hasChanges || isSavingThreshold}
                  className="rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-bold text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {isSavingThreshold ? 'Guardando…' : 'Guardar umbrales'}
                </button>
              </div>
            </form>
          </section>
        </div>
      </div>
    </ModalShell>
  );
}
