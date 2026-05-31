import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const broadcastHandlers = [
  // GET /api/v1/broadcasts
  http.get(`${BASE}/api/v1/broadcasts`, async ({ request }) => {
    await delay(getDelay('broadcasts'))
    if (shouldFail('broadcasts')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const status = url.searchParams.get('status') || ''
    const channel = url.searchParams.get('channel') || ''
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let broadcasts = [...mockState.broadcasts]
    if (status) broadcasts = broadcasts.filter(b => b.status === status)
    if (channel) broadcasts = broadcasts.filter(b => b.channel === channel)
    return HttpResponse.json(paginate(broadcasts, page, limit))
  }),

  // POST /api/v1/broadcasts — STATEFUL
  http.post(`${BASE}/api/v1/broadcasts`, async ({ request }) => {
    await delay(getDelay('broadcasts'))
    const body = await request.json()
    const newBroadcast = {
      id: 'BC-' + Date.now(),
      ...body,
      status: 'DRAFT',
      recipientCount: 0,
      sentAt: null,
      createdAt: new Date().toISOString(),
    }
    mockState.broadcasts.push(newBroadcast)
    return HttpResponse.json({ success: true, broadcast: newBroadcast }, { status: 201 })
  }),

  // GET /api/v1/broadcasts/:id/stats — MUST be before /:id
  http.get(`${BASE}/api/v1/broadcasts/:id/stats`, () => {
    return HttpResponse.json({ sent: 248, delivered: 241, failed: 7, deliveryRate: '97.2%' })
  }),

  // POST /api/v1/broadcasts/:id/send — STATEFUL
  http.post(`${BASE}/api/v1/broadcasts/:id/send`, async ({ params }) => {
    await delay(getDelay('broadcasts'))
    const broadcast = mockState.broadcasts.find(b => b.id === params.id)
    if (broadcast) {
      broadcast.status = 'SENDING'
      // Simulate async completion after 1 second
      setTimeout(() => { broadcast.status = 'SENT'; broadcast.sentAt = new Date().toISOString() }, 1000)
    }
    return HttpResponse.json({ success: true, status: 'SENDING', recipientCount: 248 })
  }),

  // POST /api/v1/broadcasts/:id/schedule — STATEFUL
  http.post(`${BASE}/api/v1/broadcasts/:id/schedule`, async ({ request, params }) => {
    await delay(300)
    const body = await request.json()
    const broadcast = mockState.broadcasts.find(b => b.id === params.id)
    if (broadcast) {
      broadcast.status = 'SCHEDULED'
      broadcast.scheduledAt = body.scheduledAt || new Date().toISOString()
    }
    return HttpResponse.json({ success: true, status: 'SCHEDULED' })
  }),

  // PATCH /api/v1/broadcasts/:id/cancel — STATEFUL
  http.patch(`${BASE}/api/v1/broadcasts/:id/cancel`, async ({ params }) => {
    await delay(300)
    const broadcast = mockState.broadcasts.find(b => b.id === params.id)
    if (broadcast) broadcast.status = 'CANCELLED'
    return HttpResponse.json({ success: true, status: 'CANCELLED' })
  }),
]
