import { useEffect, useRef, useState } from 'react'
import { CalendarDays, Check, Clock3, MapPin, UserRound, X } from 'lucide-react'
import { api, ApiError, type AvailabilitySlot, type Booking } from '../../shared/api/client'
import { branchDateValue, formatBranchDateTime, formatBranchTime } from '../../shared/time'
import { Button, PageHeader, StatusBadge, Surface } from '../../shared/ui'

const cancellableStatuses = new Set(['Pending', 'Confirmed', 'CheckedIn', 'InProgress'])

function messageFrom(cause: unknown, fallback: string) {
  return cause instanceof ApiError ? cause.message : fallback
}

function formatDateTime(value: string) {
  return formatBranchDateTime(value)
}

export function CustomerBookingsPage() {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [selected, setSelected] = useState<Booking | null>(null)
  const [mode, setMode] = useState<'cancel' | 'reschedule' | null>(null)
  const [reason, setReason] = useState('')
  const [date, setDate] = useState('')
  const [slots, setSlots] = useState<AvailabilitySlot[]>([])
  const [selectedSlot, setSelectedSlot] = useState('')
  const [detailBooking, setDetailBooking] = useState<Booking | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)
  const [detailError, setDetailError] = useState('')
  const [loading, setLoading] = useState(true)
  const [working, setWorking] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  async function load() {
    setLoading(true)
    try {
      const result = await api.getMyBookings()
      setBookings(result.items.sort((left, right) => new Date(right.bookingStartTime).getTime() - new Date(left.bookingStartTime).getTime()))
    } catch (cause) {
      setError(messageFrom(cause, 'Không thể tải lịch hẹn.'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { void load() }, [])

  function closePanel() {
    setMode(null)
    setSelected(null)
    setReason('')
    setSlots([])
    setSelectedSlot('')
    setError('')
  }

  function openReschedule(booking: Booking) {
    setSelected(booking)
    setMode('reschedule')
    setDate(branchDateValue(new Date(booking.bookingStartTime)))
    setSlots([])
    setSelectedSlot('')
    setError('')
  }

  async function openCompletedBookingDetail(booking: Booking) {
    setDetailBooking(booking)
    setDetailLoading(true)
    setDetailError('')
    try {
      const detail = await api.getBooking(booking.id)
      setDetailBooking({ ...detail, vehiclePlate: booking.vehiclePlate ?? detail.vehiclePlate })
    } catch (cause) {
      setDetailError(messageFrom(cause, 'Không thể tải chi tiết lịch hẹn.'))
    } finally {
      setDetailLoading(false)
    }
  }

  async function findSlots() {
    if (!selected || !date) return
    setWorking(true)
    setError('')
    try {
      const result = await api.getAvailability({
        branchId: selected.branchId,
        date,
        items: selected.items.map((item) => ({ serviceId: item.serviceId, quantity: item.quantity })),
      })
      setSlots(result.slots)
      setSelectedSlot(result.slots[0]?.startTime ?? '')
    } catch (cause) {
      setError(messageFrom(cause, 'Không thể tải khung giờ trống.'))
    } finally {
      setWorking(false)
    }
  }

  async function cancelBooking() {
    if (!selected) return
    setWorking(true)
    setError('')
    try {
      await api.cancelBooking(selected.id, { reason: reason.trim() || undefined })
      setNotice('Đã hủy lịch hẹn. Bạn có thể đặt lịch mới bất cứ lúc nào.')
      closePanel()
      await load()
    } catch (cause) {
      setError(messageFrom(cause, 'Không thể hủy lịch hẹn.'))
    } finally {
      setWorking(false)
    }
  }

  async function rescheduleBooking() {
    if (!selected || !selectedSlot) return
    setWorking(true)
    setError('')
    try {
      await api.rescheduleBooking(selected.id, { bookingStartTime: selectedSlot, expectedVersion: selected.version })
      setNotice('Đã gửi thay đổi lịch hẹn. Thời gian mới đã được lưu.')
      closePanel()
      await load()
    } catch (cause) {
      if (cause instanceof ApiError && cause.status === 409) {
        setError('Lịch hẹn vừa được cập nhật ở nơi khác. Dữ liệu mới đã được tải lại, vui lòng chọn lại khung giờ.')
        await load()
      } else {
        setError(messageFrom(cause, 'Không thể đổi lịch hẹn.'))
      }
    } finally {
      setWorking(false)
    }
  }

  const upcoming = bookings.filter((booking) => !['Completed', 'Cancelled', 'NoShow'].includes(booking.status))
  const past = bookings.filter((booking) => !upcoming.includes(booking))

  return <div className="page-stack customer-bookings-page">
    <PageHeader title="Lịch hẹn của tôi" description="Theo dõi, đổi lịch hoặc hủy lịch hẹn trước khi dịch vụ hoàn tất." />
    {notice && <p className="booking-notice" role="status">{notice}</p>}
    {error && !mode && <p className="form-error" role="alert">{error}</p>}
    {loading ? <Surface aria-busy="true"><p className="empty-board">Đang tải lịch hẹn…</p></Surface> : <>
      <BookingList title="Sắp tới" items={upcoming} onCancel={(booking) => { setSelected(booking); setMode('cancel'); setError('') }} onReschedule={openReschedule} />
      {past.length > 0 && <BookingList title="Đã kết thúc" items={past} onViewCompleted={(booking) => void openCompletedBookingDetail(booking)} />}
    </>}
    {selected && mode === 'cancel' && <aside className="admin-drawer booking-action-drawer" aria-label="Hủy lịch hẹn">
      <DrawerHeading title="Hủy lịch hẹn" onClose={closePanel} />
      <p className="drawer-note">Lịch {formatDateTime(selected.bookingStartTime)} sẽ bị hủy. Thao tác này không thể hoàn tác.</p>
      <label>Lý do hủy (không bắt buộc)<textarea value={reason} maxLength={500} onChange={(event) => setReason(event.target.value)} placeholder="Ví dụ: thay đổi kế hoạch" /></label>
      {error && <p className="form-error" role="alert">{error}</p>}
      <div className="drawer-actions"><Button variant="secondary" onClick={closePanel} disabled={working}>Quay lại</Button><Button onClick={() => void cancelBooking()} loading={working}>Xác nhận hủy</Button></div>
    </aside>}
    {selected && mode === 'reschedule' && <aside className="admin-drawer booking-action-drawer" aria-label="Đổi lịch hẹn">
      <DrawerHeading title="Đổi lịch hẹn" onClose={closePanel} />
      <p className="drawer-note">Giữ nguyên xe, chi nhánh và các dịch vụ đã chọn. Chỉ lịch đang chờ xác nhận mới có thể đổi.</p>
      <label>Ngày mới<input type="date" value={date} min={branchDateValue()} onChange={(event) => { setDate(event.target.value); setSlots([]); setSelectedSlot('') }} /></label>
      <Button variant="secondary" onClick={() => void findSlots()} loading={working}>Tìm khung giờ trống</Button>
      {slots.length > 0 && <div className="reschedule-slots" role="list" aria-label="Khung giờ trống">{slots.map((slot) => <button type="button" key={slot.startTime} role="listitem" className={selectedSlot === slot.startTime ? 'slot-button selected' : 'slot-button'} onClick={() => setSelectedSlot(slot.startTime)}>{formatBranchTime(slot.startTime)}<small>{slot.availableBayCount} bãi trống</small></button>)}</div>}
      {slots.length === 0 && !working && <p className="drawer-note">Chọn ngày rồi tìm khung giờ trống để tiếp tục.</p>}
      {error && <p className="form-error" role="alert">{error}</p>}
      <div className="drawer-actions"><Button variant="secondary" onClick={closePanel} disabled={working}>Hủy</Button><Button disabled={!selectedSlot || working} onClick={() => void rescheduleBooking()} loading={working}>Lưu lịch mới</Button></div>
    </aside>}
    <CompletedBookingDetailDialog booking={detailBooking} loading={detailLoading} error={detailError} onClose={() => { setDetailBooking(null); setDetailError('') }} />
  </div>
}

function DrawerHeading({ title, onClose }: { title: string; onClose: () => void }) {
  return <div className="drawer-heading"><div><p className="page-kicker">Lịch hẹn</p><h2>{title}</h2></div><Button variant="ghost" aria-label="Đóng" onClick={onClose}><X size={18} /></Button></div>
}

function BookingList({ title, items, onCancel, onReschedule, onViewCompleted }: { title: string; items: Booking[]; onCancel?: (booking: Booking) => void; onReschedule?: (booking: Booking) => void; onViewCompleted?: (booking: Booking) => void }) {
  return <Surface className="customer-booking-list"><div className="section-heading"><div><p className="eyebrow"><span /> Lịch hẹn</p><h2>{title}</h2></div><span className="booking-count">{items.length}</span></div>{items.length === 0 ? <p className="empty-board">Chưa có lịch hẹn trong mục này.</p> : <div className="customer-booking-rows">{items.map((booking) => {
    const canReschedule = booking.status === 'Pending'
    const canCancel = cancellableStatuses.has(booking.status)
    return <article className="customer-booking-row" key={booking.id}><div className="booking-time"><CalendarDays size={18} /><div><strong>{formatDateTime(booking.bookingStartTime)}</strong><p>{booking.branchName ?? 'Chi nhánh đã chọn'} · {booking.vehiclePlate ?? 'Xe của bạn'}</p></div></div><div className="booking-services"><strong>{booking.items.map((item) => `${item.serviceName} ×${item.quantity}`).join(' · ')}</strong><small><Clock3 size={13} /> {booking.items.reduce((sum, item) => sum + item.durationMinutesPerUnit * item.quantity, 0)} phút · {booking.totalAmount.toLocaleString('vi-VN')}₫</small></div><StatusBadge tone={booking.status === 'Confirmed' ? 'success' : booking.status === 'Pending' ? 'warning' : booking.status === 'Completed' ? 'info' : 'default'}>{booking.status}</StatusBadge><div className="inline-actions">{canReschedule && onReschedule && <Button variant="secondary" onClick={() => onReschedule(booking)}>Đổi lịch</Button>}{canCancel && onCancel && <Button variant="ghost" onClick={() => onCancel(booking)}>Hủy lịch</Button>}{booking.status === 'Completed' && <><Check size={18} aria-label="Đã hoàn tất" /><Button variant="ghost" onClick={() => onViewCompleted?.(booking)}>Chi tiết</Button></>}</div></article>
  })}</div>}</Surface>
}

export function CompletedBookingDetailDialog({ booking, loading, error, onClose }: { booking: Booking | null; loading: boolean; error: string; onClose: () => void }) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    const dialog = dialogRef.current
    if (booking && dialog && !dialog.open) dialog.showModal()
  }, [booking])
  if (!booking) return null
  const totalDuration = booking.items.reduce((sum, item) => sum + item.durationMinutesPerUnit * item.quantity, 0)
  return <dialog ref={dialogRef} className="booking-detail-dialog" aria-labelledby="completed-booking-title" onCancel={(event) => { event.preventDefault(); onClose() }} onClick={(event) => { if (event.target === event.currentTarget) onClose() }}>
    <div className="booking-detail-header"><div><p className="page-kicker">Lịch hẹn đã hoàn tất</p><h2 id="completed-booking-title">Chi tiết dịch vụ</h2><p>{formatDateTime(booking.bookingStartTime)}{booking.completedAt ? ` · Hoàn tất ${formatDateTime(booking.completedAt)}` : ''}</p></div><Button variant="ghost" type="button" aria-label="Đóng chi tiết" onClick={onClose}><X size={18} /></Button></div>
    {loading ? <div className="booking-detail-loading" aria-busy="true"><span /><span /><span /></div> : <>
      {error && <p className="form-error" role="alert">{error}</p>}
      <div className="booking-detail-context"><div><MapPin size={17} /><span>Chi nhánh<strong>{booking.branchName ?? 'Chưa có thông tin chi nhánh'}</strong>{booking.washBayName && <small>Khu vực rửa: {booking.washBayName}</small>}</span></div><div><UserRound size={17} /><span>Xe<strong>{booking.vehiclePlate ?? 'Xe của bạn'}</strong></span></div></div>
      <section className="booking-detail-section"><div className="booking-detail-section-title"><h3>Dịch vụ đã sử dụng</h3><span>{totalDuration} phút</span></div>{booking.items.map((item) => <div className="booking-detail-line" key={item.serviceId}><div><strong>{item.serviceName}</strong><small>{item.quantity} × {item.unitPrice.toLocaleString('vi-VN')}₫</small></div><b>{item.lineTotal.toLocaleString('vi-VN')}₫</b></div>)}<div className="booking-detail-total"><span>Tổng thanh toán</span><strong>{booking.totalAmount.toLocaleString('vi-VN')}₫</strong></div></section>
      <section className="booking-detail-section"><h3>Nhân viên phụ trách</h3>{booking.staffWork.length ? <div className="booking-staff-list">{booking.staffWork.map((staff) => <div key={staff.staffUserId}><span className="staff-avatar">{staff.staffName.slice(0, 1).toUpperCase()}</span><span><strong>{staff.staffName}</strong><small>{staff.workRole ?? 'Nhân viên thực hiện'} · {staff.contributionPercent}%</small></span></div>)}</div> : booking.assignedStaffName ? <div className="booking-staff-list"><div><span className="staff-avatar">{booking.assignedStaffName.slice(0, 1).toUpperCase()}</span><span><strong>{booking.assignedStaffName}</strong><small>Nhân viên phụ trách</small></span></div></div> : <p className="booking-detail-empty">Chưa ghi nhận nhân viên phụ trách cho lịch hẹn này.</p>}</section>
      {booking.note && <section className="booking-detail-section"><h3>Ghi chú</h3><p className="booking-detail-note">{booking.note}</p></section>}
    </>}
    <div className="booking-detail-actions"><Button variant="secondary" type="button" onClick={onClose}>Đóng</Button></div>
  </dialog>
}
