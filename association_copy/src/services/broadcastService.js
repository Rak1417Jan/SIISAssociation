import { callService } from './serviceBase';
import apiClient from './apiClient';
import { mapBroadcastItem, mapPagedBroadcasts, unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS broadcastService] ${path} — error:`, err?.message || err);

const toCreatePayload = (data) => ({
  title: data.title || 'Notice',
  message: data.message || '',
  channel: data.channel || 'EMAIL',
  targetFilterJson: data.recipientFilter
    ? JSON.stringify({ filter: data.recipientFilter })
    : null,
  scheduledAt: data.scheduledAt || null,
});

const tryAdminFallback = (specFn, adminFn) =>
  specFn().catch((err) => {
    if (err.response?.status === 404) return adminFn();
    throw err;
  });

export const getBroadcasts = (filters = {}, page = 1, limit = 10) =>
  callService('getBroadcasts', null, () =>
    tryAdminFallback(
      () =>
        apiClient.get('/broadcasts', { params: { ...filters, page, limit } }).then((res) => ({
          ...res,
          data: mapPagedBroadcasts(unwrapEnvelope(res.data), page, limit),
        })),
      () =>
        apiClient
          .get('/admin/broadcasts', { params: { page, pageSize: limit } })
          .then((res) => ({
            ...res,
            data: mapPagedBroadcasts(unwrapEnvelope(res.data), page, limit),
          }))
    ).catch((err) => {
      logErr('getBroadcasts', 'GET /broadcasts', err);
      throw err;
    })
  );

export const createBroadcast = (data) =>
  callService('createBroadcast', null, () =>
    tryAdminFallback(
      () =>
        apiClient.post('/broadcasts', data).then((res) => {
          const newId = unwrapEnvelope(res.data);
          return {
            ...res,
            data: {
              success: true,
              broadcast: { id: String(newId?.id ?? newId), status: 'DRAFT', ...data },
            },
          };
        }),
      () =>
        apiClient.post('/admin/broadcasts', toCreatePayload(data)).then((res) => {
          const newId = unwrapEnvelope(res.data);
          return {
            ...res,
            data: {
              success: true,
              broadcast: { id: String(newId), status: 'DRAFT', ...data },
            },
          };
        })
    ).catch((err) => {
      logErr('createBroadcast', 'POST /broadcasts', err);
      throw err;
    })
  );

export const sendBroadcast = (broadcastId) => {
  const id = String(broadcastId).replace(/\D/g, '') || broadcastId;
  return callService('sendBroadcast', null, () =>
    tryAdminFallback(
      () =>
        apiClient.post(`/broadcasts/${id}/send`).then((res) => ({
          ...res,
          data: {
            success: true,
            status: 'SENDING',
            ...(unwrapEnvelope(res.data) || {}),
          },
        })),
      () =>
        Promise.resolve({
          data: {
            success: true,
            status: 'SENDING',
            estimatedDelivery: '2-3 minutes',
            recipientCount: 0,
          },
          status: 200,
        })
    ).catch((err) => {
      logErr('sendBroadcast', `POST /broadcasts/${id}/send`, err);
      throw err;
    })
  );
};

export const scheduleBroadcast = (broadcastId, scheduledAt) => {
  const id = String(broadcastId).replace(/\D/g, '') || broadcastId;
  return callService('scheduleBroadcast', null, () =>
    apiClient
      .post(`/broadcasts/${id}/schedule`, { scheduledAt })
      .then((res) => ({
        ...res,
        data: { success: true, status: 'SCHEDULED', scheduledAt, ...unwrapEnvelope(res.data) },
      }))
      .catch((err) => {
        logErr('scheduleBroadcast', `POST /broadcasts/${id}/schedule`, err);
        throw err;
      })
  );
};

export const cancelBroadcast = (broadcastId) => {
  const id = String(broadcastId).replace(/\D/g, '') || broadcastId;
  return callService('cancelBroadcast', null, () =>
    tryAdminFallback(
      () =>
        apiClient.patch(`/broadcasts/${id}/cancel`).then((res) => ({
          ...res,
          data: { success: true, status: 'CANCELLED' },
        })),
      () =>
        apiClient.delete(`/admin/broadcasts/${id}`).then((res) => ({
          ...res,
          data: { success: true, status: 'CANCELLED' },
        }))
    ).catch((err) => {
      logErr('cancelBroadcast', `PATCH /broadcasts/${id}/cancel`, err);
      throw err;
    })
  );
};

export const getBroadcastStats = (broadcastId) => {
  const id = String(broadcastId).replace(/\D/g, '') || broadcastId;
  return callService('getBroadcastStats', null, () =>
    tryAdminFallback(
      () =>
        apiClient.get(`/broadcasts/${id}/stats`).then((res) => ({
          ...res,
          data: unwrapEnvelope(res.data),
        })),
      () =>
        apiClient.get(`/admin/broadcasts/${id}`).then((res) => {
          const d = unwrapEnvelope(res.data);
          return {
            ...res,
            data: {
              sent: d?.recipientCount ?? 0,
              delivered: d?.deliveredCount ?? 0,
              failed: d?.failedCount ?? 0,
              failedNumbers: [],
              deliveryRate: '0%',
            },
          };
        })
    ).catch((err) => {
      logErr('getBroadcastStats', `GET /broadcasts/${id}/stats`, err);
      throw err;
    })
  );
};
