import { callService } from './serviceBase';
import apiClient from './apiClient';
import { DEFAULT_CLIENT_ID } from '../config/apiConfig';
import {
  mapLoginResponse,
  mapOtpRequestResponse,
  mapOtpVerifyResponse,
  mapSessionUser,
  rootUrl,
  unwrapEnvelope,
} from './apiTransforms';

const USER_STORAGE_KEY = 'amms_user';
const OTP_SENT_PREFIX = 'amms_otp_sent_';

function persistUser(user) {
  if (user) {
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
    if (user.role) {
      localStorage.setItem('userRole', user.role);
    }
  }
}

function readCachedUser() {
  try {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    if (raw) {
      return JSON.parse(raw);
    }
  } catch {
    /* ignore */
  }
  const role = localStorage.getItem('userRole');
  if (role) {
    return {
      id: '',
      name: '',
      email: '',
      role,
    };
  }
  return null;
}

export const adminLogin = (email, password) => {
  const normalizedEmail = email?.toLowerCase()?.trim() || '';
  const aliasMap = {
    'admin@chamber.com': 'seed.admin',
    'finance@chamber.com': 'seed.finance',
    'operator@chamber.com': 'seed.operator',
  };
  const userName = aliasMap[normalizedEmail] ?? email;

  return callService(
    'adminLogin',
    null,
    () =>
      apiClient
        .post('/auth/admin/login', {
          clientId: DEFAULT_CLIENT_ID,
          userName,
          password,
        })
        .then((res) => {
          const mapped = mapLoginResponse(res.data);
          localStorage.setItem('token', mapped.token);
          persistUser(mapped.user);
          return { ...res, data: mapped };
        })
  );
};

const sendOtp = (phone) =>
  apiClient
    .post(rootUrl('/otp/send'), { mobileNumber: phone }, { baseURL: '' })
    .then((res) => {
      localStorage.setItem(`${OTP_SENT_PREFIX}${phone}`, '1');
      return { ...res, data: mapOtpRequestResponse(res.data) };
    });

export const requestOTP = (phone) => {
  if (localStorage.getItem(`${OTP_SENT_PREFIX}${phone}`)) {
    return resendOTP(phone);
  }
  return callService('requestOTP', null, () => sendOtp(phone));
};

const tryResendOtp = (phone) =>
  apiClient
    .post('/auth/otp/resend', { mobileNumber: phone })
    .catch((err) => {
      if (err.response?.status === 404) {
        return apiClient.post(rootUrl('/auth/otp/resend'), { mobileNumber: phone }, {
          baseURL: '',
        });
      }
      throw err;
    })
    .then((res) => ({ ...res, data: mapOtpRequestResponse(res.data) }));

export const resendOTP = (phone) => {
  return callService('resendOTP', null, () =>
    tryResendOtp(phone).catch((err) => {
      if (err.response?.status === 404) {
        return sendOtp(phone);
      }
      throw err;
    })
  );
};

export const verifyOTP = (phone, otp) => {
  return callService(
    'verifyOTP',
    null,
    () =>
      apiClient
        .post(
          rootUrl('/otp/verify'),
          { mobileNumber: phone, otp, useOtp: true },
          { baseURL: '' }
        )
        .then((res) => {
          const mapped = mapOtpVerifyResponse(res.data);
          localStorage.setItem('token', mapped.token);
          persistUser(mapped.user);
          return { ...res, data: mapped };
        })
  );
};

export const logout = () => {
  return callService(
    'logout',
    null,
    () =>
      apiClient
        .post(rootUrl('/auth/logout'), null, { baseURL: '' })
        .then((res) => {
          localStorage.removeItem(USER_STORAGE_KEY);
          return { ...res, data: { success: true } };
        })
  );
};

export const refreshToken = (refreshTokenValue) => {
  return callService(
    'refreshToken',
    null,
    () =>
      apiClient
        .post('/auth/refresh-token', { refreshToken: refreshTokenValue })
        .then((res) => {
          const d = unwrapEnvelope(res.data);
          return {
            ...res,
            data: {
              token: d?.accessToken || d?.token,
              expiresIn: 3600,
            },
          };
        })
  );
};

export const getCurrentUser = () => {
  return callService(
    'getCurrentUser',
    null,
    () =>
      apiClient
        .get('/auth/session')
        .then((res) => {
          const user = mapSessionUser(res.data);
          persistUser(user);
          return { ...res, data: user };
        })
        .catch((err) => {
          const status = err.response?.status;
          if (status === 404) {
            console.warn(
              '[AMMS] /auth/session 404 — using cached user from localStorage'
            );
          }
          const cached = readCachedUser();
          if (cached) {
            return {
              data: cached,
              status: 200,
              statusText: 'OK',
              headers: {},
              config: err.config || {},
            };
          }
          return Promise.reject(err);
        })
  );
};

export const resetPassword = (email) => {
  return callService(
    'resetPassword',
    null,
    () =>
      apiClient
        .post(rootUrl('/auth/admin/password-reset'), { email }, { baseURL: '' })
        .then((res) => ({
          ...res,
          data: {
            success: true,
            message:
              unwrapEnvelope(res.data)?.message ||
              'If the account exists, password reset instructions have been sent.',
          },
        }))
  );
};

export const confirmPasswordReset = (token, newPassword) => {
  return callService(
    'confirmPasswordReset',
    null,
    () =>
      apiClient
        .post(
          rootUrl('/auth/admin/password-change'),
          { token, newPassword },
          { baseURL: '' }
        )
        .then((res) => ({ ...res, data: { success: true } }))
  );
};
