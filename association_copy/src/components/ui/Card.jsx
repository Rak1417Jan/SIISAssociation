import React from 'react';
import { cn } from '../../utils/cn';

export function Card({ children, className = '', hover = true, ...props }) {
  return (
    <div
      className={cn(
        'glass-panel rounded-[32px] p-8',
        hover && 'transition-all duration-500 hover:shadow-[0_32px_64px_-16px_rgba(59,130,246,0.12)] hover:-translate-y-1.5',
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}
