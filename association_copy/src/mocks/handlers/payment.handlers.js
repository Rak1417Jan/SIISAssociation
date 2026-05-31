import { http, HttpResponse, delay } from 'msw'
import { mockState, paginate } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const paymentHandlers = [
  // GET /api/v1/plans
  http.get(`${BASE}/api/v1/plans`, async () => {
    await delay(getDelay('payments'))
    return HttpResponse.json([
      {
        id: 'PLAN-001', name: 'Yearly', duration: 12,
        price: 1000, gstPercent: 18, gstAmount: 180,
        platformFee: 50, totalAmount: 1230,
        features: ['Digital ID Card', 'Member Portal Access', 'Renewal Reminders', 'WhatsApp Updates'],
      },
      {
        id: 'PLAN-002', name: 'Lifetime', duration: null,
        price: 5000, gstPercent: 18, gstAmount: 900,
        platformFee: 100, totalAmount: 6000,
        features: ['Digital ID Card', 'Member Portal Access', 'Priority Support', 'Lifetime Validity', 'Free Renewals Forever'],
      },
    ])
  }),

  // POST /api/v1/payments/create-order
  http.post(`${BASE}/api/v1/payments/create-order`, async () => {
    await delay(getDelay('payments'))
    if (shouldFail('payments')) {
      return HttpResponse.json({ message: 'Payment service unavailable' }, { status: 500 })
    }
    return HttpResponse.json({
      orderId: 'order_mock_' + Date.now(),
      amount: 123000, currency: 'INR',
      keyId: 'rzp_test_placeholder',
      prefill: { name: 'Ramesh Kumar', email: 'ramesh@email.com', contact: '9876543210' },
    })
  }),

  // POST /api/v1/payments/verify
  http.post(`${BASE}/api/v1/payments/verify`, async () => {
    await delay(getDelay('payments'))
    return HttpResponse.json({
      success: true,
      paymentId: 'PAY-' + Date.now(),
      status: 'SUCCESS',
      receiptUrl: '#mock-receipt',
      receiptNo: 'RCP-2024-' + Date.now(),
    })
  }),

  // GET /api/v1/payments/history
  http.get(`${BASE}/api/v1/payments/history`, async ({ request }) => {
    await delay(getDelay('payments'))
    const url = new URL(request.url)
    const page = url.searchParams.get('page') || 1
    const limit = url.searchParams.get('limit') || 10
    return HttpResponse.json(paginate(mockState.payments, page, limit))
  }),

  // GET /api/v1/payments/summary/:memberId  — MUST be before /payments/:id
  http.get(`${BASE}/api/v1/payments/summary/:memberId`, () => {
    return HttpResponse.json({
      totalPaid: 1230, lastPayment: '2024-01-15',
      nextRenewalDate: '2025-01-15', currentPlan: 'Yearly',
      daysUntilExpiry: 285, isExpiringSoon: false,
    })
  }),

  // GET /api/v1/payments/:id/receipt  — MUST be before /payments/:id
  http.get(`${BASE}/api/v1/payments/:id/receipt`, () => {
    return HttpResponse.json({
      receiptUrl: '#mock',
      receiptNo: 'RCP-001',
      gstNo: 'GST27MOCK1234Z1Z5',
      issuedAt: new Date().toISOString(),
    })
  }),

  // GET /api/v1/payments/:id
  http.get(`${BASE}/api/v1/payments/:id`, ({ params }) => {
    const payment = mockState.payments.find(p => p.id === params.id)
    if (!payment) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    return HttpResponse.json(payment)
  }),

  // POST /api/v1/payments/renewal
  http.post(`${BASE}/api/v1/payments/renewal`, async () => {
    await delay(getDelay('payments'))
    return HttpResponse.json({
      orderId: 'order_renewal_' + Date.now(),
      amount: 123000, currency: 'INR',
      keyId: 'rzp_test_placeholder',
      prefill: { name: 'Ramesh Kumar', email: 'ramesh@email.com', contact: '9876543210' },
    })
  }),
]
