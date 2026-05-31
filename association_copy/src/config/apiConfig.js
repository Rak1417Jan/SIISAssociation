export const USE_MOCK_DATA = false;

export const BASE_URL =
  import.meta.env.VITE_BASE_URL ||
  'https://amms-api-866440119101.asia-south1.run.app';

/** Canonical API host — use in services via apiClient, not hardcoded URLs */
export const API_BASE_URL = BASE_URL;

export const API_VERSION = 'v1';
export const API_TIMEOUT = 10000;
export const DEFAULT_CLIENT_ID = Number(import.meta.env.VITE_CLIENT_ID) || 1;
export const RAZORPAY_KEY_ID =
  import.meta.env.VITE_RAZORPAY_KEY_ID || 'rzp_test_placeholder';

export const API_ROOT = `${API_BASE_URL}/api/${API_VERSION}`;
