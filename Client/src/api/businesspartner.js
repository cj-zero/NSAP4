import request from '@/utils/request'

export function getList(params) {
    return request({
      url: '/Sap/BusinessPartner/Load',  //业务伙伴分页查询
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

