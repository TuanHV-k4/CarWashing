import { useCallback, useEffect, useState } from 'react'
import { CalendarDays, Check, Droplets } from 'lucide-react'
import { ApiError, api, type Booking, type Branch } from '../../shared/api/client'
import { Button, PageHeader, Surface } from '../../shared/ui'

const columns = [
  { status: 'Pending', label: 'Chờ xác nhận' },
  { status: 'Confirmed', label: 'Đã xác nhận' },
  { status: 'CheckedIn', label: 'Đã check-in' },
  { status: 'InProgress', label: 'Đang rửa' },
  { status: 'Completed', label: 'Hoàn tất' },
] as const

type Props = { boardOnly?: boolean }

export function OperationsBookingBoardPage({ boardOnly = false }: Props) {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [branches, setBranches] = useState<Branch[]>([])
  const [branchId, setBranchId] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [working, setWorking] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const [branchResult, bookingResult] = await Promise.all([
        api.getAccessibleBranches(),
        api.getBookings(branchId ? { branchId } : {}),
      ])
      setBranches(branchResult)
      setBranchId((current) => branchResult.some((branch) => branch.id === current) ? current : (branchResult[0]?.id ?? ''))
      setBookings(bookingResult.items)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải lịch hẹn.')
    } finally {
      setLoading(false)
    }
  }, [branchId])

  useEffect(() => { void load() }, [load])

  async function advance(booking: Booking) {
    setWorking(booking.id)
    setError('')
    try {
      if (booking.status === 'Confirmed') await api.checkInBooking(booking.id)
      else if (booking.status === 'CheckedIn') await api.startBooking(booking.id)
      else if (booking.status === 'InProgress') await api.completeBooking(booking.id)
      await load()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật trạng thái xe.')
    } finally {
      setWorking('')
    }
  }

  const active = bookings.filter((booking) => booking.status === 'InProgress').length
  const completed = bookings.filter((booking) => booking.status === 'Completed').length
  const branchName = branches.find((branch) => branch.id === branchId)?.name

  return <div className="page-stack">
    {!boardOnly && <>
      <PageHeader title="Trung tâm vận hành" description="Theo dõi lịch hẹn của chi nhánh bằng dữ liệu thực tế." />
      <section className="metric-grid">
        <Metric icon={<CalendarDays />} label="Tổng lịch hẹn" value={bookings.length} detail="Theo bộ lọc hiện tại" />
        <Metric icon={<Droplets />} label="Đang rửa" value={active} detail="Đang thực hiện" />
        <Metric icon={<Check />} label="Hoàn tất" value={completed} detail="Theo bộ lọc hiện tại" />
      </section>
    </>}
    <Surface>
      <div className="section-heading">
        <div><p className="eyebrow"><span /> Vận hành theo thời gian thực</p><h2>Lịch hẹn hôm nay</h2><p>Thông tin khách hàng, xe và dịch vụ được đồng bộ với màn quản lý.</p></div>
        {branchName && <p style={{ margin: 0, fontWeight: 700 }}>{branchName}</p>}
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
    </Surface>
    {loading ? <Surface><div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div></Surface> : <div className="manager-booking-board operations-booking-board">
      {columns.map((column) => {
        const items = bookings.filter((booking) => booking.status === column.status)
        return <section className="manager-booking-column" key={column.status}>
          <header><h3>{column.label}</h3><span>{items.length}</span></header>
          <div className="manager-booking-list">
            {items.map((booking) => <BookingCard key={booking.id} booking={booking} working={working === booking.id} onAdvance={() => void advance(booking)} />)}
            {items.length === 0 && <p className="empty-board">Chưa có xe</p>}
          </div>
        </section>
      })}
    </div>}
  </div>
}

function Metric({ icon, label, value, detail }: { icon: React.ReactNode; label: string; value: number; detail: string }) {
  return <article className="metric"><span>{icon}</span><p>{label}</p><strong>{value}</strong><small>{detail}</small></article>
}

function BookingCard({ booking, working, onAdvance }: { booking: Booking; working: boolean; onAdvance: () => void }) {
  const action = booking.status === 'Confirmed' ? 'Xác nhận xe check-in' : booking.status === 'CheckedIn' ? 'Bắt đầu rửa' : booking.status === 'InProgress' ? 'Hoàn tất' : null
  const services = booking.items.map((item) => `${item.serviceName} ×${item.quantity}`).join(' · ')
  return <article className="manager-booking-card">
    <div className="booking-card-top"><span>{new Date(booking.bookingStartTime).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</span></div>
    <h4>{booking.customerName || 'Khách hàng'}</h4>
    <p className="plate">{booking.vehiclePlate || 'Chưa có biển số xe'}</p>
    <p>{services || 'Chưa có dịch vụ'}</p>
    {booking.staffWork.length > 0 && <div className="manager-staff-summary"><strong>Nhân viên phụ trách</strong><span>{booking.staffWork.map((staff) => staff.staffName).filter(Boolean).join(' · ')}</span></div>}
    {action && <Button loading={working} onClick={onAdvance}>{action}</Button>}
  </article>
}
