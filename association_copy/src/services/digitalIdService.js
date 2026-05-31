import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS digitalIdService] ${path} — error:`, err?.message || err);

export const getDigitalId = (memberId) =>
  callService('getDigitalId', null, () =>
    apiClient
      .get(`/digital-id/${memberId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getDigitalId', `GET /digital-id/${memberId}`, err);
        throw err;
      })
  );

export const generateDigitalId = (memberId) =>
  callService('generateDigitalId', null, () =>
    apiClient
      .post('/digital-id/generate', { memberId })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('generateDigitalId', 'POST /digital-id/generate', err);
        throw err;
      })
  );

export const downloadDigitalId = (memberId, format = 'pdf') =>
  callService('downloadDigitalId', null, () =>
    apiClient
      .get(`/digital-id/${memberId}/download`, { params: { format } })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('downloadDigitalId', `GET /digital-id/${memberId}/download`, err);
        throw err;
      })
  );

export const verifyDigitalId = (membershipId) =>
  callService('verifyDigitalId', null, () =>
    apiClient
      .get(`/digital-id/verify/${membershipId}`, { skipAuth: true })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('verifyDigitalId', `GET /digital-id/verify/${membershipId}`, err);
        throw err;
      })
  );

export const shareDigitalId = (memberId, channel) =>
  callService('shareDigitalId', null, () =>
    apiClient
      .post(`/digital-id/${memberId}/share`, { channel })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('shareDigitalId', `POST /digital-id/${memberId}/share`, err);
        throw err;
      })
  );
