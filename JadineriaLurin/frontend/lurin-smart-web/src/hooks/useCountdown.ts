import { useCallback, useEffect, useRef, useState } from 'react';

interface UseCountdownOptions {
  onComplete?: () => void;
}

export interface UseCountdownResult {
  remainingSeconds: number;
  isRunning: boolean;
  start: () => void;
  stop: () => void;
  reset: (seconds: number) => void;
  sync: (seconds: number) => void;
}

export function useCountdown(
  initialSeconds: number,
  options: UseCountdownOptions = {},
): UseCountdownResult {
  const [remainingSeconds, setRemainingSeconds] = useState(
    Math.max(0, initialSeconds),
  );
  const [isRunning, setIsRunning] = useState(false);
  const completedRef = useRef(false);
  const onCompleteRef = useRef(options.onComplete);

  onCompleteRef.current = options.onComplete;

  useEffect(() => {
    if (!isRunning) {
      return;
    }

    const timerId = window.setInterval(() => {
      setRemainingSeconds((previous) => Math.max(0, previous - 1));
    }, 1000);

    return () => window.clearInterval(timerId);
  }, [isRunning]);

  useEffect(() => {
    if (remainingSeconds > 0 || !isRunning || completedRef.current) {
      return;
    }

    completedRef.current = true;
    setIsRunning(false);
    onCompleteRef.current?.();
  }, [remainingSeconds, isRunning]);

  const start = useCallback(() => {
    completedRef.current = false;
    setIsRunning(true);
  }, []);

  const stop = useCallback(() => {
    setIsRunning(false);
  }, []);

  const reset = useCallback((seconds: number) => {
    completedRef.current = false;
    setRemainingSeconds(Math.max(0, seconds));
  }, []);

  const sync = useCallback((seconds: number) => {
    const next = Math.max(0, seconds);
    if (next > 0) {
      completedRef.current = false;
    }
    setRemainingSeconds(next);
  }, []);

  return {
    remainingSeconds,
    isRunning,
    start,
    stop,
    reset,
    sync,
  };
}
