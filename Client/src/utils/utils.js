import store from '@/store'
// import { isPlainObject } from './validate'
import { serializeParams } from './process'
import { getSign } from '@/api/users'
import { Message } from 'element-ui'
export async function print (url, params) {
  const printUrl = await getPdfURL(url, params)
  if (printUrl) {
    window.open(printUrl, '_blank')
  }
  // const serializedParams = isPlainObject(params) ? serializeParams(params) : params
  // let printUrl = ''
  // if (isPlainObject(params)) {
  //   printUrl = `${process.env.VUE_APP_BASE_API}${url}?${serializedParams}&X-token=${store.state.user.token}`
  // } else {
  //   printUrl = `${process.env.VUE_APP_BASE_API}${url}/${serializedParams}?X-token=${store.state.user.token}`
  // }
  // window.open(printUrl, '_blank')
}
export async function getPdfURL (url, params) {
  console.log(url, 'url')
  const { serialNumber } = params
  const NOW_DATE = +new Date()
  let pdfURL = ''
  try {
    const { data } = await getSign({ serialNumber, timespan: NOW_DATE })
    params = {
      timespan: NOW_DATE,
      sign: data,
      'X-token': store.state.user.token,
      ...params
    }
    pdfURL = `${process.env.VUE_APP_BASE_API}${url}?${serializeParams(params)}`
  } catch (err) {
    Message.error(err.message)
    pdfURL = ''
  }
  return pdfURL
}

export function isMatchRole (roleName) { // 当前用户是否拥有对应角色
  const rolesList = store.state.user.userInfoAll.roles || []
  return rolesList && rolesList.length
  ? rolesList.indexOf(roleName) > -1
  : false
}

export function flatten(arr) { // 扁平化数组
  return arr.reduce((res, next) => {
    return res.concat(Array.isArray(next)? flatten(next) : next)
  }, [])
}

