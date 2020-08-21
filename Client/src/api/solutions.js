import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/solutions/load',
    method: 'get',
    params
  })
}

export function loadForRole(roleId) {
  return request({
    url: '/solutions/loadForRole',
    method: 'get',
    params: { appId: '', firstId: roleId }
  })
}

export function add(data) {
  return request({
    url: '/solutions/add',
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: '/solutions/update',
    method: 'post',
    data
  })
}

export function del(data) {
  return request({
    url: '/solutions/delete',
    method: 'post',
    data
  })
}

export function loadTechList (params) { // 加载技术人员解决方案来列表
  return request({
    url: '/Solutions/TechnicianLoad',
    method: 'get',
    params
  })
}

export function addTch (data) {
  return request({
    url: '/Solutions/TechnicianAdd',
    method: 'post',
    data
  })
}
