import request from '@/utils/request'

export function getReturnNoteList(params) { // 获取退料单列表
  return request({
    url: 'ReturnNotes/GetReturnNoteList',
    method: 'get',
    params
  })
}


export function getServiceOrderInfo (params, _this) {
  return request({
    url: '/ReturnNotes/GetServiceOrderInfo',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestOrder = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function getQuotationMaterialCode (params) { // 根据服务单ID 获取该服务单下的所有物料信息
  return request({
    url: '/Material/Quotation/GetQuotationMaterialCode',
    method: 'get',
    params,
    // cancelToken: new request.cancelToken(function executor(c) {
    //   _this.cancelRequestOrder = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    // })
  })
}

export function returnMaterials(data) { // 退料
  return request({
    url: '/ReturnNotes/ReturnMaterials',
    method: 'post',
    data
  })
}

export function getReturnNoteDetail(params) { // 获取退料详情
  return request({
    url: '/ReturnNotes/GetReturnNoteDetail',
    method: 'get',
    params
  })
}

export function getExpressInfo (params) { // 获取物流详情
  return request({
    url: '/ReturnNotes/GetExpressageInfo',
    method: 'get',
    params
  })
}

export function saveReceiveInfo(data) { // 保存仓库验收记录
  return request({
    url: '/ReturnNotes/SaveReceiveInfo',
    method: 'post',
    data
  })
}

export function accraditate(data) { // 验收仓库记录
  return request({
    url: '/ReturnNotes/Accraditation',
    method: 'post',
    data
  })
}

export function getClearReturnNoteList (params) { // 获取结算退料列表
  return request({
    url: '/ReturnNotes/GetClearReturnNoteList',
    method: 'get',
    params
  })
}

export function getClearReturnNoteDetail (params) { // 获取结算退料详情
  return request({
    url: '/ReturnNotes/GetClearReturnNoteDetail',
    method: 'get',
    params
  })
}






 


