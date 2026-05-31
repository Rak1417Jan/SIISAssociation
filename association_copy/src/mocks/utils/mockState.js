export const mockState = {
  members: [
    {
      id: 'MEM-001', name: 'Ramesh Kumar', phone: '9876543210',
      email: 'ramesh@kumar.com', address: '12 Marine Lines, Mumbai 400002',
      firmName: 'Kumar Traders', firmId: 'FIRM-001',
      planType: 'YEARLY', status: 'APPROVED',
      applicationId: 'APP-001', appliedAt: '2024-01-05T10:00:00Z',
      daysWaiting: 0, adminNotes: 'Verified and approved.',
    },
    {
      id: 'MEM-002', name: 'Suresh Patel', phone: '9123456789',
      email: 'suresh@patel.com', address: '45 Andheri West, Mumbai 400058',
      firmName: 'Patel Enterprises', firmId: 'FIRM-002',
      planType: 'YEARLY', status: 'PENDING',
      applicationId: 'APP-002', appliedAt: '2024-01-10T09:00:00Z',
      daysWaiting: 4, adminNotes: '',
    },
    {
      id: 'MEM-003', name: 'Priya Sharma', phone: '9988776655',
      email: 'priya@sharma.com', address: '8 Bandra East, Mumbai 400051',
      firmName: 'Sharma Industries', firmId: 'FIRM-001',
      planType: 'LIFETIME', status: 'APPROVED',
      applicationId: 'APP-003', appliedAt: '2024-01-08T14:00:00Z',
      daysWaiting: 0, adminNotes: '',
    },
    {
      id: 'MEM-004', name: 'Vijay Mehta', phone: '9871234560',
      email: 'vijay@mehta.com', address: '22 Dadar, Mumbai 400014',
      firmName: 'Mehta & Co', firmId: 'FIRM-003',
      planType: 'YEARLY', status: 'PENDING',
      applicationId: 'APP-004', appliedAt: '2024-01-12T11:00:00Z',
      daysWaiting: 2, adminNotes: '',
    },
    {
      id: 'MEM-005', name: 'Anita Desai', phone: '9765432109',
      email: 'anita@desai.com', address: '33 Kurla, Mumbai 400070',
      firmName: 'Desai Textiles', firmId: 'FIRM-004',
      planType: 'YEARLY', status: 'APPLIED',
      applicationId: 'APP-005', appliedAt: '2024-01-09T08:00:00Z',
      daysWaiting: 5, adminNotes: 'Documents pending AI review.',
    },
    {
      id: 'MEM-006', name: 'Ravi Nair', phone: '9654321098',
      email: 'ravi@nair.com', address: '7 Goregaon, Mumbai 400063',
      firmName: 'Nair Exports', firmId: 'FIRM-002',
      planType: 'LIFETIME', status: 'APPROVED',
      applicationId: 'APP-006', appliedAt: '2024-01-03T15:00:00Z',
      daysWaiting: 0, adminNotes: '',
    },
    {
      id: 'MEM-007', name: 'Sunita Joshi', phone: '9543210987',
      email: 'sunita@joshi.com', address: '56 Malad, Mumbai 400064',
      firmName: 'Joshi Brothers', firmId: 'FIRM-005',
      planType: 'YEARLY', status: 'REJECTED',
      applicationId: 'APP-007', appliedAt: '2024-01-06T12:00:00Z',
      daysWaiting: 0, adminNotes: 'Documents could not be verified.',
    },
    {
      id: 'MEM-008', name: 'Deepak Gupta', phone: '9432109876',
      email: 'deepak@gupta.com', address: '19 Borivali, Mumbai 400066',
      firmName: 'Gupta Trading', firmId: 'FIRM-003',
      planType: 'YEARLY', status: 'APPLIED',
      applicationId: 'APP-008', appliedAt: '2024-01-11T10:30:00Z',
      daysWaiting: 3, adminNotes: '',
    },
    {
      id: 'MEM-009', name: 'Kavita Singh', phone: '9321098765',
      email: 'kavita@singh.com', address: '41 Worli, Mumbai 400018',
      firmName: 'Singh Pharma', firmId: 'FIRM-004',
      planType: 'LIFETIME', status: 'SUSPENDED',
      applicationId: 'APP-009', appliedAt: '2024-01-02T09:00:00Z',
      daysWaiting: 0, adminNotes: 'Suspended for non-payment.',
    },
    {
      id: 'MEM-010', name: 'Arun Verma', phone: '9210987654',
      email: 'arun@verma.com', address: '28 Colaba, Mumbai 400005',
      firmName: 'Verma Chemicals', firmId: 'FIRM-005',
      planType: 'YEARLY', status: 'PENDING',
      applicationId: 'APP-010', appliedAt: '2024-01-13T13:00:00Z',
      daysWaiting: 1, adminNotes: '',
    },
  ],

  firms: [
    {
      id: 'FIRM-001', name: 'Kumar Traders',
      registrationNo: 'REG-MH-2019-001', industry: 'Trading',
      memberCount: 2, contactPerson: 'Ramesh Kumar',
      phone: '9876543210', email: 'info@kumartraders.com',
      address: '12 Marine Lines, Mumbai 400002',
      status: 'ACTIVE', createdAt: '2019-06-01T00:00:00Z',
    },
    {
      id: 'FIRM-002', name: 'Patel Enterprises',
      registrationNo: 'REG-MH-2020-002', industry: 'Manufacturing',
      memberCount: 2, contactPerson: 'Suresh Patel',
      phone: '9123456789', email: 'info@patelenterprises.com',
      address: '45 Andheri West, Mumbai 400058',
      status: 'ACTIVE', createdAt: '2020-03-15T00:00:00Z',
    },
    {
      id: 'FIRM-003', name: 'Mehta & Co',
      registrationNo: 'REG-MH-2018-003', industry: 'Logistics',
      memberCount: 2, contactPerson: 'Vijay Mehta',
      phone: '9871234560', email: 'info@mehtaco.com',
      address: '22 Dadar, Mumbai 400014',
      status: 'ACTIVE', createdAt: '2018-11-20T00:00:00Z',
    },
    {
      id: 'FIRM-004', name: 'Desai Textiles',
      registrationNo: 'REG-MH-2021-004', industry: 'Textiles',
      memberCount: 2, contactPerson: 'Anita Desai',
      phone: '9765432109', email: 'info@desaitextiles.com',
      address: '33 Kurla, Mumbai 400070',
      status: 'ACTIVE', createdAt: '2021-01-10T00:00:00Z',
    },
    {
      id: 'FIRM-005', name: 'Joshi Brothers',
      registrationNo: 'REG-MH-2017-005', industry: 'Retail',
      memberCount: 2, contactPerson: 'Sunita Joshi',
      phone: '9543210987', email: 'info@joshibrothers.com',
      address: '56 Malad, Mumbai 400064',
      status: 'INACTIVE', createdAt: '2017-05-05T00:00:00Z',
    },
  ],

  payments: [
    {
      id: 'PAY-001', memberId: 'MEM-001',
      planName: 'Yearly', amount: 1000, gstAmount: 180,
      platformFee: 50, totalAmount: 1230,
      status: 'SUCCESS', razorpayPaymentId: 'pay_mock_001',
      receiptNo: 'RCP-2024-001', createdAt: '2024-01-15T10:00:00Z',
    },
    {
      id: 'PAY-002', memberId: 'MEM-003',
      planName: 'Lifetime', amount: 5000, gstAmount: 900,
      platformFee: 100, totalAmount: 6000,
      status: 'SUCCESS', razorpayPaymentId: 'pay_mock_002',
      receiptNo: 'RCP-2024-002', createdAt: '2024-01-08T15:00:00Z',
    },
  ],

  staff: [
    { id: 'STAFF-001', name: 'Admin User', email: 'admin@chamber.com', role: 'admin', isActive: true, createdAt: '2023-01-01T00:00:00Z' },
    { id: 'STAFF-002', name: 'Bursar Singh', email: 'finance@chamber.com', role: 'finance', isActive: true, createdAt: '2023-03-15T00:00:00Z' },
    { id: 'STAFF-003', name: 'Front Desk Patel', email: 'operator@chamber.com', role: 'operator', isActive: true, createdAt: '2023-06-01T00:00:00Z' },
    { id: 'STAFF-004', name: 'Old Staff', email: 'old@chamber.com', role: 'admin', isActive: false, createdAt: '2022-01-01T00:00:00Z' },
  ],

  events: [
    {
      id: 'EVT-001', title: 'Annual General Meeting 2025',
      description: 'Yearly review of association activities, financial report, and election of new committee members.',
      type: 'MEETING', status: 'UPCOMING',
      date: '2025-06-15', time: '2:00 PM',
      venue: 'City Hall, Main Auditorium, Mumbai',
      totalSeats: 200, bookedSeats: 143, availableSeats: 57,
      ticketPrice: 0, isFree: true, isOnline: false, meetLink: null,
      rsvpDeadline: '2025-06-12', myRsvp: null,
    },
    {
      id: 'EVT-002', title: 'Trade Networking Mixer',
      description: 'An evening of networking for all association members. Light refreshments will be served.',
      type: 'NETWORKING', status: 'UPCOMING',
      date: '2025-07-10', time: '6:00 PM',
      venue: 'Taj Mahal Palace, Mumbai',
      totalSeats: 100, bookedSeats: 88, availableSeats: 12,
      ticketPrice: 500, isFree: false, isOnline: false, meetLink: null,
      rsvpDeadline: '2025-07-07', myRsvp: 'GOING',
    },
    {
      id: 'EVT-003', title: 'GST Compliance Webinar',
      description: 'Expert-led session on the latest GST updates and compliance requirements for SMEs.',
      type: 'WEBINAR', status: 'UPCOMING',
      date: '2025-06-28', time: '11:00 AM',
      venue: 'Online – Google Meet',
      totalSeats: 500, bookedSeats: 234, availableSeats: 266,
      ticketPrice: 0, isFree: true, isOnline: true,
      meetLink: 'https://meet.google.com/mock-link',
      rsvpDeadline: '2025-06-27', myRsvp: null,
    },
    {
      id: 'EVT-004', title: 'Export Policy Workshop 2024',
      description: 'A comprehensive deep-dive into the new export-import policies for FY2025.',
      type: 'WORKSHOP', status: 'PAST',
      date: '2024-11-20', time: '10:00 AM',
      venue: 'MMRDA Grounds, BKC, Mumbai',
      totalSeats: 150, bookedSeats: 150, availableSeats: 0,
      ticketPrice: 750, isFree: false, isOnline: false, meetLink: null,
      rsvpDeadline: '2024-11-18', myRsvp: 'GOING',
    },
  ],

  grievances: [
    {
      id: 'GRV-001', memberId: 'MEM-001', ticketNo: 'TKT-2024-001',
      subject: 'Digital ID not received after payment',
      description: 'I paid for yearly membership on Jan 15 but have not received my digital ID card yet. Ref: PAY-001.',
      category: 'DIGITAL_ID', status: 'RESOLVED', priority: 'HIGH',
      submittedAt: '2024-01-18T10:00:00Z',
      resolvedAt: '2024-01-19T14:00:00Z',
      adminResponse: 'Your Digital ID has been generated and sent to your registered email. Please check spam folder.',
    },
    {
      id: 'GRV-002', memberId: 'MEM-001', ticketNo: 'TKT-2024-002',
      subject: 'Cannot update firm address in portal',
      description: 'The "My Info" page does not save when I try to update my firm address. Getting an error.',
      category: 'OTHER', status: 'OPEN', priority: 'MEDIUM',
      submittedAt: '2024-02-05T09:30:00Z',
      resolvedAt: null, adminResponse: null,
    },
  ],

  broadcasts: [
    {
      id: 'BC-001', title: 'January Renewal Reminder',
      message: 'Your membership expires on Jan 31. Renew now at the portal.',
      channel: 'WHATSAPP', status: 'SENT',
      recipientCount: 248, sentAt: '2024-01-01T10:00:00Z',
      scheduledAt: null, createdAt: '2024-01-01T09:00:00Z',
    },
    {
      id: 'BC-002', title: 'AGM 2025 Announcement',
      message: 'The Annual General Meeting will be held on June 15 at City Hall. RSVP by June 12.',
      channel: 'WHATSAPP', status: 'SCHEDULED',
      recipientCount: 0, sentAt: null,
      scheduledAt: '2025-06-01T09:00:00Z', createdAt: '2025-05-20T11:00:00Z',
    },
    {
      id: 'BC-003', title: 'GST Webinar – Draft',
      message: 'Join our upcoming GST compliance webinar on June 28.',
      channel: 'SMS', status: 'DRAFT',
      recipientCount: 0, sentAt: null,
      scheduledAt: null, createdAt: '2025-05-25T15:00:00Z',
    },
  ],

  settings: {
    associationName: 'Demo Trade Chamber',
    logo: 'https://via.placeholder.com/200x80',
    primaryColor: '#1a73e8',
    yearlyFee: 1000, lifetimeFee: 5000,
    gstPercent: 18, platformFeeFlat: 50,
    gstNumber: 'GST27MOCK1234Z1Z5',
    address: '123 Chamber Road, Mumbai 400001',
    supportPhone: '+91 98765 43210',
    supportEmail: 'support@chamber.com',
    whatsappEnabled: true, smsEnabled: true,
    autoApproval: false, renewalReminderDays: 30,
  },
}

export function paginate(array, page = 1, limit = 10) {
  const p = parseInt(page, 10) || 1
  const l = parseInt(limit, 10) || 10
  const start = (p - 1) * l
  return {
    data: array.slice(start, start + l),
    total: array.length,
    page: p,
    limit: l,
  }
}

export function filterMembers(filters = {}) {
  let result = [...mockState.members]
  if (filters.status)
    result = result.filter(m => m.status === filters.status)
  if (filters.planType)
    result = result.filter(m => m.planType === filters.planType)
  if (filters.search) {
    const q = filters.search.toLowerCase()
    result = result.filter(m =>
      m.name.toLowerCase().includes(q) ||
      m.phone.includes(q) ||
      m.id.toLowerCase().includes(q)
    )
  }
  result.sort((a, b) => {
    const aUrgent = a.status === 'PENDING' || a.status === 'APPLIED'
    const bUrgent = b.status === 'PENDING' || b.status === 'APPLIED'
    if (aUrgent && bUrgent)
      return new Date(a.appliedAt) - new Date(b.appliedAt)
    return 0
  })
  return result
}
