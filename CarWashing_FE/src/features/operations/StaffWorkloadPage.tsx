import { useCallback, useEffect, useState } from 'react'
import { CarFront, CircleDollarSign, Gauge } from 'lucide-react'
import { ApiError, api, type Workload } from '../../shared/api/client'
import { Button, PageHeader, Surface } from '../../shared/ui'
import { branchDateValue } from '../../shared/time'

const today = branchDateValue
const firstDayOfMonth = () => `${branchDateValue().slice(0, 7)}-01`

export function StaffWorkloadPage() {
  const [from, setFrom] = useState(firstDayOfMonth)
  const [to, setTo] = useState(today)
  const [data, setData] = useState<Workload | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setData(await api.getMyWorkload(from, to))
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải khối lượng rửa của bạn.')
    } finally {
      setLoading(false)
    }
  }, [from, to])

  useEffect(() => { void load() }, [load])

  return <div className="page-stack">
    <PageHeader title="Khối lượng rửa của tôi" description="Chỉ tổng hợp các xe bạn được phân công và đã hoàn tất." />
    <Surface>
      <div className="section-heading">
        <div><h2>{data?.staffName || 'Khối lượng thực hiện'}</h2><p>Chọn khoảng thời gian để xem kết quả cá nhân.</p></div>
        <div className="inline-actions"><label>Từ<input type="date" value={from} onChange={(event) => setFrom(event.target.value)} /></label><label>Đến<input type="date" value={to} onChange={(event) => setTo(event.target.value)} /></label><Button variant="secondary" loading={loading} onClick={() => void load()}>Tải lại</Button></div>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
      {loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : data && <section className="metric-grid">
        <Metric icon={<CarFront />} label="Xe đã tham gia" value={data.vehiclesParticipated} />
        <Metric icon={<Gauge />} label="Xe quy đổi" value={data.equivalentVehicles.toFixed(2)} />
        <Metric icon={<CircleDollarSign />} label="Doanh thu quy đổi" value={`${data.equivalentRevenue.toLocaleString('vi-VN')}₫`} />
      </section>}
    </Surface>
  </div>
}

function Metric({ icon, label, value }: { icon: React.ReactNode; label: string; value: string | number }) {
  return <article className="metric"><span>{icon}</span><p>{label}</p><strong>{value}</strong></article>
}
