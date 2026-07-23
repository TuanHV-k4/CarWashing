import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { motion, MotionConfig, useReducedMotion } from 'motion/react'
import {
  ArrowRight,
  Bell,
  CalendarDays,
  CarFront,
  Check,
  ChevronRight,
  CircleDollarSign,
  Clock3,
  Droplets,
  Gauge,
  History,
  LayoutDashboard,
  LogIn,
  LogOut,
  Menu,
  Plus,
  ShieldCheck,
  Sparkles,
  TicketPercent,
  UserRound,
  Star,
  UsersRound,
  Gift,
  TriangleAlert,
  TrendingUp,
  X,
} from 'lucide-react'
import {
  BrowserRouter,
  Link,
  NavLink,
  Navigate,
  Route,
  Routes,
  useLocation,
  useNavigate,
} from 'react-router'
import { api, ApiError, getStoredSession, type Booking as CustomerBooking, type Branch, type CustomerVoucher, type Customer360, type LoyaltyDashboard, type LoyaltyPointActivity, type LoyaltySegmentCustomer, type Promotion, type PromotionAnalytics, type Reward, type RewardPayload, type Service, type WashBay, type WashHistory } from './shared/api/client'
import { Button, PageHeader, StatusBadge, Surface } from './shared/ui'
import { branchDateValue } from './shared/time'
import { RegisterPage } from './features/auth/RegisterPage'
import { ForgotPasswordPage } from './features/auth/ForgotPasswordPage'
import { ResetPasswordPage } from './features/auth/ResetPasswordPage'
import { ManagerDashboardPage } from './features/manager/ManagerDashboardPage'
import { ManagerBookingBoardPage } from './features/manager/ManagerBookingBoardPage'
import { OperationsBookingBoardPage } from './features/operations/OperationsBookingBoardPage'
import { OperationsQueuePage as StaffOperationsQueuePage } from './features/operations/OperationsQueuePage'
import { StaffWorkloadPage } from './features/operations/StaffWorkloadPage'
import { CustomerFeedbackPage } from './features/operations/CustomerFeedbackPage'
import { BranchWashHistoryPage } from './features/operations/BranchWashHistoryPage'
import { AdminDashboardPage } from './features/admin/AdminDashboardPage'
import { AdminUsersPage } from './features/admin/AdminUsersPage'
import { AdminBehavioralLogsPage } from './features/admin/AdminBehavioralLogsPage'
import { CustomerAiAssistantPage } from './features/customer/CustomerAiAssistantPage'
import { CompletedBookingDetailDialog, CustomerBookingsPage } from './features/customer/CustomerBookingsPage'
import { CustomerLoyaltyPage } from './features/customer/CustomerLoyaltyPage'
import { CampaignBuilder } from './features/loyalty/CampaignBuilder'
import loginBackdrop from './Mondlicht-Studios-Porsche-Zhanna-Travkina3.jpg'
import introVideo from './0721.mp4'

type BookingStatus = 'Pending' | 'Confirmed' | 'CheckedIn' | 'InProgress' | 'Completed' | 'Cancelled' | 'NoShow'

type Booking = {
  id: string
  customer: string
  plate: string
  service: string
  time: string
  status: BookingStatus
  bay?: string
}

const bookingColumns: Array<{ status: BookingStatus; label: string }> = [
  { status: 'Pending', label: 'Chờ xác nhận' },
  { status: 'Confirmed', label: 'Đã xác nhận' },
  { status: 'CheckedIn', label: 'Đã check-in' },
  { status: 'InProgress', label: 'Đang rửa' },
  { status: 'Completed', label: 'Hoàn tất' },
  { status: 'Cancelled', label: 'Đã hủy' },
]

const customerNav = [
  { to: '/customer/dashboard', label: 'Tổng quan', icon: LayoutDashboard },
  { to: '/customer/bookings/new', label: 'Đặt lịch', icon: CalendarDays },
  { to: '/customer/bookings', label: 'Lịch hẹn', icon: Clock3 },
  { to: '/customer/vehicles', label: 'Xe của tôi', icon: CarFront },
  { to: '/customer/loyalty', label: 'Thành viên', icon: Sparkles },
  { to: '/customer/assistant', label: 'Trợ lý AI', icon: Sparkles },
  { to: '/customer/history', label: 'Lịch sử', icon: History },
  { to: '/customer/profile', label: 'Hồ sơ', icon: UserRound },
]

function App() {
  return (
    <MotionConfig reducedMotion="user" transition={{ duration: 0.2, ease: [0.22, 1, 0.36, 1] }}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/customer/*" element={<RequireSession role="Customer"><CustomerRoutes /></RequireSession>} />
          <Route path="/operations/catalog" element={<RequireSession role="Admin"><Navigate to="/admin/catalog" replace /></RequireSession>} />
          <Route path="/operations/loyalty" element={<RequireSession role="Admin"><Navigate to="/admin/loyalty" replace /></RequireSession>} />
          <Route path="/operations/reconciliation" element={<RequireSession role="Admin"><Navigate to="/admin/reconciliation" replace /></RequireSession>} />
          <Route path="/operations/*" element={<RequireSession role="Operations"><OperationsRoutes /></RequireSession>} />
          <Route path="/manager/*" element={<RequireSession role="Manager"><ManagerRoutes /></RequireSession>} />
          <Route path="/admin/catalog" element={<RequireSession role="Admin"><AppShell mode="admin"><OperationsCatalogPage /></AppShell></RequireSession>} />
          <Route path="/admin/loyalty" element={<RequireSession role="Admin"><AppShell mode="admin"><LoyaltyConsolePage /></AppShell></RequireSession>} />
          <Route path="/admin/manager-dashboard" element={<RequireSession role="Admin"><AppShell mode="admin"><ManagerDashboardPage /></AppShell></RequireSession>} />
          <Route path="/admin/manager-attendance" element={<RequireSession role="Admin"><AppShell mode="admin"><ManagerAttendancePage /></AppShell></RequireSession>} />
          <Route path="/admin/*" element={<RequireSession role="Admin"><AdminRoutes /></RequireSession>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </MotionConfig>
  )
}

function RequireSession({ role, children }: { role: 'Customer' | 'Operations' | 'Manager' | 'Admin' | 'Catalog'; children: ReactNode }) {
  const session = getStoredSession()
  if (!session) return <Navigate to="/login" replace />
  const home = session.role === 'Customer' ? '/customer/dashboard' : session.role === 'Admin' ? '/admin/dashboard' : session.role === 'BranchManager' ? '/manager/bookings' : '/operations/dashboard'
  if (role === 'Customer' && session.role !== 'Customer') return <Navigate to={home} replace />
  if (role === 'Operations' && !['Staff', 'Admin'].includes(session.role)) return <Navigate to={home} replace />
  if (role === 'Manager' && !['BranchManager', 'Admin'].includes(session.role)) return <Navigate to={home} replace />
  if (role === 'Admin' && session.role !== 'Admin') return <Navigate to={home} replace />
  if (role === 'Catalog' && !['Admin', 'BranchManager'].includes(session.role)) return <Navigate to={home} replace />
  return <>{children}</>
}

function LandingPage() {
  return (
    <main className="landing-page">
      <header className="public-header">
        <Link className="brand" to="/"><span className="brand-mark"><Droplets size={20} /></span>AutoWash <em>Pro</em></Link>
        <Link className="button button-secondary" to="/login">Đăng nhập <LogIn size={17} /></Link>
      </header>
      <section className="landing-hero">
        <div className="hero-copy">
          <p className="eyebrow"><span /> Vận hành xe sạch, không gián đoạn</p>
          <h1>Rửa xe mượt mà.<br /><strong>Quản lý rõ ràng.</strong></h1>
          <p className="hero-lede">Đặt lịch, theo dõi tiến độ và tích điểm trong một trải nghiệm nhanh, sáng rõ và đáng tin cậy.</p>
          <div className="hero-actions">
            <Link className="button button-primary" to="/login">Bắt đầu ngay <ArrowRight size={18} /></Link>
            <a className="text-link" href="#how-it-works">Khám phá cách hoạt động <ChevronRight size={16} /></a>
          </div>
          <div className="hero-proof"><ShieldCheck size={18} /> Quản lý lịch hẹn, thanh toán và khách hàng tập trung</div>
        </div>
        <LandingVideo />
      </section>
      <section id="how-it-works" className="feature-strip" aria-label="Tính năng chính">
        <Feature icon={<CalendarDays />} title="Đặt lịch rõ ràng" copy="Chọn xe, dịch vụ, chi nhánh và thời gian theo từng bước." />
        <Feature icon={<Gauge />} title="Nắm tiến độ tức thì" copy="Theo dõi trạng thái booking và sân rửa theo thời gian thực." />
        <Feature icon={<Sparkles />} title="Tích điểm dễ hiểu" copy="Quản lý hạng thành viên, ưu đãi và phần thưởng tập trung." />
      </section>
    </main>
  )
}

function LandingVideo() {
  const shouldReduceMotion = useReducedMotion()
  return <div className="landing-video">
    <video autoPlay={!shouldReduceMotion} loop muted playsInline poster={loginBackdrop} preload="metadata" aria-label="Video giới thiệu AutoWash Pro">
      <source src={introVideo} type="video/mp4" />
    </video>
  </div>
}

function Feature({ icon, title, copy }: { icon: ReactNode; title: string; copy: string }) {
  return <article className="feature-item"><span className="feature-icon">{icon}</span><div><h2>{title}</h2><p>{copy}</p></div></article>
}

function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setIsLoading(true)
    try {
      const session = await api.login({ username, password })
      api.storeSession(session)
      navigate(session.role === 'Customer' ? '/customer/dashboard' : session.role === 'Admin' ? '/admin/dashboard' : session.role === 'BranchManager' ? '/manager/bookings' : '/operations/dashboard')
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể đăng nhập. Vui lòng thử lại.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="login-page">
      <section className="login-art" aria-hidden="true">
        <img className="login-backdrop" src={loginBackdrop} alt="" />
        <div className="login-art-copy"><p>AutoWash Pro</p><strong>Chăm sóc xe<br />đến từng chi tiết.</strong></div>
      </section>
      <section className="login-panel">
        <Link className="brand login-panel-brand" to="/"><span className="brand-mark"><Droplets size={20} /></span>AutoWash <em>Pro</em></Link>
        <div className="login-card">
          <Link className="back-link" to="/"><ChevronRight size={16} className="back-icon" /> Về trang giới thiệu</Link>
          <p className="eyebrow"><span /> Chào mừng trở lại</p>
          <h1>Đăng nhập để tiếp tục.</h1>
          <p className="muted">Sử dụng tài khoản AutoWash Pro của bạn.</p>
          <form onSubmit={submit} noValidate>
            <label htmlFor="username">Tên đăng nhập</label>
            <input id="username" autoComplete="username" value={username} onChange={(event) => setUsername(event.target.value)} required placeholder="Nhập tên đăng nhập" />
            <label htmlFor="password">Mật khẩu</label>
            <input id="password" type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} required placeholder="Nhập mật khẩu" />
            <p className="form-note"><Link className="text-link" to="/forgot-password">Quên mật khẩu?</Link></p>
            {error && <p className="form-error" role="alert">{error}</p>}
            <button className="button button-primary login-submit" disabled={isLoading} type="submit">{isLoading ? 'Đang đăng nhập…' : <>Đăng nhập <ArrowRight size={18} /></>}</button>
          </form>
          <p className="form-note">Chưa có tài khoản? <Link className="text-link" to="/register">Đăng ký Customer</Link></p>
        </div>
      </section>
    </main>
  )
}

function CustomerRoutes() {
  return <AppShell mode="customer"><Routes><Route path="dashboard" element={<CustomerDashboard />} /><Route path="bookings/new" element={<BookingFlow />} /><Route path="bookings" element={<CustomerBookingsPage />} /><Route path="vehicles" element={<VehicleGarage />} /><Route path="loyalty" element={<CustomerLoyaltyPage />} /><Route path="assistant" element={<CustomerAiAssistantPage />} /><Route path="history" element={<CustomerHistoryPage />} /><Route path="profile" element={<CustomerProfilePage />} /><Route path="*" element={<ComingSoon label="Không gian khách hàng" />} /></Routes></AppShell>
}

function OperationsRoutes() {
  return <AppShell mode="operations"><Routes><Route path="dashboard" element={<OperationsBookingBoardPage />} /><Route path="bookings" element={<OperationsBookingBoardPage boardOnly />} /><Route path="queue" element={<StaffOperationsQueuePage />} /><Route path="workload" element={<StaffWorkloadPage />} /><Route path="feedback" element={<CustomerFeedbackPage />} /><Route path="staffing" element={<Navigate to="../dashboard" replace />} /><Route path="attendance" element={<AttendancePage />} /><Route path="*" element={<ComingSoon label="Khu vực vận hành" />} /></Routes></AppShell>
}

function AdminRoutes() {
  return <AppShell mode="admin"><Routes><Route path="dashboard" element={<AdminDashboardPage />} /><Route path="users" element={<AdminUsersPage />} /><Route path="behavioral-logs" element={<AdminBehavioralLogsPage />} /><Route path="feedback" element={<CustomerFeedbackPage />} /><Route path="wash-history" element={<BranchWashHistoryPage />} /><Route path="branches" element={<AdminConsole />} /><Route path="staffing" element={<WorkloadPage />} /><Route path="attendance" element={<AttendancePage admin />} /><Route path="reconciliation" element={<ReconciliationPage />} /><Route path="*" element={<ComingSoon label="Khu vực quản trị" />} /></Routes></AppShell>
}

function ManagerRoutes() {
  return <AppShell mode="operations"><Routes><Route path="dashboard" element={<ManagerDashboardPage />} /><Route path="bookings" element={<ManagerBookingBoardPage />} /><Route path="attendance" element={<ManagerAttendancePage />} /><Route path="workload" element={<WorkloadPage />} /><Route path="feedback" element={<CustomerFeedbackPage />} /><Route path="wash-history" element={<BranchWashHistoryPage />} /><Route path="*" element={<Navigate to="dashboard" replace />} /></Routes></AppShell>
}

function AppShell({ mode, children }: { mode: 'customer' | 'operations' | 'admin'; children: ReactNode }) {
  const location = useLocation()
  const session = getStoredSession()
  const [menuOpen, setMenuOpen] = useState(false)
  const [managerBranchName, setManagerBranchName] = useState<string | null>(null)
  useEffect(() => {
    if (session?.role !== 'BranchManager') { setManagerBranchName(null); return }
    api.getManagerBranchContext().then(context => setManagerBranchName(context.branchName)).catch(() => setManagerBranchName(null))
  }, [session?.accessToken, session?.role])
  const nav = mode === 'customer' ? customerNav : mode === 'admin' ? [
    { to: '/admin/dashboard', label: 'Tổng quan', icon: LayoutDashboard },
    { to: '/admin/loyalty', label: 'Loyalty', icon: Sparkles },
    { to: '/admin/users', label: 'Người dùng', icon: UsersRound },
    { to: '/admin/behavioral-logs', label: 'Nhật ký hành vi', icon: History },
    { to: '/admin/feedback', label: 'Đánh giá khách hàng', icon: Star },
    { to: '/admin/wash-history', label: 'Lịch sử rửa xe', icon: History },
    { to: '/admin/branches', label: 'Chi nhánh', icon: Gauge },
    { to: '/admin/catalog', label: 'Danh mục', icon: CarFront },
    { to: '/admin/staffing', label: 'Khối lượng rửa', icon: UserRound },
    { to: '/admin/attendance', label: 'Chấm công', icon: Clock3 },
    { to: '/admin/reconciliation', label: 'Đối soát', icon: CircleDollarSign },
  ] : session?.role === 'BranchManager' ? [
    { to: '/manager/dashboard', label: 'Tổng quan', icon: LayoutDashboard },
    { to: '/manager/bookings', label: 'Duyệt lịch hẹn', icon: CalendarDays },
    { to: '/manager/attendance', label: 'Chấm công', icon: Clock3 },
    { to: '/manager/workload', label: 'Khối lượng rửa', icon: Gauge },
    { to: '/manager/feedback', label: 'Đánh giá khách hàng', icon: Star },
    { to: '/manager/wash-history', label: 'Lịch sử rửa xe', icon: History },
  ] : [
    { to: '/operations/dashboard', label: 'Tổng quan', icon: LayoutDashboard },
    { to: '/operations/bookings', label: 'Lịch hẹn', icon: CalendarDays },
    { to: '/operations/queue', label: 'Hàng đợi', icon: Clock3 },
    { to: '/operations/workload', label: 'Khối lượng rửa', icon: Gauge },
    { to: '/operations/feedback', label: 'Đánh giá khách hàng', icon: Star },
    { to: '/operations/attendance', label: 'Chấm công', icon: Clock3 },
  ]
  const displayName = session?.fullName || session?.username || 'Tài khoản'
  const initials = displayName.split(/\s+/).filter(Boolean).slice(-2).map((part) => part[0]).join('').toUpperCase() || 'AW'
  const title = location.pathname.includes('bookings/new') ? 'Đặt lịch rửa xe' : location.pathname.includes('bookings') ? 'Lịch hẹn hôm nay' : mode === 'customer' ? `Chào buổi sáng, ${displayName}` : mode === 'admin' ? 'Quản trị hệ thống' : 'Trung tâm vận hành'
  return <div className="app-shell">
    <aside className={menuOpen ? 'sidebar sidebar-open' : 'sidebar'}>
      <Link className="brand sidebar-brand" to={mode === 'customer' ? '/customer/dashboard' : mode === 'admin' ? '/admin/dashboard' : '/operations/dashboard'}><span className="brand-mark"><Droplets size={20} /></span>AutoWash <em>Pro</em></Link>
      <nav aria-label="Điều hướng chính">{nav.map((item) => <NavLink key={item.to} to={item.to} className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'} onClick={() => setMenuOpen(false)}><item.icon size={19} />{item.label}</NavLink>)}</nav>
      <div className="sidebar-footer"><span className="role-chip">{mode === 'customer' ? 'Khách hàng' : mode === 'admin' ? 'Quản trị viên' : 'Vận hành'}</span><button className="logout" onClick={() => { sessionStorage.removeItem('autowash.session'); window.location.assign('/') }}><LogOut size={17} /> Đăng xuất</button></div>
    </aside>
    <div className="app-main">
      <header className="app-header"><button className="icon-button menu-button" aria-label="Mở menu" onClick={() => setMenuOpen(!menuOpen)}>{menuOpen ? <X /> : <Menu />}</button><div><p className="crumb">AutoWash Pro / {mode === 'customer' ? 'Khách hàng' : mode === 'admin' ? 'Quản trị' : managerBranchName ? `Chi nhánh ${managerBranchName}` : 'Vận hành'}</p><h1>{title}</h1></div><div className="header-actions"><button className="icon-button" aria-label="Thông báo"><Bell size={19} /><i /></button><div className="avatar">{initials}</div><span className="user-name">{displayName}</span></div></header>
      <main id="main-content" className="workspace">{children}</main>
    </div>
  </div>
}

function CustomerDashboard() {
  const [profile, setProfile] = useState<{ fullName: string; currentPoints: number; tierName?: string } | null>(null)
  const [history, setHistory] = useState<WashHistory[]>([])
  const [bookings, setBookings] = useState<CustomerBooking[]>([])
  const [error, setError] = useState('')
  const [feedbackItem, setFeedbackItem] = useState<WashHistory | null>(null)
  const [feedbackRating, setFeedbackRating] = useState(5)
  const [feedbackText, setFeedbackText] = useState('')
  const [feedbackSaving, setFeedbackSaving] = useState(false)
  const [activityError, setActivityError] = useState('')
  const [detailBooking, setDetailBooking] = useState<CustomerBooking | null>(null)
  const [detailLoadingId, setDetailLoadingId] = useState<string | null>(null)
  const [detailError, setDetailError] = useState('')
  const loadDashboard = useCallback(async () => {
    try {
      const [customer, historyResult, bookingResult] = await Promise.all([api.getMyProfile(), api.getMyWashHistory(), api.getMyBookings()])
      setProfile(customer)
      setHistory(historyResult.items)
      setBookings(bookingResult.items)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải dữ liệu tổng quan.')
    }
  }, [])
  useEffect(() => { void loadDashboard() }, [loadDashboard])
  const latest = history[0]
  const bookingStatuses = [
    { status: 'Pending', label: 'Đang chờ', tone: 'warning' as const },
    { status: 'Confirmed', label: 'Đã xác nhận', tone: 'success' as const },
    { status: 'CheckedIn', label: 'Đã check-in', tone: 'info' as const },
    { status: 'InProgress', label: 'Đang phục vụ', tone: 'info' as const },
    { status: 'Completed', label: 'Hoàn tất', tone: 'success' as const },
    { status: 'Cancelled', label: 'Đã hủy', tone: 'danger' as const },
    { status: 'NoShow', label: 'Vắng mặt', tone: 'danger' as const },
  ]
  const customerBookings = [...bookings].sort((left, right) => new Date(right.bookingStartTime).getTime() - new Date(left.bookingStartTime).getTime())
  const activeBookingCount = bookings.filter((booking) => ['Pending', 'Confirmed', 'CheckedIn', 'InProgress'].includes(booking.status)).length
  async function submitDashboardFeedback(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!feedbackItem) return
    setFeedbackSaving(true)
    setActivityError('')
    try {
      await api.submitFeedback(feedbackItem.washHistoryID, { rating: feedbackRating, feedback: feedbackText.trim() || undefined })
      setFeedbackItem(null)
      setFeedbackText('')
      await loadDashboard()
    } catch (cause) {
      setActivityError(cause instanceof ApiError ? cause.message : 'Không thể gửi đánh giá. Vui lòng thử lại.')
    } finally {
      setFeedbackSaving(false)
    }
  }
  async function openActivityDetail(item: WashHistory) {
    setDetailLoadingId(item.washHistoryID)
    setDetailError('')
    try {
      const booking = await api.getBooking(item.bookingID)
      setDetailBooking({ ...booking, branchName: item.branchName ?? booking.branchName, vehiclePlate: item.vehiclePlate ?? booking.vehiclePlate, totalAmount: item.finalAmount })
    } catch (cause) {
      const message = cause instanceof ApiError ? cause.message : 'Không thể tải chi tiết lần rửa.'
      setDetailError(message)
      setActivityError(message)
      setError(message)
    } finally {
      setDetailLoadingId(null)
    }
  }
  if (!profile && !error) return <section className="surface" aria-busy="true"><p className="empty-board">Đang tải tổng quan…</p></section>
  return <div className="page-stack">{error && <p className="form-error" role="alert">{error}</p>}<section className="dashboard-hero"><div><p className="eyebrow"><span /> {profile?.tierName ? `Thành viên hạng ${profile.tierName}` : 'Thành viên mới'}</p><h2>Chào {profile?.fullName ?? 'bạn'}.</h2><p>{(profile?.currentPoints ?? 0) > 0 ? <>Bạn hiện có <strong>{profile?.currentPoints.toLocaleString('vi-VN')} điểm</strong>.</> : 'Bắt đầu đặt lịch để tích điểm và theo dõi hành trình chăm sóc xe.'}</p></div><Link className="button button-primary" to="/customer/bookings/new">Đặt lịch mới <Plus size={18} /></Link></section><section className="metric-grid"><Metric icon={<Sparkles />} label="Điểm hiện có" value={(profile?.currentPoints ?? 0).toLocaleString('vi-VN')} detail={profile?.tierName ?? 'Chưa có hạng thành viên'} tone="aqua" /><Metric icon={<CalendarDays />} label="Tổng lịch hẹn" value={String(bookings.length).padStart(2, '0')} detail={`${activeBookingCount} lịch đang xử lý`} tone="blue" /><Metric icon={<History />} label="Lần rửa gần nhất" value={latest ? new Date(latest.washDate).toLocaleDateString('vi-VN') : '—'} detail={latest?.services.join(' · ') ?? 'Chưa có lịch sử rửa'} tone="orange" /></section><section className="dashboard-grid"><section className="surface upcoming-bookings"><div className="section-heading"><div><p className="eyebrow"><span /> Lịch hẹn</p><h2>Lịch hẹn của bạn</h2></div><Link className="text-link" to="/customer/bookings">Quản lý lịch hẹn <ChevronRight size={16} /></Link></div>{customerBookings.length ? <div className="upcoming-booking-list">{customerBookings.map((booking) => { const status = bookingStatuses.find((item) => item.status === booking.status) ?? { label: booking.status, tone: 'default' as const }; return <details className="upcoming-booking-row" key={booking.id}><summary><div><strong>{new Date(booking.bookingStartTime).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</strong><p>{booking.items.map((item) => item.serviceName).join(' · ')}</p></div><StatusBadge tone={status.tone}>{status.label}</StatusBadge><span className="booking-detail-link">Xem chi tiết</span></summary><dl><div><dt>Dịch vụ</dt><dd>{booking.items.map((item) => `${item.serviceName} ×${item.quantity}`).join(' · ')}</dd></div><div><dt>Thời lượng</dt><dd>{booking.items.reduce((sum, item) => sum + item.durationMinutesPerUnit * item.quantity, 0)} phút</dd></div><div><dt>Tổng thanh toán</dt><dd>{booking.totalAmount.toLocaleString('vi-VN')}₫</dd></div></dl></details> })}</div> : <p className="empty-board">Đặt lịch đầu tiên để theo dõi trạng thái dịch vụ của bạn tại đây.</p>}</section><section className="surface recent-activity"><div className="section-heading"><div><p className="eyebrow"><span /> Gần đây</p><h2>Hoạt động của bạn</h2></div><Link className="text-link" to="/customer/history">Xem tất cả <ChevronRight size={16} /></Link></div>{history.length === 0 ? <p className="empty-board">Chưa có hoạt động nào.</p> : <ul className="activity-list activity-receipts">{history.slice(0, 2).map((item) => <li key={item.washHistoryID}><span className="activity-icon success"><Check /></span><div><strong>Hoàn tất rửa xe</strong><p>{item.services.join(' · ')}</p><small>{item.vehiclePlate ?? 'Xe của bạn'} · {item.branchName ?? 'Chi nhánh'} · {new Date(item.washDate).toLocaleDateString('vi-VN')}</small>{item.pointsEarned > 0 && <em>+{item.pointsEarned.toLocaleString('vi-VN')} điểm</em>}</div><div className="activity-actions"><b>{item.finalAmount.toLocaleString('vi-VN')}₫</b>{item.customerRating ? <span className="activity-rating"><Star size={13} fill="currentColor" /> {item.customerRating}/5</span> : <button className="text-link" type="button" onClick={() => { setFeedbackItem(item); setFeedbackRating(5); setFeedbackText(''); setActivityError('') }}>Đánh giá dịch vụ</button>}<button className="text-link" type="button" disabled={detailLoadingId === item.washHistoryID} onClick={() => void openActivityDetail(item)}>{detailLoadingId === item.washHistoryID ? 'Đang tải…' : 'Xem chi tiết'}</button></div></li>)}</ul>}{feedbackItem && <form className="dashboard-feedback-form" onSubmit={(event) => void submitDashboardFeedback(event)}><div><strong>Đánh giá {feedbackItem.services.join(' · ')}</strong><p>Phản hồi của bạn giúp AutoWash Pro cải thiện chất lượng dịch vụ.</p></div><label>Số sao<select value={feedbackRating} onChange={(event) => setFeedbackRating(Number(event.target.value))}>{[5, 4, 3, 2, 1].map((rating) => <option key={rating} value={rating}>{rating} sao</option>)}</select></label><label>Nhận xét (không bắt buộc)<textarea value={feedbackText} maxLength={1000} onChange={(event) => setFeedbackText(event.target.value)} /></label>{activityError && <p className="form-error" role="alert">{activityError}</p>}<div className="dashboard-feedback-actions"><Button variant="secondary" type="button" onClick={() => { setFeedbackItem(null); setActivityError('') }} disabled={feedbackSaving}>Hủy</Button><Button type="submit" loading={feedbackSaving}>Gửi đánh giá</Button></div></form>}</section></section><CompletedBookingDetailDialog booking={detailBooking} loading={false} error={detailError} onClose={() => { setDetailBooking(null); setDetailError('') }} /></div>
}

function VehicleGarage() {
  type VehicleDraft = { vehicleID?: string; licensePlate: string; vehicleType: string; brand: string; model: string; color: string }
  const [vehicles, setVehicles] = useState<Array<{ vehicleID: string; licensePlate: string; vehicleType?: string; brand?: string; model?: string; color?: string; status: string }>>([])
  const [draft, setDraft] = useState<VehicleDraft | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  function load() { setError(''); setLoading(true); api.getMyVehicles().then(setVehicles).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải danh sách xe.')).finally(() => setLoading(false)) }
  useEffect(load, [])
  async function save(event: React.FormEvent<HTMLFormElement>) { event.preventDefault(); if (!draft) return; setSaving(true); setError(''); try { if (draft.vehicleID) await api.updateVehicle(draft.vehicleID, draft); else await api.createVehicle(draft); setDraft(null); load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể lưu thông tin xe.') } finally { setSaving(false) } }
  async function toggle(vehicle: { vehicleID: string; status: string }) { setSaving(true); setError(''); try { await api.updateVehicleStatus(vehicle.vehicleID, vehicle.status === 'Active' ? 'Inactive' : 'Active'); load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật trạng thái xe.') } finally { setSaving(false) } }
  return <div className="page-stack vehicle-workspace"><PageHeader title="Xe của tôi" description="Quản lý phương tiện được dùng khi đặt lịch." actions={<Button onClick={() => setDraft({ licensePlate: '', vehicleType: 'Sedan', brand: '', model: '', color: '' })}><Plus size={17} /> Thêm xe</Button>} />{error && <p className="form-error" role="alert">{error}</p>}<div className="vehicle-grid">{loading && <div className="ui-skeleton"><span /><span /><span /></div>}{!loading && vehicles.length === 0 && <Surface className="vehicle-empty"><div className="ui-state"><CarFront size={26} /><strong>Chưa có xe nào</strong><p>Thêm xe đầu tiên để đặt lịch nhanh hơn.</p></div></Surface>}{vehicles.map((vehicle) => <Surface as="article" className="vehicle-card" key={vehicle.vehicleID}><div className="vehicle-card-top"><span className="vehicle-plate">{vehicle.licensePlate}</span><StatusBadge tone={vehicle.status === 'Active' ? 'success' : 'default'}>{vehicle.status === 'Active' ? 'Đang dùng' : 'Tạm ngưng'}</StatusBadge></div><h3>{[vehicle.brand, vehicle.model].filter(Boolean).join(' ') || 'Xe của bạn'}</h3><p>{[vehicle.vehicleType, vehicle.color].filter(Boolean).join(' · ') || 'Chưa có thông tin chi tiết'}</p><div className="vehicle-actions"><Button variant="ghost" onClick={() => setDraft({ vehicleID: vehicle.vehicleID, licensePlate: vehicle.licensePlate, vehicleType: vehicle.vehicleType ?? 'Sedan', brand: vehicle.brand ?? '', model: vehicle.model ?? '', color: vehicle.color ?? '' })}>Chỉnh sửa</Button><Button variant="secondary" disabled={saving} onClick={() => toggle(vehicle)}>{vehicle.status === 'Active' ? 'Tạm ngưng' : 'Kích hoạt'}</Button></div></Surface>)}</div>{draft && <aside className="admin-drawer" aria-label="Thông tin xe"><div className="drawer-heading"><div><p className="page-kicker">Phương tiện</p><h2>{draft.vehicleID ? 'Chỉnh sửa xe' : 'Thêm xe mới'}</h2></div><Button variant="ghost" aria-label="Đóng" onClick={() => setDraft(null)}>×</Button></div><form onSubmit={save}><label>Biển số xe<input value={draft.licensePlate} disabled={!!draft.vehicleID} onChange={(event) => setDraft({ ...draft, licensePlate: event.target.value.toUpperCase() })} required /></label><label>Loại xe<select value={draft.vehicleType} onChange={(event) => setDraft({ ...draft, vehicleType: event.target.value })}><option>Sedan</option><option>SUV</option><option>Hatchback</option><option>Pickup</option></select></label><label>Hãng xe<input value={draft.brand} onChange={(event) => setDraft({ ...draft, brand: event.target.value })} /></label><label>Dòng xe<input value={draft.model} onChange={(event) => setDraft({ ...draft, model: event.target.value })} /></label><label>Màu xe<input value={draft.color} onChange={(event) => setDraft({ ...draft, color: event.target.value })} /></label><div className="drawer-actions"><Button variant="secondary" type="button" onClick={() => setDraft(null)}>Hủy</Button><Button loading={saving} type="submit">Lưu xe</Button></div></form></aside>}</div>
}

function Metric({ icon, label, value, detail, tone }: { icon: ReactNode; label: string; value: string; detail: string; tone: string }) { return <article className="metric"><span className={`metric-icon ${tone}`}>{icon}</span><p>{label}</p><strong>{value}</strong><small>{detail}</small></article> }

function BookingFlow() {
  const navigate = useNavigate()
  const [step, setStep] = useState(1)
  const [vehicles, setVehicles] = useState<Array<{ vehicleID: string; licensePlate: string; brand?: string; model?: string }>>([])
  const [services, setServices] = useState<Array<{ id: string; name: string; description?: string; price: number; durationMinutes: number }>>([])
  const [branches, setBranches] = useState<Array<{ id: string; name: string }>>([])
  const [vouchers, setVouchers] = useState<Array<CustomerVoucher>>([])
  const [vehicleId, setVehicleId] = useState('')
  const [items, setItems] = useState<Record<string, number>>({})
  const [branchId, setBranchId] = useState('')
  const [date, setDate] = useState(() => new Date(Date.now() + 86400000).toISOString().slice(0, 10))
  const [slots, setSlots] = useState<Array<{ startTime: string; endTime: string; availableBayCount: number }>>([])
  const [selectedSlot, setSelectedSlot] = useState('')
  const [voucherId, setVoucherId] = useState('')
  const [result, setResult] = useState<{ bookingId: string; subtotal: number; discount: number; total: number; voucherId?: string; voucherSource?: 'Promotion' | 'Reward' } | null>(null)
  const [createdBooking, setCreatedBooking] = useState<{ id: string; total: number } | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [slotsLoading, setSlotsLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  useEffect(() => { Promise.all([api.getMyVehicles(), api.getServices(), api.getBranches(), api.getMyVouchers()]).then(([vehicleResult, serviceResult, branchResult, voucherResult]) => { setVehicles(vehicleResult); setServices(serviceResult.items); setBranches(branchResult.items); setVouchers(voucherResult); setVehicleId(vehicleResult[0]?.vehicleID ?? ''); setBranchId(branchResult.items[0]?.id ?? '') }).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải dữ liệu đặt lịch.')).finally(() => setLoading(false)) }, [])
  const selectedItems = Object.entries(items).filter(([, quantity]) => quantity > 0).map(([serviceId, quantity]) => ({ serviceId, quantity }))
  const subtotal = selectedItems.reduce((sum, item) => sum + (services.find((service) => service.id === item.serviceId)?.price ?? 0) * item.quantity, 0)
  const duration = selectedItems.reduce((sum, item) => sum + (services.find((service) => service.id === item.serviceId)?.durationMinutes ?? 0) * item.quantity, 0)
  const selectedServiceNames = selectedItems.map((item) => {
    const service = services.find((entry) => entry.id === item.serviceId)
    return service ? `${service.name}${item.quantity > 1 ? ` ×${item.quantity}` : ''}` : ''
  }).filter(Boolean)
  useEffect(() => {
    const summary = document.querySelector('.booking-summary dl')
    if (!summary) return
    let row = summary.querySelector<HTMLElement>('[data-selected-services]')
    if (!row) {
      row = document.createElement('div')
      row.dataset.selectedServices = 'true'
      summary.insertBefore(row, summary.children[3] ?? null)
    }
    row.replaceChildren()
    const label = document.createElement('dt')
    label.textContent = 'Gói dịch vụ'
    const value = document.createElement('dd')
    value.textContent = selectedServiceNames.join(' · ') || 'Chưa chọn'
    row.append(label, value)
  }, [selectedServiceNames.join('|')])
  useEffect(() => {
    const confirmationCopy = document.querySelector<HTMLElement>('.confirmation h3 + p')
    if (confirmationCopy) confirmationCopy.textContent = 'Chọn voucher nếu có. Lịch hẹn sẽ được gửi đến quản lý chi nhánh để xác nhận.'
    const successHeading = document.querySelector<HTMLElement>('.booking-success h3')
    if (successHeading) successHeading.textContent = 'Đã gửi yêu cầu đặt lịch'
  }, [result, step])
  useEffect(() => {
    const container = document.querySelector<HTMLElement>('.booking-form .slot-list')
    if (!container) return
    if (!slotsLoading) {
      container.querySelector('.slot-loading-state')?.remove()
      container.removeAttribute('aria-busy')
      return
    }

    container.replaceChildren()
    container.setAttribute('aria-busy', 'true')
    const status = document.createElement('div')
    status.className = 'slot-loading-state'
    status.setAttribute('role', 'status')
    const spinner = document.createElement('span')
    spinner.className = 'slot-loading-spinner'
    spinner.setAttribute('aria-hidden', 'true')
    const copy = document.createElement('span')
    copy.textContent = 'Đang tìm khung giờ trống...'
    status.append(spinner, copy)
    container.append(status)
  }, [slotsLoading])
  const selectedVoucher = vouchers.find((voucher) => voucher.id === voucherId)
  const estimatedDiscount = selectedVoucher
    ? Math.min(
      subtotal,
      Math.max(0, selectedVoucher.maxDiscountAmount == null
        ? selectedVoucher.type === 'PercentageDiscount' ? subtotal * selectedVoucher.value / 100 : selectedVoucher.value
        : Math.min(selectedVoucher.type === 'PercentageDiscount' ? subtotal * selectedVoucher.value / 100 : selectedVoucher.value, selectedVoucher.maxDiscountAmount))
    )
    : 0
  const estimatedTotal = subtotal - estimatedDiscount
  function changeQuantity(id: string, delta: number) { setItems((current) => ({ ...current, [id]: Math.max(0, (current[id] ?? 0) + delta) })) }
  async function loadSlots() { if (!branchId || !date || selectedItems.length === 0) { setError('Hãy chọn ít nhất một dịch vụ, chi nhánh và ngày.'); return } setError(''); setSlots([]); setSelectedSlot(''); setSlotsLoading(true); try { const availability = await api.getAvailability({ branchId, date, items: selectedItems }); setSlots(availability.slots); setSelectedSlot(availability.slots[0]?.startTime ?? '') } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải khung giờ trống.') } finally { setSlotsLoading(false) } }
  async function submit() { if (!vehicleId || !branchId || !selectedSlot || selectedItems.length === 0) return; setSubmitting(true); setError(''); try { let booking = createdBooking; if (!booking) { const created = await api.createBooking({ vehicleId, branchId, bookingStartTime: selectedSlot, items: selectedItems }); booking = { id: created.id, total: created.totalAmount }; setCreatedBooking(booking) } let final = { bookingId: booking.id, subtotal: booking.total, discount: 0, total: booking.total, voucherId: undefined as string | undefined, voucherSource: undefined as 'Promotion' | 'Reward' | undefined }; if (voucherId) { const voucher = vouchers.find((item) => item.id === voucherId); if (!voucher) throw new ApiError('Voucher không còn khả dụng.'); const applied = voucher.source === 'Promotion' ? await api.applyPromotion(voucher.sourceId, booking.id) : await api.applyRewardRedemption(voucher.id, booking.id); final = { bookingId: booking.id, subtotal: applied.totalBeforeDiscount, discount: applied.discountAmount, total: applied.totalAfterDiscount, voucherId: voucher.source === 'Promotion' ? voucher.sourceId : voucher.id, voucherSource: voucher.source } } setResult(final) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tạo lịch hẹn.') } finally { setSubmitting(false) } }
  async function removeVoucher() { if (!result?.voucherId || !result.voucherSource) return; setSubmitting(true); setError(''); try { const removed = result.voucherSource === 'Promotion' ? await api.removePromotion(result.voucherId, result.bookingId) : await api.removeRewardRedemption(result.voucherId, result.bookingId); setVoucherId(''); setResult({ bookingId: result.bookingId, subtotal: removed.totalAfterDiscount, discount: 0, total: removed.totalAfterDiscount }) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể bỏ voucher.') } finally { setSubmitting(false) } }
  if (loading) return <section className="surface" aria-busy="true"><p className="empty-board">Đang chuẩn bị form đặt lịch…</p></section>
  const selectedVehicle = vehicles.find((vehicle) => vehicle.vehicleID === vehicleId)
  const selectedBranch = branches.find((branch) => branch.id === branchId)
  return <div className="booking-layout"><section className="booking-form"><div className="stepper" aria-label="Các bước đặt lịch">{['Chọn xe', 'Dịch vụ', 'Khung giờ', 'Xác nhận'].map((label, index) => <div key={label} className={index + 1 <= step ? 'step active' : 'step'}><span>{index + 1 < step ? <Check size={15} /> : index + 1}</span><p>{label}</p></div>)}</div><div className="flow-content"><p className="eyebrow"><span /> Bước {step}/4</p><h2>{step === 1 ? 'Chọn xe của bạn' : step === 2 ? 'Chọn dịch vụ và add-on' : step === 3 ? 'Chọn chi nhánh và khung giờ' : 'Xác nhận lịch hẹn'}</h2>{error && <p className="form-error" role="alert">{error}</p>}{step === 1 && <div className="service-options">{vehicles.map((vehicle) => <button key={vehicle.vehicleID} onClick={() => setVehicleId(vehicle.vehicleID)} className={vehicleId === vehicle.vehicleID ? 'service-option selected' : 'service-option'}><span className="radio" /><strong>{vehicle.licensePlate}</strong><small>{[vehicle.brand, vehicle.model].filter(Boolean).join(' · ') || 'Xe của bạn'}</small></button>)}</div>}{step === 2 && <div className="service-options">{services.map((service) => <article className={items[service.id] ? 'service-option selected booking-service-row' : 'service-option booking-service-row'} key={service.id}><div><strong>{service.name}</strong><small>{service.description ?? `${service.durationMinutes} phút`}</small><b>{service.price.toLocaleString('vi-VN')}₫</b></div><div className="quantity-control"><button aria-label={`Giảm ${service.name}`} onClick={() => changeQuantity(service.id, -1)}>-</button><span>{items[service.id] ?? 0}</span><button aria-label={`Tăng ${service.name}`} onClick={() => changeQuantity(service.id, 1)}>+</button></div></article>)}</div>}{step === 3 && <div className="choice-grid"><label>Chi nhánh<select value={branchId} onChange={(event) => { setBranchId(event.target.value); setSlots([]) }}>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label><label>Ngày<input type="date" value={date} min={new Date().toISOString().slice(0, 10)} onChange={(event) => { setDate(event.target.value); setSlots([]) }} /></label><button className="button button-secondary" onClick={loadSlots}>Tìm khung giờ trống</button><div className="slot-list">{slots.map((slot) => <button key={slot.startTime} className={selectedSlot === slot.startTime ? 'slot-button selected' : 'slot-button'} onClick={() => setSelectedSlot(slot.startTime)}>{new Date(slot.startTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}<small>{slot.availableBayCount} bãi trống</small></button>)}</div></div>}{step === 4 && <div className="confirmation"><Check /><div><h3>Sẵn sàng tạo lịch hẹn</h3><p>Chọn voucher nếu có. Lịch hẹn sẽ được xác nhận ngay sau khi tạo.</p></div><label>Voucher<select value={voucherId} onChange={(event) => setVoucherId(event.target.value)} disabled={!!result}><option value="">Không dùng voucher</option>{vouchers.filter((voucher) => voucher.canApply).map((voucher) => <option key={voucher.id} value={voucher.id}>{voucher.name}</option>)}</select></label></div>}{result && <div className="booking-success"><Check /><div><h3>Lịch hẹn đã được xác nhận</h3><p>Tổng thanh toán cuối cùng: <strong>{result.total.toLocaleString('vi-VN')}₫</strong></p>{result.voucherId && <button className="text-link" disabled={submitting} onClick={removeVoucher}>Bỏ voucher</button>}<button className="text-link" onClick={() => navigate('/customer/dashboard')}>Về trang tổng quan</button></div></div>}<div className="flow-actions"><button className="button button-secondary" disabled={step === 1 || submitting || !!result} onClick={() => setStep(step - 1)}>Quay lại</button><button className="button button-primary" disabled={submitting || !!result || (step === 1 && !vehicleId) || (step === 2 && selectedItems.length === 0) || (step === 3 && !selectedSlot)} onClick={() => step === 4 ? submit() : setStep(step + 1)}>{submitting ? 'Đang tạo…' : step === 4 ? 'Tạo lịch hẹn' : <>Tiếp tục <ArrowRight size={17} /></>}</button></div></div></section><aside className="booking-summary"><p className="eyebrow"><span /> Tóm tắt</p><h2>Thông tin đặt lịch</h2><dl><div><dt>Xe</dt><dd>{selectedVehicle?.licensePlate ?? 'Chưa chọn'}</dd></div><div><dt>Dịch vụ</dt><dd>{selectedItems.length} lựa chọn</dd></div><div><dt>Chi nhánh</dt><dd>{selectedBranch?.name ?? 'Chưa chọn'}</dd></div><div><dt>Giờ đặt lịch</dt><dd>{selectedSlot ? new Date(selectedSlot).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' }) : 'Chưa chọn'}</dd></div><div><dt>Thời lượng</dt><dd>{duration} phút</dd></div><div><dt>Tạm tính</dt><dd>{subtotal.toLocaleString('vi-VN')}₫</dd></div>{(selectedVoucher || result) && <><div><dt>Giảm giá voucher</dt><dd>-{(result?.discount ?? estimatedDiscount).toLocaleString('vi-VN')}₫</dd></div><div><dt>Tổng phải trả</dt><dd>{(result?.total ?? estimatedTotal).toLocaleString('vi-VN')}₫</dd></div></>}</dl><div className="total"><span>{result ? 'Tổng cuối cùng' : 'Tổng tạm tính'}</span><strong>{(result?.total ?? estimatedTotal).toLocaleString('vi-VN')}₫</strong></div></aside></div>
}

function VoucherWallet() {
  const [vouchers, setVouchers] = useState<Array<{ id: string; sourceId: string; source: string; name: string; description?: string; value: number; type: string; pointsSpent?: number; status: string; canApply: boolean }>>([])
  const [rewards, setRewards] = useState<Array<{ id: string; name: string; description?: string; pointsRequired: number; value: number; type: string }>>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [redeeming, setRedeeming] = useState<string | null>(null)

  useEffect(() => {
    Promise.all([api.getMyVouchers(), api.getRewards()])
      .then(([wallet, catalog]) => { setVouchers(wallet); setRewards(catalog.items) })
      .catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải Ví voucher.'))
      .finally(() => setLoading(false))
  }, [])

  async function redeem(rewardId: string) {
    setRedeeming(rewardId)
    setError('')
    try {
      await api.redeemReward(rewardId)
      setVouchers(await api.getMyVouchers())
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể đổi ưu đãi.')
    } finally {
      setRedeeming(null)
    }
  }

  return <div className="page-stack"><section className="voucher-hero"><div><p className="eyebrow"><span /> Ưu đãi của bạn</p><h2>Ví voucher</h2><p>Đổi điểm thành ưu đãi và áp dụng ở bước xác nhận lịch hẹn.</p></div><Link className="button button-primary" to="/customer/bookings/new">Đặt lịch và áp dụng <ArrowRight size={17} /></Link></section>{error && <p className="form-error" role="alert">{error}</p>}<section className="voucher-grid" aria-busy={loading}>{loading && <p className="empty-board">Đang tải voucher…</p>}{!loading && vouchers.length === 0 && <div className="voucher-empty"><TicketPercent size={26} /><h3>Chưa có voucher sẵn sàng</h3><p>Đổi reward bằng điểm để sử dụng cho lịch hẹn tiếp theo.</p></div>}{vouchers.map((voucher) => <article className={voucher.canApply ? 'voucher-card' : 'voucher-card voucher-card-muted'} key={voucher.id}><span className="voucher-icon"><TicketPercent size={20} /></span><div><p>{voucher.source === 'Reward' ? 'Đổi từ điểm' : 'Khuyến mãi dành riêng'}</p><h3>{voucher.name}</h3><small>{voucher.description ?? 'Áp dụng khi đáp ứng điều kiện của voucher.'}</small></div><div className="voucher-value"><strong>{voucher.type === 'PercentageDiscount' ? `${voucher.value}%` : `${voucher.value.toLocaleString('vi-VN')}₫`}</strong><small>{voucher.status}</small></div></article>)}</section><section className="reward-section"><div className="section-heading"><div><p className="eyebrow"><span /> Đổi bằng điểm</p><h2>Reward có thể đổi</h2></div></div><div className="voucher-grid">{rewards.map((reward) => <article className="voucher-card" key={reward.id}><span className="voucher-icon"><Sparkles size={20} /></span><div><p>{reward.pointsRequired.toLocaleString('vi-VN')} điểm</p><h3>{reward.name}</h3><small>{reward.description ?? 'Đổi ngay để thêm vào Ví voucher.'}</small></div><div className="voucher-value"><strong>{reward.type === 'PercentageDiscount' ? `${reward.value}%` : `${reward.value.toLocaleString('vi-VN')}₫`}</strong></div><button className="button button-secondary" disabled={redeeming === reward.id} onClick={() => redeem(reward.id)}>{redeeming === reward.id ? 'Đang đổi…' : 'Đổi voucher'}</button></article>)}</div></section></div>
}

function CustomerProfilePage() {
  const [profile, setProfile] = useState<{ fullName: string; email: string; phoneNumber?: string; currentPoints: number; tierName?: string } | null>(null)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  useEffect(() => { api.getMyProfile().then(setProfile).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải hồ sơ.')) }, [])
  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!profile) return
    setSaving(true); setError('')
    try { setProfile(await api.updateMyProfile(profile)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật hồ sơ.') } finally { setSaving(false) }
  }
  if (!profile) return <section className="surface profile-page" aria-busy="true"><p className="empty-board">Đang tải hồ sơ…</p>{error && <p className="form-error" role="alert">{error}</p>}</section>
  return <section className="surface profile-page"><div className="section-heading"><div><p className="eyebrow"><span /> Tài khoản</p><h2>Hồ sơ của bạn</h2></div><span className="role-chip">{profile.tierName ?? 'Thành viên'} · {profile.currentPoints.toLocaleString('vi-VN')} điểm</span></div><form className="profile-form" onSubmit={submit}><label>Họ và tên<input value={profile.fullName} onChange={(event) => setProfile({ ...profile, fullName: event.target.value })} required /></label><label>Email<input type="email" value={profile.email} onChange={(event) => setProfile({ ...profile, email: event.target.value })} required /></label><label>Số điện thoại<input value={profile.phoneNumber ?? ''} onChange={(event) => setProfile({ ...profile, phoneNumber: event.target.value })} /></label>{error && <p className="form-error" role="alert">{error}</p>}<button className="button button-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Lưu thay đổi'}</button></form></section>
}

function CustomerHistoryPage() {
  const [items, setItems] = useState<Array<{ washHistoryID: string; washDate: string; finalAmount: number; customerRating?: number; vehiclePlate?: string; branchName?: string; services: string[] }>>([])
  const [error, setError] = useState('')
  const [feedback, setFeedback] = useState<{ id: string; rating: number; text: string } | null>(null)
  const [saving, setSaving] = useState(false)
  function load() { api.getMyWashHistory().then((result) => setItems(result.items)).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải lịch sử rửa.')) }
  useEffect(load, [])
  async function submitFeedback() {
    if (!feedback) return
    setSaving(true); setError('')
    try { await api.submitFeedback(feedback.id, { rating: feedback.rating, feedback: feedback.text || undefined }); setFeedback(null); load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể gửi đánh giá.') } finally { setSaving(false) }
  }
  return <div className="page-stack"><section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Dịch vụ đã hoàn tất</p><h2>Lịch sử rửa xe</h2></div></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{items.length === 0 && !error && <p className="empty-board">Chưa có lịch sử rửa xe.</p>}{items.map((item) => <article className="history-row" key={item.washHistoryID}><div><strong>{item.services.join(' · ')}</strong><p>{item.vehiclePlate ?? 'Xe của bạn'} · {item.branchName ?? 'Chi nhánh'} · {new Date(item.washDate).toLocaleDateString('vi-VN')}</p></div><div><b>{item.finalAmount.toLocaleString('vi-VN')}₫</b>{item.customerRating ? <small><Star size={13} fill="currentColor" /> {item.customerRating}/5</small> : <button className="text-link" onClick={() => setFeedback({ id: item.washHistoryID, rating: 5, text: '' })}>Đánh giá</button>}</div></article>)}</div></section>{feedback && <section className="surface feedback-panel"><div className="section-heading"><div><p className="eyebrow"><span /> Phản hồi dịch vụ</p><h2>Đánh giá lần rửa</h2></div><button className="text-link" onClick={() => setFeedback(null)}>Đóng</button></div><label>Số sao<select value={feedback.rating} onChange={(event) => setFeedback({ ...feedback, rating: Number(event.target.value) })}>{[5, 4, 3, 2, 1].map((rating) => <option key={rating} value={rating}>{rating} sao</option>)}</select></label><label>Nhận xét (không bắt buộc)<textarea value={feedback.text} onChange={(event) => setFeedback({ ...feedback, text: event.target.value })} maxLength={1000} /></label><button className="button button-primary" disabled={saving} onClick={submitFeedback}>{saving ? 'Đang gửi…' : 'Gửi đánh giá'}</button></section>}</div>
}

function OperationsQueuePage() {
  const [branches, setBranches] = useState<Array<{ id: string; name: string }>>([])
  const [bays, setBays] = useState<Array<{ id: string; name: string }>>([])
  const [branchId, setBranchId] = useState('')
  const [queue, setQueue] = useState<Array<{ bookingId: string; serviceName: string; position: number; priority: number; estimatedStart: string; washBayId?: string }>>([])
  const [error, setError] = useState('')
  const [working, setWorking] = useState<string | null>(null)
  useEffect(() => { api.getBranches().then((result) => { setBranches(result.items); setBranchId(result.items[0]?.id ?? '') }).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải chi nhánh.')) }, [])
  useEffect(() => { if (!branchId) return; Promise.all([api.getQueue(branchId), api.getWashBays(branchId)]).then(([queueResult, baysResult]) => { setQueue(queueResult); setBays(baysResult.items) }).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải hàng đợi.')) }, [branchId])
  async function act(id: string, type: 'checkin' | 'no-show' | 'dispatch') {
    setWorking(id); setError('')
    try { if (type === 'checkin') await api.checkInBooking(id); else if (type === 'no-show') await api.markNoShow(id); else if (bays[0]) await api.dispatchBooking(id, bays[0].id); if (branchId) setQueue(await api.getQueue(branchId)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Thao tác không thành công.') } finally { setWorking(null) }
  }
  return <div className="page-stack"><section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Điều phối trực tiếp</p><h2>Hàng đợi đã check-in</h2></div><label>Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{queue.length === 0 && <p className="empty-board">Chưa có khách đã check-in.</p>}{queue.map((item) => <article className="history-row" key={item.bookingId}><div><strong>#{item.position} · {item.serviceName}</strong><p>Ưu tiên {item.priority} · ETA {new Date(item.estimatedStart).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</p></div><div className="inline-actions"><button className="button button-secondary" disabled={working === item.bookingId} onClick={() => act(item.bookingId, 'dispatch')}>Gán bãi</button><button className="button button-secondary" disabled={working === item.bookingId} onClick={() => act(item.bookingId, 'no-show')}>No-show</button></div></article>)}</div></section></div>
}

function ManagerBookingApprovalsPage() {
  const [bookings, setBookings] = useState<CustomerBooking[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [working, setWorking] = useState('')
  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setBookings(await api.getManagerPendingBookings())
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải các lịch hẹn cần xác nhận.')
    } finally {
      setLoading(false)
    }
  }, [])
  useEffect(() => { void load() }, [load])
  async function confirm(bookingId: string) {
    setWorking(bookingId)
    setError('')
    try {
      await api.confirmBooking(bookingId)
      setBookings((current) => current.filter((booking) => booking.id !== bookingId))
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể xác nhận lịch hẹn.')
    } finally {
      setWorking('')
    }
  }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Manager</p><h2>Xác nhận lịch hẹn</h2><p>Kiểm tra thông tin người đặt lịch trước khi chuyển sang trạng thái đã xác nhận.</p></div><Button variant="secondary" onClick={() => void load()} loading={loading}>Tải lại</Button></div>{error && <p className="form-error" role="alert">{error}</p>}{loading ? <p className="empty-board" aria-busy="true">Đang tải lịch hẹn…</p> : <div className="history-list">{bookings.length === 0 && <p className="empty-board">Không có lịch hẹn nào đang chờ xác nhận.</p>}{bookings.map((booking) => <article className="history-row" key={booking.id}><div><strong>{booking.customerName || 'Khách hàng'} · {booking.vehiclePlate || booking.vehicleId}</strong><p>{booking.branchName || 'Chi nhánh'} · {new Date(booking.bookingStartTime).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</p><p>{booking.items.map((item) => `${item.serviceName} ×${item.quantity}`).join(' · ')}</p><details><summary>Xem thông tin đặt lịch</summary><p>Thời lượng: {booking.items.reduce((total, item) => total + item.durationMinutesPerUnit * item.quantity, 0)} phút · Tổng tiền: {booking.totalAmount.toLocaleString('vi-VN')}đ</p>{booking.note && <p>Ghi chú: {booking.note}</p>}</details></div><div className="inline-actions"><StatusBadge tone="warning">Chờ xác nhận</StatusBadge><Button onClick={() => void confirm(booking.id)} loading={working === booking.id}>Xác nhận</Button></div></article>)}</div>}</section>
}

export function LegacyManagerAttendancePage() {
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [items, setItems] = useState<Array<{ membershipId: string; staffName: string; status: string; note?: string; checkedInAt?: string; checkedOutAt?: string; lateMinutes: number; earlyLeaveMinutes: number; overtimeMinutes: number }>>([])
  const [error, setError] = useState('')
  const [working, setWorking] = useState('')
  const load = useCallback(() => api.getManagerAttendance(date).then(setItems).catch(cause => setError(cause instanceof ApiError ? cause.message : 'Không thể tải chấm công.')), [date])
  useEffect(() => { void load() }, [load])
  async function action(id: string, type: 'in' | 'out' | 'absent' | 'reinstate') {
    const todayInBangkok = new Date().toLocaleDateString('sv-SE', { timeZone: 'Asia/Bangkok' })
    if (date !== todayInBangkok) {
      setError('Chỉ có thể cập nhật chấm công cho ngày hiện tại.')
      return
    }
    setWorking(id)
    setError('')
    try {
      if (type === 'in') await api.managerCheckIn(id)
      else if (type === 'out') await api.managerCheckOut(id)
      else {
        const reason = window.prompt(type === 'absent' ? 'Lý do đánh dấu vắng:' : 'Lý do khôi phục và check-in:')?.trim()
        if (!reason) return
        if (type === 'absent') await api.managerMarkAbsent(id, reason)
        else await api.managerReinstateCheckIn(id, reason)
      }
      await load()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật chấm công.')
    } finally {
      setWorking('')
    }
  }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Manager</p><h2>Chấm công chi nhánh</h2></div><div className="inline-actions"><label>Ngày<input type="date" value={date} onChange={event => setDate(event.target.value)} /></label></div></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{items.map(item => <article className="history-row" key={item.membershipId}><div><strong>{item.staffName} <span className="bay-pill">{item.status}</span></strong><p>Vào: {item.checkedInAt ? new Date(item.checkedInAt).toLocaleTimeString('vi-VN') : '—'} · Ra: {item.checkedOutAt ? new Date(item.checkedOutAt).toLocaleTimeString('vi-VN') : '—'}</p><p>{item.status === 'Absent' ? `Lý do: ${item.note ?? '—'}` : `${item.lateMinutes > 0 ? `Trễ ${item.lateMinutes} phút` : 'Đúng giờ'}${item.overtimeMinutes > 0 ? ` · Tăng ca ${item.overtimeMinutes} phút` : ''}`}</p></div><div className="inline-actions">{item.status === 'NotCheckedIn' && <><button className="button button-primary" disabled={working === item.membershipId} onClick={() => action(item.membershipId, 'in')}>Check-in</button><button className="button button-secondary" disabled={working === item.membershipId} onClick={() => action(item.membershipId, 'absent')}>Đánh dấu vắng</button></>}{item.status === 'Absent' && <button className="button button-primary" disabled={working === item.membershipId} onClick={() => action(item.membershipId, 'reinstate')}>Khôi phục & check-in</button>}{item.checkedInAt && !item.checkedOutAt && <button className="button button-secondary" disabled={working === item.membershipId} onClick={() => action(item.membershipId, 'out')}>Check-out</button>}</div></article>)}</div></section>
}

export function LegacyWorkloadPage() {
  const [from, setFrom] = useState(() => `${branchDateValue().slice(0, 7)}-01`)
  const [to, setTo] = useState(branchDateValue)
  const [items, setItems] = useState<Array<{ staffUserId: string; staffName: string; vehiclesParticipated: number; vehiclesCompleted: number; equivalentVehicles: number; equivalentRevenue: number }>>([])
  const [error, setError] = useState('')
  async function load() { setError(''); try { setItems(await api.getWorkload(from, to)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải báo cáo.') } }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Workload</p><h2>Khối lượng rửa xe</h2></div><div className="inline-actions"><label>Từ<input type="date" value={from} onChange={event => setFrom(event.target.value)} /></label><label>Đến<input type="date" value={to} onChange={event => setTo(event.target.value)} /></label><button className="button button-primary" onClick={load}>Xem báo cáo</button></div></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{items.map(item => <article className="history-row" key={item.staffUserId}><div><strong>{item.staffName}</strong><p>{item.vehiclesParticipated} xe tham gia · {item.vehiclesCompleted} xe hoàn thành</p></div><div><strong>{item.equivalentVehicles.toFixed(2)} xe quy đổi</strong><p>{item.equivalentRevenue.toLocaleString('vi-VN')} ₫ doanh thu quy đổi</p></div></article>)}</div></section>
}

function ManagerAttendancePage() {
  const isAdmin = getStoredSession()?.role === 'Admin'
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [branches, setBranches] = useState<Branch[]>([])
  const [branchId, setBranchId] = useState('')
  const [items, setItems] = useState<Array<{ membershipId: string; staffName: string; status: string; note?: string; checkedInAt?: string; checkedOutAt?: string; lateMinutes: number; earlyLeaveMinutes: number; overtimeMinutes: number }>>([])
  const [error, setError] = useState('')
  const [working, setWorking] = useState('')
  const load = useCallback(() => {
    if (isAdmin && !branchId) { setItems([]); return Promise.resolve() }
    return api.getManagerAttendance(date, branchId || undefined).then(setItems).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Unable to load attendance.'))
  }, [branchId, date, isAdmin])
  useEffect(() => { void load() }, [load])
  useEffect(() => { if (isAdmin) api.getBranches().then((result) => setBranches(result.items)).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Unable to load branches.')) }, [isAdmin])
  async function action(id: string, type: 'in' | 'out' | 'absent' | 'reinstate') {
    if (isAdmin) { setError('Chấm công cần được thực hiện bởi Branch Manager.'); return }
    const todayInBangkok = new Date().toLocaleDateString('sv-SE', { timeZone: 'Asia/Bangkok' })
    if (date !== todayInBangkok) { setError('Chỉ có thể cập nhật chấm công cho ngày hiện tại.'); return }
    setWorking(id); setError('')
    try {
      if (type === 'in') await api.managerCheckIn(id)
      else if (type === 'out') await api.managerCheckOut(id)
      else {
        const reason = window.prompt(type === 'absent' ? 'Lý do đánh dấu vắng:' : 'Lý do khôi phục và check-in:')?.trim()
        if (!reason) return
        if (type === 'absent') await api.managerMarkAbsent(id, reason)
        else await api.managerReinstateCheckIn(id, reason)
      }
      await load()
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Unable to update attendance.') } finally { setWorking('') }
  }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Manager</p><h2>Chấm công chi nhánh</h2></div><div className="inline-actions">{isAdmin && <label>Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}><option value="">Chọn chi nhánh</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}<label>Ngày<input type="date" value={date} onChange={event => setDate(event.target.value)} /></label></div></div>{isAdmin && !branchId ? <div className="ui-state"><strong>Chọn chi nhánh</strong><p>Chọn chi nhánh để xem chấm công.</p></div> : <><p className="form-error" role="alert">{error}</p><div className="history-list">{items.map(item => <article className="history-row" key={item.membershipId}><div><strong>{item.staffName} <span className="bay-pill">{item.status}</span></strong><p>Vào: {item.checkedInAt ? new Date(item.checkedInAt).toLocaleTimeString('vi-VN') : '—'} · Ra: {item.checkedOutAt ? new Date(item.checkedOutAt).toLocaleTimeString('vi-VN') : '—'}</p></div>{!isAdmin && <div className="inline-actions">{item.status === 'NotCheckedIn' && <><button className="button button-primary" disabled={working === item.membershipId} onClick={() => void action(item.membershipId, 'in')}>Check-in</button><button className="button button-secondary" disabled={working === item.membershipId} onClick={() => void action(item.membershipId, 'absent')}>Vắng</button></>}{item.status === 'Absent' && <button className="button button-primary" disabled={working === item.membershipId} onClick={() => void action(item.membershipId, 'reinstate')}>Khôi phục</button>}{item.checkedInAt && !item.checkedOutAt && <button className="button button-secondary" disabled={working === item.membershipId} onClick={() => void action(item.membershipId, 'out')}>Check-out</button>}</div>}</article>)}</div></>}</section>
}

function WorkloadPage() {
  const isAdmin = getStoredSession()?.role === 'Admin'
  const [from, setFrom] = useState(() => `${branchDateValue().slice(0, 7)}-01`)
  const [to, setTo] = useState(branchDateValue)
  const [branches, setBranches] = useState<Branch[]>([])
  const [branchId, setBranchId] = useState('')
  const [items, setItems] = useState<Array<{ staffUserId: string; staffName: string; vehiclesParticipated: number; vehiclesCompleted: number; equivalentVehicles: number; equivalentRevenue: number }>>([])
  const [error, setError] = useState('')
  useEffect(() => { if (isAdmin) api.getBranches().then((result) => setBranches(result.items)).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Unable to load branches.')) }, [isAdmin])
  async function load() { if (isAdmin && !branchId) { setError('Chọn chi nhánh trước khi xem báo cáo.'); return }; setError(''); try { setItems(await api.getWorkload(from, to, branchId || undefined)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Unable to load workload report.') } }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Workload</p><h2>Khối lượng rửa xe</h2></div><div className="inline-actions">{isAdmin && <label>Chi nhánh<select value={branchId} onChange={event => setBranchId(event.target.value)}><option value="">Chọn chi nhánh</option>{branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}<label>Từ<input type="date" value={from} onChange={event => setFrom(event.target.value)} /></label><label>Đến<input type="date" value={to} onChange={event => setTo(event.target.value)} /></label><button className="button button-primary" onClick={load}>Xem báo cáo</button></div></div>{error && <p className="form-error" role="alert">{error}</p>}{isAdmin && !branchId ? <div className="ui-state"><strong>Chọn chi nhánh</strong><p>Chọn chi nhánh để xem khối lượng công việc.</p></div> : <div className="history-list">{items.map(item => <article className="history-row" key={item.staffUserId}><div><strong>{item.staffName}</strong><p>{item.vehiclesParticipated} xe tham gia · {item.vehiclesCompleted} xe hoàn thành</p></div><div><strong>{item.equivalentVehicles.toFixed(2)} xe quy đổi</strong><p>{item.equivalentRevenue.toLocaleString('vi-VN')} ₫ doanh thu quy đổi</p></div></article>)}</div>}</section>
}

function StaffingPage() {
  const [branches, setBranches] = useState<Array<{ id: string; name: string }>>([])
  const [branchId, setBranchId] = useState('')
  const [shifts, setShifts] = useState<Array<{ id: string; name: string; startsAt: string; endsAt: string; isActive: boolean; assignments: Array<{ id: string; userId: string; staffName: string }> }>>([])
  const [name, setName] = useState('Ca mới')
  const [startsAt, setStartsAt] = useState(() => new Date(Date.now() + 3600000).toISOString().slice(0, 16))
  const [endsAt, setEndsAt] = useState(() => new Date(Date.now() + 5 * 3600000).toISOString().slice(0, 16))
  const [selectedShift, setSelectedShift] = useState<string | null>(null)
  const [availableStaff, setAvailableStaff] = useState<Array<{ userId: string; fullName: string }>>([])
  const [staffId, setStaffId] = useState('')
  const [bays, setBays] = useState<Array<{ id: string; name: string }>>([])
  const [washBayId, setWashBayId] = useState('')
  const [error, setError] = useState('')
  useEffect(() => { api.getBranches().then((result) => { setBranches(result.items); setBranchId(result.items[0]?.id ?? '') }).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải chi nhánh.')) }, [])
  const load = useCallback(() => { if (branchId) return api.getShifts(branchId).then(setShifts).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải ca làm.')); return Promise.resolve() }, [branchId])
  useEffect(() => { void load() }, [load])
  async function create() { if (!branchId) return; setError(''); try { await api.createShift({ branchId, name, startsAt: new Date(startsAt).toISOString(), endsAt: new Date(endsAt).toISOString() }); load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tạo ca.') } }
  async function deactivate(id: string) { try { await api.deactivateShift(id); load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể ngừng ca.') } }
  async function openAssignment(shift: typeof shifts[number]) { setSelectedShift(shift.id); setStaffId(''); setWashBayId(''); setError(''); try { const [staff, bayResult] = await Promise.all([api.getAvailableStaff(branchId, shift.startsAt, shift.endsAt), api.getWashBays(branchId)]); setAvailableStaff(staff); setBays(bayResult.items) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải dữ liệu phân công.') } }
  async function assign() { if (!selectedShift || !staffId) return; setError(''); try { await api.assignStaffToShift(selectedShift, { userId: staffId, ...(washBayId ? { washBayId } : {}) }); setSelectedShift(null); setAvailableStaff([]); await load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể phân công nhân viên.') } }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Admin</p><h2>Ca làm và phân công</h2></div><label>Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label></div><div className="inline-actions shift-create"><input value={name} onChange={(event) => setName(event.target.value)} aria-label="Tên ca" /><input type="datetime-local" value={startsAt} onChange={(event) => setStartsAt(event.target.value)} aria-label="Bắt đầu ca" /><input type="datetime-local" value={endsAt} onChange={(event) => setEndsAt(event.target.value)} aria-label="Kết thúc ca" /><button className="button button-primary" onClick={create}>Tạo ca</button></div>{selectedShift && <div className="inline-actions shift-create"><select value={staffId} onChange={(event) => setStaffId(event.target.value)} aria-label="Nhân viên có sẵn"><option value="">Chọn nhân viên</option>{availableStaff.map((staff) => <option key={staff.userId} value={staff.userId}>{staff.fullName}</option>)}</select><select value={washBayId} onChange={(event) => setWashBayId(event.target.value)} aria-label="Bãi rửa"><option value="">Không gán bãi</option>{bays.map((bay) => <option key={bay.id} value={bay.id}>{bay.name}</option>)}</select><button className="button button-primary" disabled={!staffId} onClick={assign}>Xác nhận phân công</button><button className="button button-secondary" onClick={() => setSelectedShift(null)}>Hủy</button></div>}{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{shifts.map((shift) => <article className="history-row" key={shift.id}><div><strong>{shift.name}</strong><p>{new Date(shift.startsAt).toLocaleString('vi-VN')} – {new Date(shift.endsAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} · {shift.assignments.length} nhân viên</p>{shift.assignments.length > 0 && <p>{shift.assignments.map((assignment) => assignment.staffName).join(', ')}</p>}</div><div className="inline-actions">{shift.isActive && <button className="button button-secondary" onClick={() => openAssignment(shift)}>Phân công</button>}{shift.isActive && <button className="button button-secondary" onClick={() => deactivate(shift.id)}>Ngừng ca</button>}</div></article>)}</div></section>
}

function AttendancePage({ admin = false }: { admin?: boolean }) {
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [items, setItems] = useState<Array<{ id: string; shiftAssignmentId: string; staffName: string; shiftName: string; washBayName?: string; startsAt: string; endsAt: string; status: string; checkedInAt?: string; checkedOutAt?: string; lateMinutes: number; earlyLeaveMinutes: number; workedMinutes: number; isLocked: boolean }>>([])
  const [summary, setSummary] = useState<Array<{ groupKey: string; label: string; assignedShiftCount: number; completedShiftCount: number; lateCount: number; absentCount: number; plannedMinutes: number; workedMinutes: number }>>([])
  const [branches, setBranches] = useState<Array<{ id: string; name: string }>>([])
  const [branchId, setBranchId] = useState('')
  const [status, setStatus] = useState('')
  const [error, setError] = useState('')
  const [working, setWorking] = useState<string | null>(null)
  const from = `${date}T00:00:00Z`
  const to = `${date}T23:59:59Z`
  const load = useCallback(async () => {
    setError('')
    try {
      if (admin) {
        const [records, report] = await Promise.all([api.getAttendance({ branchId: branchId || undefined, status: status || undefined, from, to }), api.getAttendanceSummary({ branchId: branchId || undefined, from, to })])
        setItems(records); setSummary(report)
      } else setItems(await api.getMyAttendance(date))
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải dữ liệu chấm công.') }
  }, [admin, branchId, date, from, status, to])
  useEffect(() => { if (admin) api.getBranches().then((result) => setBranches(result.items)).catch(() => undefined) }, [admin])
  useEffect(() => { void load() }, [load])
  async function selfAction(item: typeof items[number]) {
    setWorking(item.shiftAssignmentId); setError('')
    try { if (!item.checkedInAt) await api.checkInAttendance(item.shiftAssignmentId); else if (!item.checkedOutAt) await api.checkOutAttendance(item.shiftAssignmentId); await load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể chấm công.') } finally { setWorking(null) }
  }
  async function toggleLock(item: typeof items[number]) {
    if (!item.id) return
    const reason = window.prompt(item.isLocked ? 'Lý do mở lại bản ghi:' : 'Lý do chốt công:')?.trim()
    if (!reason) return
    setWorking(item.id); setError('')
    try { if (item.isLocked) await api.reopenAttendance(item.id, reason); else await api.lockAttendance(item.id, reason); await load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật bản ghi.') } finally { setWorking(null) }
  }
  async function adjust(item: typeof items[number]) {
    if (!item.id || item.isLocked) return
    const reason = window.prompt('Lý do điều chỉnh:')?.trim()
    if (!reason) return
    const checkedInAt = window.prompt('Giờ vào (ISO, bỏ trống để giữ nguyên):', item.checkedInAt ?? '')?.trim()
    const checkedOutAt = window.prompt('Giờ ra (ISO, bỏ trống để giữ nguyên):', item.checkedOutAt ?? '')?.trim()
    setWorking(item.id); setError('')
    try { await api.adjustAttendance(item.id, { reason, ...(checkedInAt ? { checkedInAt } : {}), ...(checkedOutAt ? { checkedOutAt } : {}) }); await load() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể điều chỉnh công.') } finally { setWorking(null) }
  }
  const formatTime = (value?: string) => value ? new Date(value).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
  return <div className="page-stack"><section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> {admin ? 'Quản trị' : 'Ca của tôi'}</p><h2>{admin ? 'Theo dõi chấm công' : 'Chấm công hôm nay'}</h2></div><div className="inline-actions"><label>Ngày<input type="date" value={date} onChange={(event) => setDate(event.target.value)} /></label>{admin && <><label>Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}><option value="">Tất cả</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label><label>Trạng thái<select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">Tất cả</option>{['NotCheckedIn', 'CheckedIn', 'CheckedOut', 'Late', 'EarlyLeave', 'Absent', 'NeedsAdjustment'].map((value) => <option key={value} value={value}>{value}</option>)}</select></label></>}</div></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="history-list">{items.length === 0 && <p className="empty-board">Không có ca phù hợp trong ngày này.</p>}{items.map((item) => <article className="history-row" key={item.shiftAssignmentId}><div><strong>{admin ? item.staffName : item.shiftName} <span className="bay-pill">{item.status}</span></strong><p>{admin ? `${item.shiftName} · ` : ''}{formatTime(item.startsAt)}–{formatTime(item.endsAt)}{item.washBayName ? ` · ${item.washBayName}` : ''}</p><p>Vào: {formatTime(item.checkedInAt)} · Ra: {formatTime(item.checkedOutAt)} · {item.workedMinutes} phút{item.lateMinutes > 0 ? ` · Muộn ${item.lateMinutes} phút` : ''}{item.earlyLeaveMinutes > 0 ? ` · Về sớm ${item.earlyLeaveMinutes} phút` : ''}</p></div><div className="inline-actions">{!admin && <button className="button button-primary" disabled={working === item.shiftAssignmentId || item.isLocked || Boolean(item.checkedOutAt)} onClick={() => selfAction(item)}>{!item.checkedInAt ? 'Check-in' : item.checkedOutAt ? 'Đã check-out' : 'Check-out'}</button>}{admin && item.id && <><button className="button button-secondary" disabled={working === item.id || item.isLocked} onClick={() => adjust(item)}>Điều chỉnh</button><button className="button button-secondary" disabled={working === item.id} onClick={() => toggleLock(item)}>{item.isLocked ? 'Mở chốt' : 'Chốt công'}</button></>}</div></article>)}</div></section>{admin && summary.length > 0 && <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Báo cáo</p><h2>Tổng hợp theo nhân viên</h2></div></div><div className="history-list">{summary.map((item) => <article className="history-row" key={item.groupKey}><div><strong>{item.label}</strong><p>{item.completedShiftCount}/{item.assignedShiftCount} ca hoàn tất · muộn {item.lateCount} · vắng {item.absentCount}</p></div><b>{Math.round(item.workedMinutes / 60 * 10) / 10}h / {Math.round(item.plannedMinutes / 60 * 10) / 10}h</b></article>)}</div></section>}</div>
}

function ReconciliationPage() {
  const [from, setFrom] = useState(() => new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10))
  const [to, setTo] = useState(branchDateValue)
  const [data, setData] = useState<{ paymentCount: number; paidAmount: number; refundedAmount: number; netAmount: number } | null>(null)
  const [error, setError] = useState('')
  async function load() { setError(''); try { setData(await api.getReconciliation(`${from}T00:00:00Z`, `${to}T23:59:59Z`)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải đối soát.') } }
  return <section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Admin</p><h2>Đối soát thanh toán</h2></div></div><div className="inline-actions"><label>Từ<input type="date" value={from} onChange={(event) => setFrom(event.target.value)} /></label><label>Đến<input type="date" value={to} onChange={(event) => setTo(event.target.value)} /></label><button className="button button-primary" onClick={load}>Xem báo cáo</button></div>{error && <p className="form-error" role="alert">{error}</p>}{data && <div className="metric-grid reconciliation-metrics"><Metric icon={<CircleDollarSign />} label="Đã thu" value={`${data.paidAmount.toLocaleString('vi-VN')}₫`} detail={`${data.paymentCount} giao dịch`} tone="aqua" /><Metric icon={<History />} label="Đã hoàn" value={`${data.refundedAmount.toLocaleString('vi-VN')}₫`} detail="Trong kỳ" tone="orange" /><Metric icon={<Check />} label="Thực nhận" value={`${data.netAmount.toLocaleString('vi-VN')}₫`} detail="Sau hoàn tiền" tone="blue" /></div>}</section>
}

type CatalogEditor =
  | { kind: 'service'; value: Service }
  | { kind: 'bay'; value: { id: string; branchId: string; name: string; isActive: boolean } }

function OperationsCatalogPage() {
  const session = getStoredSession()
  const canEdit = session?.role === 'Admin'
  const [tab, setTab] = useState<'services' | 'bays'>('services')
  const [services, setServices] = useState<Service[]>([])
  const [bays, setBays] = useState<WashBay[]>([])
  const [branches, setBranches] = useState<Branch[]>([])
  const [query, setQuery] = useState('')
  const [status, setStatus] = useState<'all' | 'active' | 'inactive'>('all')
  const [branchId, setBranchId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [working, setWorking] = useState(false)
  const [editor, setEditor] = useState<CatalogEditor | null>(null)

  const load = useCallback(async () => {
    setLoading(true); setError('')
    try {
      const [serviceResult, bayResult, branchResult] = await Promise.all([
        api.getServices(canEdit),
        api.getWashBays(branchId, canEdit),
        canEdit ? api.getBranches() : Promise.resolve<{ items: Branch[] }>({ items: [] }),
      ])
      setServices(serviceResult.items); setBays(bayResult.items); setBranches(branchResult.items)
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải danh mục vận hành.') } finally { setLoading(false) }
  }, [branchId, canEdit])
  useEffect(() => { void load() }, [load])

  const visibleServices = services.filter((item) => (status === 'all' || (status === 'active' ? item.isActive : !item.isActive)) && `${item.name} ${item.description ?? ''}`.toLocaleLowerCase('vi-VN').includes(query.toLocaleLowerCase('vi-VN')))
  const visibleBays = bays.filter((item) => (status === 'all' || (status === 'active' ? item.isActive : !item.isActive)) && `${item.name} ${item.branchName}`.toLocaleLowerCase('vi-VN').includes(query.toLocaleLowerCase('vi-VN')))
  const items = tab === 'services' ? visibleServices : visibleBays
  const activeCount = (tab === 'services' ? services : bays).filter((item) => item.isActive).length

  async function save(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!editor) return
    setSubmitError(''); setWorking(true)
    const form = new FormData(event.currentTarget)
    const name = String(form.get('name') ?? '').trim()
    try {
      if (!name) throw new ApiError('Tên là bắt buộc.')
      if (editor.kind === 'service') {
        const price = Number(form.get('price')); const durationMinutes = Number(form.get('durationMinutes'))
        if (!Number.isFinite(price) || price < 0) throw new ApiError('Giá phải là số không âm.')
        if (!Number.isInteger(durationMinutes) || durationMinutes <= 0) throw new ApiError('Thời lượng phải lớn hơn 0 phút.')
        const payload = { name, description: String(form.get('description') ?? '').trim() || undefined, price, durationMinutes, isActive: Boolean(form.get('isActive')) }
        if (editor.value.id) await api.updateService(editor.value.id, payload); else await api.createService(payload)
      } else {
        const selectedBranchId = String(form.get('branchId') ?? '')
        if (!selectedBranchId) throw new ApiError('Hãy chọn chi nhánh cho bãi rửa.')
        const payload = { branchId: selectedBranchId, name, isActive: Boolean(form.get('isActive')) }
        if (editor.value.id) await api.updateWashBay(editor.value.id, payload); else await api.createWashBay(payload)
      }
      setEditor(null); await load()
    } catch (cause) { setSubmitError(cause instanceof ApiError ? cause.message : 'Không thể lưu thay đổi.') } finally { setWorking(false) }
  }

  async function deactivate(item: Service | WashBay) {
    if (!window.confirm(`Ngừng hoạt động “${item.name}”? Dữ liệu lịch sử sẽ được giữ nguyên.`)) return
    setSubmitError(''); setWorking(true)
    try { if (tab === 'services') await api.deactivateService(item.id); else await api.deactivateWashBay(item.id); await load() } catch (cause) { setSubmitError(cause instanceof ApiError ? cause.message : 'Không thể ngừng hoạt động.') } finally { setWorking(false) }
  }

  return <div className="page-stack catalog-workspace">
    <PageHeader title="Danh mục vận hành" description={canEdit ? 'Quản lý dịch vụ và năng lực bãi rửa từ dữ liệu vận hành thực tế.' : 'Xem danh mục dịch vụ đang hoạt động và bãi rửa thuộc chi nhánh của bạn.'} actions={canEdit ? <Button onClick={() => { setSubmitError(''); setEditor(tab === 'services' ? { kind: 'service', value: { id: '', name: '', description: '', price: 0, durationMinutes: 30, isActive: true } } : { kind: 'bay', value: { id: '', branchId: branchId, name: '', isActive: true } }) }}><Plus size={17} /> {tab === 'services' ? 'Thêm dịch vụ' : 'Thêm bãi rửa'}</Button> : undefined} />
    <section className="catalog-summary"><button className={tab === 'services' ? 'catalog-tab active' : 'catalog-tab'} onClick={() => setTab('services')}><Droplets size={18} /><span>Dịch vụ</span><b>{services.length}</b></button><button className={tab === 'bays' ? 'catalog-tab active' : 'catalog-tab'} onClick={() => setTab('bays')}><CarFront size={18} /><span>Bãi rửa</span><b>{bays.length}</b></button><div className="catalog-count"><strong>{activeCount}</strong><span>đang hoạt động</span><small>{(tab === 'services' ? services : bays).length - activeCount} ngừng</small></div></section>
    <Surface className="catalog-surface"><div className="admin-toolbar"><label className="search-field"><span className="sr-only">Tìm kiếm</span><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder={tab === 'services' ? 'Tìm tên hoặc mô tả dịch vụ' : 'Tìm bãi hoặc chi nhánh'} /></label><label>Trạng thái<select value={status} onChange={(event) => setStatus(event.target.value as typeof status)}><option value="all">Tất cả</option><option value="active">Đang hoạt động</option><option value="inactive">Đã ngừng</option></select></label>{tab === 'bays' && canEdit && <label>Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}><option value="">Tất cả chi nhánh</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}<Button variant="secondary" onClick={() => { setQuery(''); setStatus('all'); setBranchId('') }}>Xóa lọc</Button></div>{submitError && <p className="form-error" role="alert">{submitError}</p>}{loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : error ? <div className="ui-state" role="alert"><strong>Không thể tải danh mục</strong><p>{error}</p><Button variant="secondary" onClick={load}>Thử lại</Button></div> : items.length === 0 ? <div className="ui-state"><strong>Chưa có dữ liệu phù hợp</strong><p>{canEdit ? 'Thử điều chỉnh bộ lọc hoặc thêm danh mục mới.' : 'Chưa có danh mục được cấp quyền hiển thị.'}</p></div> : tab === 'services' ? <div className="catalog-list">{visibleServices.map((service) => <article className="catalog-row" key={service.id}><div><strong>{service.name}</strong><p>{service.description || 'Chưa có mô tả'}</p></div><div className="catalog-number"><b>{service.price.toLocaleString('vi-VN')} ₫</b><span>{service.durationMinutes} phút</span></div><StatusBadge tone={service.isActive ? 'success' : 'default'}>{service.isActive ? 'Hoạt động' : 'Đã ngừng'}</StatusBadge>{canEdit && <div className="inline-actions"><Button variant="ghost" onClick={() => { setSubmitError(''); setEditor({ kind: 'service', value: service }) }}>Chỉnh sửa</Button>{service.isActive && <Button variant="danger" disabled={working} onClick={() => void deactivate(service)}>Ngừng</Button>}</div>}</article>)}</div> : <div className="catalog-list">{visibleBays.map((bay) => <article className="catalog-row" key={bay.id}><div><strong>{bay.name}</strong><p>{bay.branchName} · {bay.activeBookingCount} booking đang xử lý</p></div><div className="catalog-number"><b>{bay.nextBookingAt ? new Date(bay.nextBookingAt).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' }) : 'Chưa có lịch tới'}</b><span>Booking kế tiếp</span></div><StatusBadge tone={bay.isActive ? 'success' : 'default'}>{bay.isActive ? 'Hoạt động' : 'Đã ngừng'}</StatusBadge>{canEdit && <div className="inline-actions"><Button variant="ghost" onClick={() => { setSubmitError(''); setEditor({ kind: 'bay', value: bay }) }}>Chỉnh sửa</Button>{bay.isActive && <Button variant="danger" disabled={working} onClick={() => void deactivate(bay)}>Ngừng</Button>}</div>}</article>)}</div>}</Surface>
    {editor && <aside className="admin-drawer" aria-label="Biểu mẫu danh mục"><div className="drawer-heading"><div><p className="page-kicker">{editor.kind === 'service' ? 'Dịch vụ' : 'Bãi rửa'}</p><h2>{editor.value.id ? 'Chỉnh sửa' : 'Thêm mới'}</h2></div><Button variant="ghost" type="button" aria-label="Đóng" onClick={() => setEditor(null)}>×</Button></div><form onSubmit={save}>{submitError && <p className="form-error" role="alert">{submitError}</p>}<label>Tên<input name="name" defaultValue={editor.value.name} required autoFocus /></label>{editor.kind === 'service' ? <><label>Mô tả<textarea name="description" defaultValue={editor.value.description ?? ''} maxLength={500} /></label><label>Giá (₫)<input name="price" type="number" min="0" step="1000" defaultValue={editor.value.price} required /></label><label>Thời lượng (phút)<input name="durationMinutes" type="number" min="1" step="1" defaultValue={editor.value.durationMinutes} required /></label></> : <label>Chi nhánh<select name="branchId" defaultValue={editor.value.branchId} required><option value="">Chọn chi nhánh</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}<label className="checkbox-field"><input name="isActive" type="checkbox" defaultChecked={editor.value.isActive} /> Đang hoạt động</label><div className="drawer-actions"><Button variant="secondary" type="button" onClick={() => setEditor(null)}>Hủy</Button><Button type="submit" loading={working}>Lưu thay đổi</Button></div></form></aside>}
  </div>
}

function LoyaltyConsolePage() {
  const [dashboard, setDashboard] = useState<LoyaltyDashboard | null>(null)
  const [segment, setSegment] = useState<'at-risk' | 'expiring-points' | 'loyal'>('at-risk')
  const [customers, setCustomers] = useState<LoyaltySegmentCustomer[]>([])
  const [promotions, setPromotions] = useState<Promotion[]>([])
  const [analytics, setAnalytics] = useState<PromotionAnalytics | null>(null)
  const [selectedCustomer, setSelectedCustomer] = useState<Customer360 | null>(null)
  const [activeRewards, setActiveRewards] = useState<Array<{ id: string; name: string; description?: string; pointsRequired: number; value: number; type: string }>>([])
  const [rewardsOpen, setRewardsOpen] = useState(false)
  const [rewardCatalogOpen, setRewardCatalogOpen] = useState(false)
  const [rewardsLoading, setRewardsLoading] = useState(false)
  const [rewardsError, setRewardsError] = useState('')
  const [pointActivity, setPointActivity] = useState<LoyaltyPointActivity[]>([])
  const [pointActivityKind, setPointActivityKind] = useState<'issued' | 'redeemed'>('issued')
  const [pointActivityPage, setPointActivityPage] = useState(1)
  const [pointActivityTotal, setPointActivityTotal] = useState(0)
  const [pointActivityOpen, setPointActivityOpen] = useState(false)
  const [pointActivityLoading, setPointActivityLoading] = useState(false)
  const [pointActivityError, setPointActivityError] = useState('')
  const [campaignOpen, setCampaignOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    setLoading(true); setError('')
    try {
      const [summary, members, promotionList] = await Promise.all([api.getLoyaltyDashboard(), api.getLoyaltySegment(segment), api.getPromotions()])
      const loyaltyCampaign = promotionList.items.find(item => item.code === 'LOYALTY15') ?? promotionList.items[0]
      setDashboard(summary); setCustomers(members); setPromotions(promotionList.items)
      if (loyaltyCampaign) setAnalytics(await api.getPromotionAnalytics(loyaltyCampaign.id))
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải Loyalty Console.') } finally { setLoading(false) }
  }, [segment])
  useEffect(() => { void load() }, [load])

  function previewCampaign(_item?: Promotion) { setCampaignOpen(true) }

  async function openCustomer(id: string) { try { setSelectedCustomer(await api.getCustomer360(id)) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải Customer 360.') } }
  async function openActiveRewards() {
    setRewardsOpen(true); setRewardsLoading(true); setRewardsError('')
    try {
      const result = await api.getRewards()
      setActiveRewards(result.items)
    } catch (cause) {
      setRewardsError(cause instanceof ApiError ? cause.message : 'Không thể tải danh sách reward.')
    } finally {
      setRewardsLoading(false)
    }
  }
  async function loadPointActivity(kind: 'issued' | 'redeemed', page = 1) {
    setPointActivityKind(kind); setPointActivityPage(page); setPointActivityOpen(true); setPointActivityLoading(true); setPointActivityError('')
    try {
      const result = await api.getLoyaltyPointActivity(kind, page)
      setPointActivity(result.items); setPointActivityTotal(result.totalCount)
    } catch (cause) {
      setPointActivityError(cause instanceof ApiError ? cause.message : 'Không thể tải giao dịch điểm.')
    } finally {
      setPointActivityLoading(false)
    }
  }
  function openMetricDetails(label: string) {
    if (label.includes('phát hành')) { void loadPointActivity('issued'); return }
    if (label.includes('đã đổi')) { void loadPointActivity('redeemed'); return }
    if (label.includes('Reward')) { void openActiveRewards(); return }
    if (label === 'Điểm sắp hết hạn') setSegment('expiring-points')
    if (label === 'Thành viên hoạt động') setSegment('loyal')
    const selector = label === 'Điểm phát hành' || label === 'Điểm đã đổi'
      ? '.loyalty-chart'
      : label === 'Reward hoạt động' || label === 'Tỷ lệ dùng ưu đãi'
        ? '.campaign-panel'
        : '#loyalty-segments'
    window.setTimeout(() => document.querySelector(selector)?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 0)
  }
  const totalTier = dashboard?.tierDistribution.reduce((sum, item) => sum + item.customerCount, 0) ?? 0
  const dailyPoints = dashboard?.dailyPoints ?? []
  const chartPeak = Math.max(...dailyPoints.flatMap(point => [point.issued, point.redeemed]), 1)
  const chartPoints = (field: 'issued' | 'redeemed') => dailyPoints.map((item, index, all) => `${all.length < 2 ? 0 : index / (all.length - 1) * 100},${100 - item[field] / chartPeak * 82}`).join(' ')
  const chartLabels = [...new Set([0, Math.floor((dailyPoints.length - 1) / 3), Math.floor((dailyPoints.length - 1) * 2 / 3), Math.max(0, dailyPoints.length - 1)])]
  const activeDays = dailyPoints.filter(point => point.issued > 0 || point.redeemed > 0)
  const hasPointData = activeDays.length > 0
  const metrics = dashboard ? [
    ['Thành viên hoạt động', dashboard.activeCustomers.toLocaleString('vi-VN'), <UsersRound key="members" />], ['Điểm phát hành', dashboard.pointsIssued.toLocaleString('vi-VN'), <Sparkles key="issued" />], ['Điểm đã đổi', dashboard.pointsRedeemed.toLocaleString('vi-VN'), <Gift key="redeemed" />], ['Điểm sắp hết hạn', dashboard.expiringPoints.toLocaleString('vi-VN'), <TriangleAlert key="expiry" />], ['Reward hoạt động', String(dashboard.activeRewards), <TicketPercent key="rewards" />], ['Tỷ lệ dùng ưu đãi', `${dashboard.promotionUsageRate}%`, <TrendingUp key="usage" />],
  ] as const : []
  return <div className="page-stack loyalty-console">
    <PageHeader title="Loyalty Console" description="Theo dõi thành viên, điểm thưởng và hiệu quả campaign từ dữ liệu vận hành." actions={<Button onClick={() => setCampaignOpen(true)}><Plus size={17} /> Tạo campaign</Button>} />
    {error && <div className="form-error" role="alert">{error}<Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div>}
    {loading ? <div className="ui-skeleton loyalty-loading" aria-busy="true"><span /><span /><span /></div> : <>
      <section className="loyalty-metrics">{metrics.map(([label, value, icon]) => <button key={label} className="loyalty-metric" onClick={() => openMetricDetails(label)}><span>{icon}</span><small>{label}</small><strong>{value}</strong><em>Nhấn để xem chi tiết</em></button>)}</section>
      <section className="loyalty-dashboard-grid"><Surface className="loyalty-chart"><div className="loyalty-panel-title"><div><h2>Điểm phát hành và đã đổi</h2><p>Xu hướng theo ngày trong 30 ngày gần nhất.</p></div><StatusBadge tone="info">Theo ngày</StatusBadge></div>{hasPointData ? <><div className="points-chart-summary"><div><span>Phát hành</span><strong>{dashboard?.pointsIssued.toLocaleString('vi-VN')} điểm</strong></div><div><span>Đã đổi</span><strong>{dashboard?.pointsRedeemed.toLocaleString('vi-VN')} điểm</strong></div><div><span>Ngày có giao dịch</span><strong>{activeDays.length}/{dailyPoints.length}</strong></div></div><div className="points-chart-frame"><div className="points-chart-axis" aria-hidden="true"><span>{chartPeak.toLocaleString('vi-VN')}</span><span>{Math.round(chartPeak / 2).toLocaleString('vi-VN')}</span><span>0</span></div><svg viewBox="0 0 100 100" preserveAspectRatio="none" role="img" aria-label={`Biểu đồ 30 ngày: phát hành ${dashboard?.pointsIssued.toLocaleString('vi-VN')} điểm, đã đổi ${dashboard?.pointsRedeemed.toLocaleString('vi-VN')} điểm`}><path d="M0 88 H100 M0 56 H100 M0 24 H100" className="chart-grid" /><polyline points={chartPoints('issued')} className="chart-line" /><polyline points={chartPoints('redeemed')} className="chart-line chart-line-redeemed" />{dailyPoints.map((point, index) => <g key={point.date}><title>{`${new Date(point.date).toLocaleDateString('vi-VN')}: phát hành ${point.issued.toLocaleString('vi-VN')} điểm, đã đổi ${point.redeemed.toLocaleString('vi-VN')} điểm`}</title>{point.issued > 0 && <circle className="chart-dot issued" cx={dailyPoints.length < 2 ? 0 : index / (dailyPoints.length - 1) * 100} cy={100 - point.issued / chartPeak * 82} r="1.25" />}{point.redeemed > 0 && <circle className="chart-dot redeemed" cx={dailyPoints.length < 2 ? 0 : index / (dailyPoints.length - 1) * 100} cy={100 - point.redeemed / chartPeak * 82} r="1.25" />}</g>)}</svg></div><div className="points-chart-dates">{chartLabels.map(index => <span key={dailyPoints[index]?.date}>{dailyPoints[index] ? new Date(dailyPoints[index].date).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }) : ''}</span>)}</div><div className="points-chart-legend"><span><i className="issued" />Phát hành</span><span><i className="redeemed" />Đã đổi</span><b>Đỉnh: {chartPeak.toLocaleString('vi-VN')} điểm</b></div><div className="points-chart-activity" aria-label="Các ngày có giao dịch điểm">{activeDays.map(point => <div key={point.date}><time>{new Date(point.date).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })}</time><span>+{point.issued.toLocaleString('vi-VN')}</span><b>−{point.redeemed.toLocaleString('vi-VN')}</b></div>)}</div></> : <div className="ui-state"><strong>Chưa có giao dịch điểm</strong><p>Biểu đồ sẽ hiển thị điểm phát hành và điểm đã đổi khi có dữ liệu.</p></div>}</Surface>
        <Surface className="tier-panel"><div className="loyalty-panel-title"><div><h2>Phân bố thành viên theo hạng</h2><p>{totalTier.toLocaleString('vi-VN')} thành viên trong dữ liệu.</p></div></div><div className="tier-list">{dashboard?.tierDistribution.map(item => <div key={item.tierName}><span>{item.tierName}</span><b>{item.customerCount.toLocaleString('vi-VN')}</b><i style={{ width: `${totalTier ? item.customerCount / totalTier * 100 : 0}%` }} /></div>)}</div></Surface></section>
      <section id="loyalty-segments" className="loyalty-workspace"><Surface className="segment-panel"><div className="segment-tabs" role="tablist">{([['at-risk', 'Có nguy cơ rời bỏ'], ['expiring-points', 'Điểm sắp hết hạn'], ['loyal', 'Khách trung thành']] as const).map(([value, label]) => <button key={value} role="tab" aria-selected={segment === value} className={segment === value ? 'active' : ''} onClick={() => setSegment(value)}>{label}</button>)}</div><div className="segment-note"><span>Nguy cơ rời bỏ được xác định sau 45 ngày không có lượt rửa hoàn tất.</span><Button variant="secondary" onClick={() => promotions[0] && void previewCampaign(promotions[0])}>Tạo campaign cho nhóm</Button></div><div className="resource-table-wrap"><table className="resource-table"><thead><tr><th>Khách hàng</th><th>Hạng</th><th>Điểm</th><th>Hoạt động cuối</th><th>Trạng thái</th><th /></tr></thead><tbody>{customers.map(item => <tr key={item.customerId}><td><strong>{item.fullName}</strong><br /><small>{item.phoneNumber ?? 'Chưa có số điện thoại'}</small></td><td>{item.tierName ?? 'Chưa xếp hạng'}</td><td>{item.currentPoints.toLocaleString('vi-VN')}</td><td>{item.lastVisitDate ? new Date(item.lastVisitDate).toLocaleDateString('vi-VN') : 'Chưa có'}</td><td><StatusBadge tone={segment === 'at-risk' ? 'danger' : segment === 'expiring-points' ? 'warning' : 'success'}>{segment === 'at-risk' ? 'Cần chăm sóc' : segment === 'expiring-points' ? `${item.expiringPoints} sắp hết hạn` : 'Trung thành'}</StatusBadge></td><td><Button variant="ghost" onClick={() => void openCustomer(item.customerId)}>360</Button></td></tr>)}{customers.length === 0 && <tr><td colSpan={6}><div className="ui-state"><strong>Chưa có khách thỏa điều kiện</strong><p>Dữ liệu hiện tại chưa có khách thuộc phân khúc này. Hãy chọn một nhóm khác hoặc chạy seed Loyalty cho môi trường demo.</p></div></td></tr>}</tbody></table></div></Surface>
        <Surface className="campaign-panel"><div className="loyalty-panel-title"><div><h2>Hiệu quả campaign</h2><p>Campaign có dữ liệu mới nhất.</p></div></div>{analytics ? <><div className="campaign-name"><Gift size={19} /><div><strong>{promotions.find(item => item.id === analytics.promotionId)?.name ?? 'Campaign'}</strong><span>{analytics.recipientCount.toLocaleString('vi-VN')} người nhận</span></div></div><dl><div><dt>Đã áp dụng</dt><dd>{analytics.appliedCount.toLocaleString('vi-VN')}</dd></div><div><dt>Tỷ lệ redemption</dt><dd>{analytics.redemptionRate}%</dd></div><div><dt>Giá trị giảm</dt><dd>{analytics.discountValue.toLocaleString('vi-VN')} ₫</dd></div><div><dt>Doanh thu sau giảm</dt><dd>{analytics.bookingRevenueAfterDiscount.toLocaleString('vi-VN')} ₫</dd></div></dl></> : <div className="ui-state"><strong>Chưa có campaign</strong><p>Tạo campaign để theo dõi hiệu quả.</p></div>}</Surface></section>
    </>}
    {campaignOpen && <CampaignBuilder segment={segment} onClose={() => setCampaignOpen(false)} onCreated={async () => { setCampaignOpen(false); await load() }} />}
    {rewardsOpen && <aside className="admin-drawer loyalty-detail-drawer" aria-label="Active loyalty rewards"><div className="drawer-heading"><div><p className="page-kicker">Loyalty rewards</p><h2>Reward đang hoạt động</h2><p className="drawer-note">Các phần thưởng khách hàng hiện có thể đổi bằng điểm.</p></div><Button variant="ghost" aria-label="Đóng" onClick={() => setRewardsOpen(false)}><X size={18} /></Button></div><div className="drawer-actions reward-detail-actions"><Button variant="secondary" onClick={() => { setRewardsOpen(false); setRewardCatalogOpen(true) }}>Quản lý reward</Button></div>{rewardsLoading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : rewardsError ? <div className="ui-state" role="alert"><strong>Không thể tải reward</strong><p>{rewardsError}</p><Button variant="secondary" onClick={() => void openActiveRewards()}>Thử lại</Button></div> : activeRewards.length === 0 ? <div className="ui-state"><strong>Chưa có reward hoạt động</strong><p>Hãy tạo hoặc kích hoạt reward để khách hàng có thể đổi điểm.</p></div> : <div className="drawer-list loyalty-reward-list">{activeRewards.map(reward => <div key={reward.id}><div><strong>{reward.name}</strong><span>{reward.description || 'Phần thưởng loyalty'}</span></div><b>{reward.pointsRequired.toLocaleString('vi-VN')} điểm</b><small>Giá trị {reward.value.toLocaleString('vi-VN')}₫</small></div>)}</div>}</aside>}
    {rewardCatalogOpen && <RewardCatalogDrawer onClose={() => setRewardCatalogOpen(false)} onChanged={() => void load()} />}
    {pointActivityOpen && <aside className="admin-drawer loyalty-detail-drawer" aria-label="Chi tiết giao dịch điểm"><div className="drawer-heading"><div><p className="page-kicker">Loyalty point ledger</p><h2>{pointActivityKind === 'issued' ? 'Điểm phát hành' : 'Điểm đã đổi'}</h2><p className="drawer-note">{pointActivityTotal.toLocaleString('vi-VN')} giao dịch trong 30 ngày gần nhất.</p></div><Button variant="ghost" aria-label="Đóng" onClick={() => setPointActivityOpen(false)}><X size={18} /></Button></div>{pointActivityLoading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : pointActivityError ? <div className="ui-state" role="alert"><strong>Không thể tải giao dịch điểm</strong><p>{pointActivityError}</p><Button variant="secondary" onClick={() => void loadPointActivity(pointActivityKind, pointActivityPage)}>Thử lại</Button></div> : pointActivity.length === 0 ? <div className="ui-state"><strong>Chưa có giao dịch</strong><p>Không có điểm {pointActivityKind === 'issued' ? 'phát hành' : 'đã đổi'} trong 30 ngày gần nhất.</p></div> : <><div className="drawer-list loyalty-point-list">{pointActivity.map(transaction => <div key={transaction.transactionId}><div><strong>{transaction.customerName}</strong><span>{transaction.phoneNumber || 'Chưa có số điện thoại'}</span><small>{transaction.description || (pointActivityKind === 'issued' ? 'Tích điểm loyalty' : 'Đổi điểm loyalty')}</small></div><b>{pointActivityKind === 'issued' ? '+' : '-'}{transaction.points.toLocaleString('vi-VN')} điểm</b><time dateTime={transaction.createdAt}>{new Date(transaction.createdAt).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</time></div>)}</div>{pointActivityTotal > 20 && <div className="drawer-actions"><Button variant="secondary" disabled={pointActivityPage === 1} onClick={() => void loadPointActivity(pointActivityKind, pointActivityPage - 1)}>Trang trước</Button><span>Trang {pointActivityPage}/{Math.ceil(pointActivityTotal / 20)}</span><Button disabled={pointActivityPage >= Math.ceil(pointActivityTotal / 20)} onClick={() => void loadPointActivity(pointActivityKind, pointActivityPage + 1)}>Trang sau</Button></div>}</>}</aside>}
    {selectedCustomer && <aside className="admin-drawer customer-360" aria-label="Customer 360"><div className="drawer-heading"><div><p className="page-kicker">Customer 360</p><h2>{selectedCustomer.customer.fullName}</h2></div><Button variant="ghost" aria-label="Đóng" onClick={() => setSelectedCustomer(null)}><X size={18} /></Button></div><section className="customer-score"><span>{selectedCustomer.customer.tierName ?? 'Member'}</span><strong>{selectedCustomer.customer.currentPoints.toLocaleString('vi-VN')} điểm</strong><small>{selectedCustomer.customer.expiringPoints.toLocaleString('vi-VN')} điểm sắp hết hạn</small></section><h3>Voucher và ưu đãi</h3><div className="drawer-list">{selectedCustomer.vouchers.slice(0, 4).map(item => <div key={item.id}><strong>{item.name}</strong><span>{item.status}</span></div>)}</div><h3>Lịch sử điểm</h3><div className="drawer-list">{selectedCustomer.pointLedger.slice(0, 5).map(item => <div key={item.id}><strong>{item.points > 0 ? '+' : ''}{item.points} điểm</strong><span>{item.description ?? item.type}</span></div>)}</div></aside>}
  </div>
}

function RewardCatalogDrawer({ onClose, onChanged }: { onClose: () => void; onChanged: () => void }) {
  const [rewards, setRewards] = useState<Reward[]>([])
  const [services, setServices] = useState<Service[]>([])
  const [status, setStatus] = useState<'all' | Reward['status']>('all')
  const [editor, setEditor] = useState<Reward | 'new' | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    setLoading(true); setError('')
    try {
      const [catalog, serviceCatalog] = await Promise.all([api.getAdminRewards(), api.getServices(true)])
      setRewards(catalog.items); setServices(serviceCatalog.items)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải danh mục reward.')
    } finally {
      setLoading(false)
    }
  }, [])
  useEffect(() => { void load() }, [load])

  const visibleRewards = rewards.filter(reward => status === 'all' || reward.status === status)
  async function save(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError('')
    const form = new FormData(event.currentTarget)
    const type = String(form.get('type')) as Reward['type']
    const pointsRequired = Number(form.get('pointsRequired'))
    const value = Number(form.get('value'))
    const serviceId = String(form.get('serviceId') ?? '') || undefined
    const validFrom = String(form.get('validFrom') ?? '') || undefined
    const validTo = String(form.get('validTo') ?? '') || undefined
    const usageLimitValue = String(form.get('usageLimitPerCustomer') ?? '')
    if (!Number.isInteger(pointsRequired) || pointsRequired <= 0) { setError('Điểm cần đổi phải là số nguyên lớn hơn 0.'); setSaving(false); return }
    if (!Number.isFinite(value) || value < 0) { setError('Giá trị reward không hợp lệ.'); setSaving(false); return }
    if ((type === 'FreeService' || type === 'AddOnService') && !serviceId) { setError('Hãy chọn dịch vụ cho loại reward này.'); setSaving(false); return }
    const payload: RewardPayload = { name: String(form.get('name') ?? '').trim(), description: String(form.get('description') ?? '').trim() || undefined, type, pointsRequired, value, serviceId, validFrom, validTo, usageLimitPerCustomer: usageLimitValue ? Number(usageLimitValue) : undefined, isActive: Boolean(form.get('isActive')) }
    try {
      if (editor === 'new') await api.createReward(payload)
      else if (editor) await api.updateReward(editor.id, payload)
      setEditor(null); await load(); onChanged()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể lưu reward.')
    } finally {
      setSaving(false)
    }
  }
  async function archive(reward: Reward) {
    if (!window.confirm(`Archive reward “${reward.name}”? Voucher đã cấp vẫn được giữ nguyên.`)) return
    setSaving(true); setError('')
    try { await api.archiveReward(reward.id); await load(); onChanged() } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể archive reward.') } finally { setSaving(false) }
  }

  if (editor) {
    const isNew = editor === 'new'
    const reward = isNew ? undefined : editor
    const formTitle = isNew ? 'Tạo reward' : 'Chỉnh sửa reward'
    return <aside className="admin-drawer reward-catalog-drawer" aria-label={formTitle}><div className="drawer-heading"><div><p className="page-kicker">Reward catalog</p><h2>{formTitle}</h2></div><Button variant="ghost" aria-label="Đóng" onClick={() => setEditor(null)}><X size={18} /></Button></div><form className="reward-editor" onSubmit={save}>{error && <p className="form-error" role="alert">{error}</p>}<label>Tên reward<input name="name" defaultValue={reward?.name ?? ''} maxLength={200} required autoFocus /></label><label>Mô tả<textarea name="description" defaultValue={reward?.description ?? ''} maxLength={500} /></label><div className="campaign-field-grid"><label>Loại reward<select name="type" defaultValue={reward?.type ?? 'FixedDiscount'}><option value="FixedDiscount">Giảm tiền cố định</option><option value="PercentageDiscount">Giảm theo phần trăm</option><option value="FreeService">Miễn phí dịch vụ</option><option value="AddOnService">Dịch vụ bổ sung</option></select></label><label>Điểm cần đổi<input name="pointsRequired" type="number" min="1" step="1" defaultValue={reward?.pointsRequired ?? ''} required /></label></div><div className="campaign-field-grid"><label>Giá trị<input name="value" type="number" min="0" step="1000" defaultValue={reward?.value ?? 0} required /></label><label>Giới hạn/khách<input name="usageLimitPerCustomer" type="number" min="1" step="1" defaultValue={reward?.usageLimitPerCustomer ?? ''} /></label></div><label>Dịch vụ áp dụng <small>Yêu cầu cho Miễn phí dịch vụ và Dịch vụ bổ sung.</small><select name="serviceId" defaultValue={reward?.serviceId ?? ''}><option value="">Không gắn dịch vụ</option>{services.filter(service => service.isActive || service.id === reward?.serviceId).map(service => <option key={service.id} value={service.id}>{service.name}</option>)}</select></label><div className="campaign-field-grid"><label>Hiệu lực từ<input name="validFrom" type="date" defaultValue={reward?.validFrom?.slice(0, 10) ?? ''} /></label><label>Hiệu lực đến<input name="validTo" type="date" defaultValue={reward?.validTo?.slice(0, 10) ?? ''} /></label></div><label className="checkbox-field"><input name="isActive" type="checkbox" defaultChecked={reward?.status === 'Active' || isNew} /> Kích hoạt để khách có thể đổi</label><div className="drawer-actions"><Button variant="secondary" type="button" onClick={() => setEditor(null)}>Hủy</Button><Button type="submit" loading={saving}>Lưu reward</Button></div></form></aside>
  }

  return <aside className="admin-drawer reward-catalog-drawer" aria-label="Quản lý reward"><div className="drawer-heading"><div><p className="page-kicker">Loyalty rewards</p><h2>Quản lý reward</h2><p className="drawer-note">Reward là mẫu ưu đãi khách có thể đổi bằng điểm.</p></div><Button variant="ghost" aria-label="Đóng" onClick={onClose}><X size={18} /></Button></div><div className="reward-catalog-toolbar"><select value={status} aria-label="Lọc theo trạng thái" onChange={event => setStatus(event.target.value as typeof status)}><option value="all">Tất cả trạng thái</option><option value="Active">Đang hoạt động</option><option value="Inactive">Đã tắt</option><option value="Archived">Đã archive</option></select><Button onClick={() => { setError(''); setEditor('new') }}><Plus size={16} /> Tạo reward</Button></div>{error && <p className="form-error" role="alert">{error}</p>}{loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : visibleRewards.length === 0 ? <div className="ui-state"><strong>Chưa có reward phù hợp</strong><p>Tạo reward mới hoặc đổi bộ lọc trạng thái.</p></div> : <div className="reward-catalog-list">{visibleRewards.map(reward => <article key={reward.id}><div><strong>{reward.name}</strong><p>{reward.pointsRequired.toLocaleString('vi-VN')} điểm · {reward.type}</p><small>{reward.validTo ? `Đến ${new Date(reward.validTo).toLocaleDateString('vi-VN')}` : 'Không giới hạn thời gian'}</small></div><StatusBadge tone={reward.status === 'Active' ? 'success' : reward.status === 'Archived' ? 'default' : 'warning'}>{reward.status === 'Active' ? 'Đang bật' : reward.status === 'Inactive' ? 'Đã tắt' : 'Archived'}</StatusBadge><div className="inline-actions"><Button variant="ghost" disabled={saving} onClick={() => { setError(''); setEditor(reward) }}>Sửa</Button>{reward.status !== 'Archived' && <Button variant="danger" disabled={saving} onClick={() => void archive(reward)}>Archive</Button>}</div></article>)}</div>}</aside>
}

function AdminConsole() {
  const [branches, setBranches] = useState<Array<{ id: string; name: string; address: string; isActive: boolean }>>([])
  const [selected, setSelected] = useState<{ id: string; name: string; address: string; isActive: boolean } | null>(null)
  const [query, setQuery] = useState('')
  const [status, setStatus] = useState<'all' | 'active' | 'inactive'>('all')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  function load() { setError(''); setLoading(true); api.getBranches(true).then((result) => setBranches(result.items)).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải danh sách chi nhánh.')).finally(() => setLoading(false)) }
  useEffect(load, [])
  const visible = branches.filter((branch) => (status === 'all' || (status === 'active' ? branch.isActive : !branch.isActive)) && `${branch.name} ${branch.address}`.toLocaleLowerCase('vi-VN').includes(query.toLocaleLowerCase('vi-VN')))
  const activeCount = branches.filter((branch) => branch.isActive).length
  return <div className="page-stack admin-workspace">
    <PageHeader title="Quản lý chi nhánh" description="Theo dõi trạng thái vận hành và cập nhật cấu hình chi nhánh." actions={<Button onClick={() => setSelected({ id: '', name: '', address: '', isActive: true })}><Plus size={17} /> Thêm chi nhánh</Button>} />
    <section className="metric-grid admin-metrics"><Metric icon={<Gauge />} label="Tổng chi nhánh" value={`${branches.length}`} detail="Đang quản lý" tone="blue" /><Metric icon={<Check />} label="Đang hoạt động" value={`${activeCount}`} detail="Sẵn sàng nhận lịch" tone="aqua" /></section>
    <Surface className="admin-table-surface">
      <div className="admin-toolbar"><label className="search-field"><span className="sr-only">Tìm kiếm chi nhánh</span><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Tìm chi nhánh hoặc địa chỉ…" /></label><label>Trạng thái<select value={status} onChange={(event) => setStatus(event.target.value as typeof status)}><option value="all">Tất cả</option><option value="active">Đang hoạt động</option><option value="inactive">Ngừng hoạt động</option></select></label><Button variant="secondary" onClick={() => { setQuery(''); setStatus('all') }}>Xóa lọc</Button></div>
      {loading && <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div>}
      {error && <div className="form-error" role="alert">{error}<Button variant="secondary" onClick={load}>Thử lại</Button></div>}
      {!loading && !error && <div className="resource-table-wrap"><table className="resource-table"><thead><tr><th>Chi nhánh</th><th>Địa chỉ</th><th>Trạng thái</th><th aria-label="Thao tác" /></tr></thead><tbody>{visible.map((branch) => <tr key={branch.id}><td><strong>{branch.name}</strong></td><td>{branch.address}</td><td><StatusBadge tone={branch.isActive ? 'success' : 'default'}>{branch.isActive ? 'Hoạt động' : 'Ngừng hoạt động'}</StatusBadge></td><td><Button variant="ghost" aria-label={`Chỉnh sửa ${branch.name}`} onClick={() => setSelected(branch)}>Chỉnh sửa</Button></td></tr>)}{visible.length === 0 && <tr><td colSpan={4}><div className="ui-state"><strong>Không tìm thấy chi nhánh</strong><p>Thử điều chỉnh từ khóa hoặc bộ lọc trạng thái.</p></div></td></tr>}</tbody></table></div>}
    </Surface>
    {selected && <aside className="admin-drawer" aria-label="Chỉnh sửa chi nhánh"><div className="drawer-heading"><div><p className="page-kicker">Chi nhánh</p><h2>{selected.id ? 'Chỉnh sửa chi nhánh' : 'Thêm chi nhánh'}</h2></div><Button variant="ghost" onClick={() => setSelected(null)} aria-label="Đóng">×</Button></div><form onSubmit={(event) => { event.preventDefault(); setSelected(null) }}><label>Tên chi nhánh<input value={selected.name} onChange={(event) => setSelected({ ...selected, name: event.target.value })} required /></label><label>Địa chỉ<input value={selected.address} onChange={(event) => setSelected({ ...selected, address: event.target.value })} required /></label><label className="checkbox-field"><input type="checkbox" checked={selected.isActive} onChange={(event) => setSelected({ ...selected, isActive: event.target.checked })} /> Đang hoạt động</label><p className="drawer-note">Chức năng lưu sẽ được nối vào API CRUD chi nhánh khi endpoint mutation được đưa vào frontend contract.</p><div className="drawer-actions"><Button variant="secondary" type="button" onClick={() => setSelected(null)}>Hủy</Button><Button type="submit">Lưu thay đổi</Button></div></form></aside>}
  </div>
}

function OperationsDashboard({ boardOnly = false }: { boardOnly?: boolean }) {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [branches, setBranches] = useState<Array<{ id: string; name: string }>>([])
  const [branchId, setBranchId] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  function load() { setError(''); setLoading(true); Promise.all([api.getBranches(), api.getBookings(branchId ? { branchId } : {})]).then(([branchResult, bookingResult]) => { setBranches(branchResult.items); setBookings(bookingResult.items.map((item) => ({ id: item.id, customer: item.assignedStaffId ? `NV ${item.assignedStaffId.slice(0, 6)}` : 'Khách hàng', plate: item.vehicleId.slice(0, 8), service: item.items.map((line) => line.serviceName).join(' · ') || 'Dịch vụ', time: new Date(item.bookingStartTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }), status: item.status as BookingStatus, bay: item.washBayId?.slice(0, 6) }))) }).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải lịch hẹn.')).finally(() => setLoading(false)) }
  useEffect(load, [branchId])
  const completed = bookings.filter((item) => item.status === 'Completed').length
  const active = bookings.filter((item) => item.status === 'InProgress').length
  return <div className="page-stack">{!boardOnly && <><PageHeader title="Trung tâm vận hành" description="Theo dõi công suất và lịch hẹn từ dữ liệu hệ thống." /><section className="metric-grid"><Metric icon={<CalendarDays />} label="Tổng lịch hẹn" value={`${bookings.length}`} detail="Theo bộ lọc hiện tại" tone="blue" /><Metric icon={<Droplets />} label="Đang rửa" value={`${active}`} detail="Đang thực hiện" tone="orange" /><Metric icon={<Check />} label="Hoàn tất" value={`${completed}`} detail="Theo bộ lọc hiện tại" tone="aqua" /></section></>}<section className="board-toolbar"><div><p className="page-kicker">Vận hành theo thời gian thực</p><h2>Booking board</h2></div><div className="toolbar-controls"><select value={branchId} onChange={(event) => setBranchId(event.target.value)} aria-label="Lọc theo chi nhánh"><option value="">Tất cả chi nhánh</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></div></section>{error && <p className="form-error" role="alert">{error}</p>}{loading ? <section className="surface"><p className="empty-board">Đang tải lịch hẹn…</p></section> : <div className="booking-board">{bookingColumns.map((column) => <section className="board-column" key={column.status}><header><h3>{column.label}</h3><span>{bookings.filter((booking) => booking.status === column.status).length}</span></header><div className="board-cards">{bookings.filter((booking) => booking.status === column.status).map((booking) => <BookingCard key={booking.id} booking={booking} onMove={() => undefined} />)}{bookings.filter((booking) => booking.status === column.status).length === 0 && <p className="empty-board">Không có lịch hẹn</p>}</div></section>)}</div>}</div>
}

function BookingCard({ booking, onMove }: { booking: Booking; onMove?: (id: string, status: BookingStatus) => void }) {
  onMove = booking.status === 'Pending' ? undefined : async () => {
    if (booking.status === 'Confirmed') await api.checkInBooking(booking.id)
    else if (booking.status === 'CheckedIn') await api.startBooking(booking.id)
    else if (booking.status === 'InProgress') await api.completeBooking(booking.id)
    else return
    window.location.reload()
  }
  const next: Partial<Record<BookingStatus, BookingStatus>> = { Pending: 'Confirmed', Confirmed: 'InProgress', CheckedIn: 'InProgress', InProgress: 'Completed' }
  const labels: Partial<Record<BookingStatus, string>> = { Pending: 'Xác nhận', Confirmed: 'Bắt đầu', InProgress: 'Hoàn tất' }
  return <motion.article layout className="booking-card"><div className="booking-card-top"><span>{booking.time}</span><small>{booking.id}</small></div><h4>{booking.customer}</h4><p className="plate"><CarFront size={15} /> {booking.plate}</p><p className="booking-service">{booking.service}</p>{booking.bay && <span className="bay-pill">{booking.bay}</span>}{next[booking.status] && onMove && <button onClick={() => onMove(booking.id, next[booking.status]!)} className="card-action">{labels[booking.status]} <ArrowRight size={14} /></button>}</motion.article>
}

function ComingSoon({ label }: { label: string }) { return <section className="coming-soon"><Sparkles size={28} /><h2>{label} đang được hoàn thiện</h2><p>App shell, điều hướng, API foundation và design system đã sẵn sàng cho module này.</p></section> }

export default App

// Kept temporarily for a safe source-level rollback; no route or navigation exposes it.
void StaffingPage
void ManagerBookingApprovalsPage
void VoucherWallet
void OperationsDashboard
void OperationsQueuePage
