import request from '@/utils/request'

export function getQuotationList(params) { // 加载报价单列表
  return request({
    url: '/Material/Quotation/Load',
    method: 'get',
    params
  })
}
 
 export function getServiceOrderList(params, _this) { // 加载服务单列表
  return request({
    url: '/Material/Quotation/GetServiceOrderList',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      if (_this) {
        _this.cancelRequestCustom = c
      }
    })
  })
}

export function getMaterialList(params, _this) { // 加载物料列表
  return request({
    url: '/Material/Quotation/GetMaterialCodeList',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestMaterial = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function getDetailsMaterial (params, _this) { // 获取审批的页面物料表格
  return request({
    url: '/Material/Quotation/GetDetailsMaterial',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestMaterial = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function getAllMaterialList(params, _this) { // 查询所有的物料列表（选择替换物料的时候使用）
  return request({
    url: '/Material/Quotation/MaterialCodeList',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestAllMaterial = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function getSerialNumberList(params, _this) { // 加载序列号设备列表
  return request({
    url: '/Material/Quotation/GetSerialNumberList',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestSerialList = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function repealOrder (data) { // 撤销报价单
  return request({
    url: '/Material/Quotation/Revocation',
    method: 'post',
    data
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

export function approveQuotationOrder (data) { // 审批报价单
  return request({
    url: '/Material/Quotation/Accraditation',
    method: 'post',
    data
  })
} 

export function getApprovePendingList (params) { // 获取未审批报价单列表
  return request({
    url: '/Material/Quotation/ApprovalPendingLoad',
    method: 'get',
    params
  })
}

export function updateOutboundOrder (data) { // 修改出库单信息
  return request({
    url: '/Material/Quotation/UpdateMaterial',
    method: 'post',
    data
  })
}


export function getExpressInfo (params) { // 根据快递单号查询物流信息
  return request({
    url: '/Expressage/GetExpressInfo',
    method: 'get',
    params
  })
}

export function deleteOrder (data) { // 删除报价单
  return request({
    url: '/Material/Quotation/Delete',
    method: 'post',
    data
  })
}

export function getMergeMaterial (params) { // 获取合并物料 新增快递单时
  return request({
    url: '/Material/Quotation/GetMergeMaterial',
    method: 'get',
    params
  })
}

export function printPickingList (data) { // 打印出库单前置接口
  return request({
    url: '/Material/Quotation/PrintStockRequisition',
    method: 'post',
    data
  })
}

export function getMaterialCodeOnHand (params) { // 根据仓库及物料编码获取库存
  return request({
    url: '/Material/Quotation/GetMaterialCodeOnHand',
    method: 'get',
    params
  })
}