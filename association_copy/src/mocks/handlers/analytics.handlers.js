import { http, HttpResponse, delay } from 'msw'
import { mockState } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const analyticsHandlers = [
  // GET /api/v1/analytics/dashboard
  http.get(`${BASE}/api/v1/analytics/dashboard`, async () => {
    await delay(getDelay('analytics'))
    if (shouldFail('analytics')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const pendingApprovals = mockState.members.filter(
      m => m.status === 'PENDING' || m.status === 'APPLIED'
    ).length
    const activeMembers = mockState.members.filter(m => m.status === 'APPROVED').length
    return HttpResponse.json({
      totalMembers: mockState.members.length,
      pendingApprovals,
      activeMembers,
      totalFirms: mockState.firms.length,
      approvedThisMonth: 34,
      rejectedThisMonth: 5,
      totalRevenue: 284500,
      revenueThisMonth: 43200,
      expiringSoon: 23,
      oldestPendingDays: 9,
      newMembersToday: 3,
    })
  }),

  // GET /api/v1/analytics/pending
  http.get(`${BASE}/api/v1/analytics/pending`, async () => {
    await delay(getDelay('analytics'))
    const pending = mockState.members
      .filter(m => m.status === 'PENDING' || m.status === 'APPLIED')
      .sort((a, b) => new Date(a.appliedAt) - new Date(b.appliedAt))
    return HttpResponse.json({ data: pending, total: pending.length })
  }),

  // GET /api/v1/analytics/members
  http.get(`${BASE}/api/v1/analytics/members`, async () => {
    await delay(getDelay('analytics'))
    return HttpResponse.json({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      newRegistrations: [12, 19, 8, 25, 22, 30],
      approvals: [10, 15, 7, 20, 18, 25],
      rejections: [2, 4, 1, 5, 4, 5],
    })
  }),

  // GET /api/v1/analytics/revenue
  http.get(`${BASE}/api/v1/analytics/revenue`, async () => {
    await delay(getDelay('analytics'))
    return HttpResponse.json({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      totalRevenue: [28000, 42000, 31000, 55000, 48000, 62000],
      gstCollected: [5040, 7560, 5580, 9900, 8640, 11160],
      platformFees: [1400, 2100, 1550, 2750, 2400, 3100],
    })
  }),

  // GET /api/v1/analytics/registrations
  http.get(`${BASE}/api/v1/analytics/registrations`, async () => {
    await delay(getDelay('analytics'))
    return HttpResponse.json({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      registrations: [12, 19, 8, 25, 22, 30],
    })
  }),

  // GET /api/v1/analytics/firms
  http.get(`${BASE}/api/v1/analytics/firms`, async () => {
    await delay(getDelay('analytics'))
    return HttpResponse.json({
      totalFirms: mockState.firms.length,
      byIndustry: {
        Trading: 1, Manufacturing: 1, Logistics: 1, Textiles: 1, Retail: 1,
      },
    })
  }),

  // GET /api/v1/analytics/member/:id/status  — MUST be before /analytics/members
  http.get(`${BASE}/api/v1/analytics/member/:id/status`, ({ params }) => {
    return HttpResponse.json({
      applicationId: 'APP-001',
      progressPercent: 60,
      estimatedDays: 2,
      steps: [
        { step: 'Application Submitted', done: true, date: '2024-01-10' },
        { step: 'Documents Uploaded', done: true, date: '2024-01-11' },
        { step: 'Admin Review', done: false, date: null },
        { step: 'Approval & ID Generation', done: false, date: null },
      ],
    })
  }),

  // POST /api/v1/analytics/export
  http.post(`${BASE}/api/v1/analytics/export`, async () => {
    await delay(800)
    return HttpResponse.json({ downloadUrl: '#mock', fileName: 'report.csv' })
  }),
]
