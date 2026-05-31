import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const firmHandlers = [
  // GET /api/v1/firms/search  — MUST be before /firms/:id
  http.get(`${BASE}/api/v1/firms/search`, ({ request }) => {
    const url = new URL(request.url)
    const q = (url.searchParams.get('q') || '').toLowerCase()
    const result = mockState.firms.filter(f =>
      f.name.toLowerCase().includes(q) ||
      f.registrationNo.toLowerCase().includes(q)
    )
    return HttpResponse.json({ data: result, total: result.length })
  }),

  // GET /api/v1/firms/export  — MUST be before /firms/:id
  http.get(`${BASE}/api/v1/firms/export`, async () => {
    await delay(800)
    return HttpResponse.json({ downloadUrl: '#mock', fileName: 'firms.csv' })
  }),

  // GET /api/v1/firms
  http.get(`${BASE}/api/v1/firms`, async ({ request }) => {
    await delay(getDelay('firms'))
    if (shouldFail('firms')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const q = (url.searchParams.get('q') || '').toLowerCase()
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let firms = [...mockState.firms]
    if (q) firms = firms.filter(f =>
      f.name.toLowerCase().includes(q) ||
      f.registrationNo.toLowerCase().includes(q)
    )
    return HttpResponse.json(paginate(firms, page, limit))
  }),

  // GET /api/v1/firms/:id
  http.get(`${BASE}/api/v1/firms/:id`, async ({ params }) => {
    await delay(getDelay('firms'))
    const firm = mockState.firms.find(f => f.id === params.id)
    if (!firm) return HttpResponse.json({ message: 'Firm not found' }, { status: 404 })
    const members = mockState.members.filter(m => m.firmId === params.id)
    return HttpResponse.json({ ...firm, members })
  }),

  // POST /api/v1/firms — STATEFUL
  http.post(`${BASE}/api/v1/firms`, async ({ request }) => {
    await delay(getDelay('firms'))
    const body = await request.json()
    const newFirm = { id: 'FIRM-' + Date.now(), ...body, memberCount: 0, status: 'ACTIVE', createdAt: new Date().toISOString() }
    mockState.firms.push(newFirm)
    return HttpResponse.json({ success: true, firm: newFirm }, { status: 201 })
  }),

  // PUT /api/v1/firms/:id — STATEFUL
  http.put(`${BASE}/api/v1/firms/:id`, async ({ request, params }) => {
    await delay(getDelay('firms'))
    const body = await request.json()
    const idx = mockState.firms.findIndex(f => f.id === params.id)
    if (idx === -1) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    mockState.firms[idx] = { ...mockState.firms[idx], ...body }
    return HttpResponse.json({ success: true, firm: mockState.firms[idx] })
  }),

  // DELETE /api/v1/firms/:id — STATEFUL
  http.delete(`${BASE}/api/v1/firms/:id`, async ({ params }) => {
    await delay(getDelay('firms'))
    const idx = mockState.firms.findIndex(f => f.id === params.id)
    if (idx !== -1) mockState.firms.splice(idx, 1)
    return HttpResponse.json({ success: true })
  }),

  // GET /api/v1/firms/:id/members
  http.get(`${BASE}/api/v1/firms/:id/members`, ({ request, params }) => {
    const url = new URL(request.url)
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    const members = mockState.members.filter(m => m.firmId === params.id)
    return HttpResponse.json(paginate(members, page, limit))
  }),

  // POST /api/v1/firms/:id/members
  http.post(`${BASE}/api/v1/firms/:id/members`, () => {
    return HttpResponse.json({ success: true, message: 'Member linked' })
  }),

  // DELETE /api/v1/firms/:id/members/:memberId
  http.delete(`${BASE}/api/v1/firms/:id/members/:memberId`, () => {
    return HttpResponse.json({ success: true, message: 'Member removed' })
  }),
]
