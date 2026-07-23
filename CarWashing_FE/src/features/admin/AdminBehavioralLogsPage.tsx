import { useCallback, useEffect, useState } from 'react'
import { Download, RefreshCw } from 'lucide-react'
import { api, ApiError, type BehavioralActionType, type BehavioralLogFilter, type PagedResult, type BehavioralLog } from '../../shared/api/client'
import { Button, PageHeader, StatusBadge, Surface } from '../../shared/ui'
import { branchDateValue } from '../../shared/time'

const actionTypes: BehavioralActionType[] = ['ViewPromotion', 'Book', 'CancelBooking', 'LeaveFeedback', 'RedeemReward']
const actionLabels: Record<BehavioralActionType, string> = { ViewPromotion: 'Xem khuyến mãi', Book: 'Đặt lịch', CancelBooking: 'Hủy lịch', LeaveFeedback: 'Đánh giá', RedeemReward: 'Đổi ưu đãi' }
const dateValue = branchDateValue

export function AdminBehavioralLogsPage() {
  const [result, setResult] = useState<PagedResult<BehavioralLog> | null>(null)
  const [customerID, setCustomerID] = useState('')
  const [actionType, setActionType] = useState<BehavioralActionType | ''>('')
  const [from, setFrom] = useState(() => dateValue(new Date(Date.now() - 29 * 86400000)))
  const [to, setTo] = useState(() => dateValue(new Date()))
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')

  const filter = useCallback((targetPage = page): BehavioralLogFilter => ({ customerID: customerID.trim() || undefined, actionType: actionType || undefined, from: from ? `${from}T00:00:00Z` : undefined, to: to ? `${to}T23:59:59Z` : undefined, page: targetPage, pageSize: 20 }), [actionType, customerID, from, page, to])
  const load = useCallback(async (targetPage = page) => { setLoading(true); setError(''); try { setResult(await api.getBehavioralLogs(filter(targetPage))) } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể tải nhật ký hành vi.') } finally { setLoading(false) } }, [filter, page])
  useEffect(() => { void load() }, [load])

  async function exportCsv() {
    setExporting(true); setError('')
    try {
      const blob = await api.downloadBehavioralLogs(filter())
      const href = URL.createObjectURL(blob); const link = document.createElement('a')
      link.href = href; link.download = `behavioral-logs-${dateValue(new Date()).replaceAll('-', '')}.csv`; link.click(); URL.revokeObjectURL(href)
    } catch (cause) { setError(cause instanceof ApiError ? cause.message : 'Không thể xuất nhật ký hành vi.') } finally { setExporting(false) }
  }

  function applyFilters() { setPage(1); void load(1) }
  const items = result?.items ?? []
  return <div className="page-stack admin-workspace">
    <PageHeader title="Nhật ký hành vi" description="Theo dõi các tương tác khách hàng và xuất dữ liệu phục vụ kiểm tra." actions={<div className="inline-actions"><Button variant="secondary" onClick={() => void load()} loading={loading}><RefreshCw size={16} /> Làm mới</Button><Button onClick={() => void exportCsv()} loading={exporting}><Download size={16} /> Xuất CSV</Button></div>} />
    <Surface className="admin-table-surface behavioral-log-table">
      <div className="admin-toolbar">
        <label>Khách hàng ID<input value={customerID} onChange={(event) => setCustomerID(event.target.value)} placeholder="GUID (tùy chọn)" /></label>
        <label>Hành vi<select value={actionType} onChange={(event) => setActionType(event.target.value as BehavioralActionType | '')}><option value="">Tất cả hành vi</option>{actionTypes.map((item) => <option key={item} value={item}>{actionLabels[item]}</option>)}</select></label>
        <label>Từ ngày<input type="date" value={from} onChange={(event) => setFrom(event.target.value)} /></label>
        <label>Đến ngày<input type="date" value={to} onChange={(event) => setTo(event.target.value)} /></label>
        <Button variant="secondary" onClick={applyFilters}>Áp dụng</Button>
      </div>
      {error && <div className="form-error" role="alert">{error}<Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div>}
      {loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : <div className="resource-table-wrap"><table className="resource-table"><thead><tr><th>Thời điểm</th><th>Khách hàng</th><th>Hành vi</th><th>Điểm</th><th>Chi tiêu</th><th>Ghi chú</th></tr></thead><tbody>{items.map((item) => <tr key={item.logID}><td>{new Date(item.actionTime).toLocaleString('vi-VN')}</td><td><strong>{item.customerName ?? 'Không xác định'}</strong>{item.customerID && <><br /><small>{item.customerID}</small></>}</td><td><StatusBadge tone="info">{actionLabels[item.actionType]}</StatusBadge></td><td>{item.pointsChanged === 0 ? '—' : `${item.pointsChanged > 0 ? '+' : ''}${item.pointsChanged}`}</td><td>{item.spendingAmount ? `${item.spendingAmount.toLocaleString('vi-VN')} ₫` : '—'}</td><td>{item.notes ?? '—'}</td></tr>)}{items.length === 0 && <tr><td colSpan={6}><div className="ui-state"><strong>Không có nhật ký phù hợp</strong><p>Hãy thay đổi bộ lọc hoặc khoảng thời gian.</p></div></td></tr>}</tbody></table></div>}
      {result && result.totalPages > 1 && <div className="admin-toolbar"><span>{result.totalCount} bản ghi · Trang {result.page}/{result.totalPages}</span><Button variant="secondary" disabled={!result.hasPrevious || loading} onClick={() => setPage((value) => value - 1)}>Trang trước</Button><Button variant="secondary" disabled={!result.hasNext || loading} onClick={() => setPage((value) => value + 1)}>Trang sau</Button></div>}
    </Surface>
  </div>
}
