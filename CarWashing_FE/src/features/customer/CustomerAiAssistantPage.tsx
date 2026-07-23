import { useState } from 'react'
import { Link } from 'react-router'
import { api, ApiError, type AiCustomerAssistant } from '../../shared/api/client'

export function CustomerAiAssistantPage() {
  const [preference, setPreference] = useState('')
  const [data, setData] = useState<AiCustomerAssistant | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const load = () => { setLoading(true); setError(''); api.getCustomerAssistant({ preference }).then(setData).catch(cause => setError(cause instanceof ApiError ? cause.message : 'Không thể tải gợi ý AI.')).finally(() => setLoading(false)) }
  return <div className="page-stack"><section className="surface"><div className="section-heading"><div><p className="eyebrow"><span /> Trợ lý chăm sóc xe</p><h2>Gợi ý dành cho bạn</h2><p>AI chỉ đề xuất từ danh mục và ưu đãi đang hợp lệ; bạn luôn tự xác nhận khi đặt lịch.</p></div></div><label>Nhu cầu hôm nay<select value={preference} onChange={event => setPreference(event.target.value)}><option value="">Gợi ý tổng quát</option><option value="nhanh">Rửa nhanh</option><option value="deep clean">Làm sạch kỹ</option><option value="shine">Tăng độ bóng</option></select></label><button className="button button-primary" disabled={loading} onClick={load}>{loading ? 'Đang phân tích…' : 'Nhận gợi ý'}</button>{error && <p className="form-error">{error}</p>}</section>{data && <><section className="surface"><h2>Dịch vụ phù hợp</h2><div className="history-list">{data.recommendations.map(item => <article className="history-row" key={item.serviceId}><div><strong>{item.serviceName}</strong><p>{item.reason}</p></div><div><b>{item.price.toLocaleString('vi-VN')}₫</b><Link className="button button-secondary" to="/customer/bookings/new">Đặt lịch</Link></div></article>)}</div></section><section className="surface assistant-loyalty"><h2>Thành viên và ưu đãi</h2><div className="assistant-loyalty-summary"><p>{data.loyaltySummary}</p><p>{data.careTip}</p></div><div className="assistant-offer-list">{data.eligibleOffers.map(offer => <article className="history-row" key={offer.promotionId}><div><strong>{offer.name}</strong><p>{offer.description ?? offer.eligibilityNote}</p></div><small>Hết hạn {new Date(offer.expiresAt).toLocaleDateString('vi-VN')}</small></article>)}</div></section></>}</div>
}
