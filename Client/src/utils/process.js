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

// 处理查询字符
export function serializeParams (params) {
  let result = []
  for (let key in params) {
    if (params[key] !== undefined) {
      result.push(`${key}=${params[key]}`)
    } 
  }
  return result.join('&')
}

// 处理浮点数相加
export function accAdd (num1, num2) { 
  var r1,r2,m; 
  try {
    r1 = num1.toString().split(".")[1].length
  } catch(e) { 
    r1 = 0 
  } 
  try { 
    r2 = num2.toString().split(".")[1].length
  } catch(e) {
    r2 = 0  
  } 
  m = Math.pow(10,Math.max(r1, r2)) 
  console.log(num1, num2, r1, 2, m)
  return (num1 * m + num2 * m) / m 
}

export function accMul(num1, num2) { 
  let m = 0,
    s1 = num1.toString(),
    s2= num2.toString()
  try{
    m += s1.split(".")[1].length
  } catch(e){
    console.log(e)
  } 
  try{
    m += s2.split(".")[1].length
  } catch (e) {
    console.log(e)
  } 
  return Number(s1.replace(".","")) * Number(s2.replace(".","")) / Math.pow(10,m) 
} 

export function accDiv(num1,num2){ 
  let t1 = 0,t2 = 0, r1, r2 
  try{
    t1 = num1.toString().split(".")[1].length
  } catch(e)  {
    t1 = 0
  } 
  try{
    t2 = num2.toString().split(".")[1].length
  } catch(e)  {
    t2 = 0
  } 
  r1 = Number(num1.toString().replace(".","")) 
  r2 = Number(num2.toString().replace(".","")) 
  return (r1 / r2) * Math.pow(10, t2 - t1)
} 