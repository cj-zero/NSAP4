import request from '@/utils/request'

export function getList() {
  return request({
    url: '/Corps/Load',  //加载应用列表
    method: 'get',
    
  })
}

  export function add(data) {
    return request({
      url: '/Applications/AddOrUpdate',
      method: 'post',
      data
    })
  }
  export function getImgUrl(data) {     
    return request({
      url:`/Files/Download/${data}`,
      method: 'get',
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
