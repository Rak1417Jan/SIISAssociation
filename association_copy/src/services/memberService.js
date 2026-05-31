import { callService } from './serviceBase';
import apiClient from './apiClient';
import {
  mapMemberDetail,
  mapMemberListItem,
  mapPagedMembers,
  unwrapEnvelope,
} from './apiTransforms';

const memberParams = (filters = {}, page = 1, limit = 10, search) => ({
  page,
  pageSize: limit,
  status: filters.status || undefined,
  planId: filters.planId || undefined,
  search: search || filters.q || undefined,
  sortBy: filters.sortBy || 'oldest',
  sortOrder: filters.sortOrder || 'asc',
});

export const getMembers = (filters = {}, page = 1, limit = 10) => {
  return callService(
    'getMembers',
    null,
    () =>
      apiClient
        .get('/admin/members', { params: memberParams(filters, page, limit) })
        .then((res) => ({
          ...res,
          data: mapPagedMembers(unwrapEnvelope(res.data), page, limit),
        }))
  );
};

export const getMemberById = (memberId) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'getMemberById',
    null,
    () =>
      apiClient.get(`/admin/members/${id}`).then((res) => ({
        ...res,
        data: mapMemberDetail(unwrapEnvelope(res.data)),
      }))
  );
};

export const updateMember = (memberId, data) => {
  // BACKEND PENDING: no general member update endpoint
  return callService('updateMember', null, () =>
    Promise.resolve({
      data: { success: true, member: { id: memberId, ...data } },
      status: 200,
    })
  );
};

export const approveMember = (memberId, remarks) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'approveMember',
    null,
    () =>
      apiClient
        .put(`/admin/members/${id}/verify`, { notes: remarks || '' })
        .then((res) => ({
          ...res,
          data: {
            success: true,
            memberId: id,
            status: 'APPROVED',
            approvedAt: new Date().toISOString(),
            digitalIdGenerated: false,
            notificationSent: true,
          },
        }))
  );
};

export const rejectMember = (memberId, reason) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'rejectMember',
    null,
    () =>
      apiClient
        .put(`/admin/members/${id}/reject`, { feedback: reason || '' })
        .then((res) => ({
          ...res,
          data: {
            success: true,
            memberId: id,
            status: 'REJECTED',
            rejectionReason: reason,
            notificationSent: true,
          },
        }))
  );
};

export const suspendMember = (memberId, reason) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'suspendMember',
    null,
    () =>
      apiClient
        .put(`/admin/members/${id}/hold`, { reason: reason || '' })
        .then((res) => ({
          ...res,
          data: { success: true, status: 'SUSPENDED' },
        }))
  );
};

export const reactivateMember = (memberId) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return approveMember(id, 'Reactivated by admin');
};

export const searchMembers = (query) => {
  return callService(
    'searchMembers',
    null,
    () =>
      apiClient
        .get('/admin/members', {
          params: { page: 1, pageSize: 50, search: query },
        })
        .then((res) => {
          const paged = unwrapEnvelope(res.data);
          const records = paged?.records || [];
          return {
            ...res,
            data: records.map(mapMemberListItem),
          };
        })
  );
};

export const getMembershipHistory = () => {
  // BACKEND PENDING: /admin/members/:id/history not implemented
  return callService('getMembershipHistory', null, () =>
    Promise.resolve({ data: [], status: 200 })
  );
};

export const exportMembers = () => {
  // BACKEND PENDING: members export endpoint not implemented
  return callService('exportMembers', null, () =>
    Promise.resolve({
      data: {
        downloadUrl: null,
        fileName: 'members-export.csv',
        message: 'Export not available on backend yet',
      },
      status: 200,
    })
  );
};
