import request from '@/utils/request'
import { getToken } from '@/utils/auth' // 验权

export function login(username, password) {
  return request({
    url: '/check/login',
    method: 'post',
    data: {
      Account: username,
      Password: password,
      AppKey: 'openauth'
    }
  })
}

export function getInfo(token) {
  return request({
    url: '/check/getusername',
    method: 'get',
    params: { token }
  })
}

export function getUserProfile() {
  return request({
    url: '/check/getuserprofile',
    method: 'get',
    params: { token: getToken() }
  })
}

export function getModules() {
  return request({
    url: '/check/getmodules',
    method: 'get',
    params: { token: getToken() }
  })
}

export function getModulesTree() {
  return request({
    url: '/Check/GetModulesTree',
    method: 'get',
    params: { token: getToken() }
  })
}

export function getOrgs(params) {
  return request({
    url: '/check/getorgs/',
    method: 'get',
     params
  })
}

export function getCorp() {
  return request({
    url: '/Check/GetCorp',
    method: 'get',
    params: { token: getToken() }
  })
}

export function getSubOrgs(data) {
  return request({
    url: '/check/getSubOrgs',
    method: 'get',
    params: data
  })
}

export function logout() {
  return request({
    url: '/check/logout',
    method: 'post'
  })
}


export function GetQrCode(data) {  //获取二维码
  return request({
    url: '/QrCode/Get',
    method: 'get',
    params:data
  })
}

export function ValidateLogin(data) {  //获取二维码
  return request({
    url: '/QrCode/ValidateLoginState',
    method: 'get',
    params:data
  })
}