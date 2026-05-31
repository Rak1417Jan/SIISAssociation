import { http, HttpResponse, delay } from 'msw'
import { mockState } from '../utils/mockState'
import { shouldFail, getDelay } from '../utils/errorScenarios'

const BASE = 'https://amms-api-866440119101.asia-south1.run.app'

export const settingsHandlers = [
  // GET /api/v1/settings
  http.get(`${BASE}/api/v1/settings`, async () => {
    await delay(getDelay('settings'))
    if (shouldFail('settings')) {
      return HttpResponse.json({ message: 'Server error' }, { status: 500 })
    }
    return HttpResponse.json(mockState.settings)
  }),

  // PUT /api/v1/settings — STATEFUL
  http.put(`${BASE}/api/v1/settings`, async ({ request }) => {
    await delay(getDelay('settings'))
    const body = await request.json()
    Object.assign(mockState.settings, body)
    return HttpResponse.json({ success: true, settings: mockState.settings })
  }),

  // POST /api/v1/settings/logo
  http.post(`${BASE}/api/v1/settings/logo`, async () => {
    await delay(600)
    return HttpResponse.json({
      success: true,
      logoUrl: 'https://via.placeholder.com/200x80',
    })
  }),
]
