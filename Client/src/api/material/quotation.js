import request from '@/utils/request'

export function getQuotationList(params) { // 加载报价单列表
  return request({
    url: '/Material/Quotation/Load',
    method: 'get',
    params
  })
}
 
 export function getServiceOrderList(params) { // 加载服务单列表
  return request({
    url: '/Material/Quotation/GetServiceOrderList',
    method: 'get',
    params
  })
}

export function getMaterialList(params) { // 加载物料列表
  return request({
    url: '/Material/Quotation/GetMaterialCodeList',
    method: 'get',
    params
  })
}

export function getSerialNumberList(params) { // 加载序列号设备列表
  return request({
    url: '/Material/Quotation/GetSerialNumberList',
    method: 'get',
    params
  })
}

export function AddQuotationOrder(data) { // 添加报价单
  return request({
    url: '/Material/Quotation/Add',
    method: 'post',
    data
  })
}

export function getQuotationDetail (params) { // 获取报价单详情
  return request({
    url: '/Material/Quotation/GetDetails',
    method: 'get',
    params
  })
}

export function updateQuotationOrder (data) { // 修改报价单
  return request({
    url: '/Material/Quotation/Update',
    method: 'post',
    data
  })
}

export function getQuotationMaterialCode (params) { // 获取该服务单所有报价单零件
  return request({
    url: '/Material/Quotation/GetQuotationMaterialCode',
    method: 'get',
    params
  })
}

export function approveQuotationOrder (data) {
  return request({
    url: '/Material/Quotation/Accraditation',
    method: 'post',
    data
  })
} 
