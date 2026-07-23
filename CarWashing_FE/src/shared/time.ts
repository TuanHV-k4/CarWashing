export const branchTimeZone = 'Asia/Bangkok'

const dateFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: branchTimeZone,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

export function branchDateValue(value = new Date()) {
  const parts = dateFormatter.formatToParts(value)
  const part = (type: Intl.DateTimeFormatPartTypes) => parts.find((item) => item.type === type)?.value ?? ''
  return `${part('year')}-${part('month')}-${part('day')}`
}

export function formatBranchTime(value: string) {
  return new Intl.DateTimeFormat('vi-VN', { timeZone: branchTimeZone, hour: '2-digit', minute: '2-digit' }).format(new Date(value))
}

export function formatBranchDateTime(value: string, dateStyle: 'short' | 'medium' = 'medium') {
  return new Intl.DateTimeFormat('vi-VN', { timeZone: branchTimeZone, dateStyle, timeStyle: 'short' }).format(new Date(value))
}
