/**
 * Created by jiachenpan on 16/11/18.
 */

export function parseTime(time, cFormat) {
  if (arguments.length === 0) {
    return null
  }
  const format = cFormat || '{y}-{m}-{d} {h}:{i}:{s}'
  let date
  if (typeof time === 'object') {
    date = time
  } else {
    if (('' + time).length === 10) time = parseInt(time) * 1000
    date = new Date(time)
  }
  const formatObj = {
    y: date.getFullYear(),
    m: date.getMonth() + 1,
    d: date.getDate(),
    h: date.getHours(),
    i: date.getMinutes(),
    s: date.getSeconds(),
    a: date.getDay()
  }
  const time_str = format.replace(/{(y|m|d|h|i|s|a)+}/g, (result, key) => {
    let value = formatObj[key]
    if (key === 'a') return ['一', '二', '三', '四', '五', '六', '日'][value - 1]
    if (result.length > 0 && value < 10) {
      value = '0' + value
    }
    return value || 0
  })
  return time_str
}

//将此刻的时间转化为我们需要的格式
export function timeToFormat(format, date){
  let result =''
  const now = date || new Date()
  const year = now.getFullYear()
  const month = now.getMonth() + 1
  const day = now.getDate()
  const hours = now.getHours()
  const minute = now.getMinutes()
  const seconds = now.getSeconds()
  if(format==='ymd'){ 
    //  /2020-01-02格式/
    result = year +'-'+ isZero(month) + '-' + isZero(day)
  } else if (format === 'yyyy-MM-dd HH-mm-ss') {
    return `${year}-${isZero(month)}-${isZero(day)} ${isZero(hours)}-${isZero(minute)}-${isZero(seconds)}`
  }
return result
}

function isZero (num) {
  return Number(num) < 10 ? '0' + num : num 
}

export function formatTime(format) {
  const now = new Date()
  var o = {
    'M+': now.getMonth() + 1, // month
    'd+': now.getDate(), // day
    'h+': now.getHours(), // hour
    'm+': now.getMinutes(), // minute
    's+': now.getSeconds(), // second
    'q+': Math.floor((now.getMonth() + 3) / 3), // quarter
    'S': now.getMilliseconds() // millisecond
  }
  if (/(y+)/.test(format)) {
    format = format.replace(RegExp.$1,
      (now.getFullYear() + '').substr(4 - RegExp.$1.length))
  }
  for (var k in o) {
    if (new RegExp('(' + k + ')').test(format)) {
      format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : ('00' + o[k]).substr(('' + o[k]).length))
    }
  }
  return format
}

// 将list转成tree，使用前注意把array进行深拷贝
export function listToTreeSelect(array, parent, tree) {
  tree = typeof tree !== 'undefined' ? tree : []
  parent = typeof parent !== 'undefined' ? parent : {
    id: null
  }

  var children = array.filter((val) => {
    return val.parentId === parent.id
  })

  if (children.length > 0) {
    if (parent.id === null) {
      tree = children
    } else {
      parent['children'] = children
    }

    children.forEach((val) => {
      listToTreeSelect(array, val)
    })
  }

  return tree
}

// 要转换数据库里面的layui图标为svg icon
// 如果数据库存的本来就是svg icon，请直接return icon
export function changeIcon(icon) {
  switch (icon) {
    case 'layui-icon-engine':
      return 'nested'
    case 'layui-icon-senior':
      return 'tree'
    case 'layui-icon-ok-circle':
      return 'eye'
    case 'layui-icon-help':
      return 'lock'
    case 'layui-icon-group':
      return 'chart'
    case 'layui-icon-set-fill':
      return 'edit'
    case 'layui-icon-theme':
      return 'icon'
    case 'layui-icon-share':
      return 'star'
    case 'layui-icon-app':
      return 'excel'
    case 'layui-icon-tabs':
      return 'edit'
    case 'layui-icon-user':
      return 'people'
    case 'layui-icon-cellphone':
      return 'example'
    case 'layui-icon-friends':
      return 'user'
    default:
      return 'list'
  }
}
// 深拷贝
export const deepClone = (obj) => {
  if (typeof obj !== 'object' && !obj) return obj
  var objArray = Array.isArray(obj) ? [] : {};
  if(obj && typeof obj === "object") {
    for(let key in obj) {
      if(Object.prototype.hasOwnProperty.call(obj, key)){
        if(obj[key] && typeof obj[key] === "object"){
          objArray[key] = deepClone(obj[key]);
        } else {
          objArray[key] = obj[key];
        }
      }
    }
  }
  return objArray;
}
