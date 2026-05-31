import { http, HttpResponse, delay } from 'msw'
import { paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

const AUDIT_LOGS = [
  { id: 'LOG-001', staffName: 'Admin User', staffRole: 'admin', action: 'MEMBER_APPROVED', target: 'MEM-002', targetType: 'Member', timestamp: '2024-01-15T10:30:00Z', ip: '192.168.1.1', changes: { status: { from: 'PENDING', to: 'APPROVED' } } },
  { id: 'LOG-002', staffName: 'Admin User', staffRole: 'admin', action: 'MEMBER_REJECTED', target: 'MEM-007', targetType: 'Member', timestamp: '2024-01-14T14:00:00Z', ip: '192.168.1.1', changes: { status: { from: 'APPLIED', to: 'REJECTED' } } },
  { id: 'LOG-003', staffName: 'Bursar Singh', staffRole: 'finance', action: 'PAYMENT_REFUNDED', target: 'PAY-001', targetType: 'Payment', timestamp: '2024-01-13T11:00:00Z', ip: '10.0.0.5', changes: { status: { from: 'SUCCESS', to: 'REFUNDED' } } },
  { id: 'LOG-004', staffName: 'Admin User', staffRole: 'admin', action: 'STAFF_CREATED', target: 'STAFF-003', targetType: 'Staff', timestamp: '2024-01-12T09:00:00Z', ip: '192.168.1.1', changes: { name: 'Front Desk Patel', role: 'operator' } },
  { id: 'LOG-005', staffName: 'Admin User', staffRole: 'admin', action: 'SETTINGS_UPDATED', target: 'SETTINGS', targetType: 'Settings', timestamp: '2024-01-11T16:00:00Z', ip: '192.168.1.1', changes: { yearlyFee: { from: 800, to: 1000 } } },
]

export const auditHandlers = [
  // GET /api/v1/audit-logs/export — MUST be before /audit-logs
  http.get(`${BASE}/api/v1/audit-logs/export`, async () => {
    await delay(800)
    return HttpResponse.json({ downloadUrl: '#mock', fileName: 'audit.csv' })
  }),

  // GET /api/v1/audit-logs
  http.get(`${BASE}/api/v1/audit-logs`, async ({ request }) => {
    await delay(getDelay('audit'))
    if (shouldFail('audit')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const staffId = url.searchParams.get('staffId') || ''
    const action = url.searchParams.get('action') || ''
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let logs = [...AUDIT_LOGS]
    if (staffId) logs = logs.filter(l => l.staffName.toLowerCase().includes(staffId.toLowerCase()))
    if (action) logs = logs.filter(l => l.action === action)
    return HttpResponse.json(paginate(logs, page, limit))
  }),
]
