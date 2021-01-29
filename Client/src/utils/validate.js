/**
 * Created by jiachenpan on 16/11/18.
 */

/* 合法uri*/
export function validateURL(textval) {
  const urlregex = /^(https?|ftp):\/\/([a-zA-Z0-9.-]+(:[a-zA-Z0-9.&%$-]+)*@)*((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])){3}|([a-zA-Z0-9-]+\.)*[a-zA-Z0-9-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(:[0-9]+)*(\/($|[a-zA-Z0-9.,?'\\+&%$#=~_-]+))*$/
  return urlregex.test(textval)
}

/* 小写字母*/
export function validateLowerCase(str) {
  const reg = /^[a-z]+$/
  return reg.test(str)
}

/* 大写字母*/
export function validateUpperCase(str) {
  const reg = /^[A-Z]+$/
  return reg.test(str)
}

/* 大小写字母*/
export function validatAlphabets(str) {
  const reg = /^[A-Za-z]+$/
  return reg.test(str)
}

export function isMobile (mobile) {
  return /^1[3-9]\d{9}$/.test(mobile)
}

export function isPhone (phone) {
  return /^(?:0[1-9][0-9]{1,2}-)?[2-8][0-9]{6,7}$/.test(phone)
}

export function isCustomerCode (code) { // 客户代码规范
  return /^[a-z|A-Z]\d{5}$/g.test(code)
}

const _toString = Object.prototype.toString

export function isPlainObject (val) {
  return _toString.call(val) === '[object Object]'
}


export function isFunction (val) {
  return typeof val === 'function'
}

export function isNumber (val) {
  return typeof val === 'number' && !isNaN(val)
}

export function isIntegerNumber (val) {
  return isNumber(val) && (val % 1 === 0)
}
export function isObjectLike(value) {
  return !!value && typeof value == 'object'
}

export function isEqual (value, other) { // 比较两个值是否长的一样
  // 如果其中一个不为对象
  if (!isObjectLike(value) || !isObjectLike(other)) {
    return value === other
  }
  //  值相同直接return true
  if (value === other) {
    return true
  }
  let valueKeys = Object.keys(value).length
  let otherKeys = Object.keys(value).length
  // key的数量不一样
  if (valueKeys !== otherKeys) {
    return false
  }
  // 递归调用isEqual判断
  for (let key in valueKeys) {
    let res = isEqual(value[key], other[key])
    if (!res) {
      return false
    }
  }
  return true
}