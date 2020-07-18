
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

  export function updateService(data) {    
    return request({ //修改服务端
      url: '/serve/ServiceOrder/ModifyServiceOrder',
      method: 'post',
      data
    })
  }
  
  export function CreateWorkOrder(data) {    
    return request({ //创建工单
      url: '/serve/ServiceOrder/CreateWorkOrder',
      method: 'post',
      data
    })
  }
  export function addWorkOrder(data) {    
    return request({ //新增一个工单
      url: '/serve/ServiceOrder/AddWorkOrder',
      method: 'post',
      data
    })
  }
  
  export function forServe(data) {    
    return request({ ///api/Sap/BusinessPartner/GetCardInfoForServe
      url: `/Sap/BusinessPartner/GetCardInfoForServe?cardCode=${data}`,
      method: 'get'
    })
  }
  export function update(data) {
    return request({
      url: '/certinfos/delete',
      method: 'post',
      data
    })}

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