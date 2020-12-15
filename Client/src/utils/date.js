import Day from 'dayjs'
const WEEK_MAP = {
  0: '日',
  1: '一',
  2: '二',
  3: '三',
  4: '四',
  5: '五',
  6: '六'
}

export function formatDate(value, formatType) {
  let result = Day(value).format(formatType)
  return result === 'Invalid Date' ? '' : result
}

export function getWeek (value) {
  return WEEK_MAP[Day(value).day()]
}
