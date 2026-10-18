import { useCallback, useEffect, useRef, useState } from 'react';

/** 2 horas de inactividad disparan el aviso de cierre de sesión. */
const IDLE_LIMIT_MS = 2 * 60 * 60 * 1000;

/** El usuario dispone de 60 segundos para confirmar que sigue ahí. */
const CONFIRM_LIMIT_SECONDS = 60;

/** Los movimientos de mouse se agrupan para no reiniciar el temporizador 60 veces por segundo. */
const ACTIVITY_THROTTLE_MS = 1000;

const ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'touchstart'] as const;

interface UseInactivityLogoutOptions {
  isActive: boolean;
  onForceLogout: () => void;
}

interface UseInactivityLogoutResult {
  isWarningOpen: boolean;
  remainingSeconds: number;
  confirmPresence: () => void;
}

/**
 * Control de inactividad:
 * 1. Rastrea mouse, teclado y toques en pantalla.
 * 2. Tras 2 horas sin actividad muestra el modal de advertencia.
 * 3. Si el usuario no confirma en 60 segundos, ejecuta el logout forzado.
 *
 * Mientras el modal está abierto, la actividad NO lo disipa: solo lo hace
 * el botón "Sí, sigo aquí" (según el criterio del negocio).
 */
export function useInactivityLogout({
  isActive,
  onForceLogout,
}: UseInactivityLogoutOptions): UseInactivityLogoutResult {
  const [isWarningOpen, setIsWarningOpen] = useState(false);
  const [remainingSeconds, setRemainingSeconds] = useState(CONFIRM_LIMIT_SECONDS);

  const warningOpenRef = useRef(isWarningOpen);
  const onForceLogoutRef = useRef(onForceLogout);
  const armIdleTimerRef = useRef<() => void>(() => {});

  useEffect(() => {
    warningOpenRef.current = isWarningOpen;
  }, [isWarningOpen]);

  useEffect(() => {
    onForceLogoutRef.current = onForceLogout;
  }, [onForceLogout]);

  // Temporizador principal de inactividad (2 horas).
  useEffect(() => {
    if (!isActive) {
      setIsWarningOpen(false);
      return;
    }

    let idleTimer: number | undefined;
    let lastActivityAt = 0;

    const armIdleTimer = () => {
      window.clearTimeout(idleTimer);
      idleTimer = window.setTimeout(() => setIsWarningOpen(true), IDLE_LIMIT_MS);
    };

    armIdleTimerRef.current = armIdleTimer;

    const handleActivity = () => {
      if (warningOpenRef.current) return;

      const now = Date.now();
      if (now - lastActivityAt < ACTIVITY_THROTTLE_MS) return;

      lastActivityAt = now;
      armIdleTimer();
    };

    lastActivityAt = Date.now();
    ACTIVITY_EVENTS.forEach((eventName) =>
      window.addEventListener(eventName, handleActivity, { passive: true }),
    );

    return () => {
      window.clearTimeout(idleTimer);
      ACTIVITY_EVENTS.forEach((eventName) =>
        window.removeEventListener(eventName, handleActivity),
      );
    };
  }, [isActive]);

  // Cuenta regresiva de 60 segundos mientras el modal está abierto.
  useEffect(() => {
    if (!isWarningOpen) return;

    setRemainingSeconds(CONFIRM_LIMIT_SECONDS);
    const intervalId = window.setInterval(() => {
      setRemainingSeconds((previous) => Math.max(0, previous - 1));
    }, 1000);

    return () => window.clearInterval(intervalId);
  }, [isWarningOpen]);

  // Al llegar a 0 segundos: logout forzado.
  useEffect(() => {
    if (isWarningOpen && remainingSeconds <= 0) {
      onForceLogoutRef.current();
    }
  }, [isWarningOpen, remainingSeconds]);

  const confirmPresence = useCallback(() => {
    setIsWarningOpen(false);
    armIdleTimerRef.current();
  }, []);

  return {
    isWarningOpen,
    remainingSeconds,
    confirmPresence,
  };
}
