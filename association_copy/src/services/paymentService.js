import { callService } from './serviceBase';
import apiClient from './apiClient';
import { RAZORPAY_KEY_ID } from '../config/apiConfig';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (fn, path, err) =>
  console.error(`[AMMS paymentService] ${path} — error:`, err?.message || err);

const mapPlan = (p) => ({
  id: String(p.id ?? p.planId),
  name: p.name || p.planName || '',
  duration: p.duration ?? p.durationMonths ?? null,
  price: p.price ?? p.baseAmount ?? 0,
  gstPercent: p.gstPercent ?? 18,
  gstAmount: p.gstAmount ?? 0,
  platformFee: p.platformFee ?? p.platformFeeFlat ?? 0,
  totalAmount: p.totalAmount ?? p.price ?? 0,
  features: p.features || [],
});

const mapOrder = (d) => ({
  orderId: d.orderId || d.razorpayOrderId || '',
  amount: d.amount ?? 0,
  currency: d.currency || 'INR',
  keyId: d.keyId || RAZORPAY_KEY_ID,
  prefill: d.prefill || {},
});

export const getMembershipPlans = () =>
  callService('getMembershipPlans', null, () =>
    apiClient
      .get('/plans')
      .then((res) => {
        const raw = unwrapEnvelope(res.data);
        const list = Array.isArray(raw) ? raw : raw?.data || raw?.records || [];
        return { ...res, data: list.map(mapPlan) };
      })
      .catch((err) => {
        logErr('getMembershipPlans', 'GET /plans', err);
        throw err;
      })
  );

export const createRazorpayOrder = (planId, memberId) =>
  callService('createRazorpayOrder', null, () =>
    apiClient
      .post('/payments/create-order', { planId, memberId })
      .then((res) => ({ ...res, data: mapOrder(unwrapEnvelope(res.data) || {}) }))
      .catch((err) => {
        logErr('createRazorpayOrder', 'POST /payments/create-order', err);
        throw err;
      })
  );

export const verifyPayment = (razorpayOrderId, razorpayPaymentId, razorpaySignature) =>
  callService('verifyPayment', null, () =>
    apiClient
      .post('/payments/verify', { razorpayOrderId, razorpayPaymentId, razorpaySignature })
      .then((res) => {
        const d = unwrapEnvelope(res.data) || {};
        return {
          ...res,
          data: {
            success: d.success !== false,
            paymentId: d.paymentId || d.id,
            status: d.status || 'SUCCESS',
            receiptNo: d.receiptNo,
            receiptUrl: d.receiptUrl,
          },
        };
      })
      .catch((err) => {
        logErr('verifyPayment', 'POST /payments/verify', err);
        throw err;
      })
  );

export const getPaymentHistory = (memberId, filters = {}, page = 1, limit = 10) =>
  callService('getPaymentHistory', null, () =>
    apiClient
      .get('/payments/history', { params: { memberId, page, limit, ...filters } })
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
        logErr('getPaymentHistory', 'GET /payments/history', err);
        throw err;
      })
  );

export const getPaymentById = (paymentId) =>
  callService('getPaymentById', null, () =>
    apiClient
      .get(`/payments/${paymentId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getPaymentById', `GET /payments/${paymentId}`, err);
        throw err;
      })
  );

export const generateReceipt = (paymentId) =>
  callService('generateReceipt', null, () =>
    apiClient
      .get(`/payments/${paymentId}/receipt`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('generateReceipt', `GET /payments/${paymentId}/receipt`, err);
        throw err;
      })
  );

export const getPaymentSummary = (memberId) =>
  callService('getPaymentSummary', null, () =>
    apiClient
      .get(`/payments/summary/${memberId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getPaymentSummary', `GET /payments/summary/${memberId}`, err);
        throw err;
      })
  );

export const processRenewal = (memberId, planId) =>
  callService('processRenewal', null, () =>
    apiClient
      .post('/payments/renewal', { memberId, planId })
      .then((res) => ({ ...res, data: mapOrder(unwrapEnvelope(res.data) || {}) }))
      .catch((err) => {
        logErr('processRenewal', 'POST /payments/renewal', err);
        throw err;
      })
  );

export const refundPayment = (paymentId, reason) =>
  callService('refundPayment', null, () =>
    apiClient
      .post(`/payments/${paymentId}/refund`, { reason })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('refundPayment', `POST /payments/${paymentId}/refund`, err);
        throw err;
      })
  );
