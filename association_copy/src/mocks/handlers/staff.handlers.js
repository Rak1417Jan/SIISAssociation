import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const staffHandlers = [
  // GET /api/v1/roles/permissions  — MUST be before /roles/:id
  http.get(`${BASE}/api/v1/roles/permissions`, () => {
    return HttpResponse.json({
      super_admin: {
        members: { view: true, create: true, approve: true, delete: true, export: true, manage: true },
        payments: { view: true, create: true, approve: true, delete: true, export: true, manage: true },
        staff: { view: true, create: true, approve: true, delete: true, export: true, manage: true },
        settings: { view: true, create: true, approve: true, delete: true, export: true, manage: true },
        analytics: { view: true, create: false, approve: false, delete: false, export: true, manage: true },
        audit: { view: true, create: false, approve: false, delete: false, export: true, manage: true },
      },
      admin: {
        members: { view: true, create: true, approve: true, delete: false, export: true, manage: false },
        payments: { view: true, create: false, approve: false, delete: false, export: true, manage: false },
        staff: { view: true, create: false, approve: false, delete: false, export: false, manage: false },
        settings: { view: true, create: false, approve: false, delete: false, export: false, manage: false },
        analytics: { view: true, create: false, approve: false, delete: false, export: true, manage: false },
        audit: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
      },
      finance: {
        members: { view: true, create: false, approve: false, delete: false, export: true, manage: false },
        payments: { view: true, create: false, approve: false, delete: false, export: true, manage: false },
        staff: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        settings: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        analytics: { view: true, create: false, approve: false, delete: false, export: true, manage: false },
        audit: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
      },
      operator: {
        members: { view: true, create: false, approve: false, delete: false, export: false, manage: false },
        payments: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        staff: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        settings: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        analytics: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
        audit: { view: false, create: false, approve: false, delete: false, export: false, manage: false },
      },
    })
  }),

  // GET /api/v1/roles
  http.get(`${BASE}/api/v1/roles`, () => {
    return HttpResponse.json([
      { id: 'super_admin', label: 'Super Admin', description: 'Full system access' },
      { id: 'admin', label: 'Admin', description: 'Member and application management' },
      { id: 'finance', label: 'Finance', description: 'Payment and revenue access' },
      { id: 'operator', label: 'Operator', description: 'View-only member access' },
    ])
  }),

  // PUT /api/v1/roles/:id/permissions
  http.put(`${BASE}/api/v1/roles/:id/permissions`, async () => {
    await delay(300)
    return HttpResponse.json({ success: true })
  }),

  // GET /api/v1/staff/:id/activity  — MUST be before /staff/:id
  http.get(`${BASE}/api/v1/staff/:id/activity`, ({ params }) => {
    return HttpResponse.json([
      { action: 'MEMBER_APPROVED', target: 'MEM-002', timestamp: '2024-01-15T10:00:00Z', ip: '192.168.1.1' },
      { action: 'BROADCAST_SENT', target: 'BC-001', timestamp: '2024-01-14T09:00:00Z', ip: '192.168.1.1' },
      { action: 'SETTINGS_UPDATED', target: 'SETTINGS', timestamp: '2024-01-13T11:00:00Z', ip: '192.168.1.1' },
    ])
  }),

  // GET /api/v1/staff/:id/reset-password — before /:id
  http.post(`${BASE}/api/v1/staff/:id/reset-password`, async () => {
    await delay(300)
    return HttpResponse.json({ success: true, message: 'Reset email sent' })
  }),

  // GET /api/v1/staff
  http.get(`${BASE}/api/v1/staff`, async ({ request }) => {
    await delay(getDelay('staff'))
    if (shouldFail('staff')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const role = url.searchParams.get('role') || ''
    const isActiveParam = url.searchParams.get('isActive')
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let staff = [...mockState.staff]
    if (role) staff = staff.filter(s => s.role === role)
    if (isActiveParam !== null && isActiveParam !== '')
      staff = staff.filter(s => String(s.isActive) === isActiveParam)
    return HttpResponse.json(paginate(staff, page, limit))
  }),

  // GET /api/v1/staff/:id
  http.get(`${BASE}/api/v1/staff/:id`, ({ params }) => {
    const s = mockState.staff.find(s => s.id === params.id)
    if (!s) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    return HttpResponse.json(s)
  }),

  // POST /api/v1/staff — STATEFUL
  http.post(`${BASE}/api/v1/staff`, async ({ request }) => {
    await delay(getDelay('staff'))
    const body = await request.json()
    const newStaff = { id: 'STAFF-' + Date.now(), ...body, isActive: true, createdAt: new Date().toISOString() }
    mockState.staff.push(newStaff)
    return HttpResponse.json({ success: true, staff: newStaff, temporaryPassword: 'Temp@1234' }, { status: 201 })
  }),

  // PUT /api/v1/staff/:id — STATEFUL
  http.put(`${BASE}/api/v1/staff/:id`, async ({ request, params }) => {
    await delay(getDelay('staff'))
    const body = await request.json()
    const idx = mockState.staff.findIndex(s => s.id === params.id)
    if (idx === -1) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    mockState.staff[idx] = { ...mockState.staff[idx], ...body }
    return HttpResponse.json({ success: true, staff: mockState.staff[idx] })
  }),

  // PATCH /api/v1/staff/:id/deactivate — STATEFUL
  http.patch(`${BASE}/api/v1/staff/:id/deactivate`, async ({ params }) => {
    await delay(getDelay('staff'))
    const s = mockState.staff.find(s => s.id === params.id)
    if (!s) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    s.isActive = false
    return HttpResponse.json({ success: true })
  }),

  // PATCH /api/v1/staff/:id/reactivate — STATEFUL
  http.patch(`${BASE}/api/v1/staff/:id/reactivate`, async ({ params }) => {
    await delay(getDelay('staff'))
    const s = mockState.staff.find(s => s.id === params.id)
    if (!s) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    s.isActive = true
    return HttpResponse.json({ success: true })
  }),

  // PATCH /api/v1/staff/:id/role — STATEFUL
  http.patch(`${BASE}/api/v1/staff/:id/role`, async ({ request, params }) => {
    await delay(getDelay('staff'))
    const body = await request.json()
    const s = mockState.staff.find(s => s.id === params.id)
    if (!s) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    s.role = body.role
    return HttpResponse.json({ success: true, role: body.role })
  }),
]
