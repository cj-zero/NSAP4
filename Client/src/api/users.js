import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/users/load',
    method: 'get',
    params
  })
}

export function get(params) {
  return request({
    url: '/users/get',
    method: 'get',
    params
  })
}

export function add(data) {
  return request({
    url: '/users/addorupdate',
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: '/users/addorupdate',
    method: 'post',
    data
  })
}

export function changePassword(data) {
  return request({
    url: '/users/changepassword',
    method: 'post',
    data
  })
}

export function changeProfile(data) {
  return request({
    url: '/users/changeprofile',
    method: 'post',
    data
  })
}

export function del(data) {
  return request({
    url: '/users/delete',
    method: 'post',
    data
  })
}

export function loadByRole(params) {
  return request({
    url: '/users/loadByRole',
    method: 'get',
    params
  })
}
export function LoadByOrg(params) {
  return request({
    url: '/users/LoadByOrg',
    method: 'get',
    params
  })
}

export function blockUp (data) { // 停用
  return request({
    url: 'Users/BlockUp',
    method: 'post',
    data
  })
}

export function getUserInfoAll(params) { // 获取用户的所有信息
  return request({
    url: '/Users/GetUserAll', 
    method: 'get',
    params
  })
}
