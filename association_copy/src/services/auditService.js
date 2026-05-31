import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS auditService] ${path} — error:`, err?.message || err);

export const getAuditLogs = (filters = {}, page = 1, limit = 10) =>
  callService('getAuditLogs', null, () =>
    apiClient
      .get('/audit-logs', {
        params: {
          staffId: filters.staffId,
          action: filters.action,
          page,
          limit,
          startDate: filters.startDate,
          endDate: filters.endDate,
          ...filters,
        },
      })
      .then((res) => {
        const raw = unwrapEnvelope(res.data) || {};
        return {
          ...res,
          data: {
            data: raw.records || raw.data || [],
            total: raw.total ?? 0,
            page,
            limit,
          },
        };
      })
      .catch((err) => {
        logErr('getAuditLogs', 'GET /audit-logs', err);
        throw err;
      })
  );

export const exportAuditLogs = (filters = {}) =>
  callService('exportAuditLogs', null, () =>
    apiClient
      .get('/audit-logs/export', { params: filters })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('exportAuditLogs', 'GET /audit-logs/export', err);
        throw err;
      })
  );
