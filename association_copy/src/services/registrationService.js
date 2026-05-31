import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS registrationService] ${path} — error:`, err?.message || err);

export const startRegistration = (formData) =>
  callService('startRegistration', null, () =>
    apiClient
      .post('/registration/start', formData)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('startRegistration', 'POST /registration/start', err);
        throw err;
      })
  );

export const saveStep = (stepNumber, applicationId, data) =>
  callService('saveStep', null, () =>
    apiClient
      .post('/registration/step', { stepNumber, applicationId, data })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('saveStep', 'POST /registration/step', err);
        throw err;
      })
  );

export const submitRegistration = (applicationId) =>
  callService('submitRegistration', null, () =>
    apiClient
      .post(`/registration/${applicationId}/submit`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('submitRegistration', `POST /registration/${applicationId}/submit`, err);
        throw err;
      })
  );

export const getRegistrationStatus = (applicationId) =>
  callService('getRegistrationStatus', null, () =>
    apiClient
      .get(`/registration/${applicationId}/status`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getRegistrationStatus', `GET /registration/${applicationId}/status`, err);
        throw err;
      })
  );
