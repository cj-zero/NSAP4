import request from '@/utils/request'

export function getReturnRepairList(params) { // 获取返厂维修列表
  return request({
    url: '/Serve/ReturnRepair/GetReturnRepairList', 
    method: 'get',
    params
  })
}

export function addExpress (data) { // 寄件
  return request({
    url: '/Serve/ReturnRepair/AddExpress',
    method: 'post',
    data
  })
}


export function withDrawExpress(data) { // 撤回
  return request({
    url: '/Serve/ReturnRepair/WithDrawExpress',  
    method: 'post',
    data
  })
}


export function getExpressInfo(params, _this) { // 获取物流信息
  return request({
    url: '/Serve/ReturnRepair/GetExpressInfo',  
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestFn = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}


export function importX (data) {
  return request({
    url: '/Material/Quotation/ImportMaterialPrice',  
    method: 'post',
    data
  })
}