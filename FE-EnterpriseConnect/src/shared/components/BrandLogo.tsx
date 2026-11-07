import React from 'react';

interface BrandLogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  className?: string;
  showText?: boolean;
}

export const BrandLogo: React.FC<BrandLogoProps> = ({
  size = 'md',
  className = '',
  showText = true,
}) => {
  const iconSizeMap = {
    sm: 'w-8 h-8',
    md: 'w-10 h-10',
    lg: 'w-12 h-12',
    xl: 'w-16 h-16',
  };

  const textSizeMap = {
    sm: 'text-base',
    md: 'text-xl',
    lg: 'text-2xl',
    xl: 'text-3xl',
  };

  return (
    <div className={`flex items-center gap-3.5 select-none ${className}`}>
      {/* 3D Glassmorphic Icon Badge */}
      <div
        className={`${iconSizeMap[size]} relative flex items-center justify-center rounded-2xl bg-white/95 backdrop-blur-xl border border-white/80 shadow-[0_8px_20px_-4px_rgba(14,165,233,0.3),inset_0_1px_2px_rgba(255,255,255,1)] p-2`}
      >
        <svg
          viewBox="0 0 100 80"
          className="w-full h-full text-blue-600 drop-shadow-[0_2px_4px_rgba(29,78,216,0.3)]"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          {/* Left Hexagon Link */}
          <path
            d="M32 12 L50 22 L50 42 L32 52 L14 42 L14 22 Z"
            stroke="url(#blueGrad1)"
            strokeWidth="8"
            strokeLinejoin="round"
            strokeLinecap="round"
          />
          {/* Right Hexagon Link Interlocking */}
          <path
            d="M68 28 L86 38 L86 58 L68 68 L50 58 L50 38 Z"
            stroke="url(#blueGrad2)"
            strokeWidth="8"
            strokeLinejoin="round"
            strokeLinecap="round"
          />
          {/* Gradients */}
          <defs>
            <linearGradient id="blueGrad1" x1="14" y1="12" x2="50" y2="52" gradientUnits="userSpaceOnUse">
              <stop stopColor="#2563eb" />
              <stop offset="1" stopColor="#38bdf8" />
            </linearGradient>
            <linearGradient id="blueGrad2" x1="50" y1="28" x2="86" y2="68" gradientUnits="userSpaceOnUse">
              <stop stopColor="#1d4ed8" />
              <stop offset="1" stopColor="#0ea5e9" />
            </linearGradient>
          </defs>
        </svg>
      </div>

      {showText && (
        <span
          className={`${textSizeMap[size]} font-extrabold tracking-tight text-white drop-shadow-[0_2px_8px_rgba(0,0,0,0.25)]`}
        >
          Enterprise Connection
        </span>
      )}
    </div>
  );
};
