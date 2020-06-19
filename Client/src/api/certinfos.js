import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/certinfos/load',
    method: 'get',
    params
  })
}

export function loadForRole(roleId) {
  return request({
    url: '/certinfos/loadForRole',
    method: 'get',
    params: { appId: '', firstId: roleId }
  })
}

export function add(data) {
  return request({
    url: '/certinfos/add',
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: '/certinfos/update',
    method: 'post',
    data
  })
}

export function del(data) {
  return request({
    url: '/certinfos/delete',
    method: 'post',
    data
  })
}

