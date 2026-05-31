import React from 'react';
import { usePermissions } from '../hooks/usePermissions';

/**
 * PermissionGate conditionally renders children based on the current user's permissions.
 * If the user lacks the required permission, nothing is rendered (silent hide).
 *
 * @param {string} action - The action to check (e.g. "approve", "delete", "export")
 * @param {string} resource - The resource to check (e.g. "members", "payments")
 * @param {React.ReactNode} children - The UI to conditionally render
 * @param {React.ReactNode} [fallback] - Optional fallback UI if permission denied
 */
export default function PermissionGate({ action, resource, children, fallback = null }) {
  const { hasPermission } = usePermissions();

  if (!hasPermission(action, resource)) {
    return fallback;
  }

  return children;
}
