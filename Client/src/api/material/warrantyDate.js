import request from '@/utils/request'

export function getSalesOrderList (params) { // 加载销售单列表
  return request({
    url: '/Material/SalesOrderWarrantyDate/Load',
    method: 'get',
    params
  })
}

export function updateDate (data) { // 修改保修i时间
  return request({
    url: '/Material/SalesOrderWarrantyDate/UpDate',
    method: 'post',
    data
  })
}

export function approve (data) { // 审批保修时间
  return request({
    url: '/Material/SalesOrderWarrantyDate/Approval',
    method: 'post',
    data
  })
}
