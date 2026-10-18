import { useCallback, useEffect, useRef, useState } from 'react';
import { IOT_HUB_URL } from '../config/api';
import type { DeviceAlert, IotDevice, TelemetryRecord } from '../types/iot';

export type IotSocketMode = 'simulation' | 'signalr';

export type IotSocketStatus =
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'disconnected';

export type IotEventHandler<T> = (payload: T) => void;

interface UseIotWebSocketsOptions {
  mode?: IotSocketMode;
  url?: string;
  deviceCode?: string;
  joinDashboard?: boolean;
}

export interface UseIotWebSocketsResult {
  mode: IotSocketMode;
  status: IotSocketStatus;
  lastEventAt: string | null;
  onDeviceUpdated: (handler: IotEventHandler<IotDevice>) => () => void;
  onTelemetryReceived: (handler: IotEventHandler<TelemetryRecord>) => () => void;
  onAlertRaised: (handler: IotEventHandler<DeviceAlert>) => () => void;
  onAlertResolved: (handler: IotEventHandler<DeviceAlert>) => () => void;
}

export function useIotWebSockets(
  options: UseIotWebSocketsOptions = {},
): UseIotWebSocketsResult {
  const {
    mode = 'signalr',
    url = IOT_HUB_URL,
    deviceCode,
    joinDashboard = true,
  } = options;

  const [status, setStatus] = useState<IotSocketStatus>(
    mode === 'simulation' ? 'connected' : 'connecting',
  );
  const [lastEventAt, setLastEventAt] = useState<string | null>(null);

  const deviceHandlersRef = useRef<Set<IotEventHandler<IotDevice>>>(new Set());
  const telemetryHandlersRef = useRef<Set<IotEventHandler<TelemetryRecord>>>(
    new Set(),
  );
  const alertRaisedHandlersRef = useRef<Set<IotEventHandler<DeviceAlert>>>(
    new Set(),
  );
  const alertResolvedHandlersRef = useRef<Set<IotEventHandler<DeviceAlert>>>(
    new Set(),
  );

  const subscribe = useCallback(<T,>(
    bucket: { current: Set<IotEventHandler<T>> },
    handler: IotEventHandler<T>,
  ) => {
    bucket.current.add(handler);

    return () => {
      bucket.current.delete(handler);
    };
  }, []);

  const onDeviceUpdated = useCallback(
    (handler: IotEventHandler<IotDevice>) =>
      subscribe(deviceHandlersRef, handler),
    [subscribe],
  );

  const onTelemetryReceived = useCallback(
    (handler: IotEventHandler<TelemetryRecord>) =>
      subscribe(telemetryHandlersRef, handler),
    [subscribe],
  );

  const onAlertRaised = useCallback(
    (handler: IotEventHandler<DeviceAlert>) =>
      subscribe(alertRaisedHandlersRef, handler),
    [subscribe],
  );

  const onAlertResolved = useCallback(
    (handler: IotEventHandler<DeviceAlert>) =>
      subscribe(alertResolvedHandlersRef, handler),
    [subscribe],
  );

  useEffect(() => {
    if (mode !== 'signalr') {
      setStatus('connected');
      return;
    }

    let disposed = false;
    let connection: { stop: () => Promise<void> } | undefined;
    let rejoinGroups: (() => Promise<void>) | undefined;

    setStatus('connecting');

    const connect = async () => {
      const signalr = await import('@microsoft/signalr');

      const hubConnection = new signalr.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .configureLogging(signalr.LogLevel.Warning)
        .build();

      connection = hubConnection;

      const dispatch = <T>(bucket: Set<IotEventHandler<T>>, payload: T) => {
        if (disposed) {
          return;
        }

        setLastEventAt(new Date().toISOString());
        bucket.forEach((handler) => handler(payload));
      };

      hubConnection.on('deviceUpdated', (device: IotDevice) =>
        dispatch(deviceHandlersRef.current, device),
      );

      hubConnection.on('telemetryReceived', (record: TelemetryRecord) =>
        dispatch(telemetryHandlersRef.current, record),
      );

      hubConnection.on('alertRaised', (alert: DeviceAlert) =>
        dispatch(alertRaisedHandlersRef.current, alert),
      );

      hubConnection.on('alertResolved', (alert: DeviceAlert) =>
        dispatch(alertResolvedHandlersRef.current, alert),
      );

      hubConnection.onreconnecting(() => {
        if (!disposed) {
          setStatus('reconnecting');
        }
      });

      rejoinGroups = async () => {
        if (joinDashboard) {
          await hubConnection.invoke('JoinDashboard');
        }
        if (deviceCode) {
          await hubConnection.invoke('JoinDevice', deviceCode);
        }
      };

      hubConnection.onreconnected(async () => {
        if (disposed) {
          return;
        }
        setStatus('connected');
        try {
          await rejoinGroups?.();
        } catch {
          setStatus('disconnected');
        }
      });

      hubConnection.onclose(() => {
        if (!disposed) {
          setStatus('disconnected');
        }
      });

      await hubConnection.start();

      if (disposed) {
        await hubConnection.stop();
        return;
      }

      await rejoinGroups?.();
      setStatus('connected');
    };

    connect().catch(() => {
      if (!disposed) {
        setStatus('disconnected');
      }
    });

    return () => {
      disposed = true;
      void connection?.stop();
    };
  }, [mode, url, deviceCode, joinDashboard]);

  return {
    mode,
    status,
    lastEventAt,
    onDeviceUpdated,
    onTelemetryReceived,
    onAlertRaised,
    onAlertResolved,
  };
}
