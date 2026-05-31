import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS settingsService] ${path} — error:`, err?.message || err);

export const getSettings = () =>
  callService('getSettings', null, () =>
    apiClient
      .get('/settings')
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getSettings', 'GET /settings', err);
        throw err;
      })
  );

export const updateSettings = (data) =>
  callService('updateSettings', null, () =>
    apiClient
      .put('/settings', data)
      .then((res) => ({
        ...res,
        data: { success: true, settings: unwrapEnvelope(res.data) || data },
      }))
      .catch((err) => {
        logErr('updateSettings', 'PUT /settings', err);
        throw err;
      })
  );

export const uploadLogo = (file) => {
  const formData = new FormData();
  formData.append('file', file);
  return callService('uploadLogo', null, () =>
    apiClient
      .post('/settings/logo', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('uploadLogo', 'POST /settings/logo', err);
        throw err;
      })
  );
};
