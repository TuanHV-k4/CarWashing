import { useState } from 'react'
import { Check, ChevronLeft, ChevronRight, Send, X } from 'lucide-react'
import { api, ApiError } from '../../shared/api/client'
import { Button } from '../../shared/ui'

type Segment = 'all' | 'at-risk' | 'expiring-points' | 'loyal'
type Props = { segment: Segment; onClose: () => void; onCreated: () => Promise<void> }

const labels: Record<Segment, string> = { all: 'Tất cả khách hàng', 'at-risk': 'Có nguy cơ rời bỏ', 'expiring-points': 'Điểm sắp hết hạn', loyal: 'Khách trung thành' }

export function CampaignBuilder({ segment, onClose, onCreated }: Props) {
  const [step, setStep] = useState(0)
  const [selectedSegment, setSelectedSegment] = useState<Segment>(segment)
  const [inactiveDays, setInactiveDays] = useState(45)
  const [name, setName] = useState('Ưu đãi quay lại rửa xe')
  const [code, setCode] = useState('')
  const [type, setType] = useState('PercentageDiscount')
  const [value, setValue] = useState(15)
  const [minimumSpend, setMinimumSpend] = useState(0)
  const [limit, setLimit] = useState<number | undefined>()
  const [endDate, setEndDate] = useState(() => new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10))
  const [preview, setPreview] = useState<{ eligibleCount: number; excludedCount: number; exclusionReasons: string[] } | null>(null)
  const [error, setError] = useState('')
  const [working, setWorking] = useState(false)

  const payload = () => ({
    promotion: { name: name.trim(), code: code.trim() || undefined, type, value, minimumSpend, startDate: new Date().toISOString(), endDate: new Date(`${endDate}T23:59:59`).toISOString(), totalUsageLimit: limit, isActive: true },
    audience: { segment: selectedSegment, inactiveDays },
  })
  const validateOffer = () => {
    if (!name.trim()) return 'Nhập tên campaign.'
    if (!endDate || new Date(`${endDate}T23:59:59`) <= new Date()) return 'Ngày hết hạn phải ở tương lai.'
    if (value < 0 || minimumSpend < 0 || limit !== undefined && limit <= 0) return 'Giá trị và quota phải hợp lệ.'
    return ''
  }
  async function previewAudience() {
    const validation = validateOffer(); if (validation) { setError(validation); return }
    setWorking(true); setError('')
    try { setPreview(await api.previewCampaign(payload())); setStep(2) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể xem trước đối tượng.') } finally { setWorking(false) }
  }
  async function create() {
    setWorking(true); setError('')
    try { await api.createCampaign(payload()); await onCreated(); setStep(3) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tạo campaign.') } finally { setWorking(false) }
  }
  const next = step === 0 ? () => setStep(1) : step === 1 ? previewAudience : () => setStep(Math.min(step + 1, 3))
  return <aside className="admin-drawer loyalty-drawer campaign-builder" aria-label="Tạo campaign" aria-live="polite">
    <div className="drawer-heading"><div><p className="page-kicker">Campaign in-app</p><h2>Tạo campaign</h2></div><Button variant="ghost" aria-label="Đóng" onClick={onClose}><X size={18} /></Button></div>
    <ol className="campaign-steps" aria-label="Các bước tạo campaign">{['Đối tượng', 'Ưu đãi', 'Xem trước', 'Xác nhận'].map((label, index) => <li key={label} className={index === step ? 'active' : index < step ? 'done' : ''}><span>{index < step ? <Check size={14} /> : index + 1}</span><b>{label}</b></li>)}</ol>
    {error && <p className="form-error" role="alert">{error}</p>}
    <div className="campaign-builder-body">
      {step === 0 && <section><h3>Chọn đối tượng</h3><p className="campaign-helper">Server sẽ xác định lại người nhận khi xem trước và gửi.</p><label>Phân khúc khách hàng<select value={selectedSegment} onChange={(event) => setSelectedSegment(event.target.value as Segment)}><option value="all">Tất cả khách hàng</option><option value="at-risk">Có nguy cơ rời bỏ</option><option value="expiring-points">Điểm sắp hết hạn</option><option value="loyal">Khách trung thành</option></select></label>{selectedSegment === 'at-risk' && <label>Không hoạt động từ<input type="number" min="1" max="3650" value={inactiveDays} onChange={(event) => setInactiveDays(Number(event.target.value))} /><small>ngày không có lượt rửa hoàn tất</small></label>}<div className="campaign-note"><strong>Nhóm đang chọn</strong><span>{labels[selectedSegment]}</span></div></section>}
      {step === 1 && <section><h3>Cấu hình ưu đãi</h3><label>Tên campaign<input value={name} maxLength={120} onChange={(event) => setName(event.target.value)} autoFocus /></label><label>Mã ưu đãi <small>(không bắt buộc)</small><input value={code} maxLength={50} onChange={(event) => setCode(event.target.value)} /></label><div className="campaign-field-grid"><label>Loại ưu đãi<select value={type} onChange={(event) => setType(event.target.value)}><option value="PercentageDiscount">Giảm theo %</option><option value="FixedDiscount">Giảm tiền mặt</option></select></label><label>Giá trị<input type="number" min="0" value={value} onChange={(event) => setValue(Number(event.target.value))} /></label></div><div className="campaign-field-grid"><label>Đơn tối thiểu<input type="number" min="0" value={minimumSpend} onChange={(event) => setMinimumSpend(Number(event.target.value))} /></label><label>Hết hạn<input type="date" value={endDate} onChange={(event) => setEndDate(event.target.value)} /></label></div><label>Quota tổng <small>(không bắt buộc)</small><input type="number" min="1" value={limit ?? ''} onChange={(event) => setLimit(event.target.value ? Number(event.target.value) : undefined)} /></label></section>}
      {step === 2 && <section><h3>Xem trước đối tượng</h3><div className="campaign-preview-count"><strong>{preview?.eligibleCount.toLocaleString('vi-VN') ?? 0}</strong><span>khách đủ điều kiện</span></div><dl className="campaign-preview-meta"><div><dt>Campaign</dt><dd>{name}</dd></div><div><dt>Đã loại</dt><dd>{preview?.excludedCount.toLocaleString('vi-VN') ?? 0}</dd></div></dl>{preview?.exclusionReasons.length ? <p className="campaign-helper">{preview.exclusionReasons.join(' ')}</p> : <p className="campaign-helper">Danh sách này sẽ được kiểm tra lại ngay khi gửi để bảo vệ quota và điều kiện ưu đãi.</p>}</section>}
      {step === 3 && <section className="campaign-success"><span><Check size={22} /></span><h3>Đã tạo và gửi campaign</h3><p>Campaign đã được lưu cùng snapshot người nhận. Analytics sẽ được làm mới trong Loyalty Console.</p><Button onClick={onClose}>Hoàn tất</Button></section>}
    </div>
    {step < 3 && <div className="drawer-actions"><Button variant="secondary" onClick={step === 0 ? onClose : () => setStep(step - 1)} disabled={working}>{step === 0 ? 'Huỷ' : <><ChevronLeft size={16} /> Quay lại</>}</Button>{step === 2 ? <Button loading={working} onClick={() => void create()}><Send size={16} /> Tạo và gửi</Button> : <Button loading={working} onClick={() => void next()}>Tiếp tục <ChevronRight size={16} /></Button>}</div>}
  </aside>
}
