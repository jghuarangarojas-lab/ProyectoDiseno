import { useCallback, useMemo, useState } from 'react';
import { DndContext } from '@dnd-kit/core';
import toast from 'react-hot-toast';
import { AlertsSidebar } from '../components/iot/AlertsSidebar';
import { IoTMapDashboard } from '../components/iot/IoTMapDashboard';
import { DeviceDetailsModal } from '../components/iot/DeviceDetailsModal';
import { BatteryEnergyChart, HumidityDistributionChart } from '../components/iot/AnalyticsCharts';
import { useIotDevices } from '../hooks/useIotDevices';
import { useIotAlerts } from '../hooks/useIotAlerts';
import { useIotWebSockets } from '../hooks/useIotWebSockets';
import { useTelemetryAnalytics } from '../hooks/useTelemetryAnalytics';
import { toUserMessage } from '../services/iot.service';
import type { UpdateThresholdRequest } from '../types/iot';

const SOCKET_MODE: 'simulation' | 'signalr' = 'signalr';

export default function IoTMonitoringPage() {
  const [selectedDeviceCode, setSelectedDeviceCode] = useState<string | null>(null);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [isSavingThreshold, setIsSavingThreshold] = useState(false);

  const {
    devices,
    isLoading,
    isRefreshing,
    refresh,
    replaceDevice,
    relocateSensor,
    updateThreshold,
  } = useIotDevices({ pollMs: 15000 });

  const {
    alerts,
    isLoading: isAlertsLoading,
    resolvingId,
    refresh: refreshAlerts,
    resolveAlert,
    upsertAlert,
    removeAlert,
  } = useIotAlerts({ pollMs: 20000 });

  const {
    latestReadings,
    batteryRows,
    areas,
    selectedArea,
    humiditySlices,
    selectArea,
    pushTelemetry,
  } = useTelemetryAnalytics(devices);

  const handleRelocate = useCallback(
    async (deviceCode: string, targetTileId: string): Promise<boolean> => {
      return relocateSensor(deviceCode, targetTileId);
    },
    [relocateSensor],
  );

  const handleSelectTile = useCallback(
    (deviceCode: string | null, _tileId: string) => {
      if (!deviceCode) {
        return;
      }

      setSelectedDeviceCode(deviceCode);
      setIsDetailsOpen(true);
    },
    [],
  );

  const handleCloseDetails = useCallback(() => {
    setIsDetailsOpen(false);
    setSelectedDeviceCode(null);
  }, []);

  const handleSaveThreshold = useCallback(
    async (deviceCode: string, payload: UpdateThresholdRequest) => {
      setIsSavingThreshold(true);
      try {
        const result = await updateThreshold(deviceCode, payload);
        return result;
      } catch (error) {
        toast.error(toUserMessage(error));
        return null;
      } finally {
        setIsSavingThreshold(false);
      }
    },
    [updateThreshold],
  );

  const handleResolveAlert = useCallback(
    async (alert: { id: string; deviceCode: string; issueType: string; message: string; isResolved: boolean; createdAtUtc: string; resolvedAtUtc: string | null; resolutionNote: string | null; }) => {
      await resolveAlert(alert as any);
    },
    [resolveAlert],
  );

  const selectedDevice = useMemo(
    () =>
      selectedDeviceCode
        ? devices.find((device) => device.deviceCode === selectedDeviceCode) ?? null
        : null,
    [devices, selectedDeviceCode],
  );

  const selectedReading = useMemo(
    () =>
      selectedDeviceCode
        ? latestReadings[selectedDeviceCode] ?? null
        : null,
    [latestReadings, selectedDeviceCode],
  );

  const deviceCodesForBattery = useMemo(() => devices.map((d) => d.deviceCode), [devices]);

  const { onDeviceUpdated, onTelemetryReceived, onAlertRaised, onAlertResolved } =
    useIotWebSockets({ mode: SOCKET_MODE, joinDashboard: true });

  useCallback(() => {
    const unsubscribeDevice = onDeviceUpdated((device) => {
      replaceDevice(device);
    });

    const unsubscribeTelemetry = onTelemetryReceived((record) => {
      pushTelemetry(record);
    });

    const unsubscribeAlertRaised = onAlertRaised((alert) => {
      if (!alert.isResolved) {
        upsertAlert(alert);
      } else {
        removeAlert(alert);
      }
    });

    const unsubscribeAlertResolved = onAlertResolved((alert) => {
      removeAlert(alert);
    });

    return () => {
      unsubscribeDevice();
      unsubscribeTelemetry();
      unsubscribeAlertRaised();
      unsubscribeAlertResolved();
    };
  }, [
    onDeviceUpdated,
    onTelemetryReceived,
    onAlertRaised,
    onAlertResolved,
    replaceDevice,
    pushTelemetry,
    upsertAlert,
    removeAlert,
  ])();

  return (
    <DndContext>
      <div className="min-h-screen bg-slate-50">
        <header className="border-b border-slate-200 bg-white">
          <div className="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-3 px-4 py-4">
            <div>
              <h1 className="text-base font-extrabold tracking-tight text-slate-900">
                Jardinería Lurín Smart
              </h1>
              <p className="text-xs font-medium text-slate-500">
                Monitoreo IoT · Mosaicos, alertas y analítica
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <span className="rounded-full bg-slate-100 px-3 py-1.5 text-[11px] font-bold text-slate-500 ring-1 ring-inset ring-slate-200">
                {devices.length} sensores
              </span>
              <span className="rounded-full bg-red-50 px-3 py-1.5 text-[11px] font-bold text-red-700 ring-1 ring-inset ring-red-200">
                {alerts.length} alertas
              </span>
              <button
                type="button"
                onClick={() => {
                  void refresh();
                  void refreshAlerts();
                }}
                disabled={isRefreshing || isLoading}
                className="inline-flex items-center gap-1.5 rounded-full bg-white px-3 py-1.5 text-[11px] font-bold text-slate-600 ring-1 ring-inset ring-slate-200 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isRefreshing || isLoading ? 'Actualizando…' : 'Actualizar'}
              </button>
            </div>
          </div>
        </header>

        <main className="mx-auto flex max-w-7xl gap-6 px-4 py-6">
          <div className="flex-1 space-y-6">
            <IoTMapDashboard
              devices={devices}
              latestReadings={latestReadings}
              onRelocate={handleRelocate}
              onSelectTile={handleSelectTile}
            />

            <div className="grid gap-6 lg:grid-cols-2">
              <BatteryEnergyChart
                data={batteryRows}
                deviceCodes={deviceCodesForBattery}
              />
              <HumidityDistributionChart
                data={humiditySlices}
                areas={areas}
                selectedArea={selectedArea}
                onSelectArea={selectArea}
              />
            </div>
          </div>

          <AlertsSidebar
            alerts={alerts}
            isLoading={isAlertsLoading}
            resolvingId={resolvingId}
            onResolve={handleResolveAlert}
          />
        </main>

        {isDetailsOpen && (
          <DeviceDetailsModal
            device={selectedDevice}
            reading={selectedReading}
            onClose={handleCloseDetails}
            onSaveThreshold={handleSaveThreshold}
            isSavingThreshold={isSavingThreshold}
          />
        )}
      </div>
    </DndContext>
  );
}
