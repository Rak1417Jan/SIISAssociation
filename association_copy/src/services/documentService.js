import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS documentService] ${path} — error:`, err?.message || err);

export const uploadDocument = (file, type, applicationId) => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('type', type);
  formData.append('applicationId', applicationId);

  return callService('uploadDocument', null, () =>
    apiClient
      .post('/documents/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('uploadDocument', 'POST /documents/upload', err);
        throw err;
      })
  );
};

export const aiVerifyDocument = (documentId) =>
  callService('aiVerifyDocument', null, () =>
    apiClient
      .post(`/documents/${documentId}/ai-verify`)
      .then((res) => {
        const d = unwrapEnvelope(res.data) || {};
        return {
          ...res,
          data: {
            documentId,
            isValid: d.isValid ?? false,
            confidence: d.confidence ?? 0,
            checks: d.checks || {},
            reason: d.reason || '',
          },
        };
      })
      .catch((err) => {
        logErr('aiVerifyDocument', `POST /documents/${documentId}/ai-verify`, err);
        throw err;
      })
  );

export const getDocumentStatus = (applicationId) =>
  callService('getDocumentStatus', null, () =>
    apiClient
      .get('/documents', { params: { applicationId } })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getDocumentStatus', 'GET /documents', err);
        throw err;
      })
  );

export const verifyDocument = (documentId, status, remarks) =>
  callService('verifyDocument', null, () =>
    apiClient
      .patch(`/documents/${documentId}/verify`, { status, remarks })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('verifyDocument', `PATCH /documents/${documentId}/verify`, err);
        throw err;
      })
  );

export const getDocumentTypes = () =>
  callService('getDocumentTypes', null, () =>
    apiClient
      .get('/documents/types')
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getDocumentTypes', 'GET /documents/types', err);
        throw err;
      })
  );
