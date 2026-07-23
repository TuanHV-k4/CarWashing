import { useCallback, useEffect, useState } from 'react'
import { ApiError, api, type Branch } from '../../shared/api/client'
import { Button, Surface } from '../../shared/ui'

type QueueItem = {
  bookingId: string
  serviceName: string
  position: number
  priority: number
  estimatedStart: string
}

export function OperationsQueuePage() {
  const [branches, setBranches] = useState<Branch[]>([])
  const [branchId, setBranchId] = useState('')
  const [queue, setQueue] = useState<QueueItem[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.getAccessibleBranches()
      .then((result) => {
        setBranches(result)
        setBranchId((current) => current || result[0]?.id || '')
      })
      .catch((cause) => setError(cause instanceof ApiError ? cause.message : 'Không thể tải chi nhánh.'))
  }, [])

  const loadQueue = useCallback(async () => {
    if (!branchId) return
    setLoading(true)
    setError('')
    try {
      setQueue(await api.getQueue(branchId))
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải hàng đợi.')
    } finally {
      setLoading(false)
    }
  }, [branchId])

  useEffect(() => { void loadQueue() }, [loadQueue])

  return <div className="page-stack">
    <Surface>
      <div className="section-heading">
        <div><p className="eyebrow"><span /> Điều phối tiếp thị</p><h2>Hàng đợi đã check-in</h2><p>Danh sách xe đang chờ được sắp theo mức ưu tiên và thời gian check-in.</p></div>
        <label className="manager-date-filter">Chi nhánh<select value={branchId} onChange={(event) => setBranchId(event.target.value)}>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
      {loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : <div className="history-list">
        {queue.length === 0 ? <p className="empty-board">Chưa có xe đã check-in đang chờ.</p> : queue.map((item) => <article className="history-row" key={item.bookingId}>
          <div><strong>Vị trí #{item.position} · {item.serviceName || 'Dịch vụ rửa xe'}</strong><p>Dự kiến bắt đầu {new Date(item.estimatedStart).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} · Ưu tiên {item.priority}</p></div>
        </article>)}
      </div>}
      <div className="inline-actions"><Button variant="secondary" onClick={() => void loadQueue()} loading={loading}>Tải lại</Button></div>
    </Surface>
  </div>
}
