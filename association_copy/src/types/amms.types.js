/**
 * @typedef {Object} ApiResponse
 * @property {any} data
 * @property {any} error
 * @property {number} status
 * @property {boolean} [loading]
 */

/**
 * @typedef {Object} PaginatedResponse
 * @property {Array<any>} data
 * @property {number} total
 * @property {number} page
 * @property {number} limit
 */

/**
 * @typedef {Object} Member
 * @property {string} id
 * @property {string} name
 * @property {string} phone
 * @property {string} email
 * @property {MemberStatus} status
 * @property {string} planType
 * @property {string} firmId
 * @property {string} applicationId
 * @property {string} createdAt
 */

/**
 * @typedef {"APPLIED"|"PENDING"|"APPROVED"|"REJECTED"|"SUSPENDED"|"EXPIRED"} MemberStatus
 */

/**
 * @typedef {Object} Firm
 * @property {string} id
 * @property {string} name
 * @property {string} registrationNo
 * @property {number} memberCount
 */

/**
 * @typedef {Object} Staff
 * @property {string} id
 * @property {string} name
 * @property {string} email
 * @property {UserRole} role
 * @property {boolean} isActive
 */

/**
 * @typedef {Object} Plan
 * @property {string} id
 * @property {string} name
 * @property {number} duration
 * @property {number} price
 * @property {number} gstAmount
 * @property {number} platformFee
 * @property {number} totalAmount
 */

/**
 * @typedef {Object} Payment
 * @property {string} id
 * @property {string} memberId
 * @property {number} amount
 * @property {string} status
 * @property {string} razorpayOrderId
 * @property {string} receipt
 * @property {string} createdAt
 */

/**
 * @typedef {Object} Document
 * @property {string} id
 * @property {string} memberId
 * @property {string} type
 * @property {string} status
 * @property {string} url
 * @property {number} aiConfidence
 * @property {boolean} aiVerified
 */

/**
 * @typedef {Object} Broadcast
 * @property {string} id
 * @property {string} title
 * @property {string} message
 * @property {string} channel
 * @property {string} status
 * @property {string} scheduledAt
 */

/**
 * @typedef {"super_admin"|"admin"|"finance"|"operator"|"member"} UserRole
 */

// This file is used purely for JSDoc type definitions to provide IDE intellisense.
export {};
