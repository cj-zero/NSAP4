
import request from '@/utils/request'

export function getTableList(params) {
    return request({
      url: '/serve/ServiceOrder/UnConfirmedServiceOrderList',  //服务单查询
      method: 'get',
      params
    })
  }

  export function SerialList(params) {
    return request({
      url: '/Sap/SerialNumber/Get',  //序列号查询
      method: 'get',
      params
    })
  }

export function getForm(data) {
  console.log(data)
    return request({
      url: `/serve/ServiceOrder/GetUnConfirmedServiceOrderDetails?serviceOrderId=${data}`,
      method: 'get'
      
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
    })}

    export function add(data) {
      return request({
        url: '/certinfos/delete',
        method: 'post',
        data
      })}