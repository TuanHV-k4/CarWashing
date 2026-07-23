import { useEffect, useState } from 'react'
import { CalendarClock, Sparkles, TicketPercent } from 'lucide-react'
import { api, ApiError, type CustomerVoucher, type LoyaltyOverview } from '../../shared/api/client'
import { Button, PageHeader, Surface } from '../../shared/ui'

type Reward = { id: string; name: string; description?: string; pointsRequired: number; value: number; type: string }

function toError(cause: unknown) {
  return cause instanceof ApiError ? cause.message : 'Không thể tải thông tin thành viên.'
}

function currency(value: number) {
  return value.toLocaleString('vi-VN') + '₫'
}

export function CustomerLoyaltyPage() {
  const [overview, setOverview] = useState<LoyaltyOverview | null>(null)
  const [vouchers, setVouchers] = useState<CustomerVoucher[]>([])
  const [rewards, setRewards] = useState<Reward[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [redeeming, setRedeeming] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError('')
    try {
      const [summary, wallet, catalog] = await Promise.all([api.getMyLoyaltyOverview(), api.getMyVouchers(), api.getRewards()])
      setOverview(summary)
      setVouchers(wallet)
      setRewards(catalog.items)
    } catch (cause) {
      setError(toError(cause))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { void load() }, [])

  async function redeem(rewardId: string) {
    setRedeeming(rewardId)
    setError('')
    try {
      await api.redeemReward(rewardId)
      await load()
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể đổi ưu đãi.')
    } finally {
      setRedeeming(null)
    }
  }

  const balance = overview?.balance
  const progressTarget = overview?.nextTierMinSpent ?? 0
  const tierProgress = progressTarget > 0 ? Math.min(100, (balance?.totalSpent ?? 0) / progressTarget * 100) : 100

  return <div className="page-stack customer-loyalty-page">
    <PageHeader title="Thành viên & ưu đãi" description="Theo dõi điểm, hạng thành viên, ưu đãi và các giao dịch điểm gần đây." />
    {error && <div className="form-error" role="alert">{error}<Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div>}
    {loading ? <Surface aria-busy="true"><p className="empty-board">Đang tải thông tin thành viên…</p></Surface> : overview && <>
      <section className="loyalty-overview" aria-label="Tổng quan loyalty">
        <article><Sparkles size={19} /><small>Điểm hiện có</small><strong>{balance?.currentPoints.toLocaleString('vi-VN')}</strong><span>{balance?.lifetimePoints.toLocaleString('vi-VN')} điểm đã tích lũy</span></article>
        <article><TicketPercent size={19} /><small>Hạng thành viên</small><strong>{balance?.currentTier ?? 'Thành viên'}</strong><span>{balance?.totalVisits ?? 0} lượt rửa hoàn tất</span></article>
        <article><CalendarClock size={19} /><small>Điểm sắp hết hạn</small><strong>{overview.expiringPoints.toLocaleString('vi-VN')}</strong><span>{overview.nearestExpiryDate ? `Hạn gần nhất: ${new Date(overview.nearestExpiryDate).toLocaleDateString('vi-VN')}` : 'Chưa có điểm sắp hết hạn'}</span></article>
      </section>
      <Surface className="tier-progress-panel"><div className="section-heading"><div><p className="eyebrow"><span /> Tiến độ hạng</p><h2>{overview.nextTierName ? `Hướng tới hạng ${overview.nextTierName}` : 'Bạn đang ở hạng cao nhất'}</h2></div><strong>{Math.round(tierProgress)}%</strong></div>{overview.nextTierName ? <><p className="tier-progress-copy">Chi tiêu thêm {Math.max(0, progressTarget - (balance?.totalSpent ?? 0)).toLocaleString('vi-VN')}₫ để đạt mốc hạng tiếp theo.</p><div className="progress-track" aria-label={`${Math.round(tierProgress)}% tiến độ hạng`}><span style={{ width: `${tierProgress}%` }} /></div></> : <p className="tier-progress-copy">Bạn đã hoàn thành tất cả mốc hạng hiện có.</p>}</Surface>
      <section className="loyalty-content-grid"><Surface><div className="section-heading"><div><p className="eyebrow"><span /> Ví của bạn</p><h2>Voucher sẵn sàng</h2></div><span className="booking-count">{vouchers.length}</span></div><div className="voucher-grid">{vouchers.length === 0 ? <p className="empty-board">Chưa có voucher. Đổi reward bằng điểm để sử dụng cho lịch hẹn kế tiếp.</p> : vouchers.map((voucher) => <article className={voucher.canApply ? 'voucher-card' : 'voucher-card voucher-card-muted'} key={voucher.id}><span className="voucher-icon"><TicketPercent size={20} /></span><div><p>{voucher.source === 'Reward' ? 'Đổi từ điểm' : 'Khuyến mãi dành riêng'}</p><h3>{voucher.name}</h3><small>{voucher.description ?? 'Áp dụng khi đáp ứng điều kiện của voucher.'}</small></div><div className="voucher-value"><strong>{voucher.type === 'PercentageDiscount' ? `${voucher.value}%` : currency(voucher.value)}</strong><small>{voucher.status}</small></div></article>)}</div></Surface>
        <Surface><div className="section-heading"><div><p className="eyebrow"><span /> Điểm gần đây</p><h2>Sổ cái điểm</h2></div></div><div className="point-ledger">{overview.recentLedger.length === 0 ? <p className="empty-board">Chưa có giao dịch điểm.</p> : overview.recentLedger.map((entry) => <article key={entry.id}><div><strong className={entry.points >= 0 ? 'points-positive' : 'points-negative'}>{entry.points >= 0 ? '+' : ''}{entry.points.toLocaleString('vi-VN')} điểm</strong><p>{entry.description ?? entry.type}</p></div><small>{new Date(entry.createdAt).toLocaleDateString('vi-VN')}{entry.expiryDate ? ` · Hạn ${new Date(entry.expiryDate).toLocaleDateString('vi-VN')}` : ''}</small></article>)}</div></Surface>
      </section>
      <section className="reward-section"><div className="section-heading"><div><p className="eyebrow"><span /> Đổi bằng điểm</p><h2>Reward có thể đổi</h2></div></div><div className="voucher-grid">{rewards.map((reward) => <article className="voucher-card" key={reward.id}><span className="voucher-icon"><Sparkles size={20} /></span><div><p>{reward.pointsRequired.toLocaleString('vi-VN')} điểm</p><h3>{reward.name}</h3><small>{reward.description ?? 'Đổi ngay để thêm vào Ví voucher.'}</small></div><div className="voucher-value"><strong>{reward.type === 'PercentageDiscount' ? `${reward.value}%` : currency(reward.value)}</strong></div><Button variant="secondary" disabled={redeeming === reward.id} loading={redeeming === reward.id} onClick={() => void redeem(reward.id)}>Đổi voucher</Button></article>)}</div></section>
    </>}
  </div>
}
