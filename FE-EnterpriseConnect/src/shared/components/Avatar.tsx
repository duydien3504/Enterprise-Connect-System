import React from 'react';
import { cn } from '@/shared/utils/cn';
import { ENV } from '@/core/config/env';

interface AvatarProps {
  name: string;
  src?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  status?: 'online' | 'offline' | 'busy' | 'away';
  className?: string;
}

export const Avatar: React.FC<AvatarProps> = ({
  name,
  src,
  size = 'md',
  status,
  className,
}) => {
  const [prevSrc, setPrevSrc] = React.useState(src);
  const [hasError, setHasError] = React.useState(false);

  if (prevSrc !== src) {
    setPrevSrc(src);
    setHasError(false);
  }

  const sizeMap = {
    xs: 'w-5 h-5 text-[9px]',
    sm: 'w-7 h-7 text-xs',
    md: 'w-9 h-9 text-sm',
    lg: 'w-11 h-11 text-base',
    xl: 'w-14 h-14 text-lg',
  };

  const statusSizeMap = {
    xs: 'w-1.5 h-1.5',
    sm: 'w-2 h-2',
    md: 'w-2.5 h-2.5',
    lg: 'w-3 h-3',
    xl: 'w-3.5 h-3.5',
  };

  const statusColorMap = {
    online: 'bg-emerald-500',
    offline: 'bg-slate-500',
    busy: 'bg-rose-500',
    away: 'bg-amber-500',
  };

  const parts = (name || '').trim().split(/\s+/).filter(Boolean);
  const initials =
    parts.length === 0
      ? 'EC'
      : parts.length === 1
      ? parts[0].slice(0, 2).toUpperCase()
      : (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();

  const formatAvatarUrl = (url?: string): string | null => {
    if (!url) return null;
    const trimmed = url.trim();
    if (trimmed === '' || trimmed === 'string' || trimmed.length <= 5) return null;
    // Bỏ qua URL placeholder demo Cloudinary không tồn tại
    if (trimmed.includes('cloudinary.com/demo/') || trimmed.includes('/demo/image/upload/')) {
      return null;
    }
    if (
      trimmed.startsWith('http://') ||
      trimmed.startsWith('https://') ||
      trimmed.startsWith('blob:') ||
      trimmed.startsWith('data:')
    ) {
      return trimmed;
    }
    if (trimmed.startsWith('/')) {
      return `${ENV.API_BASE_URL}${trimmed}`;
    }
    return trimmed;
  };

  const cachedActiveAvatar = localStorage.getItem('ec_custom_avatar_active');
  const formattedSrc = formatAvatarUrl(src) || formatAvatarUrl(cachedActiveAvatar || undefined);
  const isValidSrc = !!formattedSrc && !hasError;

  return (
    <div className={cn('relative inline-flex flex-shrink-0', className)}>
      {isValidSrc ? (
        <img
          src={formattedSrc!}
          alt=""
          onError={() => setHasError(true)}
          className={cn('rounded-full object-cover border border-slate-700', sizeMap[size])}
        />
      ) : (
        <div
          className={cn(
            'flex items-center justify-center rounded-full bg-[#0052cc] font-bold text-white shadow-inner select-none',
            sizeMap[size]
          )}
        >
          {initials}
        </div>
      )}

      {status && (
        <span
          className={cn(
            'absolute bottom-0 right-0 rounded-full ring-2 ring-slate-950',
            statusSizeMap[size],
            statusColorMap[status]
          )}
        />
      )}
    </div>
  );
};
