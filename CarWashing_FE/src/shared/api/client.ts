export type AuthSession = {
  userID: string
  username: string
  fullName: string
  email: string
  role: 'Customer' | 'Staff' | 'BranchManager' | 'Admin'
  accessToken: string
  accessTokenExpiration: string
}
export type RegisterRequest = { username: string; password: string; confirmPassword: string; fullName: string; email: string; phoneNumber: string }
export type RegisterResponse = { userID: string; username: string; fullName: string; email: string; phoneNumber?: string; role: 'Customer'; createdAt: string }
export type ResetPasswordRequest = { token: string; newPassword: string; confirmPassword: string }
export type DashboardMetric = { label: string; value: number; format: string }
export type DashboardException = { type: string; severity: 'info' | 'warning' | 'danger'; title: string; detail: string; branchId?: string; branchName?: string; actionPath: string; occurredAt: string }
export type ManagerRevenuePeriod = { period: string; label: string; from: string; to: string; completedVehicles: number; grossRevenue: number; refundedAmount: number; netRevenue: number }
export type ManagerDashboard = { branchId: string; branchName: string; date: string; generatedAt: string; metrics: DashboardMetric[]; revenuePeriods: ManagerRevenuePeriod[]; actions: DashboardException[] }
export type ManagerBranchContext = { branchId: string; branchName: string }
export type AdminDashboard = { from: string; to: string; generatedAt: string; metrics: DashboardMetric[]; branches: Array<{ branchId: string; branchName: string; bookings: number; completionRate: number; noShowRate: number; netAmount: number; bayUtilization: number }> }
export type UserStatus = 'Active' | 'Inactive' | 'Suspended' | 'Deleted'
export type AdminUser = { userID: string; username: string; fullName: string; email: string; role: 'Admin' | 'BranchManager' | 'Staff' | 'Customer'; status: UserStatus; emailVerified: boolean; createdAt: string; isCurrentUser: boolean }
export type PagedResult<T> = { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; hasNext: boolean; hasPrevious: boolean }
export type BranchMembership = { id: string; membershipType: 'Manager' | 'Staff'; branchId: string; branchName: string; userId: string; isActive: boolean; effectiveFrom?: string; effectiveTo?: string }
export type BehavioralActionType = 'ViewPromotion' | 'Book' | 'CancelBooking' | 'LeaveFeedback' | 'RedeemReward'
export type BehavioralLog = { logID: string; customerID?: string; customerName?: string; actionType: BehavioralActionType; actionTime: string; pointsChanged: number; spendingAmount: number; notes?: string }
export type BehavioralLogFilter = { customerID?: string; actionType?: BehavioralActionType; from?: string; to?: string; page?: number; pageSize?: number }
export type CustomerFeedback = { washHistoryId: string; washDate: string; rating: number; feedback?: string; branchName: string; services: string[]; staffMembers: Array<{ staffUserId: string; staffName: string; workRole?: string }> }
export type CustomerFeedbackFilter = { from?: string; to?: string; rating?: number; branchId?: string; page?: number; pageSize?: number }
export type OperationalWashHistoryStaff = { staffUserId: string; staffName: string; workRole?: string; contributionPercent?: number }
export type OperationalWashHistory = { washHistoryId: string; washDate: string; customerName: string; vehiclePlate: string; branchName: string; actualTotalAmount: number; discountAmount: number; finalAmount: number; customerRating?: number; feedback?: string; services: string[]; staffMembers: OperationalWashHistoryStaff[] }
export type OperationalWashHistoryFilter = { from?: string; to?: string; search?: string; branchId?: string; page?: number; pageSize?: number }

export type CustomerVoucher = {
  id: string
  sourceId: string
  source: 'Promotion' | 'Reward'
  name: string
  description?: string
  type: string
  value: number
  maxDiscountAmount?: number
  pointsSpent?: number
  status: string
  expiresAt?: string
  canApply: boolean
}
export type LoyaltyDailyPoint = { date: string; issued: number; redeemed: number }
export type LoyaltyPointActivity = { transactionId: string; customerId: string; customerName: string; phoneNumber?: string; points: number; type: string; description?: string; createdAt: string }
export type Reward = { id: string; name: string; description?: string; type: 'FixedDiscount' | 'PercentageDiscount' | 'FreeService' | 'AddOnService'; pointsRequired: number; value: number; serviceId?: string; validFrom?: string; validTo?: string; usageLimitPerCustomer?: number; status: 'Active' | 'Inactive' | 'Archived'; createdAt: string }
export type RewardPayload = { name: string; description?: string; type: Reward['type']; pointsRequired: number; value: number; serviceId?: string; validFrom?: string; validTo?: string; usageLimitPerCustomer?: number; isActive: boolean }
export type LoyaltyTierDistribution = { tierName: string; customerCount: number }
export type LoyaltyDashboard = { activeCustomers: number; activeRewards: number; pointsIssued: number; pointsRedeemed: number; expiringPoints: number; promotionUsageRate: number; revenue: number; completedWashes: number; dailyPoints: LoyaltyDailyPoint[]; tierDistribution: LoyaltyTierDistribution[] }
export type LoyaltySegmentCustomer = { customerId: string; fullName: string; phoneNumber?: string; tierName?: string; currentPoints: number; lifetimePoints: number; lastVisitDate?: string; nearestExpiryDate?: string; expiringPoints: number; totalSpent: number }
export type LoyaltyOverview = { balance: { currentPoints: number; lifetimePoints: number; currentTier?: string; totalSpent: number; totalVisits: number }; nextTierName?: string; nextTierMinSpent: number; nextTierMinVisits: number; expiringPoints: number; nearestExpiryDate?: string; vouchers: CustomerVoucher[]; recentLedger: Array<{ id: string; points: number; type: string; description?: string; createdAt: string; expiryDate?: string }> }
export type Promotion = { id: string; name: string; code?: string; type: string; value: number; startDate: string; endDate: string; status: string }
export type PromotionAnalytics = { promotionId: string; recipientCount: number; appliedCount: number; usedCount: number; redemptionRate: number; discountValue: number; bookingRevenueAfterDiscount: number }
export type Customer360 = { customer: LoyaltySegmentCustomer; pointLedger: LoyaltyOverview['recentLedger']; vouchers: CustomerVoucher[]; washHistory: Array<{ id: string; washDate: string; finalAmount: number; customerRating?: number; feedback?: string }> }

type ApiEnvelope<T> = { success: boolean; data?: T; message?: string }
export type Vehicle = { vehicleID: string; licensePlate: string; vehicleType?: string; brand?: string; model?: string; color?: string; status: string }
export type Service = { id: string; name: string; description?: string; price: number; durationMinutes: number; isActive: boolean }
export type Branch = { id: string; name: string; address: string; phone?: string; openTime?: string; closeTime?: string; isActive: boolean }
export type BookingItem = { serviceId: string; serviceName: string; quantity: number; unitPrice: number; lineTotal: number; durationMinutesPerUnit: number }
export type BookingStaffWork = { staffUserId: string; staffName: string; contributionPercent: number; workRole?: string }
export type Booking = { id: string; vehicleId: string; branchId: string; bookingStartTime: string; bookingEndTime: string; status: string; totalAmount: number; washBayId?: string; washBayName?: string; assignedStaffId?: string; assignedStaffName?: string; staffWork: BookingStaffWork[]; version: number; items: BookingItem[]; note?: string; completedAt?: string; cancelledAt?: string; cancellationReason?: string; customerName?: string; vehiclePlate?: string; branchName?: string }
export type ManagerAttendance = { id: string; membershipId: string; staffName: string; status: string; note?: string; checkedInAt?: string; checkedOutAt?: string; lateMinutes: number; earlyLeaveMinutes: number; overtimeMinutes: number }
export type Workload = { staffUserId: string; staffName: string; vehiclesParticipated: number; vehiclesCompleted: number; equivalentVehicles: number; equivalentRevenue: number }
export type AvailabilitySlot = { startTime: string; endTime: string; availableBayCount: number }
export type Availability = { branchId: string; durationMinutes: number; slots: AvailabilitySlot[] }
export type PromotionApplication = { bookingId: string; promotionId: string; discountAmount: number; totalBeforeDiscount: number; totalAfterDiscount: number }
export type CustomerProfile = { fullName: string; email: string; phoneNumber?: string; currentPoints: number; tierName?: string }
export type WashHistory = { washHistoryID: string; bookingID: string; washDate: string; finalAmount: number; pointsEarned: number; customerRating?: number; vehiclePlate?: string; branchName?: string; services: string[] }
export type WashBay = { id: string; branchId: string; branchName: string; name: string; isActive: boolean; activeBookingCount: number; nextBookingAt?: string }
export type QueueBooking = { bookingId: string; branchId: string; washBayId?: string; serviceName: string; scheduledStart: string; checkedInAt: string; priority: number; position: number; estimatedStart: string }
export type StaffShift = { id: string; branchId: string; name: string; startsAt: string; endsAt: string; isActive: boolean; assignments: Array<{ id: string; userId: string; staffName: string; washBayName?: string }> }
export type Reconciliation = { paymentCount: number; paidAmount: number; refundedAmount: number; netAmount: number }
export type Payment = { id: string; bookingId: string; branchId: string; amount: number; method: string; status: string; createdAt: string; paidAt?: string; referenceNumber?: string; note?: string; refundedAmount: number; refundableAmount: number }
export type ShiftCapacity = { shiftId: string; assignedStaffCount: number; assignedBayCount: number; activeBayCount: number; availableStaffCount: number }
export type AvailableStaff = { userId: string; fullName: string; shiftId: string; washBayId?: string; washBayName?: string }
export type Attendance = { id: string; shiftAssignmentId: string; staffUserId: string; staffName: string; shiftId: string; shiftName: string; branchId: string; washBayName?: string; startsAt: string; endsAt: string; status: string; checkedInAt?: string; checkedOutAt?: string; lateMinutes: number; earlyLeaveMinutes: number; workedMinutes: number; isLocked: boolean; adminNote?: string }
export type AttendanceSummary = { groupKey: string; label: string; assignedShiftCount: number; completedShiftCount: number; lateCount: number; absentCount: number; plannedMinutes: number; workedMinutes: number }
export type AiCustomerAssistant = { recommendations: Array<{ serviceId: string; serviceName: string; price: number; reason?: string }>; eligibleOffers: Array<{ promotionId: string; name: string; description?: string; expiresAt: string; eligibilityNote: string }>; suggestedSlots: Array<{ startTime: string; endTime: string; availableBayCount: number; reason: string }>; loyaltySummary: string; careTip: string; isFallback: boolean; source: string }
export type AiFeedbackInsights = { from: string; to: string; branchId?: string; feedbackCount: number; ratingDistribution: Record<string, number>; themes: Array<{ theme: string; count: number }>; lowRatings: Array<{ washHistoryId: string; bookingId: string; rating: number; feedbackPreview?: string; washDate: string }>; isFallback: boolean; source: string }
export type AiOperationsCopilot = { answer: string; branchId?: string; evidence: Array<{ label: string; value: string; period: string }>; actions: Array<{ label: string; path: string }>; isFallback: boolean; source: string }

export class ApiError extends Error {
  readonly status?: number

  constructor(message: string, status?: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5152'
const sessionKey = 'autowash.session'

export function getStoredSession(): AuthSession | null {
  try {
    const value = sessionStorage.getItem(sessionKey)
    return value ? JSON.parse(value) as AuthSession : null
  } catch {
    return null
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const session = getStoredSession()
  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      ...(session ? { Authorization: `Bearer ${session.accessToken}` } : {}),
      ...init?.headers,
    },
  })
  if (!response.ok) {
    const body: unknown = await response.json().catch(() => null)
    const message = typeof body === 'object' && body !== null && 'detail' in body && typeof body.detail === 'string' ? body.detail : 'Yêu cầu không thành công. Vui lòng thử lại.'
    throw new ApiError(message, response.status)
  }
  return response.json() as Promise<T>
}

async function envelope<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await request<ApiEnvelope<T>>(path, init)
  if (!response.success || response.data === undefined) throw new ApiError(response.message ?? 'Yêu cầu không thành công.')
  return response.data
}

export const api = {
  register: (payload: RegisterRequest) => request<RegisterResponse>('/api/Auth/register', { method: 'POST', body: JSON.stringify(payload) }),
  forgotPassword: (email: string) => request<{ message: string }>('/api/Auth/forgot-password', { method: 'POST', body: JSON.stringify({ email }) }),
  resetPassword: (payload: ResetPasswordRequest) => request<{ message: string }>('/api/Auth/reset-password', { method: 'POST', body: JSON.stringify(payload) }),
  getManagerDashboard: (date: string, branchId?: string) => request<ManagerDashboard>(`/api/manager/dashboard?${new URLSearchParams({ date, ...(branchId ? { branchId } : {}) }).toString()}`),
  getManagerBranchContext: () => request<ManagerBranchContext>('/api/manager/branch-context'),
  getAdminDashboard: (from: string, to: string, branchId?: string) => request<AdminDashboard>(`/api/admin/dashboard?${new URLSearchParams({ from, to, ...(branchId ? { branchId } : {}) }).toString()}`),
  getAdminUsers: (params: { query?: string; role?: string; status?: UserStatus; page?: number; pageSize?: number } = {}) => request<PagedResult<AdminUser>>(`/api/admin/users?${new URLSearchParams({ page: String(params.page ?? 1), pageSize: String(params.pageSize ?? 20), ...(params.query ? { query: params.query } : {}), ...(params.role ? { role: params.role } : {}), ...(params.status ? { status: params.status } : {}) }).toString()}`),
  updateAdminUserStatus: (userId: string, status: Exclude<UserStatus, 'Deleted'>) => envelope<AdminUser>(`/api/admin/users/${userId}/status`, { method: 'PUT', body: JSON.stringify({ status }) }),
  updateAdminUserRole: (userId: string, role: AdminUser['role']) => envelope<AdminUser>(`/api/admin/users/${userId}/role`, { method: 'PUT', body: JSON.stringify({ role }) }),
  getBehavioralLogs: (filter: BehavioralLogFilter = {}) => envelope<PagedResult<BehavioralLog>>(`/api/admin/behavioral-logs?${new URLSearchParams({ page: String(filter.page ?? 1), pageSize: String(filter.pageSize ?? 20), ...(filter.customerID ? { customerID: filter.customerID } : {}), ...(filter.actionType ? { actionType: filter.actionType } : {}), ...(filter.from ? { from: filter.from } : {}), ...(filter.to ? { to: filter.to } : {}) }).toString()}`),
  downloadBehavioralLogs: async (filter: BehavioralLogFilter = {}) => {
    const session = getStoredSession()
    const query = new URLSearchParams({ ...(filter.customerID ? { customerID: filter.customerID } : {}), ...(filter.actionType ? { actionType: filter.actionType } : {}), ...(filter.from ? { from: filter.from } : {}), ...(filter.to ? { to: filter.to } : {}) })
    const response = await fetch(`${baseUrl}/api/admin/behavioral-logs/export?${query.toString()}`, { headers: session ? { Authorization: `Bearer ${session.accessToken}` } : {} })
    if (!response.ok) throw new ApiError('Không thể xuất nhật ký hành vi.', response.status)
    return response.blob()
  },
  getUserBranchMemberships: (userId: string) => request<BranchMembership[]>(`/api/admin/branch-workforce?userId=${encodeURIComponent(userId)}`),
  addStaffBranchMembership: (payload: { userId: string; branchId: string; effectiveFrom: string }) => request<unknown>('/api/admin/branch-workforce/staff', { method: 'POST', body: JSON.stringify(payload) }),
  addManagerBranchMembership: (payload: { userId: string; branchId: string }) => request<unknown>('/api/admin/branch-workforce/managers', { method: 'POST', body: JSON.stringify(payload) }),
  endStaffBranchMembership: (membershipId: string, effectiveTo: string) => request<BranchMembership>(`/api/admin/branch-workforce/staff/${membershipId}/end`, { method: 'PATCH', body: JSON.stringify({ effectiveTo }) }),
  deactivateManagerBranchMembership: (membershipId: string) => request<BranchMembership>(`/api/admin/branch-workforce/managers/${membershipId}/deactivate`, { method: 'PATCH' }),
  login: (payload: { username: string; password: string }) => request<AuthSession>('/api/Auth/login', { method: 'POST', body: JSON.stringify(payload) }),
  storeSession: (session: AuthSession) => sessionStorage.setItem(sessionKey, JSON.stringify(session)),
  getMyVouchers: () => request<CustomerVoucher[]>('/api/loyalty/me/vouchers'),
  getMyLoyaltyOverview: () => request<LoyaltyOverview>('/api/loyalty/me/overview'),
  getRewards: () => request<{ items: Reward[] }>('/api/loyalty/rewards?page=1&pageSize=20'),
  getAdminRewards: () => request<{ items: Reward[] }>('/api/loyalty/rewards?page=1&pageSize=100&includeInactive=true'),
  createReward: (payload: RewardPayload) => request<Reward>('/api/loyalty/rewards', { method: 'POST', body: JSON.stringify(payload) }),
  updateReward: (rewardId: string, payload: RewardPayload) => request<Reward>(`/api/loyalty/rewards/${rewardId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  archiveReward: (rewardId: string) => request<unknown>(`/api/loyalty/rewards/${rewardId}`, { method: 'DELETE' }),
  getLoyaltyDashboard: (fromDate?: string, toDate?: string) => request<LoyaltyDashboard>(`/api/loyalty/dashboard?${new URLSearchParams({ ...(fromDate ? { fromDate } : {}), ...(toDate ? { toDate } : {}) }).toString()}`),
  getLoyaltyPointActivity: (activity: 'issued' | 'redeemed', page = 1, pageSize = 20) => request<PagedResult<LoyaltyPointActivity>>(`/api/loyalty/dashboard/point-activity?${new URLSearchParams({ activity, page: String(page), pageSize: String(pageSize) }).toString()}`),
  getLoyaltySegment: (segment: 'at-risk' | 'expiring-points' | 'loyal', inactiveDays = 45) => request<LoyaltySegmentCustomer[]>(`/api/loyalty/segments?${new URLSearchParams({ segment, inactiveDays: String(inactiveDays) }).toString()}`),
  getCustomer360: (customerId: string) => request<Customer360>(`/api/loyalty/customers/${customerId}/360`),
  getPromotions: () => request<{ items: Promotion[] }>('/api/loyalty/promotions?page=1&pageSize=50&includeInactive=true'),
  getPromotionAnalytics: (promotionId: string) => request<PromotionAnalytics>(`/api/loyalty/promotions/${promotionId}/analytics`),
  previewCampaign: (payload: { promotion: { name: string; code?: string; description?: string; type: string; value: number; minimumSpend: number; startDate: string; endDate: string; totalUsageLimit?: number; usageLimitPerCustomer?: number; isActive: boolean }; audience: { segment: string; inactiveDays: number } }) => request<{ eligibleCount: number; excludedCount: number; exclusionReasons: string[] }>('/api/loyalty/campaigns/preview', { method: 'POST', body: JSON.stringify(payload) }),
  createCampaign: (payload: { promotion: { name: string; code?: string; description?: string; type: string; value: number; minimumSpend: number; startDate: string; endDate: string; totalUsageLimit?: number; usageLimitPerCustomer?: number; isActive: boolean }; audience: { segment: string; inactiveDays: number } }) => request<{ promotionId: string; sentCount: number; skippedCount: number }>('/api/loyalty/campaigns', { method: 'POST', body: JSON.stringify(payload) }),
  previewPromotionAudience: (promotionId: string, payload: { segment?: string; customerIds?: string[]; inactiveDays?: number }) => request<{ customers: LoyaltySegmentCustomer[]; eligibleCount: number; excludedCount: number }>(`/api/loyalty/promotions/${promotionId}/audience-preview`, { method: 'POST', body: JSON.stringify(payload) }),
  sendPromotion: (promotionId: string, payload: { segment?: string; customerIds?: string[]; inactiveDays?: number }) => request<{ promotionId: string; sentCount: number; skippedCount: number }>(`/api/loyalty/promotions/${promotionId}/send`, { method: 'POST', body: JSON.stringify(payload) }),
  redeemReward: (rewardId: string, bookingId?: string) => request<CustomerVoucher>(`/api/loyalty/me/rewards/${rewardId}/redeem`, {
    method: 'POST',
    body: JSON.stringify({ bookingId, idempotencyKey: crypto.randomUUID() }),
  }),
  getMyVehicles: () => envelope<Vehicle[]>('/api/vehicles/me'),
  createVehicle: (payload: { licensePlate: string; vehicleType: string; brand?: string; model?: string; color?: string }) => envelope<Vehicle>('/api/vehicles', { method: 'POST', body: JSON.stringify(payload) }),
  updateVehicle: (vehicleId: string, payload: { vehicleType: string; brand?: string; model?: string; color?: string }) => envelope<Vehicle>(`/api/vehicles/${vehicleId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  updateVehicleStatus: (vehicleId: string, status: 'Active' | 'Inactive') => envelope<Vehicle>(`/api/vehicles/${vehicleId}/status`, { method: 'PUT', body: JSON.stringify({ status }) }),
  getServices: (includeInactive = false) => request<{ items: Service[] }>(`/api/services?page=1&pageSize=100${includeInactive ? '&includeInactive=true' : ''}`),
  createService: (payload: Omit<Service, 'id' | 'isActive'>) => request<Service>('/api/services', { method: 'POST', body: JSON.stringify(payload) }),
  updateService: (serviceId: string, payload: Omit<Service, 'id'>) => request<Service>(`/api/services/${serviceId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deactivateService: (serviceId: string) => request<unknown>(`/api/services/${serviceId}`, { method: 'DELETE' }),
  getBranches: (includeInactive = false) => request<{ items: Branch[] }>(`/api/branches?page=1&pageSize=100&includeInactive=${includeInactive}`),
  getAccessibleBranches: () => request<Branch[]>('/api/bookings/accessible-branches'),
  createBranch: (payload: { name: string; address: string; phone?: string; openTime: string; closeTime: string }) => request<Branch>('/api/branches', { method: 'POST', body: JSON.stringify(payload) }),
  updateBranch: (branchId: string, payload: { name: string; address: string; phone?: string; openTime: string; closeTime: string; isActive: boolean }) => request<Branch>(`/api/branches/${branchId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deactivateBranch: (branchId: string) => request<unknown>(`/api/branches/${branchId}`, { method: 'DELETE' }),
  getAvailability: (payload: { branchId: string; date: string; items: Array<{ serviceId: string; quantity: number }> }) => request<Availability>('/api/bookings/availability', { method: 'POST', body: JSON.stringify(payload) }),
  getMyBookings: () => request<{ items: Booking[] }>('/api/bookings?page=1&pageSize=50'),
  getBooking: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}`),
  createBooking: (payload: { vehicleId: string; branchId: string; bookingStartTime: string; note?: string; items: Array<{ serviceId: string; quantity: number }> }) => request<Booking>('/api/bookings', { method: 'POST', body: JSON.stringify(payload) }),
  cancelBooking: (bookingId: string, payload: { reason?: string } = {}) => request<Booking>(`/api/bookings/${bookingId}/cancel`, { method: 'POST', body: JSON.stringify(payload) }),
  rescheduleBooking: (bookingId: string, payload: { bookingStartTime: string; washBayId?: string; note?: string; expectedVersion?: number }) => request<Booking>(`/api/bookings/${bookingId}/reschedule`, { method: 'POST', body: JSON.stringify(payload) }),
  getMyProfile: () => envelope<CustomerProfile>('/api/customers/me'),
  getCustomerAssistant: (payload: { preference?: string; branchId?: string; date?: string; serviceIds?: string[] } = {}) => envelope<AiCustomerAssistant>('/api/ai/customer/assistant', { method: 'POST', body: JSON.stringify(payload) }),
  getFeedbackInsights: (from: string, to: string, branchId?: string) => envelope<AiFeedbackInsights>(`/api/ai/feedback-insights?${new URLSearchParams({ from, to, ...(branchId ? { branchId } : {}) }).toString()}`),
  getCustomerFeedback: (filter: CustomerFeedbackFilter) => request<PagedResult<CustomerFeedback>>(`/api/feedback?${new URLSearchParams(Object.entries(filter).filter(([, value]) => value !== undefined && value !== '').map(([key, value]) => [key, String(value)])).toString()}`),
  getOperationalWashHistory: (filter: OperationalWashHistoryFilter) => request<PagedResult<OperationalWashHistory>>(`/api/wash-histories/operations?${new URLSearchParams(Object.entries(filter).filter(([, value]) => value !== undefined && value !== '').map(([key, value]) => [key, String(value)])).toString()}`),
  askOperationsCopilot: (payload: { message: string; from: string; to: string; branchId?: string }) => envelope<AiOperationsCopilot>('/api/ai/operations-copilot', { method: 'POST', body: JSON.stringify(payload) }),
  updateMyProfile: (payload: { fullName: string; email: string; phoneNumber?: string }) => envelope<CustomerProfile>('/api/customers/me', { method: 'PUT', body: JSON.stringify(payload) }),
  getMyWashHistory: () => envelope<{ items: WashHistory[] }>('/api/wash-histories/me?page=1&pageSize=50'),
  submitFeedback: (washHistoryId: string, payload: { rating: number; feedback?: string }) => envelope<unknown>(`/api/wash-histories/me/${washHistoryId}/feedback`, { method: 'POST', body: JSON.stringify(payload) }),
  applyPromotion: (promotionId: string, bookingId: string) => request<PromotionApplication>(`/api/loyalty/me/promotions/${promotionId}/apply`, { method: 'POST', body: JSON.stringify({ bookingId }) }),
  removePromotion: (promotionId: string, bookingId: string) => request<PromotionApplication>(`/api/loyalty/me/promotions/${promotionId}/bookings/${bookingId}`, { method: 'DELETE' }),
  applyRewardRedemption: (redemptionId: string, bookingId: string) => request<PromotionApplication>(`/api/loyalty/me/reward-redemptions/${redemptionId}/apply`, { method: 'POST', body: JSON.stringify({ bookingId }) }),
  removeRewardRedemption: (redemptionId: string, bookingId: string) => request<PromotionApplication>(`/api/loyalty/me/reward-redemptions/${redemptionId}/bookings/${bookingId}`, { method: 'DELETE' }),
  getWashBays: (branchId = '', includeInactive = false) => request<{ items: WashBay[] }>(`/api/wash-bays?${new URLSearchParams({ page: '1', pageSize: '100', ...(branchId ? { branchId } : {}), ...(includeInactive ? { includeInactive: 'true' } : {}) }).toString()}`),
  createWashBay: (payload: { branchId: string; name: string }) => request<WashBay>('/api/wash-bays', { method: 'POST', body: JSON.stringify(payload) }),
  updateWashBay: (washBayId: string, payload: { branchId: string; name: string; isActive: boolean }) => request<WashBay>(`/api/wash-bays/${washBayId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deactivateWashBay: (washBayId: string) => request<unknown>(`/api/wash-bays/${washBayId}`, { method: 'DELETE' }),
  getQueue: (branchId: string, washBayId?: string) => request<QueueBooking[]>(`/api/bookings/queue?branchId=${branchId}${washBayId ? `&washBayId=${washBayId}` : ''}`),
  checkInBooking: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}/check-in`, { method: 'POST' }),
  markNoShow: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}/no-show`, { method: 'POST' }),
  dispatchBooking: (bookingId: string, washBayId: string) => request<Booking>(`/api/bookings/${bookingId}/dispatch`, { method: 'POST', body: JSON.stringify({ washBayId }) }),
  getBookings: (params: { branchId?: string; status?: string; from?: string; to?: string } = {}) => {
    const query = new URLSearchParams({ page: '1', pageSize: '100' })
    if (params.branchId) query.set('branchId', params.branchId)
    if (params.status) query.set('status', params.status)
    if (params.from) query.set('fromDate', params.from)
    if (params.to) query.set('toDate', params.to)
    return request<{ items: Booking[] }>(`/api/bookings?${query.toString()}`)
  },
  confirmBooking: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}/confirm`, { method: 'POST' }),
  getManagerPendingBookings: () => request<Booking[]>('/api/bookings/manager/pending'),
  getManagerBookings: (date?: string) => request<Booking[]>(`/api/bookings/manager${date ? `?date=${encodeURIComponent(date)}` : ''}`),
  startBooking: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}/start`, { method: 'POST' }),
  completeBooking: (bookingId: string) => request<Booking>(`/api/bookings/${bookingId}/complete`, { method: 'POST' }),
  assignBookingStaff: (bookingId: string, payload: { staffUserId: string; expectedVersion?: number }) => request<Booking>(`/api/bookings/${bookingId}/assigned-staff`, { method: 'POST', body: JSON.stringify(payload) }),
  getEligibleBookingStaff: (bookingId: string) => request<Array<{ userId: string; fullName: string; membershipId: string }>>(`/api/bookings/${bookingId}/eligible-staff`),
  setBookingStaffWork: (bookingId: string, payload: { staff: Array<{ staffUserId: string; contributionPercent: number; workRole?: string }>; adjustmentReason?: string; expectedVersion?: number }) => request<Booking>(`/api/bookings/${bookingId}/staff-work`, { method: 'PUT', body: JSON.stringify(payload) }),
  getManagerAttendance: (workDate: string, branchId?: string) => request<ManagerAttendance[]>(`/api/manager/attendance?${new URLSearchParams({ workDate, ...(branchId ? { branchId } : {}) }).toString()}`),
  managerCheckIn: (membershipId: string) => request<ManagerAttendance>(`/api/manager/attendance/${membershipId}/check-in`, { method: 'POST' }),
  managerCheckOut: (membershipId: string) => request<ManagerAttendance>(`/api/manager/attendance/${membershipId}/check-out`, { method: 'POST' }),
  managerMarkAbsent: (membershipId: string, reason: string) => request<ManagerAttendance>(`/api/manager/attendance/${membershipId}/mark-absent`, { method: 'POST', body: JSON.stringify({ reason }) }),
  managerReinstateCheckIn: (membershipId: string, reason: string) => request<ManagerAttendance>(`/api/manager/attendance/${membershipId}/reinstate-check-in`, { method: 'POST', body: JSON.stringify({ reason }) }),
  getWorkload: (from: string, to: string, branchId?: string) => request<Workload[]>(`/api/manager/workload?${new URLSearchParams({ from, to, ...(branchId ? { branchId } : {}) }).toString()}`),
  getMyWorkload: (from: string, to: string) => request<Workload>(`/api/staff/workload?${new URLSearchParams({ from, to }).toString()}`),
  getShifts: (branchId?: string) => request<StaffShift[]>(`/api/staffing/shifts${branchId ? `?branchId=${branchId}` : ''}`),
  createShift: (payload: { branchId: string; startsAt: string; endsAt: string; name: string }) => request<StaffShift>('/api/staffing/shifts', { method: 'POST', body: JSON.stringify(payload) }),
  deactivateShift: (shiftId: string) => request<boolean>(`/api/staffing/shifts/${shiftId}`, { method: 'DELETE' }),
  getShiftCapacity: (shiftId: string) => request<ShiftCapacity>(`/api/staffing/shifts/${shiftId}/capacity`),
  getAvailableStaff: (branchId: string, startsAt: string, endsAt: string) => request<AvailableStaff[]>(`/api/staffing/shifts/available?branchId=${branchId}&startsAt=${encodeURIComponent(startsAt)}&endsAt=${encodeURIComponent(endsAt)}`),
  assignStaffToShift: (shiftId: string, payload: { userId: string; washBayId?: string }) => request<StaffShift>(`/api/staffing/shifts/${shiftId}/assignments`, { method: 'POST', body: JSON.stringify(payload) }),
  removeShiftAssignment: (shiftId: string, assignmentId: string) => request<boolean>(`/api/staffing/shifts/${shiftId}/assignments/${assignmentId}`, { method: 'DELETE' }),
  getMyAttendance: (date?: string) => request<Attendance[]>(`/api/attendance/me${date ? `?date=${encodeURIComponent(date)}` : ''}`),
  checkInAttendance: (assignmentId: string) => request<Attendance>(`/api/attendance/assignments/${assignmentId}/check-in`, { method: 'POST' }),
  checkOutAttendance: (assignmentId: string) => request<Attendance>(`/api/attendance/assignments/${assignmentId}/check-out`, { method: 'POST' }),
  getAttendance: (params: { branchId?: string; from: string; to: string; staffId?: string; status?: string }) => {
    const query = new URLSearchParams({ from: params.from, to: params.to })
    if (params.branchId) query.set('branchId', params.branchId)
    if (params.staffId) query.set('staffId', params.staffId)
    if (params.status) query.set('status', params.status)
    return request<Attendance[]>(`/api/attendance?${query.toString()}`)
  },
  adjustAttendance: (id: string, payload: { checkedInAt?: string; checkedOutAt?: string; status?: string; reason: string; adminNote?: string }) => request<Attendance>(`/api/attendance/${id}`, { method: 'PATCH', body: JSON.stringify(payload) }),
  lockAttendance: (id: string, reason: string) => request<Attendance>(`/api/attendance/${id}/lock`, { method: 'POST', body: JSON.stringify({ reason }) }),
  reopenAttendance: (id: string, reason: string) => request<Attendance>(`/api/attendance/${id}/reopen`, { method: 'POST', body: JSON.stringify({ reason }) }),
  getAttendanceSummary: (params: { branchId?: string; from: string; to: string; groupBy?: 'staff' | 'day' }) => request<AttendanceSummary[]>(`/api/attendance/summary?${new URLSearchParams({ ...(params.branchId ? { branchId: params.branchId } : {}), from: params.from, to: params.to, groupBy: params.groupBy ?? 'staff' }).toString()}`),
  getPayments: (params: { branchId?: string; from?: string; to?: string; status?: string } = {}) => {
    const query = new URLSearchParams({ page: '1', pageSize: '100' })
    if (params.branchId) query.set('branchId', params.branchId)
    if (params.from) query.set('from', params.from)
    if (params.to) query.set('to', params.to)
    if (params.status) query.set('status', params.status)
    return request<{ items: Payment[] }>(`/api/payments?${query.toString()}`)
  },
  downloadReconciliation: async (from: string, to: string, branchId?: string) => {
    const session = getStoredSession()
    const response = await fetch(`${baseUrl}/api/payments/reconciliation/export?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}${branchId ? `&branchId=${branchId}` : ''}`, { headers: session ? { Authorization: `Bearer ${session.accessToken}` } : {} })
    if (!response.ok) throw new ApiError('Không thể xuất báo cáo đối soát.', response.status)
    return response.blob()
  },
  getReconciliation: (from: string, to: string, branchId?: string) => request<Reconciliation>(`/api/payments/reconciliation?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}${branchId ? `&branchId=${branchId}` : ''}`),
  createRefund: (paymentId: string, payload: { amount: number; reason: string; referenceNumber?: string }) => request<unknown>(`/api/payments/${paymentId}/refunds`, { method: 'POST', body: JSON.stringify(payload) }),
}
