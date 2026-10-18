import { useEffect } from 'react';

export const useQrScanner = (onScan: (code: string) => void) => {
  useEffect(() => {
    // Hook preparado para futura lectura de códigos QR
    // Integración: ZXing, HTML5 QR, o cámara web
  }, [onScan]);
};
