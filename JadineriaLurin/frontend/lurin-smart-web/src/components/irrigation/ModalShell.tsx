import { useEffect, type ReactNode } from 'react';
import { CloseIcon } from '../icons';

interface ModalShellProps {
  children: ReactNode;
  ariaLabel: string;
  onClose?: () => void;
  maxWidth?: string;
  zIndexClass?: string;
  overlayClassName?: string;
}

export function ModalShell({
  children,
  ariaLabel,
  onClose,
  maxWidth = 'max-w-xl',
  zIndexClass = 'z-50',
  overlayClassName = 'bg-slate-950/60 backdrop-blur-sm',
}: ModalShellProps) {
  useEffect(() => {
    if (!onClose) {
      return;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  useEffect(() => {
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';

    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, []);

  return (
    <div
      className={`fixed inset-0 ${zIndexClass} flex items-center justify-center overflow-y-auto p-4 ${overlayClassName}`}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && onClose) {
          onClose();
        }
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label={ariaLabel}
        className={`relative max-h-[92vh] w-full ${maxWidth} overflow-y-auto rounded-2xl bg-white shadow-2xl ring-1 ring-slate-900/5`}
      >
        {onClose && (
          <button
            type="button"
            onClick={onClose}
            aria-label="Cerrar"
            className="absolute right-3 top-3 z-10 rounded-full p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          >
            <CloseIcon className="h-5 w-5" />
          </button>
        )}
        {children}
      </div>
    </div>
  );
}
