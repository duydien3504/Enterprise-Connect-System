import React, { useEffect, useRef } from 'react';

export const EnterpriseCursor: React.FC = () => {
  const haloRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let animId: number;
    let mouseX = -100;
    let mouseY = -100;
    let trailX = -100;
    let trailY = -100;
    let isHovered = false;

    const onMouseMove = (e: MouseEvent) => {
      mouseX = e.clientX;
      mouseY = e.clientY;

      const target = e.target as HTMLElement | null;
      if (target) {
        const isInputField =
          target.tagName === 'INPUT' ||
          target.tagName === 'TEXTAREA' ||
          target.isContentEditable;

        if (isInputField) {
          if (haloRef.current) haloRef.current.style.opacity = '0';
          return;
        }

        if (haloRef.current) haloRef.current.style.opacity = '1';

        const isClickable =
          target.tagName === 'BUTTON' ||
          target.tagName === 'A' ||
          target.closest('button') ||
          target.closest('a') ||
          target.getAttribute('role') === 'button' ||
          target.classList.contains('cursor-pointer');

        if (isClickable !== isHovered) {
          isHovered = Boolean(isClickable);
          if (haloRef.current) {
            if (isHovered) {
              haloRef.current.style.width = '46px';
              haloRef.current.style.height = '46px';
              haloRef.current.style.borderColor = '#38bdf8';
              haloRef.current.style.backgroundColor = 'rgba(0, 82, 204, 0.25)';
              haloRef.current.style.boxShadow = '0 0 20px rgba(56, 189, 248, 0.6)';
            } else {
              haloRef.current.style.width = '32px';
              haloRef.current.style.height = '32px';
              haloRef.current.style.borderColor = 'rgba(56, 189, 248, 0.6)';
              haloRef.current.style.backgroundColor = 'rgba(0, 82, 204, 0.08)';
              haloRef.current.style.boxShadow = '0 0 10px rgba(56, 189, 248, 0.3)';
            }
          }
        }
      }
    };

    const onMouseLeave = () => {
      if (haloRef.current) haloRef.current.style.opacity = '0';
    };

    const onMouseEnter = () => {
      if (haloRef.current) haloRef.current.style.opacity = '1';
    };

    window.addEventListener('mousemove', onMouseMove, { passive: true });
    document.addEventListener('mouseleave', onMouseLeave);
    document.addEventListener('mouseenter', onMouseEnter);

    // Direct GPU RAF Lerp (Zero React State Re-renders, 0ms Lag)
    const render = () => {
      trailX += (mouseX - trailX) * 0.32;
      trailY += (mouseY - trailY) * 0.32;

      if (haloRef.current) {
        haloRef.current.style.transform = `translate3d(${trailX}px, ${trailY}px, 0)`;
      }

      animId = requestAnimationFrame(render);
    };

    animId = requestAnimationFrame(render);

    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseleave', onMouseLeave);
      document.removeEventListener('mouseenter', onMouseEnter);
      cancelAnimationFrame(animId);
    };
  }, []);

  return (
    <div className="pointer-events-none fixed inset-0 z-[9999] overflow-hidden select-none">
      {/* Trailing Magnetic Halo Ring - Direct GPU accelerated */}
      <div
        ref={haloRef}
        style={{ willChange: 'transform', transform: 'translate3d(-100px, -100px, 0)' }}
        className="absolute -top-[16px] -left-[16px] w-[32px] h-[32px] rounded-full border border-sky-400/60 bg-[#0052cc]/10 transition-[width,height,background-color,border-color,box-shadow] duration-150 ease-out pointer-events-none"
      />
    </div>
  );
};
