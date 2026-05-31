import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const engagementHandlers = [
  // ── DIRECTORY ────────────────────────────────────────────────
  http.get(`${BASE}/api/v1/directory/industries`, () => {
    return HttpResponse.json([
      'Trading', 'Manufacturing', 'Logistics', 'Textiles', 'Retail',
      'Pharmaceuticals', 'IT Services', 'Construction', 'Food & Beverages',
      'Healthcare', 'Education', 'Finance', 'Consulting',
    ])
  }),

  http.get(`${BASE}/api/v1/directory`, async ({ request }) => {
    await delay(getDelay('directory'))
    if (shouldFail('directory')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 12
    const search = (url.searchParams.get('search') || '').toLowerCase()
    const industry = url.searchParams.get('industry') || ''
    const planType = url.searchParams.get('planType') || ''

    let members = mockState.members.filter(m => m.status === 'APPROVED').map(m => ({
      ...m,
      photo: `https://api.dicebear.com/7.x/initials/svg?seed=${encodeURIComponent(m.name)}`,
      isVerified: true,
      city: 'Mumbai',
      memberSince: '2022',
      designation: 'Proprietor',
      industry: 'Trade',
      email: 'HIDDEN',
      phone: 'HIDDEN',
    }))
    if (search) members = members.filter(m => m.name.toLowerCase().includes(search) || m.firmName.toLowerCase().includes(search))
    if (planType) members = members.filter(m => m.planType === planType)
    return HttpResponse.json(paginate(members, page, limit))
  }),

  http.post(`${BASE}/api/v1/directory/connect`, async () => {
    await delay(400)
    return HttpResponse.json({ success: true, message: 'Connection request sent via WhatsApp' })
  }),

  // ── EVENTS ───────────────────────────────────────────────────
  http.get(`${BASE}/api/v1/events`, async ({ request }) => {
    await delay(getDelay('events'))
    if (shouldFail('events')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const status = url.searchParams.get('status') || ''
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let events = [...mockState.events]
    if (status && status !== 'ALL') events = events.filter(e => e.status === status)
    return HttpResponse.json(paginate(events, page, limit))
  }),

  // GET /api/v1/events/:id/attendees — MUST be before /:id
  http.get(`${BASE}/api/v1/events/:id/attendees`, ({ params }) => {
    return HttpResponse.json({ data: [], total: 0, eventId: params.id })
  }),

  http.get(`${BASE}/api/v1/events/:id`, ({ params }) => {
    const event = mockState.events.find(e => e.id === params.id)
    if (!event) return HttpResponse.json({ message: 'Event not found' }, { status: 404 })
    return HttpResponse.json({ ...event, attendeeList: [] })
  }),

  // POST /api/v1/events — STATEFUL
  http.post(`${BASE}/api/v1/events`, async ({ request }) => {
    await delay(400)
    const body = await request.json()
    const newEvent = { id: 'EVT-' + Date.now(), ...body, myRsvp: null }
    mockState.events.push(newEvent)
    return HttpResponse.json({ success: true, event: newEvent }, { status: 201 })
  }),

  // POST /api/v1/events/:id/rsvp — STATEFUL
  http.post(`${BASE}/api/v1/events/:id/rsvp`, async ({ request, params }) => {
    await delay(400)
    const body = await request.json()
    const response = body.response // "GOING" | "NOT_GOING"
    const event = mockState.events.find(e => e.id === params.id)
    if (!event) return HttpResponse.json({ message: 'Event not found' }, { status: 404 })
    const prev = event.myRsvp
    if (response === 'GOING' && prev !== 'GOING') {
      event.bookedSeats = Math.min(event.bookedSeats + 1, event.totalSeats)
      event.availableSeats = Math.max(event.availableSeats - 1, 0)
    } else if (response === 'NOT_GOING' && prev === 'GOING') {
      event.bookedSeats = Math.max(event.bookedSeats - 1, 0)
      event.availableSeats = Math.min(event.availableSeats + 1, event.totalSeats)
    }
    event.myRsvp = response
    return HttpResponse.json({
      success: true,
      myRsvp: response,
      message: response === 'GOING' ? 'Registered! Check WhatsApp for confirmation.' : 'RSVP cancelled.',
    })
  }),

  // PUT /api/v1/events/:id — STATEFUL
  http.put(`${BASE}/api/v1/events/:id`, async ({ request, params }) => {
    await delay(400)
    const body = await request.json()
    const idx = mockState.events.findIndex(e => e.id === params.id)
    if (idx === -1) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    mockState.events[idx] = { ...mockState.events[idx], ...body }
    return HttpResponse.json({ success: true, event: mockState.events[idx] })
  }),

  // DELETE /api/v1/events/:id — STATEFUL
  http.delete(`${BASE}/api/v1/events/:id`, async ({ params }) => {
    await delay(400)
    const idx = mockState.events.findIndex(e => e.id === params.id)
    if (idx !== -1) mockState.events.splice(idx, 1)
    return HttpResponse.json({ success: true })
  }),

  // PATCH /api/v1/events/:id/cancel — STATEFUL
  http.patch(`${BASE}/api/v1/events/:id/cancel`, async ({ params }) => {
    await delay(400)
    const event = mockState.events.find(e => e.id === params.id)
    if (event) event.status = 'CANCELLED'
    return HttpResponse.json({ success: true, status: 'CANCELLED' })
  }),

  // ── REFERRALS ────────────────────────────────────────────────
  http.get(`${BASE}/api/v1/referrals/leaderboard`, async () => {
    await delay(getDelay('referrals'))
    return HttpResponse.json([
      { rank: 1, memberId: 'MEM-006', name: 'Ravi Nair', firmName: 'Nair Exports', approvedReferrals: 8, totalReferrals: 10 },
      { rank: 2, memberId: 'MEM-001', name: 'Ramesh Kumar', firmName: 'Kumar Traders', approvedReferrals: 5, totalReferrals: 6 },
      { rank: 3, memberId: 'MEM-003', name: 'Priya Sharma', firmName: 'Sharma Industries', approvedReferrals: 3, totalReferrals: 5 },
      { rank: 4, memberId: 'MEM-002', name: 'Suresh Patel', firmName: 'Patel Enterprises', approvedReferrals: 2, totalReferrals: 3 },
      { rank: 5, memberId: 'MEM-004', name: 'Vijay Mehta', firmName: 'Mehta & Co', approvedReferrals: 1, totalReferrals: 2 },
    ])
  }),

  http.get(`${BASE}/api/v1/referrals/:memberId`, async ({ params }) => {
    await delay(getDelay('referrals'))
    if (shouldFail('referrals')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    return HttpResponse.json({
      memberId: params.memberId,
      referralLink: `https://join.amms.in/ref/${params.memberId}`,
      totalReferrals: 6,
      approvedReferrals: 5,
      pendingReferrals: 1,
      rewardPoints: 2500,
      referrals: [
        { refereeName: 'Anil Shah', refereeFirm: 'Shah Textiles', status: 'APPROVED', appliedAt: '2024-02-10', points: 500 },
        { refereeName: 'Meena Jain', refereeFirm: 'Jain & Sons', status: 'APPROVED', appliedAt: '2024-03-05', points: 500 },
        { refereeName: 'Kiran Rao', refereeFirm: 'Rao Logistics', status: 'PENDING', appliedAt: '2024-04-12', points: 0 },
      ],
    })
  }),

  http.post(`${BASE}/api/v1/referrals/send`, async ({ request }) => {
    await delay(400)
    const body = await request.json()
    return HttpResponse.json({
      success: true,
      message: `Invitation sent to ${body.name || 'contact'} via WhatsApp`,
    })
  }),

  http.post(`${BASE}/api/v1/referrals/share`, async ({ request }) => {
    await delay(300)
    const body = await request.json()
    const memberId = body.memberId || 'MEM-001'
    return HttpResponse.json({
      success: true,
      referralLink: `https://join.amms.in/ref/${memberId}`,
      message: 'Referral link ready to share',
    })
  }),

  // ── GRIEVANCES ───────────────────────────────────────────────
  http.get(`${BASE}/api/v1/grievances/stats`, async () => {
    await delay(300)
    const total = mockState.grievances.length
    const open = mockState.grievances.filter(g => g.status === 'OPEN').length
    const inProgress = mockState.grievances.filter(g => g.status === 'IN_PROGRESS').length
    const resolved = mockState.grievances.filter(g => g.status === 'RESOLVED').length
    const closed = mockState.grievances.filter(g => g.status === 'CLOSED').length
    return HttpResponse.json({ total, open, inProgress, resolved, closed, avgResolutionHours: 18 })
  }),

  http.get(`${BASE}/api/v1/grievances`, async ({ request }) => {
    await delay(getDelay('grievances'))
    if (shouldFail('grievances')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    const url = new URL(request.url)
    const memberId = url.searchParams.get('memberId') || ''
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    let grievances = [...mockState.grievances]
    if (memberId) grievances = grievances.filter(g => g.memberId === memberId)
    return HttpResponse.json(paginate(grievances, page, limit))
  }),

  // POST /api/v1/grievances — STATEFUL
  http.post(`${BASE}/api/v1/grievances`, async ({ request }) => {
    await delay(500)
    const body = await request.json()
    const year = new Date().getFullYear()
    const idx = mockState.grievances.length + 1
    const ticketNo = `TKT-${year}-${String(idx).padStart(3, '0')}`
    const newGrievance = {
      id: 'GRV-' + Date.now(),
      memberId: body.memberId || 'MEM-001',
      ticketNo,
      subject: body.subject,
      description: body.description,
      category: body.category || 'OTHER',
      status: 'OPEN',
      priority: 'MEDIUM',
      submittedAt: new Date().toISOString(),
      resolvedAt: null,
      adminResponse: null,
    }
    mockState.grievances.push(newGrievance)
    return HttpResponse.json({
      success: true,
      ticketNo,
      status: 'OPEN',
      message: `Complaint registered. Ticket: ${ticketNo}`,
      estimatedResolution: '24-48 hours',
    }, { status: 201 })
  }),

  // PATCH /api/v1/grievances/:id/respond — STATEFUL
  http.patch(`${BASE}/api/v1/grievances/:id/respond`, async ({ request, params }) => {
    await delay(400)
    const body = await request.json()
    const grievance = mockState.grievances.find(g => g.id === params.id)
    if (!grievance) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    grievance.adminResponse = body.response
    grievance.status = body.newStatus || 'IN_PROGRESS'
    if (grievance.status === 'RESOLVED') grievance.resolvedAt = new Date().toISOString()
    return HttpResponse.json({ success: true, notificationSent: true, status: grievance.status })
  }),

  // PATCH /api/v1/grievances/:id/close — STATEFUL
  http.patch(`${BASE}/api/v1/grievances/:id/close`, async ({ params }) => {
    await delay(300)
    const grievance = mockState.grievances.find(g => g.id === params.id)
    if (grievance) grievance.status = 'CLOSED'
    return HttpResponse.json({ success: true, status: 'CLOSED' })
  }),
]
