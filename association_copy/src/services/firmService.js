import { callService } from './serviceBase';
import apiClient from './apiClient';
import {
  mapFirmDetail,
  mapFirmListItem,
  mapPagedFirms,
  unwrapEnvelope,
} from './apiTransforms';

export const getFirms = (filters = {}, page = 1, limit = 10) => {
  return callService(
    'getFirms',
    null,
    () =>
      apiClient
        .get('/admin/firms', {
          params: { page, pageSize: limit, search: filters.search || undefined },
        })
        .then((res) => ({
          ...res,
          data: mapPagedFirms(unwrapEnvelope(res.data), page, limit),
        }))
  );
};

export const getFirmById = (firmId) => {
  const id = String(firmId).replace(/\D/g, '') || firmId;
  return callService(
    'getFirmById',
    null,
    () =>
      apiClient.get(`/admin/firms/${id}`).then((res) => ({
        ...res,
        data: mapFirmDetail(unwrapEnvelope(res.data)),
      }))
  );
};

export const createFirm = (data) => {
  const payload = {
    name: data.name,
    companyTypeId: data.companyTypeId || 6,
    companyCode: data.registrationNo || data.companyCode,
    gstNo: data.gstNo,
    address: data.address,
    city: data.city,
    regNo: data.registrationNo,
    mobile: data.phone,
  };
  return callService(
    'createFirm',
    null,
    () =>
      apiClient.post('/admin/firms', payload).then((res) => {
        const newId = unwrapEnvelope(res.data);
        return {
          ...res,
          data: {
            success: true,
            firm: { id: String(newId), ...data, memberCount: 0, createdAt: new Date().toISOString() },
          },
        };
      })
  );
};

export const updateFirm = (firmId, data) => {
  const id = String(firmId).replace(/\D/g, '') || firmId;
  const payload = {
    name: data.name,
    companyTypeId: data.companyTypeId,
    gstNo: data.gstNo,
    address: data.address,
    city: data.city,
    regNo: data.registrationNo,
    mobile: data.phone,
  };
  return callService(
    'updateFirm',
    null,
    () =>
      apiClient.put(`/admin/firms/${id}`, payload).then((res) => ({
        ...res,
        data: { success: true, firm: { id, ...data } },
      }))
  );
};

export const deleteFirm = (firmId) => {
  const id = String(firmId).replace(/\D/g, '') || firmId;
  return callService(
    'deleteFirm',
    null,
    () =>
      apiClient.delete(`/admin/firms/${id}`).then((res) => ({
        ...res,
        data: { success: true },
      }))
  );
};

export const getFirmMembers = (firmId, page = 1, limit = 10) => {
  // BACKEND PENDING: dedicated firm members list — use members filter by firm
  const id = String(firmId).replace(/\D/g, '') || firmId;
  return callService(
    'getFirmMembers',
    null,
    () =>
      apiClient
        .get('/admin/members', { params: { page, pageSize: limit, firmId: id } })
        .then((res) => {
          const paged = unwrapEnvelope(res.data);
          const records = (paged?.records || []).map((m) => ({
            id: String(m.memberId),
            name: m.ownerName,
            status: m.isActive ? 'APPROVED' : 'PENDING',
            email: m.email,
            phone: m.mobileNumber,
          }));
          return {
            ...res,
            data: { data: records, total: paged?.total ?? records.length, page, limit },
          };
        })
  );
};

export const addMemberToFirm = (firmId, memberId) => {
  const fid = String(firmId).replace(/\D/g, '') || firmId;
  const mid = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'addMemberToFirm',
    null,
    () =>
      apiClient
        .post(`/admin/firms/${fid}/members`, { memberId: Number(mid) })
        .then((res) => ({
          ...res,
          data: { success: true, message: 'Member linked to firm successfully' },
        }))
  );
};

export const removeMemberFromFirm = (firmId, memberId) => {
  const fid = String(firmId).replace(/\D/g, '') || firmId;
  const mid = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'removeMemberFromFirm',
    null,
    () =>
      apiClient.delete(`/admin/firms/${fid}/members/${mid}`).then((res) => ({
        ...res,
        data: { success: true, message: 'Member removed from firm' },
      }))
  );
};

export const searchFirms = (query) => {
  return callService(
    'searchFirms',
    null,
    () =>
      apiClient
        .get('/admin/firms', { params: { page: 1, pageSize: 50, search: query } })
        .then((res) => {
          const paged = unwrapEnvelope(res.data);
          return {
            ...res,
            data: (paged?.records || []).map(mapFirmListItem),
          };
        })
  );
};

export const exportFirms = () => {
  // BACKEND PENDING: firms export not implemented
  return callService('exportFirms', null, () =>
    Promise.resolve({
      data: { downloadUrl: null, fileName: 'firms-export.csv' },
      status: 200,
    })
  );
};
