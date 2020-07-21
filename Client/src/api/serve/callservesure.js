
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
    return request({   //待确认服务申请信息
      url: `/serve/ServiceOrder/GetUnConfirmedServiceOrderDetails?serviceOrderId=${data}`,
      method: 'get'
      
    })
  }

  export function GetDetails(data) {    
    return request({ ///api/serve/ServiceOrder/GetDetails
      url: '/serve/ServiceOrder/GetDetails',
      method: 'get',
      params:{id:data}
    })
  }

  export function CreateOrder(data) {     //客服新建服务单
    return request({
      url:"/serve/ServiceOrder/CustomerServiceAgentCreateOrder",
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
    return request({ //查看客户代码详细信息
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