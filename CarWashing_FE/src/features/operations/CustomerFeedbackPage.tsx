import { useCallback, useEffect, useState } from 'react'
import { ChevronRight, Star, UsersRound, X } from 'lucide-react'
import { ApiError, api, getStoredSession, type Branch, type CustomerFeedback } from '../../shared/api/client'
import { Button, PageHeader, Surface } from '../../shared/ui'
import { branchDateValue } from '../../shared/time'

const monthStart = () => `${branchDateValue().slice(0, 7)}-01`
const commentPreview = (value?: string) => !value ? 'Không có nhận xét.' : value.length <= 160 ? value : `${value.slice(0, 160)}…`

export function CustomerFeedbackPage() {
  const isAdmin = getStoredSession()?.role === 'Admin'
  const [from, setFrom] = useState(monthStart)
  const [to, setTo] = useState(branchDateValue)
  const [rating, setRating] = useState('')
  const [branchId, setBranchId] = useState('')
  const [branches, setBranches] = useState<Branch[]>([])
  const [items, setItems] = useState<CustomerFeedback[]>([])
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<CustomerFeedback | null>(null)

  const load = useCallback(async (targetPage = page) => {
    setLoading(true); setError('')
    try {
      const result = await api.getCustomerFeedback({ from, to, rating: rating ? Number(rating) : undefined, branchId: isAdmin ? branchId || undefined : undefined, page: targetPage, pageSize: 20 })
      setItems(result.items); setPage(result.page); setTotalCount(result.totalCount)
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải đánh giá khách hàng.') } finally { setLoading(false) }
  }, [branchId, from, isAdmin, page, rating, to])

  useEffect(() => { void load(1) }, [from, to, rating, branchId])
  useEffect(() => { if (isAdmin) void api.getBranches().then(result => setBranches(result.items)).catch(() => undefined) }, [isAdmin])

  const totalPages = Math.max(1, Math.ceil(totalCount / 20))
  return <div className="page-stack feedback-workspace"><PageHeader title="Đánh giá khách hàng" description="Theo dõi phản hồi sau khi rửa xe và nhân viên đã thực hiện dịch vụ." /><Surface><div className="feedback-toolbar"><label>Từ ngày<input type="date" value={from} onChange={event => setFrom(event.target.value)} /></label><label>Đến ngày<input type="date" value={to} onChange={event => setTo(event.target.value)} /></label><label>Số sao<select value={rating} onChange={event => setRating(event.target.value)}><option value="">Tất cả</option>{[5, 4, 3, 2, 1].map(value => <option key={value} value={value}>{value} sao</option>)}</select></label>{isAdmin && <label>Chi nhánh<select value={branchId} onChange={event => setBranchId(event.target.value)}><option value="">Tất cả chi nhánh</option>{branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}</div>{loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : error ? <div className="ui-state" role="alert"><strong>Không thể tải đánh giá</strong><p>{error}</p><Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div> : items.length === 0 ? <div className="ui-state"><strong>Chưa có đánh giá phù hợp</strong><p>Thử thay đổi khoảng thời gian hoặc bộ lọc số sao.</p></div> : <div className="feedback-list">{items.map(item => <article key={item.washHistoryId} className="feedback-row"><div className="feedback-row-main"><div className="feedback-rating" aria-label={`${item.rating} trên 5 sao`}><Star size={16} fill="currentColor" /> {item.rating}/5</div><strong>{item.services.join(' · ') || 'Dịch vụ đã sử dụng'}</strong><p>{commentPreview(item.feedback)}</p><small>{item.branchName} · {new Date(item.washDate).toLocaleDateString('vi-VN')}</small></div><div className="feedback-row-meta"><span><UsersRound size={15} /> {item.staffMembers.length ? item.staffMembers.map(staff => staff.staffName).join(', ') : 'Chưa ghi nhận nhân viên thực hiện'}</span><Button variant="ghost" onClick={() => setSelected(item)}>Xem chi tiết <ChevronRight size={16} /></Button></div></article>)}</div>}{totalPages > 1 && <div className="feedback-pagination"><Button variant="secondary" disabled={page === 1} onClick={() => void load(page - 1)}>Trang trước</Button><span>Trang {page}/{totalPages}</span><Button disabled={page >= totalPages} onClick={() => void load(page + 1)}>Trang sau</Button></div>}</Surface>{selected && <aside className="admin-drawer feedback-detail-drawer" aria-label="Chi tiết đánh giá"><div className="drawer-heading"><div><p className="page-kicker">Đánh giá dịch vụ</p><h2>{selected.rating}/5 sao</h2><p className="drawer-note">{selected.branchName} · {new Date(selected.washDate).toLocaleDateString('vi-VN')}</p></div><Button variant="ghost" aria-label="Đóng" onClick={() => setSelected(null)}><X size={18} /></Button></div><section className="feedback-detail-section"><h3>Dịch vụ đã sử dụng</h3><p>{selected.services.join(' · ') || 'Không có dữ liệu dịch vụ'}</p></section><section className="feedback-detail-section"><h3>Nhân viên thực hiện</h3>{selected.staffMembers.length ? <ul>{selected.staffMembers.map(staff => <li key={staff.staffUserId}><strong>{staff.staffName}</strong>{staff.workRole && <span>{staff.workRole}</span>}</li>)}</ul> : <p>Chưa ghi nhận nhân viên thực hiện.</p>}</section><section className="feedback-detail-section"><h3>Nhận xét của khách hàng</h3><p className={selected.feedback ? '' : 'muted'}>{selected.feedback || 'Không có nhận xét.'}</p></section></aside>}</div>
}
