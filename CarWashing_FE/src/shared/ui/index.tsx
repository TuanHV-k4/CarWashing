import { LoaderCircle, type LucideIcon } from 'lucide-react'
import { useEffect, useRef, type ButtonHTMLAttributes, type ReactNode } from 'react'

type Tone = 'default' | 'info' | 'success' | 'warning' | 'danger'

export function Button({ variant = 'primary', loading, children, className = '', ...props }: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'primary' | 'secondary' | 'danger' | 'ghost'; loading?: boolean }) {
  return <button {...props} disabled={props.disabled || loading} className={`button button-${variant} ${className}`.trim()}>{loading && <LoaderCircle className="button-spinner" size={16} aria-hidden="true" />}{children}</button>
}

export function Surface({ children, className = '', as: Tag = 'section' }: { children: ReactNode; className?: string; as?: 'section' | 'article' | 'aside' }) {
  return <Tag className={`surface ${className}`.trim()}>{children}</Tag>
}

export function StatusBadge({ tone = 'default', children }: { tone?: Tone; children: ReactNode }) {
  return <span className={`status-badge status-${tone}`}>{children}</span>
}

export function PageHeader({ title, description, actions }: { title: string; description?: string; actions?: ReactNode }) {
  return <header className="page-header"><div><p className="page-kicker">AutoWash Pro</p><h2>{title}</h2>{description && <p>{description}</p>}</div>{actions && <div className="page-header-actions">{actions}</div>}</header>
}

export function MetricCard({ icon: Icon, label, value, detail, tone = 'aqua' }: { icon: LucideIcon; label: string; value: string; detail: string; tone?: 'aqua' | 'blue' | 'orange' }) {
  return <article className="metric"><span className={`metric-icon ${tone}`}><Icon size={18} /></span><p>{label}</p><strong>{value}</strong><small>{detail}</small></article>
}

export function AsyncState({ loading, error, isEmpty, onRetry, emptyTitle = 'Chưa có dữ liệu', emptyCopy = 'Dữ liệu sẽ xuất hiện ở đây khi sẵn sàng.', children }: { loading?: boolean; error?: string; isEmpty?: boolean; onRetry?: () => void; emptyTitle?: string; emptyCopy?: string; children: ReactNode }) {
  if (loading) return <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div>
  if (error) return <div className="ui-state ui-state-error" role="alert"><strong>Không thể tải dữ liệu</strong><p>{error}</p>{onRetry && <Button variant="secondary" onClick={onRetry}>Thử lại</Button>}</div>
  if (isEmpty) return <div className="ui-state"><strong>{emptyTitle}</strong><p>{emptyCopy}</p></div>
  return <>{children}</>
}

export function Drawer({ open, title, onClose, children }: { open: boolean; title: string; onClose: () => void; children: ReactNode }) {
  const ref = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])
  return <dialog ref={ref} className="admin-drawer" aria-labelledby="drawer-title" onCancel={(event) => { event.preventDefault(); onClose() }} onClick={(event) => { if (event.target === ref.current) onClose() }}><div className="drawer-heading"><h2 id="drawer-title">{title}</h2><Button variant="ghost" type="button" aria-label="Đóng" onClick={onClose}>×</Button></div>{children}</dialog>
}

export function Money({ value }: { value: number }) { return <>{value.toLocaleString('vi-VN')}₫</> }
