/**
 * 
 * @param {*} val 值
 * @param {*} range 多少位数进行分割
 * @param {*} split 分隔符
 */
export function toThousands (val, range =  3, separator = ',') { // 千分位化
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
  return result + (decimal ? '.' + decimal : '')
}