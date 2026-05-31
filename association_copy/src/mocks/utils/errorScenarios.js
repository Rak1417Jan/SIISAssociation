export const errorScenarios = {
  auth: false,
  members: false,
  memberDetail: false,
  firms: false,
  payments: false,
  staff: false,
  analytics: false,
  broadcasts: false,
  audit: false,
  settings: false,
  events: false,
  grievances: false,
  referrals: false,
  directory: false,
  digitalId: false,
}

if (typeof window !== 'undefined') {
  window.__AMMS_ERRORS__ = errorScenarios
}

export function shouldFail(key) {
  return errorScenarios[key] === true
}

export const delayOverrides = {}
if (typeof window !== 'undefined') {
  window.__AMMS_DELAYS__ = delayOverrides
}

export function getDelay(key) {
  return delayOverrides[key] ?? 400
}
