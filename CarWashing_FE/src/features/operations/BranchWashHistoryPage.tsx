import { useCallback, useEffect, useState } from 'react'
import { ChevronRight, Star, UsersRound, X } from 'lucide-react'
import { ApiError, api, getStoredSession, type Branch, type OperationalWashHistory } from '../../shared/api/client'
import { Button, PageHeader, Surface } from '../../shared/ui'
import { branchDateValue } from '../../shared/time'

const monthStart = () => `${branchDateValue().slice(0, 7)}-01`
const currency = (value: number) => `${value.toLocaleString('vi-VN')}₫`

export function BranchWashHistoryPage() {
  const isAdmin = getStoredSession()?.role === 'Admin'
  const [from, setFrom] = useState(monthStart)
  const [to, setTo] = useState(branchDateValue)
  const [search, setSearch] = useState('')
  const [branchId, setBranchId] = useState('')
  const [branches, setBranches] = useState<Branch[]>([])
  const [items, setItems] = useState<OperationalWashHistory[]>([])
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<OperationalWashHistory | null>(null)

  const load = useCallback(async (targetPage = page) => {
    setLoading(true); setError('')
    try {
      const result = await api.getOperationalWashHistory({ from, to, search: search.trim() || undefined, branchId: isAdmin ? branchId || undefined : undefined, page: targetPage, pageSize: 20 })
      setItems(result.items); setPage(result.page); setTotalCount(result.totalCount)
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải lịch sử rửa xe.') } finally { setLoading(false) }
  }, [branchId, from, isAdmin, page, search, to])

  useEffect(() => { void load(1) }, [from, to, search, branchId])
  useEffect(() => { if (isAdmin) void api.getBranches().then(result => setBranches(result.items)).catch(() => undefined) }, [isAdmin])

  const totalPages = Math.max(1, Math.ceil(totalCount / 20))
  return <div className="page-stack feedback-workspace"><PageHeader title="Lịch sử rửa xe" description={isAdmin ? 'Theo dõi các lần rửa xe trên toàn hệ thống hoặc theo chi nhánh.' : 'Theo dõi các lần rửa xe đã hoàn tất tại chi nhánh của bạn.'} /><Surface><div className="feedback-toolbar"><label>Từ ngày<input type="date" value={from} onChange={event => setFrom(event.target.value)} /></label><label>Đến ngày<input type="date" value={to} onChange={event => setTo(event.target.value)} /></label><label className="wash-history-search">Khách hàng hoặc biển số<input value={search} placeholder="Nhập tên hoặc biển số" onChange={event => setSearch(event.target.value)} /></label>{isAdmin && <label>Chi nhánh<select value={branchId} onChange={event => setBranchId(event.target.value)}><option value="">Tất cả chi nhánh</option>{branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}</div>{loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : error ? <div className="ui-state" role="alert"><strong>Không thể tải lịch sử rửa xe</strong><p>{error}</p><Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div> : items.length === 0 ? <div className="ui-state"><strong>Không có lịch sử phù hợp</strong><p>Thử thay đổi khoảng thời gian, tên khách hàng, biển số hoặc chi nhánh.</p></div> : <div className="feedback-list">{items.map(item => <article key={item.washHistoryId} className="feedback-row"><div className="feedback-row-main"><strong>{item.customerName} · {item.vehiclePlate}</strong><p>{item.services.join(' · ') || 'Chưa có dữ liệu dịch vụ'}</p><small>{item.branchName} · {new Date(item.washDate).toLocaleDateString('vi-VN')}</small></div><div className="feedback-row-meta"><b>{currency(item.finalAmount)}</b><span><UsersRound size={15} /> {item.staffMembers.length ? item.staffMembers.map(staff => staff.staffName).join(', ') : 'Chưa ghi nhận nhân viên'}</span><Button variant="ghost" onClick={() => setSelected(item)}>Xem chi tiết <ChevronRight size={16} /></Button></div></article>)}</div>}{totalPages > 1 && <div className="feedback-pagination"><Button variant="secondary" disabled={page === 1} onClick={() => void load(page - 1)}>Trang trước</Button><span>Trang {page}/{totalPages}</span><Button disabled={page >= totalPages} onClick={() => void load(page + 1)}>Trang sau</Button></div>}</Surface>{selected && <aside className="admin-drawer feedback-detail-drawer" aria-label="Chi tiết lịch sử rửa xe"><div className="drawer-heading"><div><p className="page-kicker">Lần rửa đã hoàn tất</p><h2>{selected.customerName} · {selected.vehiclePlate}</h2><p className="drawer-note">{selected.branchName} · {new Date(selected.washDate).toLocaleDateString('vi-VN')}</p></div><Button variant="ghost" aria-label="Đóng" onClick={() => setSelected(null)}><X size={18} /></Button></div><section className="feedback-detail-section"><h3>Thanh toán</h3><ul><li><span>Tổng dịch vụ</span><strong>{currency(selected.actualTotalAmount)}</strong></li><li><span>Giảm giá</span><strong>{currency(selected.discountAmount)}</strong></li><li><span>Thành tiền</span><strong>{currency(selected.finalAmount)}</strong></li></ul></section><section className="feedback-detail-section"><h3>Dịch vụ đã sử dụng</h3><p>{selected.services.join(' · ') || 'Không có dữ liệu dịch vụ.'}</p></section><section className="feedback-detail-section"><h3>Nhân viên thực hiện</h3>{selected.staffMembers.length ? <ul>{selected.staffMembers.map(staff => <li key={staff.staffUserId}><strong>{staff.staffName}</strong><span>{[staff.workRole, staff.contributionPercent === undefined ? undefined : `${staff.contributionPercent}%`].filter(Boolean).join(' · ') || 'Đã phân công'}</span></li>)}</ul> : <p>Chưa ghi nhận nhân viên thực hiện.</p>}</section><section className="feedback-detail-section"><h3>Đánh giá của khách hàng</h3>{selected.customerRating ? <p className="feedback-rating"><Star size={16} fill="currentColor" /> {selected.customerRating}/5 sao</p> : <p className="muted">Khách hàng chưa đánh giá.</p>}<p className={selected.feedback ? '' : 'muted'}>{selected.feedback || 'Không có nhận xét.'}</p></section></aside>}</div>
}
