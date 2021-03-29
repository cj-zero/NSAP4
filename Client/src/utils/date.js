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
const typeList = ['day', 'week', 'month', 'year', 'hour', 'minute', 'second']
const methodNameList = ['add', 'subtract']

function capitalizeFirstLetter (str) {
  return str.slice(0, 1).toUpperCase() + str.slice(1)
}

export function formatDate(value, formatType = 'YYYY-MM-DD') {
  let result = Day(value).format(formatType)
  return result === 'Invalid Date' ? '' : result
}

export function getWeek (value) {
  return WEEK_MAP[Day(value).day()]
}

export function getTimeStamp (date) {
  return Day(date).valueOf()
}

export function toDate (date) {
  return Day(date).toDate()
}

/* 定义 日期增加减少函数集合 */
const collections = {}
function setCollection () {
  // 生成类似 collections.addYear(date, number)的方法
  // collections.subtractYear(date, number)
  methodNameList.forEach(methodName => {
    typeList.forEach(type => {
      collections[methodName + capitalizeFirstLetter(type)] = (date, number) => {
        return Day(date)[methodName](number, type)
      }
    })
  })
}
setCollection()

export {
  collections
}
