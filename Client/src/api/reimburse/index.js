import request from '@/utils/request'

export function addOrder(data) { // 新建报销单
  return request({
    url: '/serve/Reimburse/Add',
    method: 'post',
    data
  })
}

export function updateOrder(data) { // 新建报销单
  return request({
    url: '/serve/Reimburse/UpDate',
    method: 'post',
    data
  })
}
export function getUserDetail (params) { // 获取用户详细信息 （部门、出差补贴金额）
  return request({
    url: '/serve/Reimburse/GetUserDetails',
    method: 'get',
    params
  })
}

export function getOrder(params) { // 新建报销单获取当前用户客户代码
  return request({
    url: '/serve/Reimburse/GetServiceOrder',
    method: 'get',
    params
  })
}

export function getDetails(params) { // 新建报销单获取当前用户客户代码
  return request({
    url: '/serve/Reimburse/GetDetails',
    method: 'get',
    params
  })
}

export function deleteCost (data) { // 删除费用
  return request({
    url: '/serve/Reimburse/DeleteCost',
    method: 'post',
    data
  })
}

export function getList (params) { // 加载表格数据
  return request({
    url: '/serve/Reimburse/Load',
    method: 'get',
    params
  })
}

export function getCategoryName (params) { // 获取字典类别
  return request({
    url: '/serve/Reimburse/GetListCategoryName',
    method: 'get',
    params
  })
}


export function withdraw (data) { // 撤销报销单
  return request({
    url: '/serve/Reimburse/Revocation',
    method: 'post',
    data
  })
}

export function deleteOrder (data) { // 删除报销单
  return request({
    url: '/serve/Reimburse/Delete',
    method: 'post',
    data
  })
}

export function approve (data) { // 审核接口
  return request({
    url: '/serve/Reimburse/Accraditation',
    method: 'post',
    data
  })
}

export function identifyInvoice (data) { // 识别发票
  return request({
    url: '/ocr/TecentOCR/TecentInvoiceOCR',
    method: 'post',
    data
  })
}

export function isSole (data) { // 判断发票是否唯一
  return request({
    url: 'serve/Reimburse/IsSole',
    method: 'post',
    data
  })
}

export function printOrder (params) { // 打印报销单
  return request({
    url: '/serve/Reimburse/Print',
    method: 'get',
    params
  })
}

export function getHistoryReimburseInfo (params) { // 客户历史报销单
  return request({
    url: '/serve/Reimburse/HistoryReimburseInfo',
    method: 'get',
    params
  })
}

export function exportList (params) { // 导出支付表格
  return request({
    url: '/serve/Reimburse/Export',
    method: 'get',
    params
  })
}

export function pay (data) { // 支付
  return request({
    url: '/serve/Reimburse/BatchAccraditation',
    method: 'post',
    data
  })
}

export function getReimburseOrgs (params) { // 查询费用归属数据
  return request({
    url: '/serve/ServiceOrder/GetReimburseOrgs',
    method: 'get',
    params
  })
}

export function addTravellingAllowance (data) { // 新增出差补贴
  return request({
    url: '/serve/Reimburse/AddTravellingAllowance',
    method: 'post',
    data
  })
}

export function getServiceDailyExpendSum (params, _this) { // 获取日费集合
  return request({
    url: '/serve/ServiceOrder/GetServiceDailyExpendSum',
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      if (_this) {
        _this.cancelRequestDailyExpend = c
      }
    })
  })
}