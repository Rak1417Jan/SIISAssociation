import { useCallback } from 'react';
import { useAuth } from '../context/AuthContext';

/**
 * Permission matrix: action + resource → which roles are allowed.
 * Extend this map as new features are added.
 */
const PERMISSION_MATRIX = {
  'approve:members':    ['admin', 'super_admin'],
  'reject:members':     ['admin', 'super_admin'],
  'delete:members':     ['super_admin'],
  'export:members':     ['admin', 'super_admin'],
  'export:analytics':   ['admin', 'super_admin'],
  'export:audit':       ['admin', 'super_admin'],
  'refund:payments':    ['finance', 'super_admin'],
  'edit:roles':         ['super_admin'],
  'save:settings':      ['super_admin'],
  'deactivate:staff':   ['super_admin'],
  'send:broadcast':     ['admin', 'super_admin'],
  'create:broadcast':   ['admin', 'super_admin'],
  'view:analytics':     ['finance', 'admin', 'super_admin'],
  'view:audit':         ['super_admin'],
  'manage:staff':       ['super_admin'],
  'manage:roles':       ['super_admin'],
  'manage:settings':    ['super_admin'],
  'manage:firms':       ['admin', 'super_admin'],
  
  // S29-S32 Engagement Tools
  'viewContact:directory': ['admin', 'super_admin'],
  'create:events':         ['admin', 'super_admin'],
  'delete:events':         ['admin', 'super_admin'],
  'manage:grievances':     ['super_admin'],
  'respond:grievances':    ['admin', 'super_admin'],
};

/**
 * Route access control: path → array of allowed roles.
 */
const ROUTE_ACCESS = {
  // Admin routes
  '/admin':           ['admin', 'super_admin'],
  '/members':         ['admin', 'super_admin'],
  '/member/:id':      ['admin', 'super_admin'],
  '/firms':           ['admin', 'super_admin'],
  '/firm-editor':     ['admin', 'super_admin'],
  '/staff':           ['super_admin'],
  '/roles':           ['super_admin'],
  '/analytics':       ['finance', 'admin', 'super_admin'],
  '/audit':           ['super_admin'],
  '/settings':        ['super_admin'],
  '/broadcast':       ['admin', 'super_admin'],

  // Engagement (staff + admin)
  '/support':         ['operator', 'admin', 'super_admin'],
  '/directory':       ['member', 'admin', 'super_admin'],
  '/events':          ['member', 'admin', 'super_admin'],
  '/referral':        ['member', 'admin', 'super_admin'],

  // Member routes — accessible to member and above
  '/dashboard':       ['member', 'operator', 'finance', 'admin', 'super_admin'],
  '/plan-selection':  ['member', 'admin', 'super_admin'],
  '/payment-summary': ['member', 'admin', 'super_admin'],
  '/payment-success': ['member', 'admin', 'super_admin'],
  '/my-info':         ['member', 'operator', 'finance', 'admin', 'super_admin'],
  '/renewal':         ['member', 'admin', 'super_admin'],
  '/payments':        ['member', 'finance', 'admin', 'super_admin'],
  '/status':          ['member', 'admin', 'super_admin'],
  '/id-card':         ['member', 'admin', 'super_admin'],
  '/inbox':           ['member', 'operator', 'admin', 'super_admin'],
  '/grievance':       ['member', 'operator', 'admin', 'super_admin'],
};

export function usePermissions() {
  const { user } = useAuth();
  const role = user?.role || 'member';

  /**
   * Check if the current user has permission for action:resource.
   */
  const hasPermission = useCallback((action, resource) => {
    const key = `${action}:${resource}`;
    const allowed = PERMISSION_MATRIX[key];
    if (!allowed) return false;
    return allowed.includes(role);
  }, [role]);

  /**
   * Check if the current user can access a given route path.
   * Supports parameterized routes like /member/:id.
   */
  const canAccessRoute = useCallback((path) => {
    // Direct match first
    if (ROUTE_ACCESS[path]) {
      return ROUTE_ACCESS[path].includes(role);
    }

    // Try parameterized match (e.g. /member/MEM-001 → /member/:id)
    for (const [pattern, roles] of Object.entries(ROUTE_ACCESS)) {
      if (pattern.includes(':')) {
        const regex = new RegExp(
          '^' + pattern.replace(/:[^/]+/g, '[^/]+') + '$'
        );
        if (regex.test(path)) {
          return roles.includes(role);
        }
      }
    }

    // Default: allow (for unlisted routes like public pages)
    return true;
  }, [role]);

  return { hasPermission, canAccessRoute, role };
}

export { ROUTE_ACCESS };
