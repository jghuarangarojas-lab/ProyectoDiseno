interface PlantIrrigationAnimationProps {
  className?: string;
}

const dropDelays = ['0s', '0.4s', '0.75s'];
const dropX = [90, 120, 150];

export function PlantIrrigationAnimation({
  className = '',
}: PlantIrrigationAnimationProps) {
  return (
    <svg
      viewBox="0 0 240 160"
      role="img"
      aria-label="Planta siendo regada"
      className={className}
    >
      <circle cx="212" cy="26" r="13" fill="#fde68a" />
      <circle cx="212" cy="26" r="8" fill="#fbbf24" />

      <rect x="36" y="14" width="8" height="42" rx="3" fill="#64748b" />
      <rect x="40" y="14" width="140" height="8" rx="4" fill="#94a3b8" />

      {dropX.map((x, index) => (
        <g key={x} transform={`translate(${x} 24)`}>
          <rect x="-3" y="0" width="6" height="6" rx="2" fill="#64748b" />
          <path
            className="irrigation-drop"
            style={{ animationDelay: dropDelays[index] }}
            d="M0 8 C 3.4 12.4, 5.6 14.8, 5.6 17.4 A 5.6 5.6 0 0 1 -5.6 17.4 C -5.6 14.8, -3.4 12.4, 0 8 Z"
            fill="#38bdf8"
          />
        </g>
      ))}

      <ellipse cx="120" cy="157" rx="48" ry="4" fill="#e2e8f0" />

      <path d="M85 132 L155 132 L147 156 L93 156 Z" fill="#b45309" />
      <rect x="78" y="124" width="84" height="11" rx="4" fill="#92400e" />
      <ellipse cx="120" cy="127.5" rx="37" ry="4.5" fill="#451a03" />

      <g className="irrigation-plant-sway">
        <path
          d="M120 127 C 117 108, 123 96, 120 78"
          stroke="#16a34a"
          strokeWidth="4.5"
          strokeLinecap="round"
          fill="none"
        />
        <ellipse
          cx="103"
          cy="99"
          rx="14"
          ry="6.5"
          fill="#22c55e"
          transform="rotate(-32 103 99)"
        />
        <ellipse
          cx="137"
          cy="92"
          rx="14"
          ry="6.5"
          fill="#16a34a"
          transform="rotate(32 137 92)"
        />
        <ellipse
          cx="105"
          cy="113"
          rx="12"
          ry="5.5"
          fill="#15803d"
          transform="rotate(-26 105 113)"
        />
        <ellipse
          cx="135"
          cy="108"
          rx="12"
          ry="5.5"
          fill="#22c55e"
          transform="rotate(26 135 108)"
        />
        <circle cx="120" cy="73" r="8" fill="#fb7185" />
        <circle cx="120" cy="73" r="3.5" fill="#fde047" />
      </g>

      <circle
        className="irrigation-ripple"
        cx="106"
        cy="128"
        r="5"
        fill="none"
        stroke="#38bdf8"
        strokeWidth="1.6"
      />
      <circle
        className="irrigation-ripple"
        style={{ animationDelay: '0.8s' }}
        cx="134"
        cy="129"
        r="5"
        fill="none"
        stroke="#38bdf8"
        strokeWidth="1.6"
      />
    </svg>
  );
}
