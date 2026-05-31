import React from 'react';

const config = {
  active:   { bg: 'bg-green-50',  text: 'text-green-700',  dot: 'bg-green-500'  },
  approved: { bg: 'bg-green-50',  text: 'text-green-700',  dot: 'bg-green-500'  },
  pending:  { bg: 'bg-amber-50',  text: 'text-amber-700',  dot: 'bg-amber-500'  },
  inactive: { bg: 'bg-red-50',    text: 'text-red-700',    dot: 'bg-red-500'    },
  rejected: { bg: 'bg-red-50',    text: 'text-red-700',    dot: 'bg-red-500'    },
  paid:     { bg: 'bg-blue-50',   text: 'text-blue-700',   dot: 'bg-blue-500'   },
  unpaid:   { bg: 'bg-orange-50', text: 'text-orange-700', dot: 'bg-orange-500' },
  admin:    { bg: 'bg-purple-50', text: 'text-purple-700', dot: 'bg-purple-500' },
  member:   { bg: 'bg-sky-50',    text: 'text-sky-700',    dot: 'bg-sky-500'    },
  staff:    { bg: 'bg-indigo-50', text: 'text-indigo-700', dot: 'bg-indigo-500' },
};

export function Badge({ status = 'active', label }) {
  const c = config[status?.toLowerCase()] || config.pending;
  return (
    <span className={`badge ${c.bg} ${c.text}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${c.dot}`} />
      {label || status}
    </span>
  );
}
