import { callService } from './serviceBase';
import apiClient from './apiClient';
import {
  mapAnalyticsMembers,
  mapAnalyticsRevenue,
  mapDashboardMetrics,
  mapPendingApplications,
  unwrapEnvelope,
} from './apiTransforms';

export const getDashboardMetrics = () => {
  return callService(
    'getDashboardMetrics',
    null,
    () =>
      apiClient.get('/admin/dashboard').then((res) => ({
        ...res,
        data: mapDashboardMetrics(unwrapEnvelope(res.data)),
      }))
  );
};

export const getPendingApplications = (page = 1, limit = 10) => {
  return callService(
    'getPendingApplications',
    null,
    () =>
      apiClient
        .get('/admin/pending-queue', { params: { page, pageSize: limit } })
        .then((res) => ({
          ...res,
          data: mapPendingApplications(unwrapEnvelope(res.data)),
        }))
  );
};

export const getMemberStats = (dateRange) => {
  const year = dateRange?.year || new Date().getFullYear();
  return callService(
    'getMemberStats',
    null,
    () =>
      apiClient.get('/admin/analytics', { params: { year } }).then((res) => ({
        ...res,
        data: mapAnalyticsMembers(unwrapEnvelope(res.data)),
      }))
  );
};

export const getRevenueStats = (dateRange) => {
  const year = dateRange?.year || new Date().getFullYear();
  return callService(
    'getRevenueStats',
    null,
    () =>
      apiClient.get('/admin/analytics', { params: { year } }).then((res) => ({
        ...res,
        data: mapAnalyticsRevenue(unwrapEnvelope(res.data)),
      }))
  );
};

export const getRegistrationTrends = (dateRange) => {
  const year = dateRange?.year || new Date().getFullYear();
  return callService(
    'getRegistrationTrends',
    null,
    () =>
      apiClient.get('/admin/analytics', { params: { year } }).then((res) => {
        const d = unwrapEnvelope(res.data);
        const growth = d?.membershipGrowth || [];
        return {
          ...res,
          data: {
            daily: growth.map((p) => p.newMembers ?? 0),
            weekly: [],
            monthly: growth.map((p) => p.newMembers ?? 0),
          },
        };
      })
  );
};

export const getFirmStats = () => {
  return callService(
    'getFirmStats',
    null,
    () =>
      apiClient.get('/admin/firms', { params: { page: 1, pageSize: 1 } }).then((res) => {
        const paged = unwrapEnvelope(res.data);
        return {
          ...res,
          data: {
            totalFirms: paged?.total ?? 0,
            activeFirms: paged?.total ?? 0,
            avgMembersPerFirm: 0,
            topFirms: [],
          },
        };
      })
  );
};

export const getMemberApplicationStatus = (memberId) => {
  const id = String(memberId).replace(/\D/g, '') || memberId;
  return callService(
    'getMemberApplicationStatus',
    null,
    () =>
      apiClient.get(`/admin/members/${id}`).then((res) => {
        const d = unwrapEnvelope(res.data);
        const status = d?.applicationStatus || 'PENDING';
        return {
          ...res,
          data: {
            applicationId: String(d?.applicationId || ''),
            status,
            progressPercent: status === 'VERIFIED' ? 100 : 60,
            steps: [
              { label: 'Applied', done: true, date: d?.createdDate },
              { label: 'Under Review', done: status !== 'PENDING', date: null },
              { label: 'Approved', done: status === 'VERIFIED', date: null },
            ],
            estimatedDays: 2,
            adminNote: d?.applicationRemarks || '',
          },
        };
      })
  );
};

export const exportAnalyticsReport = (type, filters) => {
  // BACKEND PENDING: analytics export not implemented
  return callService('exportAnalyticsReport', null, () =>
    Promise.resolve({
      data: { downloadUrl: null, fileName: `${type}-report.csv` },
      status: 200,
    })
  );
};
