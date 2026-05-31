import { callService } from './serviceBase';
import apiClient from './apiClient';
import { unwrapEnvelope } from './apiTransforms';

const logErr = (svc, path, err) =>
  console.error(`[AMMS engagementService] ${path} — error:`, err?.message || err);

const pageParams = (filters = {}, page = 1, limit = 10) => ({
  ...filters,
  page,
  limit,
});

const mapPaged = (raw, page, limit) => {
  const data = raw?.records || raw?.data || (Array.isArray(raw) ? raw : []);
  return {
    data,
    total: raw?.total ?? data.length,
    page,
    limit,
  };
};

// ── DIRECTORY ──────────────────────────────────────────

export const getDirectory = (filters = {}, page = 1, limit = 12) =>
  callService('getDirectory', null, () =>
    apiClient
      .get('/directory', { params: pageParams(filters, page, limit) })
      .then((res) => ({ ...res, data: mapPaged(unwrapEnvelope(res.data), page, limit) }))
      .catch((err) => {
        logErr('getDirectory', 'GET /directory', err);
        throw err;
      })
  );

export const getIndustryList = () =>
  callService('getIndustryList', null, () =>
    apiClient
      .get('/directory/industries')
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getIndustryList', 'GET /directory/industries', err);
        throw err;
      })
  );

export const sendConnectionRequest = (fromMemberId, toMemberId, message) =>
  callService('sendConnectionRequest', null, () =>
    apiClient
      .post('/directory/connect', { fromMemberId, toMemberId, message })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('sendConnectionRequest', 'POST /directory/connect', err);
        throw err;
      })
  );

// ── EVENTS ───────────────────────────────────────────

export const getEvents = (filters = {}, page = 1, limit = 10) =>
  callService('getEvents', null, () =>
    apiClient
      .get('/events', { params: pageParams(filters, page, limit) })
      .then((res) => ({ ...res, data: mapPaged(unwrapEnvelope(res.data), page, limit) }))
      .catch((err) => {
        logErr('getEvents', 'GET /events', err);
        throw err;
      })
  );

export const getEventById = (eventId) =>
  callService('getEventById', null, () =>
    apiClient
      .get(`/events/${eventId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getEventById', `GET /events/${eventId}`, err);
        throw err;
      })
  );

export const createEvent = (data) =>
  callService('createEvent', null, () =>
    apiClient
      .post('/events', data)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('createEvent', 'POST /events', err);
        throw err;
      })
  );

export const updateEvent = (eventId, data) =>
  callService('updateEvent', null, () =>
    apiClient
      .put(`/events/${eventId}`, data)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('updateEvent', `PUT /events/${eventId}`, err);
        throw err;
      })
  );

export const deleteEvent = (eventId) =>
  callService('deleteEvent', null, () =>
    apiClient
      .delete(`/events/${eventId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) || { success: true } }))
      .catch((err) => {
        logErr('deleteEvent', `DELETE /events/${eventId}`, err);
        throw err;
      })
  );

export const rsvpEvent = (eventId, memberId, response) =>
  callService('rsvpEvent', null, () =>
    apiClient
      .post(`/events/${eventId}/rsvp`, { memberId, response })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('rsvpEvent', `POST /events/${eventId}/rsvp`, err);
        throw err;
      })
  );

export const getEventAttendees = (eventId, page = 1, limit = 10) =>
  callService('getEventAttendees', null, () =>
    apiClient
      .get(`/events/${eventId}/attendees`, { params: { page, limit } })
      .then((res) => ({ ...res, data: mapPaged(unwrapEnvelope(res.data), page, limit) }))
      .catch((err) => {
        logErr('getEventAttendees', `GET /events/${eventId}/attendees`, err);
        throw err;
      })
  );

export const cancelEvent = (eventId, reason) =>
  callService('cancelEvent', null, () =>
    apiClient
      .patch(`/events/${eventId}/cancel`, { reason })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('cancelEvent', `PATCH /events/${eventId}/cancel`, err);
        throw err;
      })
  );

// ── REFERRALS ────────────────────────────────────────

export const getMemberReferrals = (memberId) =>
  callService('getMemberReferrals', null, () =>
    apiClient
      .get(`/referrals/${memberId}`)
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getMemberReferrals', `GET /referrals/${memberId}`, err);
        throw err;
      })
  );

export const sendReferralInvite = (memberId, inviteData) =>
  callService('sendReferralInvite', null, () =>
    apiClient
      .post('/referrals/send', { memberId, ...inviteData })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('sendReferralInvite', 'POST /referrals/send', err);
        throw err;
      })
  );

export const getReferralLeaderboard = () =>
  callService('getReferralLeaderboard', null, () =>
    apiClient
      .get('/referrals/leaderboard')
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getReferralLeaderboard', 'GET /referrals/leaderboard', err);
        throw err;
      })
  );

export const shareReferralLink = (memberId, channel) =>
  callService('shareReferralLink', null, () =>
    apiClient
      .post('/referrals/share', { memberId, channel })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('shareReferralLink', 'POST /referrals/share', err);
        throw err;
      })
  );

// ── GRIEVANCES ───────────────────────────────────────

export const getMemberGrievances = (memberId) =>
  callService('getMemberGrievances', null, () =>
    apiClient
      .get('/grievances', { params: { memberId } })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getMemberGrievances', 'GET /grievances', err);
        throw err;
      })
  );

export const getAllGrievances = (filters = {}, page = 1, limit = 10) =>
  callService('getAllGrievances', null, () =>
    apiClient
      .get('/grievances', { params: pageParams(filters, page, limit) })
      .then((res) => ({ ...res, data: mapPaged(unwrapEnvelope(res.data), page, limit) }))
      .catch((err) => {
        logErr('getAllGrievances', 'GET /grievances', err);
        throw err;
      })
  );

export const submitGrievance = (memberId, data) =>
  callService('submitGrievance', null, () =>
    apiClient
      .post('/grievances', { memberId, ...data })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('submitGrievance', 'POST /grievances', err);
        throw err;
      })
  );

export const respondToGrievance = (grievanceId, response, newStatus) =>
  callService('respondToGrievance', null, () =>
    apiClient
      .patch(`/grievances/${grievanceId}/respond`, { response, newStatus })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('respondToGrievance', `PATCH /grievances/${grievanceId}/respond`, err);
        throw err;
      })
  );

export const closeGrievance = (grievanceId, reason) =>
  callService('closeGrievance', null, () =>
    apiClient
      .patch(`/grievances/${grievanceId}/close`, { reason })
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('closeGrievance', `PATCH /grievances/${grievanceId}/close`, err);
        throw err;
      })
  );

export const getGrievanceStats = () =>
  callService('getGrievanceStats', null, () =>
    apiClient
      .get('/grievances/stats')
      .then((res) => ({ ...res, data: unwrapEnvelope(res.data) }))
      .catch((err) => {
        logErr('getGrievanceStats', 'GET /grievances/stats', err);
        throw err;
      })
  );
