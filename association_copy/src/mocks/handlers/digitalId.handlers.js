import { http, HttpResponse, delay } from 'msw'
import { mockState } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const digitalIdHandlers = [
  // GET /api/v1/digital-id/verify/:membershipId — MUST be before /:memberId
  http.get(`${BASE}/api/v1/digital-id/verify/:membershipId`, ({ params }) => {
    return HttpResponse.json({
      isValid: true,
      member: {
        name: 'Ramesh Kumar',
        membershipId: params.membershipId,
        planType: 'YEARLY',
        validUntil: '2025-01-14',
        status: 'ACTIVE',
      },
    })
  }),

  // GET /api/v1/digital-id/:memberId/download — MUST be before /:memberId
  http.get(`${BASE}/api/v1/digital-id/:memberId/download`, ({ params }) => {
    return HttpResponse.json({
      downloadUrl: '#mock',
      fileName: `digital-id-${params.memberId}.pdf`,
    })
  }),

  // GET /api/v1/digital-id/:memberId
  http.get(`${BASE}/api/v1/digital-id/:memberId`, async ({ params }) => {
    await delay(getDelay('digitalId'))
    if (shouldFail('digitalId')) {
      return HttpResponse.json({ message: 'Digital ID not found' }, { status: 404 })
    }
    const member = mockState.members.find(m => m.id === params.memberId)
    if (!member) return HttpResponse.json({ message: 'Member not found' }, { status: 404 })
    return HttpResponse.json({
      memberId: params.memberId,
      memberName: member.name,
      membershipId: 'AMMS/2024/001',
      planType: member.planType,
      validFrom: '2024-01-15',
      validUntil: '2025-01-14',
      firmName: member.firmName,
      designation: 'Proprietor',
      photo: 'https://via.placeholder.com/150',
      qrCode: `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=AMMS-${params.memberId}`,
      qrValue: `https://verify.amms.in/${params.memberId}`,
      associationName: 'Demo Trade Chamber',
      status: 'ACTIVE',
      isGenerated: true,
    })
  }),

  // POST /api/v1/digital-id/generate
  http.post(`${BASE}/api/v1/digital-id/generate`, async () => {
    await delay(600)
    return HttpResponse.json({ success: true, message: 'Digital ID generated' })
  }),

  // POST /api/v1/digital-id/:memberId/share
  http.post(`${BASE}/api/v1/digital-id/:memberId/share`, async () => {
    await delay(400)
    return HttpResponse.json({ success: true, message: 'ID card sent via WhatsApp' })
  }),
]
