import { useCallback, useEffect, useState } from 'react'
import { ApiError, api, type Booking, type WashBay } from '../../shared/api/client'
import { Button, Surface } from '../../shared/ui'
import './ManagerBookingBoardPage.css'

type Stage = 'Pending' | 'Confirmed' | 'CheckedIn' | 'InProgress' | 'Completed' | 'Cancelled'

const columns: Array<{ status: Stage; label: string }> = [
  { status: 'Pending', label: 'Chờ xác nhận' },
  { status: 'Confirmed', label: 'Đã xác nhận' },
  { status: 'CheckedIn', label: 'Đã check-in' },
  { status: 'InProgress', label: 'Đang rửa' },
  { status: 'Completed', label: 'Hoàn tất' },
  { status: 'Cancelled', label: 'Đã hủy' },
]

export function ManagerBookingBoardPage() {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [bays, setBays] = useState<WashBay[]>([])
  const [selectedBay, setSelectedBay] = useState<Record<string, string>>({})
  const [staffBookingId, setStaffBookingId] = useState<string | null>(null)
  const [eligibleStaff, setEligibleStaff] = useState<Array<{ userId: string; fullName: string }>>([])
  const [staffId, setStaffId] = useState('')
  const [staffIds, setStaffIds] = useState<string[]>([])
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [working, setWorking] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const context = await api.getManagerBranchContext()
      const [items, bayResult] = await Promise.all([api.getManagerBookings(date), api.getWashBays(context.branchId)])
      setBookings(items)
      setBays(bayResult.items.filter((bay) => bay.isActive))
      setSelectedBay(Object.fromEntries(items.filter((item) => item.washBayId).map((item) => [item.id, item.washBayId!])))
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải lịch hẹn chi nhánh.')
    } finally {
      setLoading(false)
    }
  }, [date])

  useEffect(() => { void load() }, [load])

  async function perform(bookingId: string, action: 'confirm' | 'checkin' | 'start' | 'complete' | 'dispatch') {
    setWorking(bookingId)
    setError('')
    try {
      if (action === 'confirm') await api.confirmBooking(bookingId)
      if (action === 'checkin') await api.checkInBooking(bookingId)
      if (action === 'start') await api.startBooking(bookingId)
      if (action === 'complete') await api.completeBooking(bookingId)
      if (action === 'dispatch') {
        const washBayId = selectedBay[bookingId]
        if (!washBayId) throw new ApiError('Hãy chọn bãi rửa trước khi gán.')
        await api.dispatchBooking(bookingId, washBayId)
      }
      await load()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật trạng thái xe.')
    } finally {
      setWorking('')
    }
  }

  async function openStaff(booking: Booking) {
    setWorking(booking.id)
    setError('')
    try {
      setEligibleStaff(await api.getEligibleBookingStaff(booking.id))
      setStaffId('')
      setStaffIds([])
      setStaffBookingId(booking.id)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải nhân viên đang sẵn sàng.')
    } finally {
      setWorking('')
    }
  }

  async function saveStaff() {
    if (!staffBookingId || (staffIds.length === 0 && !staffId)) return
    setWorking(staffBookingId)
    setError('')
    try {
      const selected = staffIds.length > 0 ? staffIds : [staffId]
      const baseShare = Math.floor(10000 / selected.length) / 100
      const firstShare = Number((100 - baseShare * (selected.length - 1)).toFixed(2))
      await api.setBookingStaffWork(staffBookingId, {
        staff: selected.map((staffUserId, index) => ({ staffUserId, contributionPercent: index === 0 ? firstShare : baseShare })),
      })
      setStaffBookingId(null)
      await load()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể gán nhân viên.')
    } finally {
      setWorking('')
    }
  }

  return <div className="page-stack manager-booking-page">
    <Surface>
      <div className="section-heading">
        <label className="manager-date-filter">Ngày xem<input type="date" value={date} onChange={(event) => setDate(event.target.value)} /></label>
        <div><p className="eyebrow"><span /> Manager</p><h2>Điều phối lịch hẹn</h2><p className="manager-booking-intro">Xác nhận lịch, check-in xe, gán bãi/nhân viên, bắt đầu rửa và hoàn tất trong một luồng.</p></div>
        <Button variant="secondary" onClick={() => void load()} loading={loading}>Tải lại</Button>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
    </Surface>

    {staffBookingId && <Surface className="manager-staff-panel">
      <div><strong>Gán nhân viên thực hiện</strong><p>Danh sách chỉ gồm nhân viên đã check-in tại chi nhánh.</p></div>
      <select aria-label="Nhân viên thực hiện" value={staffId} onChange={(event) => setStaffId(event.target.value)}><option value="">Chọn nhân viên</option>{eligibleStaff.map((staff) => <option key={staff.userId} value={staff.userId}>{staff.fullName}</option>)}</select>
      <Button disabled={!staffId} loading={working === staffBookingId} onClick={() => void saveStaff()}>Xác nhận gán</Button>
      <div className="manager-staff-options">{eligibleStaff.map((staff) => <label key={staff.userId}><input type="checkbox" checked={staffIds.includes(staff.userId)} onChange={() => setStaffIds((current) => current.includes(staff.userId) ? current.filter((id) => id !== staff.userId) : [...current, staff.userId])} />{staff.fullName}</label>)}{eligibleStaff.length === 0 && <p>Không có nhân viên đang check-in.</p>}</div>
      <Button className="manager-multi-staff-submit" disabled={staffIds.length === 0} loading={working === staffBookingId} onClick={() => void saveStaff()}>Xác nhận gán ({staffIds.length})</Button>
      <Button variant="ghost" onClick={() => setStaffBookingId(null)}>Đóng</Button>
    </Surface>}

    {loading ? <Surface><div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div></Surface> : <div className="manager-booking-board">
      {columns.map((column) => {
        const items = bookings.filter((booking) => booking.status === column.status)
        return <section className={`manager-booking-column${column.status === 'Cancelled' ? ' manager-booking-column--cancelled' : ''}`} key={column.status}>
          <header><h3>{column.label}</h3><span>{items.length}</span></header>
          <div className="manager-booking-list">{items.map((booking) => <BookingCard key={booking.id} booking={booking} bays={bays} selectedBay={selectedBay[booking.id] ?? ''} working={working === booking.id} onBayChange={(value) => setSelectedBay((current) => ({ ...current, [booking.id]: value }))} onAction={(action) => void perform(booking.id, action)} onStaff={() => void openStaff(booking)} />)}{items.length === 0 && <p className="empty-board">Chưa có xe</p>}</div>
        </section>
      })}
    </div>}
  </div>
}

function BookingCard({ booking, bays, selectedBay, working, onBayChange, onAction, onStaff }: { booking: Booking; bays: WashBay[]; selectedBay: string; working: boolean; onBayChange: (value: string) => void; onAction: (action: 'confirm' | 'checkin' | 'start' | 'complete' | 'dispatch') => void; onStaff: () => void }) {
  const canDispatch = booking.status === 'CheckedIn'
  const isCancelled = booking.status === 'Cancelled'
  const formatDateTime = (value: string) => new Date(value).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })
  return <article className={`manager-booking-card${isCancelled ? ' manager-booking-card--cancelled' : ''}`}>
    <div className="booking-card-top"><span>{new Date(booking.bookingStartTime).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</span><small>{booking.vehiclePlate || booking.vehicleId.slice(0, 8)}</small></div>
    <h4>{booking.customerName || 'Khách hàng'}</h4>
    <p>{booking.items.map((item) => `${item.serviceName} ×${item.quantity}`).join(' · ')}</p>
    {booking.note && <small className="manager-booking-note">Ghi chú: {booking.note}</small>}
    {booking.staffWork.length > 0 && <div className="manager-staff-summary"><strong>Nhân viên phụ trách</strong><span>{booking.staffWork.map((staff) => `${staff.staffName} (${staff.contributionPercent}%)`).join(' · ')}</span></div>}
    {booking.status === 'Completed' && <div className="manager-completion-summary"><strong>Đã hoàn tất</strong><span>{new Date(booking.completedAt ?? booking.bookingEndTime).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</span></div>}
    {isCancelled && <div className="manager-cancellation-summary"><strong>Khách hàng đã hủy</strong><dl><div><dt>Giờ hẹn gốc</dt><dd>{formatDateTime(booking.bookingStartTime)}</dd></div><div><dt>Hủy lúc</dt><dd>{booking.cancelledAt ? formatDateTime(booking.cancelledAt) : 'Không có dữ liệu'}</dd></div><div><dt>Lý do hủy</dt><dd>{booking.cancellationReason?.trim() || 'Không nêu lý do'}</dd></div></dl></div>}
    {canDispatch && <label className="manager-bay-choice">Bãi rửa<select value={selectedBay} onChange={(event) => onBayChange(event.target.value)}><option value="">Chọn bãi rửa</option>{bays.map((bay) => <option key={bay.id} value={bay.id}>{bay.name}</option>)}</select></label>}
    {canDispatch && <Button variant="secondary" disabled={!selectedBay} loading={working} onClick={() => onAction('dispatch')}>Gán bãi rửa</Button>}
    {booking.status === 'Pending' && <Button loading={working} onClick={() => onAction('confirm')}>Xác nhận lịch</Button>}
    {booking.status === 'Confirmed' && <Button loading={working} onClick={() => onAction('checkin')}>Xác nhận xe check-in</Button>}
    {(booking.status === 'CheckedIn' || booking.status === 'InProgress') && <Button variant="secondary" loading={working} onClick={onStaff}>{booking.staffWork.length ? 'Đổi nhân viên' : 'Gán nhân viên'}</Button>}
    {booking.status === 'CheckedIn' && <Button loading={working} onClick={() => onAction('start')}>Bắt đầu rửa</Button>}
    {booking.status === 'InProgress' && <Button loading={working} onClick={() => onAction('complete')}>Xác nhận hoàn tất</Button>}
  </article>
}
