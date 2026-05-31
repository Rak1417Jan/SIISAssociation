import { callService } from './serviceBase';
import apiClient from './apiClient';
import {
  buildPermissionMatrix,
  mapRolesList,
  unwrapEnvelope,
} from './apiTransforms';

// TODO: Confirm real permissionIds with backend team — GET /api/v1/roles/permissions
// should return id field on each permission
const PERMISSION_ID_MAP = {
  'members.view': 1,
  'members.create': 2,
  'members.approve': 3,
  'members.delete': 4,
  'payments.view': 5,
  'payments.refund': 6,
  'staff.manage': 7,
  'settings.manage': 8,
  'analytics.view': 9,
  'analytics.export': 10,
  'audit.view': 11,
};

export const getRoles = () => {
  return callService(
    'getRoles',
    null,
    () =>
      apiClient.get('/admin/roles').then((res) => {
        const roles = unwrapEnvelope(res.data) || [];
        return { ...res, data: mapRolesList(roles) };
      })
  );
};

export const getPermissionMatrix = () => {
  return callService(
    'getPermissionMatrix',
    null,
    () =>
      apiClient.get('/admin/roles').then((res) => {
        const roles = unwrapEnvelope(res.data) || [];
        return { ...res, data: buildPermissionMatrix(roles) };
      })
  );
};

function permissionsToIds(permissions) {
  return Object.entries(permissions || {})
    .flatMap(([resource, actions]) =>
      Object.entries(actions || {})
        .filter(([, enabled]) => enabled)
        .map(([action]) => PERMISSION_ID_MAP[`${resource}.${action}`])
    )
    .filter((id) => id != null);
}

export const updateRolePermissions = (roleId, permissions) => {
  const permissionIds = permissionsToIds(permissions);

  return callService(
    'updateRolePermissions',
    null,
    async () => {
      const rolesRes = await apiClient.get('/admin/roles');
      const roles = unwrapEnvelope(rolesRes.data) || [];
      const match = roles.find((r) => String(r.roleId) === String(roleId));
      const roleName = match?.roleName || roleId;
      const res = await apiClient.put(`/admin/roles/${encodeURIComponent(roleName)}`, {
        permissionIds,
      });
      return {
        ...res,
        data: { success: true, role: { id: roleId, permissions } },
      };
    }
  );
};

export const assignRoleToStaff = (staffId, roleId) => {
  const id = String(staffId).replace(/\D/g, '') || staffId;
  return callService(
    'assignRoleToStaff',
    null,
    () =>
      apiClient
        .put(`/admin/staff/${id}`, { roleId: Number(roleId) || 1 })
        .then((res) => ({
          ...res,
          data: { success: true, newRole: roleId },
        }))
  );
};
