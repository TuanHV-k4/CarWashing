import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router'
import { api, ApiError, getStoredSession, type AiOperationsCopilot, type Branch, type ManagerDashboard } from '../../shared/api/client'
import './ManagerDashboardPage.css'

const today = new Date().toISOString().slice(0, 10)
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })

export function ManagerDashboardPage() {
  const isAdmin = getStoredSession()?.role === 'Admin'
  const [date, setDate] = useState(today)
  const [branches, setBranches] = useState<Branch[]>([])
  const [branchId, setBranchId] = useState('')
  const [data, setData] = useState<ManagerDashboard | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [copilot, setCopilot] = useState<AiOperationsCopilot | null>(null)
  const [copilotError, setCopilotError] = useState('')

  const load = useCallback(() => {
    if (isAdmin && !branchId) { setData(null); setError(''); setLoading(false); return }
    setLoading(true)
    setError('')
    api.getManagerDashboard(date, branchId || undefined)
      .then(setData)
      .catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Unable to load dashboard.'))
      .finally(() => setLoading(false))
  }, [branchId, date, isAdmin])

  useEffect(() => { void load() }, [load])
  useEffect(() => {
    if (!isAdmin) return
    api.getBranches().then((result) => setBranches(result.items)).catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Unable to load branches.'))
  }, [isAdmin])

  const askCopilot = () => {
    if (isAdmin && !branchId) return
    setCopilotError('')
    api.askOperationsCopilot({ message: 'Tóm tắt vận hành và việc cần xử lý', from: `${date}T00:00:00Z`, to: `${date}T23:59:59Z`, ...(branchId ? { branchId } : {}) })
      .then(setCopilot)
      .catch((cause) => setCopilotError(cause instanceof ApiError ? cause.message : 'Unable to load operational analysis.'))
  }

  return <div className="page-stack command-center">
    <div className="page-header">
      <div><p className="page-kicker">Hiệu quả chi nhánh</p><h2>{data?.branchName ?? 'Dashboard quản lý'}</h2><p>Doanh thu, tiến độ booking và nhân sự theo từng chi nhánh.</p></div>
      <div className="inline-actions">
        {isAdmin && <label>Chi nhánh<select aria-label="Chi nhánh" value={branchId} onChange={(event) => setBranchId(event.target.value)}><option value="">Chọn chi nhánh</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}
        <label className="manager-dashboard-date">Ngày<input aria-label="Ngày" type="date" value={date} onChange={(event) => setDate(event.target.value)} /></label>
        <button className="button button-secondary" onClick={load}>Làm mới</button>
      </div>
    </div>
    {isAdmin && !branchId ? <div className="ui-state"><strong>Chọn chi nhánh</strong><p>Chọn một chi nhánh để xem dashboard vận hành.</p></div> : loading ? <div className="ui-skeleton"><span /><span /><span /></div> : error ? <div className="ui-state" role="alert"><strong>Không thể tải dữ liệu</strong><p>{error}</p><button className="button button-secondary" onClick={load}>Thử lại</button></div> : <>
      <section className="manager-revenue-section" aria-label="Doanh thu và số xe"><div className="section-heading"><div><h2>Doanh thu và xe hoàn tất</h2><p>So sánh theo ngày, tuần và tháng của ngày đã chọn.</p></div></div><div className="manager-revenue-grid">{data?.revenuePeriods.map((period) => <article className="manager-revenue-card" key={period.period}><p>{period.label}</p><strong>{money.format(period.netRevenue)}</strong><span>{period.completedVehicles} xe hoàn tất</span><dl><div><dt>Doanh thu</dt><dd>{money.format(period.grossRevenue)}</dd></div><div><dt>Hoàn tiền</dt><dd>{money.format(period.refundedAmount)}</dd></div></dl></article>)}</div></section>
      <div className="metric-grid">{data?.metrics.map((metric) => <article className="metric" key={metric.label}><span>{metric.label}</span><strong>{metric.value}{metric.format.startsWith('of:') ? `/${metric.format.slice(3)}` : ''}</strong></article>)}</div>
      <section className="surface"><div className="section-heading"><div><h2>AI Operations Copilot</h2><p>Phân tích chỉ dùng dữ liệu của chi nhánh đang chọn.</p></div><button className="button button-secondary" onClick={askCopilot}>Phân tích hôm nay</button></div>{copilotError && <p className="form-error">{copilotError}</p>}{copilot && <><p>{copilot.answer}</p><div className="history-list">{copilot.evidence.map((item) => <article className="history-row" key={item.label}><div><strong>{item.label}: {item.value}</strong><p>{item.period}</p></div></article>)}</div></>}</section>
      <section className="surface"><div className="section-heading"><div><h2>Việc cần xử lý</h2><p>Các hạng mục được xếp theo mức độ khẩn cấp.</p></div></div><div className="history-list">{data?.actions.length ? data.actions.map((action) => <article className="history-row" key={`${action.type}-${action.occurredAt}`}><div><strong>{action.title}</strong><p>{action.detail}</p></div><Link className="button button-secondary" to={action.actionPath}>Mở xử lý</Link></article>) : <div className="ui-state"><strong>Không có việc khẩn</strong><p>Lịch hẹn và điều phối hiện ổn định.</p></div>}</div></section>
    </>}
  </div>
}
