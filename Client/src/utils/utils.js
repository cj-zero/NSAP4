import store from '@/store'
import { serializeParams } from './process'

export function print (url, params) {
  const printUrl = `${process.env.VUE_APP_BASE_API}${url}?${serializeParams(params)}&X-token=${store.state.user.token}`
  window.open(printUrl, '_blank')
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

