/**
 * @desc 函数防抖
 * @param fn 执行函数
 * @param delay 延迟执行毫秒数
 * @param immediate true 表立即执行，false 表非立即执行
 */
export function debounce (fn, delay = 500, immediate = false) {
  let timer = null
  return function (...args) {
    let context = this
    if (timer) {
      clearTimeout(timer)
    }
    if (immediate) {
      const callNow = !timer
      timer = setTimeout(() => {
        timer = null
      }, delay)
      if (callNow) { // 如果没有timer则立即执行
        fn.apply(context, args)
      }
    } else {
      timer = setTimeout(() => {
        fn.apply(context, args)
      }, delay)
    }
  }
}

/**
 * @desc 函数节流
 * @param fn 执行函数
 * @param delay 延迟执行毫秒数
 * @param type timestamp 表时间戳版，timer 表定时器版
 */
export function throttle(fn, delay = 500, type = 'timestamp') {
  if (type=== 'timestamp') {
    var previous = 0
  } else if (type=== 'timer'){
    var timer = null
  }
  return function (...args) {
    let context = this
    if (type === 'timestamp') { // 立即执行
      let now = Date.now()
      if (now - previous > delay) {
        fn.apply(context, args)
        previous = now
      }
    } else if (type === 'timer') { // 规定时间内最后执行
      if (!timer) {
        timer = setTimeout(() => {
          timer = null
          fn.apply(context, args)
        }, delay)
      }
    }
  }
}

/**
 * @desc 数组交换
 * @param {*} list 数组
 * @param {*} a 索引值
 * @param {*} b 索引值
 */
export function swap (list, a, b) {
  let temp = list[a]
  list[a] = list[b]
  list[b] = temp
}

/**
 * @desc 找到数组中对应的索引
 * @param {*} list 数组列表
 * @param {*} fn 自定义查询索引的函数
 */
export function findIndex (list, fn) {
  let index = list.findIndex(fn)
  return index
}

