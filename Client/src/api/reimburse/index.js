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