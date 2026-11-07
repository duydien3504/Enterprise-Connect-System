import React, { useEffect, useState } from 'react';

export const EnterpriseCursor: React.FC = () => {
  const [position, setPosition] = useState({ x: -100, y: -100 });
  const [isHovered, setIsHovered] = useState(false);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const onMouseMove = (e: MouseEvent) => {
      setPosition({ x: e.clientX, y: e.clientY });
      if (!isVisible) setIsVisible(true);

      const target = e.target as HTMLElement | null;
      if (target) {
        const isClickable =
          target.tagName === 'BUTTON' ||
          target.tagName === 'A' ||
          target.tagName === 'INPUT' ||
          target.closest('button') ||
          target.closest('a') ||
          target.getAttribute('role') === 'button' ||
          target.classList.contains('cursor-pointer');
        setIsHovered(Boolean(isClickable));
      }
    };

    const onMouseLeave = () => setIsVisible(false);

    window.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseleave', onMouseLeave);

    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseleave', onMouseLeave);
    };
  }, [isVisible]);

  if (!isVisible) return null;

  return (
    <div className="pointer-events-none fixed inset-0 z-[9999] overflow-hidden">
      {/* Outer Spring Ring */}
      <div
        className={`absolute -translate-x-1/2 -translate-y-1/2 rounded-full border transition-transform duration-150 ease-out ${
          isHovered
            ? 'h-11 w-11 border-sky-300 bg-sky-400/15 shadow-[0_0_20px_rgba(56,189,248,0.5)] scale-125'
            : 'h-8 w-8 border-white/60 bg-white/5 backdrop-blur-[1px] scale-100'
        }`}
        style={{
          left: `${position.x}px`,
          top: `${position.y}px`,
        }}
      />
      {/* Inner Precision Orb */}
      <div
        className={`absolute -translate-x-1/2 -translate-y-1/2 rounded-full transition-all duration-75 ${
          isHovered
            ? 'h-2.5 w-2.5 bg-cyan-300 shadow-[0_0_12px_#38bdf8]'
            : 'h-1.5 w-1.5 bg-white shadow-[0_0_8px_#ffffff]'
        }`}
        style={{
          left: `${position.x}px`,
          top: `${position.y}px`,
        }}
      />
    </div>
  );
};
