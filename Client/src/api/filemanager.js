import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/Files/Load',  //加载应用列表
    method: 'get',
    params
  })
}

  export function add(data) {
    return request({
      url: '/Applications/AddOrUpdate',
      method: 'post',
      data
    })
  }

  export function upFile(data) {
    return request({
      url: '/Files/Upload',
      method: 'post',
      data
    })
  }

  export function update(data) {    
    return request({
      url: '/Applications/AddOrUpdate',
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
