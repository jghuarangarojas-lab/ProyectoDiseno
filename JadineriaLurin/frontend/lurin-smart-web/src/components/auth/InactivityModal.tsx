import React from 'react';
import { Button } from '../common/Button';
import { ClockIcon } from '../icons';
import { ViveroLogo } from '../layout/ViveroLogo';

interface InactivityModalProps {
  isOpen: boolean;
  remainingSeconds: number;
  onConfirm: () => void;
}

/**
 * Modal no disipable por backdrop ni por teclado: la única salida es
 * pulsar "Sí, sigo aquí" antes de que se agote la cuenta regresiva.
 */
export const InactivityModal: React.FC<InactivityModalProps> = ({
  isOpen,
  remainingSeconds,
  onConfirm,
}) => {
  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-sm"
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="inactivity-title"
    >
      <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 text-center">
        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-amber-100">
          <ClockIcon className="h-7 w-7 text-amber-600" />
        </div>

        <h2 id="inactivity-title" className="text-lg font-bold text-slate-900">
          ¿Sigue ahí? La sesión se cerrará por inactividad
        </h2>

        <p className="text-sm text-slate-500 mt-2">
          Detectamos 2 horas sin actividad. Si no confirmas, tu sesión y tu JWT se eliminarán.
        </p>

        <div className="mt-4 mb-5 inline-flex items-center gap-2 rounded-lg bg-slate-100 px-3 py-2 text-sm text-slate-700">
          <ViveroLogo className="h-4 w-4" />
          <span>
            Se cerrará en <b className="tabular-nums">{remainingSeconds}s</b>
          </span>
        </div>

        <Button size="lg" className="w-full" onClick={onConfirm}>
          Sí, sigo aquí
        </Button>

        <p className="mt-3 text-xs text-slate-400">
          Cualquier otra acción no cuenta como confirmación.
        </p>
      </div>
    </div>
  );
};
