import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/ModuleFlowSchemes/Load',
    method: 'get',
    params
  })
}

export function getPullDown() {
    return request({    //获取模块下拉框信息
      url: '/Check/GetDropdownModules',
      method: 'get'
    })
  }
  
  export function GetDropdownFlow() {
    return request({    //获取流程设计下拉列表
      url: '/FlowSchemes/GetDropdownFlowSchemes',
      method: 'get'
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
