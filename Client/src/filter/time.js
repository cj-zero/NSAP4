import { isZero } from '@/utils/index'
export function m2DHM (val) { // 分钟转化为 天-小时-分钟
  if (typeof val === 'number') {
    val = parseInt(val)
    let days = Math.floor(val / (24 * 60)) || '' // 几天
    let daysMin = days * 24 * 60 // 天数对应的分钟
    let hours = Math.floor((val - daysMin) / 60) || '' // 减去天数对应的分钟后剩余的小时
    let hoursMin = hours * 60
    let mins = (val - daysMin - hoursMin)
    return (days ? days + '天' : days) 
      + (hours ? hours + '小时' : hours)
      + (mins ? mins + '分钟' : (days || hours) ? '' : mins + '分钟') 
  }
  return ''
}

export function s2HMS (val) { // s -> HH::mm::ss
  if (typeof val === 'number') {
    val = parseInt(val)
    let hours = isZero(Math.floor(val / (60 * 60)) || 0) // 多少小时
    let hoursSec = hours * 60 * 60
    let mins = isZero(Math.floor((val - hoursSec) / 60) || 0)
    let minsSec = mins * 60
    let seconds = isZero(val - hoursSec - minsSec)
    console.log(hours, hoursSec, mins, minsSec, seconds)
    return `${hours}:${mins}:${seconds}`
  }
  return ''
}