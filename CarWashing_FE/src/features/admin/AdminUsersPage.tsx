import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { Building2, RefreshCw, ShieldCheck, UserCheck, UsersRound, X } from 'lucide-react'
import { api, ApiError, type AdminUser, type Branch, type BranchMembership, type PagedResult, type UserStatus } from '../../shared/api/client'
import { Button, PageHeader, StatusBadge, Surface } from '../../shared/ui'

const roles: AdminUser['role'][] = ['Admin', 'BranchManager', 'Staff', 'Customer']
const statuses: UserStatus[] = ['Active', 'Inactive', 'Suspended', 'Deleted']

function roleLabel(role: AdminUser['role']) {
  return role === 'BranchManager' ? 'Quản lý chi nhánh' : role === 'Staff' ? 'Nhân viên' : role === 'Customer' ? 'Khách hàng' : 'Quản trị viên'
}

function statusLabel(status: UserStatus) {
  return status === 'Active' ? 'Đang hoạt động' : status === 'Inactive' ? 'Vô hiệu hóa' : status === 'Suspended' ? 'Tạm khóa' : 'Đã xóa'
}

function statusTone(status: UserStatus) {
  return status === 'Active' ? 'success' : status === 'Suspended' ? 'warning' : status === 'Deleted' ? 'danger' : 'default'
}

export function AdminUsersPage() {
  const [result, setResult] = useState<PagedResult<AdminUser> | null>(null)
  const [query, setQuery] = useState('')
  const [role, setRole] = useState('')
  const [status, setStatus] = useState<'all' | UserStatus>('all')
  const [page, setPage] = useState(1)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [updatingId, setUpdatingId] = useState<string | null>(null)
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null)
  const [memberships, setMemberships] = useState<BranchMembership[]>([])
  const [branches, setBranches] = useState<Branch[]>([])
  const [selectedBranchId, setSelectedBranchId] = useState('')
  const [effectiveFrom, setEffectiveFrom] = useState(() => new Date().toISOString().slice(0, 10))
  const [membershipLoading, setMembershipLoading] = useState(false)
  const [membershipSaving, setMembershipSaving] = useState(false)

  const load = useCallback(async () => {
    setError('')
    setLoading(true)
    try {
      setResult(await api.getAdminUsers({ query: query.trim() || undefined, role: role || undefined, status: status === 'all' ? undefined : status, page, pageSize: 20 }))
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải danh sách người dùng.')
    } finally {
      setLoading(false)
    }
  }, [page, query, role, status])

  useEffect(() => { void load() }, [load])

  async function updateUser(user: AdminUser, action: () => Promise<AdminUser>) {
    setError('')
    setUpdatingId(user.userID)
    try {
      const updated = await action()
      setResult((current) => current ? { ...current, items: current.items.map((item) => item.userID === updated.userID ? updated : item) } : current)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể cập nhật người dùng.')
    } finally {
      setUpdatingId(null)
    }
  }

  function changeStatus(user: AdminUser, nextStatus: Exclude<UserStatus, 'Deleted'>) {
    if (nextStatus === user.status) return
    if (!window.confirm(`Xác nhận chuyển trạng thái của ${user.fullName} thành “${statusLabel(nextStatus)}”?`)) return
    void updateUser(user, () => api.updateAdminUserStatus(user.userID, nextStatus))
  }

  async function openMemberships(user: AdminUser) {
    setSelectedUser(user)
    setSelectedBranchId('')
    setMembershipLoading(true)
    setError('')
    try {
      const [items, branchResult] = await Promise.all([api.getUserBranchMemberships(user.userID), api.getBranches(false)])
      setMemberships(items)
      setBranches(branchResult.items)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể tải phân công chi nhánh.')
    } finally {
      setMembershipLoading(false)
    }
  }

  async function refreshMemberships(userId: string) {
    setMemberships(await api.getUserBranchMemberships(userId))
  }

  async function addMembership() {
    if (!selectedUser || !selectedBranchId) return
    setMembershipSaving(true)
    setError('')
    try {
      if (selectedUser.role === 'Staff') await api.addStaffBranchMembership({ userId: selectedUser.userID, branchId: selectedBranchId, effectiveFrom })
      else await api.addManagerBranchMembership({ userId: selectedUser.userID, branchId: selectedBranchId })
      await refreshMemberships(selectedUser.userID)
      setSelectedBranchId('')
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể thêm phân công chi nhánh.')
    } finally {
      setMembershipSaving(false)
    }
  }

  async function endMembership(item: BranchMembership) {
    if (!selectedUser) return
    setMembershipSaving(true)
    setError('')
    try {
      if (item.membershipType === 'Staff') await api.endStaffBranchMembership(item.id, new Date().toISOString().slice(0, 10))
      else await api.deactivateManagerBranchMembership(item.id)
      await refreshMemberships(selectedUser.userID)
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Không thể kết thúc phân công chi nhánh.')
    } finally {
      setMembershipSaving(false)
    }
  }

  const users = result?.items ?? []
  const activeUsers = users.filter((user) => user.status === 'Active').length

  return <div className="page-stack admin-workspace">
    <PageHeader title="Quản lý người dùng" description="Tìm kiếm tài khoản, phân quyền và kiểm soát trạng thái hoạt động. Thay đổi vai trò hoặc vô hiệu hóa sẽ yêu cầu người dùng đăng nhập lại." actions={<Button variant="secondary" onClick={() => void load()} loading={loading}><RefreshCw size={16} /> Làm mới</Button>} />
    <section className="metric-grid admin-metrics" aria-label="Tóm tắt người dùng">
      <Metric icon={<UsersRound />} label="Người dùng hiển thị" value={String(result?.totalCount ?? 0)} detail="Theo bộ lọc hiện tại" tone="blue" />
      <Metric icon={<UserCheck />} label="Đang hoạt động" value={String(activeUsers)} detail="Trong trang hiện tại" tone="aqua" />
      <Metric icon={<ShieldCheck />} label="Bảo vệ quản trị" value="Bật" detail="Không thể tự đổi quyền hoặc khóa mình" tone="orange" />
    </section>
    <Surface className="admin-table-surface">
      <div className="admin-toolbar">
        <label className="search-field"><span className="sr-only">Tìm kiếm người dùng</span><input value={query} onChange={(event) => { setPage(1); setQuery(event.target.value) }} placeholder="Tên đăng nhập, họ tên hoặc email" /></label>
        <label>Vai trò<select value={role} onChange={(event) => { setPage(1); setRole(event.target.value) }}><option value="">Tất cả vai trò</option>{roles.map((item) => <option key={item} value={item}>{roleLabel(item)}</option>)}</select></label>
        <label>Trạng thái<select value={status} onChange={(event) => { setPage(1); setStatus(event.target.value as typeof status) }}><option value="all">Tất cả trạng thái</option>{statuses.map((item) => <option key={item} value={item}>{statusLabel(item)}</option>)}</select></label>
      </div>
      {error && <div className="form-error" role="alert">{error}<Button variant="secondary" onClick={() => void load()}>Thử lại</Button></div>}
      {loading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : <div className="resource-table-wrap"><table className="resource-table"><thead><tr><th>Người dùng</th><th>Vai trò</th><th>Trạng thái</th><th>Đã xác thực email</th><th>Ngày tạo</th><th aria-label="Thao tác" /></tr></thead><tbody>
        {users.map((user) => <tr key={user.userID}><td><strong>{user.fullName}</strong><br /><small>{user.username} · {user.email}</small>{user.isCurrentUser && <><br /><StatusBadge tone="info">Tài khoản hiện tại</StatusBadge></>}</td><td><select value={user.role} disabled={user.isCurrentUser || user.status !== 'Active' || updatingId === user.userID} onChange={(event) => void updateUser(user, () => api.updateAdminUserRole(user.userID, event.target.value as AdminUser['role']))}>{roles.map((item) => <option key={item} value={item}>{roleLabel(item)}</option>)}</select></td><td><StatusBadge tone={statusTone(user.status)}>{statusLabel(user.status)}</StatusBadge></td><td><StatusBadge tone={user.emailVerified ? 'success' : 'warning'}>{user.emailVerified ? 'Đã xác thực' : 'Chưa xác thực'}</StatusBadge></td><td>{new Date(user.createdAt).toLocaleDateString('vi-VN')}</td><td><div className="inline-actions">{(user.role === 'Staff' || user.role === 'BranchManager') && <Button variant="ghost" disabled={user.status !== 'Active' || updatingId === user.userID} onClick={() => void openMemberships(user)}><Building2 size={16} /> Chi nhánh</Button>}{user.status === 'Deleted' ? <StatusBadge tone="danger">Chỉ xem</StatusBadge> : <select aria-label={`Trạng thái ${user.fullName}`} value={user.status} disabled={user.isCurrentUser || updatingId === user.userID} onChange={(event) => changeStatus(user, event.target.value as Exclude<UserStatus, 'Deleted'>)}>{statuses.filter((item): item is Exclude<UserStatus, 'Deleted'> => item !== 'Deleted').map((item) => <option key={item} value={item}>{statusLabel(item)}</option>)}</select>}</div></td></tr>)}
        {users.length === 0 && <tr><td colSpan={6}><div className="ui-state"><strong>Không có người dùng phù hợp</strong><p>Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc vai trò, trạng thái.</p></div></td></tr>}
      </tbody></table></div>}
      {result && result.totalPages > 1 && <div className="admin-toolbar"><span>{result.totalCount} người dùng · Trang {result.page}/{result.totalPages}</span><Button variant="secondary" disabled={!result.hasPrevious || loading} onClick={() => setPage((value) => value - 1)}>Trang trước</Button><Button variant="secondary" disabled={!result.hasNext || loading} onClick={() => setPage((value) => value + 1)}>Trang sau</Button></div>}
    </Surface>
    {selectedUser && <aside className="admin-drawer" aria-label="Phân công chi nhánh"><div className="drawer-heading"><div><p className="page-kicker">{roleLabel(selectedUser.role)}</p><h2>Chi nhánh trực thuộc</h2><p className="drawer-note">{selectedUser.fullName}</p></div><Button variant="ghost" aria-label="Đóng" onClick={() => setSelectedUser(null)}><X size={18} /></Button></div>{membershipLoading ? <div className="ui-skeleton" aria-busy="true"><span /><span /><span /></div> : <><div className="drawer-list workforce-list">{memberships.map((item) => <div key={item.id}><div><strong>{item.branchName}</strong><span>{item.membershipType === 'Manager' ? 'Quản lý chi nhánh' : `Nhân viên từ ${item.effectiveFrom ? new Date(item.effectiveFrom).toLocaleDateString('vi-VN') : '—'}`}</span></div><div><StatusBadge tone={item.isActive ? 'success' : 'default'}>{item.isActive ? 'Đang hiệu lực' : 'Đã kết thúc'}</StatusBadge>{item.isActive && <Button variant="ghost" loading={membershipSaving} onClick={() => void endMembership(item)}>Kết thúc</Button>}</div></div>)}{memberships.length === 0 && <div className="ui-state"><strong>Chưa có chi nhánh</strong><p>Thêm chi nhánh đầu tiên cho user này.</p></div>}</div><form onSubmit={(event) => { event.preventDefault(); void addMembership() }}><label>Chi nhánh<select value={selectedBranchId} onChange={(event) => setSelectedBranchId(event.target.value)} required><option value="">Chọn chi nhánh đang mở</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>{selectedUser.role === 'Staff' && <label>Ngày hiệu lực<input type="date" value={effectiveFrom} onChange={(event) => setEffectiveFrom(event.target.value)} required /></label>}<div className="drawer-actions"><Button type="submit" loading={membershipSaving} disabled={!selectedBranchId}><Building2 size={16} /> Thêm chi nhánh</Button></div></form></>}</aside>}
  </div>
}

function Metric({ icon, label, value, detail, tone }: { icon: ReactNode; label: string; value: string; detail: string; tone: 'blue' | 'aqua' | 'orange' }) {
  return <article className="metric"><span className={`metric-icon ${tone}`}>{icon}</span><p>{label}</p><strong>{value}</strong><small>{detail}</small></article>
}
