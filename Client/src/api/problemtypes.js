import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/problemtypes/load',
    method: 'get',
    params
  })
}

export function loadForRole(roleId) {
  return request({
    url: '/problemtypes/loadForRole',
    method: 'get',
    params: { appId: '', firstId: roleId }
  })
}

export function add(data) {
  return request({
    url: '/problemtypes/add',
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: '/problemtypes/update',
    method: 'post',
    data
  })
}

export function del(data) {
  return request({
    url: '/problemtypes/delete',
    method: 'post',
    data
  })
}

