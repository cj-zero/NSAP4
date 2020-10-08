/**
 * 
 * @param {*} val 值
 * @param {*} range 多少位数进行分割
 * @param {*} split 分隔符
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