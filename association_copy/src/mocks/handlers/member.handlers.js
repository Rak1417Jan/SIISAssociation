import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate, filterMembers } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const memberHandlers = [
  // GET /api/v1/members/search  — MUST be before /members/:id
  http.get(`${BASE}/api/v1/members/search`, ({ request }) => {
    const url = new URL(request.url)
    const q = (url.searchParams.get('q') || '').toLowerCase()
    const result = mockState.members.filter(m =>
      m.name.toLowerCase().includes(q) ||
      m.phone.includes(q) ||
      m.id.toLowerCase().includes(q)
    )
    return HttpResponse.json({ data: result, total: result.length })
  }),

  // GET /api/v1/members/export  — MUST be before /members/:id
  http.get(`${BASE}/api/v1/members/export`, async () => {
    await delay(800)
    return HttpResponse.json({ downloadUrl: '#mock', fileName: 'members.csv' })
  }),

  // GET /api/v1/members
  http.get(`${BASE}/api/v1/members`, async ({ request }) => {
    await delay(getDelay('members'))
    if (shouldFail('members')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const status = url.searchParams.get('status') || ''
    const planType = url.searchParams.get('planType') || ''
    const q = url.searchParams.get('q') || ''
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    const filtered = filterMembers({ status, planType, search: q })
    return HttpResponse.json(paginate(filtered, page, limit))
  }),

  // GET /api/v1/members/:id
  http.get(`${BASE}/api/v1/members/:id`, async ({ params }) => {
    await delay(getDelay('members'))
    if (shouldFail('memberDetail')) {
      return HttpResponse.json({ message: 'Member not found' }, { status: 404 })
    }
    const member = mockState.members.find(m => m.id === params.id)
    if (!member) return HttpResponse.json({ message: 'Not found' }, { status: 404 })

    const expiry = new Date()
    expiry.setFullYear(expiry.getFullYear() + 1)

    return HttpResponse.json({
      ...member,
      documents: [
        { type: 'AADHAR', status: 'VERIFIED', aiVerified: true, aiConfidence: 0.94 },
        { type: 'PHOTO', status: 'PENDING', aiVerified: false, aiConfidence: 0 },
      ],
      paymentHistory: mockState.payments.filter(p => p.memberId === params.id),
      membershipExpiry: member.status === 'APPROVED' ? expiry.toISOString() : null,
    })
  }),

  // PATCH /api/v1/members/:id/approve — STATEFUL
  http.patch(`${BASE}/api/v1/members/:id/approve`, async ({ params }) => {
    await delay(getDelay('members'))
    const member = mockState.members.find(m => m.id === params.id)
    if (!member) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    member.status = 'APPROVED'
    member.daysWaiting = 0
    return HttpResponse.json({
      success: true, status: 'APPROVED',
      approvedAt: new Date().toISOString(),
      digitalIdGenerated: true, notificationSent: true,
    })
  }),

  // PATCH /api/v1/members/:id/reject — STATEFUL
  http.patch(`${BASE}/api/v1/members/:id/reject`, async ({ request, params }) => {
    await delay(getDelay('members'))
    const body = await request.json()
    const reason = body.reason || 'Rejected by admin'
    const member = mockState.members.find(m => m.id === params.id)
    if (!member) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    member.status = 'REJECTED'
    member.adminNotes = reason
    return HttpResponse.json({ success: true, status: 'REJECTED', rejectionReason: reason, notificationSent: true })
  }),

  // PATCH /api/v1/members/:id/suspend — STATEFUL
  http.patch(`${BASE}/api/v1/members/:id/suspend`, async ({ params }) => {
    await delay(getDelay('members'))
    const member = mockState.members.find(m => m.id === params.id)
    if (!member) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    member.status = 'SUSPENDED'
    return HttpResponse.json({ success: true, status: 'SUSPENDED' })
  }),

  // PATCH /api/v1/members/:id/reactivate — STATEFUL
  http.patch(`${BASE}/api/v1/members/:id/reactivate`, async ({ params }) => {
    await delay(getDelay('members'))
    const member = mockState.members.find(m => m.id === params.id)
    if (!member) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    member.status = 'APPROVED'
    return HttpResponse.json({ success: true, status: 'APPROVED' })
  }),

  // GET /api/v1/members/:id/history
  http.get(`${BASE}/api/v1/members/:id/history`, ({ params }) => {
    return HttpResponse.json([
      { step: 'Application Submitted', date: '2024-01-10', status: 'DONE', note: 'Application received' },
      { step: 'Documents Verified', date: '2024-01-12', status: 'DONE', note: 'AI confidence 94%' },
      { step: 'Admin Approved', date: '2024-01-15', status: 'DONE', note: 'Approved by Admin User' },
    ])
  }),
]
