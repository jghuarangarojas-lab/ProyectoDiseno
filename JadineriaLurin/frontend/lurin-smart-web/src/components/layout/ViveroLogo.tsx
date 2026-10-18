import { useId } from 'react';

interface ViveroLogoProps {
  className?: string;
}

/** Logo del vivero: hoja estilizada dentro de un círculo verde. */
export function ViveroLogo({ className = 'h-12 w-12' }: ViveroLogoProps) {
  const gradientId = useId();

  return (
    <svg viewBox="0 0 48 48" className={className} role="img" aria-label="Jardinería Lurín">
      <defs>
        <linearGradient id={gradientId} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#22c55e" />
          <stop offset="100%" stopColor="#15803d" />
        </linearGradient>
      </defs>
      <circle cx="24" cy="24" r="24" fill={`url(#${gradientId})`} />
      <path
        d="M34 13c-13 0-22 7-22 17 0 3 1 6 3 8-1-10 6-18 19-25Z"
        fill="#ffffff"
        opacity="0.95"
      />
      <path
        d="M14 34c3 1 7 2 10 1 5-1 8-5 9-11"
        fill="none"
        stroke="#15803d"
        strokeWidth="2"
        strokeLinecap="round"
      />
      <path d="M13 37c2-1 5-1 7 0" fill="none" stroke="#ffffff" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}
