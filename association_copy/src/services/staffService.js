import { callService } from './serviceBase';
import apiClient from './apiClient';
import { mapStaffPage, rootUrl, unwrapEnvelope } from './apiTransforms';

export const getStaffList = (filters = {}, page = 1, limit = 10) => {
  return callService(
    'getStaffList',
    null,
    () =>
      apiClient.get('/admin/staff').then((res) => {
        let list = unwrapEnvelope(res.data) || [];
        if (filters.role) {
          list = list.filter(
            (s) => (s.roleName || '').toLowerCase() === String(filters.role).toLowerCase()
          );
        }
        if (filters.isActive !== undefined) {
          list = list.filter((s) => s.isActive === filters.isActive);
        }
        return {
          ...res,
          data: mapStaffPage(list, page, limit),
        };
      })
  );
};

export const getStaffById = (staffId) => {
  const id = String(staffId).replace(/\D/g, '') || staffId;
  return callService(
    'getStaffById',
    null,
    () =>
      apiClient.get('/admin/staff').then((res) => {
        const list = unwrapEnvelope(res.data) || [];
        const found = list.find((s) => String(s.userId) === id) || list[0];
        return {
          ...res,
          data: { ...found, activityLog: [] },
        };
      })
  );
};

export const createStaff = (data) => {
  const payload = {
    username: data.email?.split('@')[0] || data.username,
    email: data.email,
    fullName: data.name || data.fullName,
    roleId: data.roleId || 1,
    mobileNo: data.phone || '',
  };
  return callService(
    'createStaff',
    null,
    () =>
      apiClient.post('/admin/staff', payload).then((res) => {
        const newId = unwrapEnvelope(res.data);
        return {
          ...res,
          data: {
            success: true,
            staff: {
              id: String(newId),
              ...data,
              isActive: true,
              temporaryPassword: 'Set via email',
              createdAt: new Date().toISOString(),
            },
          },
        };
      })
  );
};

export const updateStaff = (staffId, data) => {
  const id = String(staffId).replace(/\D/g, '') || staffId;
  return callService(
    'updateStaff',
    null,
    () =>
      apiClient
        .put(`/admin/staff/${id}`, {
          email: data.email,
          fullName: data.name || data.fullName,
          roleId: data.roleId,
          mobileNo: data.phone,
        })
        .then((res) => ({
          ...res,
          data: { success: true, staff: { id, ...data } },
        }))
  );
};

export const deactivateStaff = (staffId) => {
  const id = String(staffId).replace(/\D/g, '') || staffId;
  return callService(
    'deactivateStaff',
    null,
    () =>
      apiClient.delete(`/admin/staff/${id}`).then((res) => ({
        ...res,
        data: { success: true, isActive: false },
      }))
  );
};

export const reactivateStaff = (staffId) => {
  // BACKEND PENDING: no reactivate endpoint — use update with isActive
  return callService('reactivateStaff', null, () =>
    Promise.resolve({ data: { success: true, isActive: true }, status: 200 })
  );
};

export const resetStaffPassword = (staffId) => {
  return callService(
    'resetStaffPassword',
    null,
    async () => {
      const list = unwrapEnvelope((await apiClient.get('/admin/staff')).data) || [];
      const staff = list.find((s) => String(s.userId) === String(staffId));
      if (staff?.email) {
        await apiClient.post(
          rootUrl('/auth/admin/password-reset'),
          { email: staff.email },
          { baseURL: '' }
        );
      }
      return {
        data: { success: true, message: 'Password reset email sent to staff' },
        status: 200,
      };
    }
  );
};

export const getStaffActivity = () => {
  // BACKEND PENDING: staff activity log not implemented
  return callService('getStaffActivity', null, () =>
    Promise.resolve({ data: [], status: 200 })
  );
};
