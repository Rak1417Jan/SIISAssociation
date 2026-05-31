import React from 'react';
import { motion } from 'framer-motion';

const variants = {
  primary: 'btn-premium text-white',
  secondary: 'bg-white border border-slate-100 text-slate-900 hover:bg-slate-50 hover:border-slate-200 shadow-sm',
  ghost: 'bg-transparent text-slate-500 hover:bg-slate-100 hover:text-slate-900',
  danger: 'bg-red-50 border border-red-100 text-red-600 hover:bg-red-100 shadow-sm',
  success: 'bg-green-50 border border-green-100 text-green-600 hover:bg-green-100 shadow-sm',
};

export function Button({ variant = 'primary', children, className = '', onClick, disabled, type = 'button', size = 'md', ...props }) {
  const sizes = {
    sm: 'px-4 py-2 text-[10px] uppercase tracking-widest font-black',
    md: 'px-6 py-3 text-xs uppercase tracking-widest font-black',
    lg: 'px-10 py-4 text-sm uppercase tracking-widest font-black',
  };

  return (
    <motion.button
      type={type}
      onClick={onClick}
      disabled={disabled}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      className={`
        inline-flex items-center justify-center gap-3 rounded-[18px]
        transition-all duration-300 cursor-pointer select-none outline-none
        disabled:opacity-50 disabled:pointer-events-none
        ${variants[variant]} ${sizes[size]} ${className}
      `}
      {...props}
    >
      {children}
    </motion.button>
  );
}
