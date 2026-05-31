import { useState, useCallback } from 'react';

export function useSidebar() {
  const [collapsed, setCollapsed] = useState(false);
  const toggle = useCallback(() => setCollapsed(c => !c), []);
  return { collapsed, toggle, setCollapsed };
}
