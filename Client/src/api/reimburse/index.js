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

export function getList (params) {
  return request({
    url: '/serve/Reimburse/Load',
    method: 'get',
    params
  })
}

export function getCategoryName (params) {
  return request({
    url: '/serve/Reimburse/GetListCategoryName',
    method: 'get',
    params
  })
}