
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

export function getContractList (params) {
  return request({
    url: '/Sap/SerialNumber/GetContractList',
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
export function delWorkOrder(params) {    
  return request({ //删除一个工单
    url: '/serve/ServiceOrder/DeleteWorkOrder',
    method: 'delete',
    params
  })
}
export function forServe(data) {      
  return request({ //查看客户代码详细信息
    url: `/Sap/BusinessPartner/GetCardInfoForServe?cardCode=${data}`,
    method: 'get'
  })
}

export function leftList() {      
  return request({ //呼叫服务(客服)页面左侧树数据源
    url: "/serve/ServiceOrder/ServiceWorkOrderTree",
    method: 'get',

  })
}

export function rightList(data) {      
  return request({ //呼叫服务(客服)右侧查询列表
    url: "/serve/ServiceOrder/ServiceWorkOrderList",
    method: 'get',
    params:data
  })
}

export function GetServiceOrderPictures(params) {
  return request({
    url: '/serve/ServiceOrder/GetServiceOrderPictures',
    method: 'get',
    params
  })}
// 撤销服务单
export function deleteServiceOrder(params) {
  return request({
    url: `/serve/ServiceOrder/UpDateServiceOrderStatus`,  //获取个人信息
    method: 'get',
    params
  })
}

// 查看工单详情
export function getWorkOrderDetailById(params) {
  return request({
    url: `/serve/ServiceOrder/GetWorkOrderDetailById`,
    method: 'get',
    params
  })
}

export function update(data) {
  return request({
    url: '/certinfos/delete',
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

export function add(data) {
  return request({
    url: '/certinfos/delete',
    method: 'post',
    data
  })
}
export function getReportDetail (params) { // 获取完工报告详情
  return request({
    url: '/serve/CompletionReports/GetCompletionReportDetailsWeb',
    method: 'get',
    params
  })
}

export function getSMServiceWorkOrderList (params) { // 获取销售呼叫服务列表
  return request({
    url: '/serve/ServiceOrder/SalesManServiceWorkOrderList',
    method: 'get',
    params
  })
}

// export function resetServiceOrder (data) {
//   return request({
//     url: '/serve/ServiceOrder/OneKeyResetServiceOrder',
//     method: 'post',
//     data
//   })
// }

export function resetServiceOrder (data) { // 一键重派服务单
  return request({
    url: '/serve/ServiceOrder/OneKeyResetServiceOrder',
    method: 'post',
    data
  })
}