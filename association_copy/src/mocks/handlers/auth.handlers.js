import { http, HttpResponse, delay } from 'msw'
import { mockState } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const authHandlers = [
  // POST /api/v1/auth/login
  http.post(`${BASE}/api/v1/auth/login`, async ({ request }) => {
    await delay(getDelay('auth'))
    if (shouldFail('auth')) {
      return HttpResponse.json({ message: 'Invalid credentials' }, { status: 401 })
    }
    const body = await request.json()
    const { email, password } = body

    const accounts = {
      'admin@chamber.com': { password: 'admin123', token: 'mock-jwt-admin', role: 'admin', id: 'STAFF-001', name: 'Admin User' },
      'finance@chamber.com': { password: 'finance123', token: 'mock-jwt-finance', role: 'finance', id: 'STAFF-002', name: 'Bursar Singh' },
      'operator@chamber.com': { password: 'operator123', token: 'mock-jwt-operator', role: 'operator', id: 'STAFF-003', name: 'Front Desk Patel' },
    }

    const match = accounts[email]
    if (!match || match.password !== password) {
      return HttpResponse.json({ message: 'Invalid credentials' }, { status: 401 })
    }

    return HttpResponse.json({
      token: match.token,
      user: { id: match.id, name: match.name, email, role: match.role },
    })
  }),

  // POST /api/v1/auth/otp/request
  http.post(`${BASE}/api/v1/auth/otp/request`, async () => {
    await delay(getDelay('auth'))
    if (shouldFail('auth')) {
      return HttpResponse.json({ message: 'Phone not found' }, { status: 404 })
    }
    return HttpResponse.json({ success: true, expiresIn: 60, maskedPhone: '+91 XXXXX X1234' })
  }),

  // POST /api/v1/auth/otp/verify
  http.post(`${BASE}/api/v1/auth/otp/verify`, async ({ request }) => {
    await delay(getDelay('auth'))
    if (shouldFail('auth')) {
      return HttpResponse.json({ message: 'Invalid OTP' }, { status: 400 })
    }
    const body = await request.json()
    const isNewUser = body.otp === '000000'
    return HttpResponse.json({
      token: 'mock-member-token',
      isNewUser,
      user: { id: 'MEM-001', name: 'Ramesh Kumar', role: 'member' },
    })
  }),

  // GET /api/v1/auth/me
  http.get(`${BASE}/api/v1/auth/me`, ({ request }) => {
    const auth = request.headers.get('Authorization')
    if (!auth || !auth.startsWith('Bearer ')) {
      return HttpResponse.json({ message: 'Unauthorized' }, { status: 401 })
    }
    return HttpResponse.json(mockState.staff[0])
  }),

  // POST /api/v1/auth/logout
  http.post(`${BASE}/api/v1/auth/logout`, () => {
    return HttpResponse.json({ success: true })
  }),

  // POST /api/v1/auth/password/reset
  http.post(`${BASE}/api/v1/auth/password/reset`, async () => {
    await delay(getDelay('auth'))
    if (shouldFail('auth')) {
      return HttpResponse.json({ message: 'Email not found' }, { status: 404 })
    }
    return HttpResponse.json({ success: true, message: 'Reset link sent' })
  }),

  // POST /api/v1/auth/password/confirm
  http.post(`${BASE}/api/v1/auth/password/confirm`, async () => {
    await delay(getDelay('auth'))
    return HttpResponse.json({ success: true, message: 'Password updated' })
  }),

  // POST /api/v1/auth/refresh
  http.post(`${BASE}/api/v1/auth/refresh`, () => {
    return HttpResponse.json({ token: 'mock-jwt-refreshed' })
  }),
]
