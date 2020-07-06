import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/materialtypes/load',
    method: 'get',
    params
  })
}

export function loadForRole(roleId) {
  return request({
    url: '/materialtypes/loadForRole',
    method: 'get',
    params: { appId: '', firstId: roleId }
  })
}

export function add(data) {
  return request({
    url: '/materialtypes/add',
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: '/materialtypes/update',
    method: 'post',
    data
  })
}

export function del(data) {
  return request({
    url: '/materialtypes/delete',
    method: 'post',
    data
  })
}

