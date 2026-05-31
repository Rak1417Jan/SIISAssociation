import { authHandlers } from './handlers/auth.handlers'
import { memberHandlers } from './handlers/member.handlers'
import { firmHandlers } from './handlers/firm.handlers'
import { paymentHandlers } from './handlers/payment.handlers'
import { staffHandlers } from './handlers/staff.handlers'
import { analyticsHandlers } from './handlers/analytics.handlers'
import { broadcastHandlers } from './handlers/broadcast.handlers'
import { auditHandlers } from './handlers/audit.handlers'
import { settingsHandlers } from './handlers/settings.handlers'
import { digitalIdHandlers } from './handlers/digitalId.handlers'
import { engagementHandlers } from './handlers/engagement.handlers'

export const handlers = [
  ...authHandlers,
  ...memberHandlers,
  ...firmHandlers,
  ...paymentHandlers,
  ...staffHandlers,
  ...analyticsHandlers,
  ...broadcastHandlers,
  ...auditHandlers,
  ...settingsHandlers,
  ...digitalIdHandlers,
  ...engagementHandlers,
]
