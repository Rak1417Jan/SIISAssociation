import { unwrapEnvelope } from './apiTransforms';

/**
 * @param {string} endpoint
 * @param {any} _mockData
 * @param {Function} axiosFn
 * @returns {Promise<{data: any, error: any, status: number}>}
 */
export async function callService(endpoint, _mockData, axiosFn) {
  try {
    const response = await axiosFn();
    const payload = unwrapEnvelope(response.data);
    console.log('[AMMS API SUCCESS]', endpoint, payload);
    return {
      data: payload,
      error: null,
      status: response.status,
    };
  } catch (error) {
    const status = error.response?.status || 0;
    const body = error.response?.data;
    const message =
      body?.errorMessage ||
      body?.message ||
      (typeof body === 'string' ? body : null) ||
      error.message ||
      'Something went wrong';
    console.error(`[AMMS ${endpoint}] — error:`, message);
    return {
      data: null,
      error: message,
      status,
    };
  }
}
