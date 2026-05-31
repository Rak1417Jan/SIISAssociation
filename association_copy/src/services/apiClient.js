import axios from 'axios';
import { API_BASE_URL, API_VERSION, API_TIMEOUT } from '../config/apiConfig';

const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api/${API_VERSION}`,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

const PUBLIC_PATHS = ['/digital-id/verify/'];

apiClient.interceptors.request.use(
  (config) => {
    const url = config.url || '';
    const isPublic = config.skipAuth || PUBLIC_PATHS.some((p) => url.includes(p));
    if (!isPublic) {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const url = error.config?.url || '';
    const isSessionCall = url.includes('/auth/session');

    if (error.response) {
      if (error.response.status === 401 && !isSessionCall) {
        localStorage.clear();
        window.location.href = '/login';
      } else if (error.response.status >= 500) {
        console.error('[API Server Error]', error.response.status, error.message);
      }
    } else {
      console.error('[API Network/Config Error]', error.message);
    }
    return Promise.reject(error);
  }
);

export default apiClient;
