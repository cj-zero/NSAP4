/**
 * 
 * @param {*} val 值
 * @param {*} range 多少位数进行分割
 * @param {*} split 分隔符
 * @param {*} decimalNumber 保留小数位数
 */
export function toThousands (val, range =  3, separator = ',', decimalNumber = 2) { // 千分位化
  let [integer, decimal] = String(val).split('.')
  let result = '', j = 1
  for (let i = integer.length - 1; i >= 0; i--) {
    result = integer[i] + result
    if (i !== integer.length - 1 && integer[i - 1]) {
      if (j % range === 0) {
        result = separator + result
      }
    }
    j++
  }
  return result + (decimal ? '.' + addZero(decimal, decimalNumber) : '.00')
}

/**
 * 
 * @param {*} str 值
 * @param {*} number 保留多少位数,不够则补零
 */
function addZero (str, number) {
  let result = ''
  let arr = str.split('')
  for (let i = 0; i < number; i++) {
    result += arr[i] || 0
  }
  return result
}

export function normalizeFormConfig (config) { /* 格式化表单的配置项 */
  let noneSlotConfig = config
  let result = [], j = 0
  for (let i = 0; i < noneSlotConfig.length; i++) {
    if (!result[j]) {
      result[j] = []
    }
    result[j].push(noneSlotConfig[i])
    if (noneSlotConfig[i].isEnd) {
      j++
    }
  }
  return result
}