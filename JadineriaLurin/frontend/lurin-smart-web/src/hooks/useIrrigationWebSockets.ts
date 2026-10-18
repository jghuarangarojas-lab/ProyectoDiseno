import { useCallback, useEffect, useRef, useState } from 'react';
import { HUB_URL } from '../config/api';
import type { IrrigationAction } from '../types/irrigation';

export type IrrigationSocketMode = 'simulation' | 'signalr';

export type IrrigationSocketStatus =
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'disconnected';

export type IrrigationUpdatedHandler = (action: IrrigationAction) => void;

interface UseIrrigationWebSocketsOptions {
  mode?: IrrigationSocketMode;
  url?: string;
  deviceId?: string;
  joinDashboard?: boolean;
}

export interface UseIrrigationWebSocketsResult {
  mode: IrrigationSocketMode;
  status: IrrigationSocketStatus;
  lastEventAt: string | null;
  onIrrigationUpdated: (handler: IrrigationUpdatedHandler) => () => void;
  emitSimulatedEvent: (action: IrrigationAction) => void;
}

export function useIrrigationWebSockets(
  options: UseIrrigationWebSocketsOptions = {},
): UseIrrigationWebSocketsResult {
  const {
    mode = 'simulation',
    url = HUB_URL,
    deviceId,
    joinDashboard = true,
  } = options;

  const [status, setStatus] = useState<IrrigationSocketStatus>(
    mode === 'simulation' ? 'connected' : 'connecting',
  );
  const [lastEventAt, setLastEventAt] = useState<string | null>(null);
  const handlersRef = useRef<Set<IrrigationUpdatedHandler>>(new Set());

  const emitSimulatedEvent = useCallback((action: IrrigationAction) => {
    setLastEventAt(new Date().toISOString());
    handlersRef.current.forEach((handler) => handler(action));
  }, []);

  const onIrrigationUpdated = useCallback((handler: IrrigationUpdatedHandler) => {
    handlersRef.current.add(handler);

    return () => {
      handlersRef.current.delete(handler);
    };
  }, []);

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

      hubConnection.on('irrigationUpdated', (action: IrrigationAction) => {
        if (!disposed) {
          setLastEventAt(new Date().toISOString());
          handlersRef.current.forEach((handler) => handler(action));
        }
      });

      hubConnection.onreconnecting(() => {
        if (!disposed) {
          setStatus('reconnecting');
        }
      });

      rejoinGroups = async () => {
        if (joinDashboard) {
          await hubConnection.invoke('JoinDashboard');
        }
        if (deviceId) {
          await hubConnection.invoke('JoinDevice', deviceId);
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
  }, [mode, url, deviceId, joinDashboard]);

  return {
    mode,
    status,
    lastEventAt,
    onIrrigationUpdated,
    emitSimulatedEvent,
  };
}
