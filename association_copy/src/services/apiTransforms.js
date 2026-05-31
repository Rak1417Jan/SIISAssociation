import { API_BASE_URL } from '../config/apiConfig';

/** ASP.NET ResponseModel<T> envelope */
export function unwrapEnvelope(body) {
  if (body && typeof body === 'object' && 'data' in body && 'success' in body) {
    if (!body.success && body.errorMessage) {
      const err = new Error(body.errorMessage);
      err.response = { data: body, status: 400 };
      throw err;
    }
    return body.data;
  }
  return body;
}

export function rootUrl(path) {
  return `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
}

export function normalizeRole(role) {
  if (!role) return 'member';
  const r = String(role).toLowerCase().replace(/\s+/g, '_');
  if (r === 'superadmin' || r === 'super_admin') return 'super_admin';
  if (r === 'admin') return 'admin';
  if (r === 'manager') return 'admin';
  if (r === 'finance' || r === 'bursar') return 'finance';
  if (r === 'operator') return 'operator';
  return r;
}

export function mapApplicationStatus(status) {
  const s = String(status || '').toUpperCase();
  if (s === 'VERIFIED' || s === 'APPROVED' || s === 'ACTIVE') return 'APPROVED';
  if (s === 'REJECTED') return 'REJECTED';
  if (s === 'ON_HOLD' || s === 'HOLD' || s === 'SUSPENDED') return 'SUSPENDED';
  if (s === 'PENDING' || s === 'APPLIED' || s === 'IN_PROGRESS') return 'PENDING';
  return s || 'PENDING';
}

function daysSince(isoDate) {
  if (!isoDate) return 0;
  const diff = Date.now() - new Date(isoDate).getTime();
  return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)));
}

export function mapLoginResponse(raw) {
  const d = raw?.data ?? raw;
  if (!d?.accessToken && !d?.token) {
    throw new Error(raw?.errorMessage || 'Login failed');
  }
  const token = d.accessToken || d.token;
  const role = normalizeRole(d.role);
  return {
    token,
    refreshToken: d.refreshToken || '',
    expiresIn: 3600,
    user: {
      id: String(d.userId ?? d.id ?? ''),
      name: d.fullName || d.name || d.username || '',
      email: d.email || d.username || '',
      role,
    },
  };
}

export function mapOtpRequestResponse(raw) {
  const d = unwrapEnvelope(raw) ?? raw;
  return {
    success: true,
    expiresIn: 60,
    maskedPhone: d?.mobileNo
      ? `+91 XXXXX ${String(d.mobileNo).slice(-4)}`
      : '+91 XXXXX XXXX',
  };
}

export function mapOtpVerifyResponse(raw) {
  const d = unwrapEnvelope(raw) ?? raw;
  const token = d?.accessToken || d?.token;
  if (!token) throw new Error('OTP verification failed');
  return {
    token,
    refreshToken: d.refreshToken || '',
    user: {
      id: String(d.userId ?? ''),
      name: d.fullName || d.mobileNumber || '',
      phone: d.mobileNumber || '',
      role: 'member',
    },
    isNewUser: Boolean(d.isNewUser),
  };
}

export function mapSessionUser(raw) {
  const d = unwrapEnvelope(raw) ?? raw;
  return {
    id: String(d?.userId ?? ''),
    name: d?.fullName || d?.username || '',
    email: d?.email || d?.username || '',
    role: normalizeRole(d?.role || d?.roleName),
    lastLogin: d?.lastLogin || null,
    permissions: d?.permissions || [],
  };
}

export function mapMemberListItem(m) {
  const status = mapApplicationStatus(m.applicationStatus || (m.isActive ? 'VERIFIED' : 'PENDING'));
  return {
    id: String(m.memberId ?? m.id),
    name: m.ownerName || m.name || '',
    phone: m.mobileNumber || m.phone || '',
    email: m.email || '',
    status,
    planType: m.planType || m.planName || 'YEARLY',
    firmName: m.companyName || m.firmName || '',
    appliedAt: m.createdDate || m.appliedAt || '',
    daysWaiting: daysSince(m.createdDate || m.appliedAt),
  };
}

export function mapPagedMembers(paged, page, limit) {
  const records = paged?.records || paged?.data || [];
  return {
    data: records.map(mapMemberListItem),
    total: paged?.total ?? records.length,
    page: paged?.page ?? page,
    limit: paged?.pageSize ?? limit,
  };
}

export function mapMemberDetail(d) {
  if (!d) return null;
  const status = mapApplicationStatus(d.applicationStatus || (d.isActive ? 'VERIFIED' : 'PENDING'));
  return {
    id: String(d.memberId),
    name: d.ownerName || '',
    phone: d.mobileNumber || '',
    email: d.email || '',
    status,
    planType: d.planType || 'YEARLY',
    firmName: d.companyName || '',
    firmId: String(d.companyId ?? ''),
    applicationId: d.applicationId ? String(d.applicationId) : '',
    address: [d.address, d.city].filter(Boolean).join(', '),
    appliedAt: d.createdDate || '',
    documents: d.documents || [],
    paymentHistory: d.paymentHistory || [],
    adminNotes: d.applicationRemarks || '',
    membershipExpiry: d.membershipExpiry || null,
  };
}

export function mapDashboardMetrics(d) {
  if (!d) return { data: {} };
  return {
    data: {
      totalMembers: d.totalMembers ?? 0,
      pendingApprovals: d.pendingApplications ?? 0,
      approvedThisMonth: 0,
      rejectedThisMonth: d.rejectedApplications ?? 0,
      totalRevenue: d.currentYearRevenue ?? 0,
      revenueThisMonth: d.currentYearRevenue ?? 0,
      activeMembers: d.activeMembers ?? 0,
      expiringSoon: 0,
      totalFirms: 0,
      oldestPendingDays: 0,
      newMembersToday: d.last7DaysRegistrations?.slice(-1)?.[0]?.count ?? 0,
    },
  };
}

export function mapPendingApplications(paged) {
  const records = (paged?.records || []).map((r) => ({
    memberId: String(r.memberId ?? r.applicationId ?? ''),
    name: r.ownerName || r.name || '',
    firmName: r.firmName || r.companyName || '',
    appliedAt: r.createdDate || '',
    daysWaiting: daysSince(r.createdDate),
    status: mapApplicationStatus(r.status),
  }));
  return {
    data: records,
    total: paged?.total ?? records.length,
    page: paged?.page ?? 1,
    limit: paged?.pageSize ?? 10,
  };
}

export function mapFirmListItem(f) {
  return {
    id: String(f.firmId ?? f.id),
    name: f.name || '',
    registrationNo: f.regNo || f.companyCode || f.gstNo || '',
    industry: f.companyTypeName || f.industry || 'General',
    memberCount: f.memberCount ?? 0,
    contactPerson: f.contactPerson || '',
    status: f.isActive === false ? 'INACTIVE' : 'ACTIVE',
    createdAt: f.createdDate || '',
  };
}

export function mapPagedFirms(paged, page, limit) {
  const records = paged?.records || [];
  return {
    data: records.map(mapFirmListItem),
    total: paged?.total ?? records.length,
    page: paged?.page ?? page,
    limit: paged?.pageSize ?? limit,
  };
}

export function mapFirmDetail(d) {
  if (!d) return null;
  return {
    id: String(d.firmId ?? d.id),
    name: d.name || '',
    registrationNo: d.regNo || d.companyCode || '',
    industry: d.companyTypeName || '',
    memberCount: d.memberCount ?? (d.members?.length ?? 0),
    address: d.address || '',
    phone: d.telephoneNo || d.mobile || '',
    email: d.email || '',
    status: d.isActive === false ? 'INACTIVE' : 'ACTIVE',
    members: (d.members || []).map((m) => ({
      id: String(m.memberId ?? m.id),
      name: m.ownerName || m.name || '',
      status: mapApplicationStatus(m.status),
    })),
  };
}

export function mapStaffListItem(s) {
  return {
    id: String(s.userId ?? s.id),
    name: s.fullName || s.name || '',
    email: s.email || '',
    role: normalizeRole(s.roleName || s.role),
    isActive: s.isActive !== false,
    lastLogin: s.lastLogin || s.createdDate || '',
    createdAt: s.createdDate || '',
  };
}

export function mapStaffPage(list, page, limit) {
  const records = Array.isArray(list) ? list : [];
  return {
    data: records.map(mapStaffListItem),
    total: records.length,
    page,
    limit,
  };
}

export function mapRolesList(roles) {
  return (roles || []).map((r) => ({
    id: String(r.roleId ?? r.id),
    name: (r.roleName || r.name || '').toLowerCase(),
    displayName: r.roleName || r.name || '',
    permissionCount: tryParsePermissions(r.permissions).length,
  }));
}

function tryParsePermissions(perms) {
  if (!perms) return [];
  if (Array.isArray(perms)) return perms;
  try {
    const parsed = JSON.parse(perms);
    return Array.isArray(parsed) ? parsed : Object.keys(parsed);
  } catch {
    return String(perms).split(',').filter(Boolean);
  }
}

export function buildPermissionMatrix(roles) {
  const matrix = {};
  (roles || []).forEach((r) => {
    const roleKey = (r.roleName || r.name || '').toLowerCase();
    const codes = tryParsePermissions(r.permissions);
    matrix[roleKey] = permissionsCodesToMatrix(codes);
  });
  return matrix;
}

function permissionsCodesToMatrix(codes) {
  const matrix = {
    members: { view: false, create: false, approve: false, delete: false },
    payments: { view: false, refund: false },
    staff: { manage: false },
    settings: { manage: false },
    analytics: { view: false, export: false },
    audit: { view: false },
  };
  const all = codes.includes('*') || codes.includes('admin.full');
  if (all) {
    Object.keys(matrix).forEach((res) => {
      Object.keys(matrix[res]).forEach((act) => {
        matrix[res][act] = true;
      });
    });
    return matrix;
  }
  codes.forEach((code) => {
    const [resource, action] = String(code).split('.');
    if (matrix[resource] && action && action in matrix[resource]) {
      matrix[resource][action] = true;
    }
    if (code === 'members.read') matrix.members.view = true;
    if (code === 'members.write') {
      matrix.members.view = true;
      matrix.members.create = true;
      matrix.members.approve = true;
    }
    if (code === 'staff.manage') matrix.staff.manage = true;
    if (code === 'roles.manage') matrix.staff.manage = true;
    if (code === 'broadcast.send') matrix.analytics.view = true;
  });
  return matrix;
}

export function mapBroadcastItem(b) {
  const status = b.sentAt ? 'SENT' : b.scheduledAt ? 'SCHEDULED' : 'DRAFT';
  return {
    id: String(b.broadcastId ?? b.id),
    title: b.title || '',
    message: b.message || '',
    channel: b.channel || 'EMAIL',
    status,
    recipientCount: b.recipientCount ?? 0,
    deliveredCount: b.deliveredCount ?? 0,
    failedCount: b.failedCount ?? 0,
    sentAt: b.sentAt,
    scheduledAt: b.scheduledAt,
    createdBy: b.createdBy || 'Admin',
  };
}

export function mapPagedBroadcasts(paged, page, limit) {
  const records = paged?.records || [];
  return {
    data: records.map(mapBroadcastItem),
    total: paged?.total ?? records.length,
    page: paged?.page ?? page,
    limit: paged?.pageSize ?? limit,
  };
}

export function mapAnalyticsMembers(d) {
  const growth = d?.membershipGrowth || [];
  const labels = growth.map((p) => `M${p.month}`);
  return {
    labels: labels.length ? labels : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    newRegistrations: growth.map((p) => p.newMembers ?? 0),
    approvals: growth.map((p) => p.newMembers ?? 0),
    rejections: growth.map(() => 0),
  };
}

export function mapAnalyticsRevenue(d) {
  const rev = d?.monthlyRevenue || [];
  const labels = rev.map((p) => `M${p.month}`);
  return {
    labels: labels.length ? labels : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    revenue: rev.map((p) => Number(p.total ?? 0)),
    gstCollected: rev.map((p) => Number(p.total ?? 0) * 0.18),
    yearlyPlanSales: [],
    lifetimePlanSales: [],
  };
}

export const EMPTY_PAGE = { data: [], total: 0, page: 1, limit: 10 };
