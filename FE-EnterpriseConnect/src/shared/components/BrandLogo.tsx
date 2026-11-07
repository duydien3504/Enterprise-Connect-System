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
    <div className={`flex items-center gap-3 select-none ${className}`}>
      {/* Pure Crisp Vector Brand Link Icon matching Image 6 */}
      <div className={`${iconSizeMap[size]} relative flex items-center justify-center flex-shrink-0`}>
        <svg
          viewBox="0 0 100 80"
          className="w-full h-full drop-shadow-[0_2px_8px_rgba(0,0,0,0.18)]"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          {/* Left Rounded Hex Link */}
          <path
            d="M32 14 L50 24 L50 44 L32 54 L14 44 L14 24 Z"
            stroke="#ffffff"
            strokeWidth="9"
            strokeLinejoin="round"
            strokeLinecap="round"
          />
          {/* Right Rounded Hex Link Interlocking */}
          <path
            d="M66 26 L84 36 L84 56 L66 66 L48 56 L48 36 Z"
            stroke="#ffffff"
            strokeWidth="9"
            strokeLinejoin="round"
            strokeLinecap="round"
          />
        </svg>
      </div>

      {showText && (
        <span
          className={`${textSizeMap[size]} font-bold tracking-tight text-white drop-shadow-[0_1px_4px_rgba(0,0,0,0.2)]`}
        >
          Enterprise Connection
        </span>
      )}
    </div>
  );
};
